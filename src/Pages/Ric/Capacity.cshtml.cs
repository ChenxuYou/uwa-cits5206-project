using CostingTool.Data;
using CostingTool.Engine;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;

namespace CostingTool.Pages.Ric;

/// <summary>
/// Step 4: usable capacity (US-07), then forecast utilisation (US-08).
///
/// Capacity is built from one of the method's baselines, less each deduction with its note,
/// capped by staff where a person must be present. The arithmetic is
/// <see cref="CapacityEngine"/>'s; this page only collects the inputs, checks them and
/// stores the answer in <see cref="RicCapability.MaximumCapacity"/> for every later step.
/// Forecast utilisation is then judged against that answer.
/// </summary>
public class CapacityModel(CostingDbContext db, MethodConfigProvider methods) : RicPageModel(db)
{
    [BindProperty] public int CycleId { get; set; }

    [BindProperty] public List<CapacityInput> Inputs { get; set; } = [];

    [BindProperty] public string? UtilisationAssumptions { get; set; }

    /// <summary>
    /// The address of a link followed while this page held unsaved figures — the step bar,
    /// the breadcrumb, the sidebar. The page saves on the way out and then goes there (US-02:
    /// every entered value persists on navigation, without an explicit save).
    /// </summary>
    [BindProperty] public string? LeavingFor { get; set; }

    /// <summary>The method's baselines in this cycle's billable unit; empty for samples.</summary>
    public IReadOnlyList<CapacityBaseline> Baselines => CapacityEngine.BaselinesFor(Method, Cycle.BillableUnit);

    /// <summary>What one FTE gives in a year in this cycle's unit; null for samples.</summary>
    public decimal? StaffPerFte => CapacityEngine.StaffAvailabilityPerFte(Method, Cycle.BillableUnit);

    public MethodConfig Method => methods.Current;

    public async Task<IActionResult> OnGetAsync(int cycleId)
    {
        if (!await LoadCycleAsync(cycleId))
        {
            return NotFound();
        }

        await RememberStepAsync(4);
        CycleId = cycleId;
        Inputs = Cycle.Capabilities.Select(CapacityInput.From).ToList();
        UtilisationAssumptions = Cycle.UtilisationAssumptions;

        return Page();
    }

    public Task<IActionResult> OnPostAsync() => SaveAsync("/Ric/Rates");

    /// <summary>
    /// Going back a step saves first, as the rates step does.
    ///
    /// US-10 asks that nothing be lost by navigating backwards. The link that used to sit
    /// here threw away every baseline, deduction, forecast and note typed on this screen,
    /// so a custodian who went back to check one funding line came back to an empty step.
    /// It is held to the same checks as continuing: what is saved is always a capacity the
    /// later steps can price from.
    /// </summary>
    public Task<IActionResult> OnPostBackAsync() => SaveAsync("/Ric/Funding");

    private async Task<IActionResult> SaveAsync(string nextPage)
    {
        if (!await LoadCycleAsync(CycleId))
        {
            return NotFound();
        }

        if (!Cycle.IsEditable)
        {
            return RedirectToPage("/Ric/Review", new { cycleId = CycleId });
        }

        EntryChecks.ExplainUnreadableNumbers(ModelState, FieldLabel, "a number of billable units, such as 1200");

        var results = new Dictionary<int, UsableCapacity>();

        foreach (var input in Inputs)
        {
            var capability = Cycle.Capabilities.FirstOrDefault(x => x.Id == input.Id);
            if (capability is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid capability.");
                continue;
            }

            if (Usable(input, capability.Name) is { } usable)
            {
                results[input.Id] = usable;
                CheckForecast(input, capability.Name, usable.Usable);
            }
        }

        // The guide's Step 5 checklist asks for the utilisation assumptions to be documented,
        // and utilisation is the divisor behind every rate — so this is one of the three
        // places the tool insists on an explanation rather than merely offering room for one
        // (US-13).
        if (string.IsNullOrWhiteSpace(UtilisationAssumptions))
        {
            ModelState.AddModelError(
                nameof(UtilisationAssumptions),
                "Say how these forecast figures were arrived at — bookings, last year's usage, "
                + "planned downtime. Every rate is divided by them.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Cycle.UtilisationAssumptions = UtilisationAssumptions;

        foreach (var input in Inputs)
        {
            var capability = Cycle.Capabilities.First(x => x.Id == input.Id);
            var usable = results[input.Id];

            capability.CapacityBaseline = input.Baseline;
            capability.StatedBaseline = input.Baseline == CapacityBaseline.Stated ? input.StatedBaseline : 0m;
            capability.StatedBaselineNote = input.Baseline == CapacityBaseline.Stated ? input.StatedBaselineNote?.Trim() : null;
            capability.IsStaffReliant = input.IsStaffReliant;
            capability.StaffFte = input.IsStaffReliant ? input.StaffFte : 0m;
            capability.MaximumCapacity = usable.Usable;

            capability.ForecastUwaUse = input.UwaUse;
            capability.ForecastApfrUse = input.ApfrUse;
            capability.ForecastCommercialUse = input.CommercialUse;
            capability.AboveCapacityReason = input.Forecast > usable.Usable ? input.AboveCapacityReason?.Trim() : null;

            // Only deductions that take something off are kept, so the record itemises what
            // was actually deducted.
            Db.RicCapacityDeductions.RemoveRange(capability.CapacityDeductions);
            capability.CapacityDeductions = input.Deductions
                .Where(x => x.Amount > 0)
                .Select(x => new RicCapacityDeduction { Kind = x.Kind, Amount = x.Amount, Note = x.Note!.Trim() })
                .ToList();
        }

        RecordEdit();
        await Db.SaveChangesAsync();

        return Continue(LeavingFor, nextPage);
    }

    /// <summary>
    /// Usable capacity for what was typed, for the page to show beside the inputs; null where
    /// the inputs cannot produce one yet.
    /// </summary>
    public UsableCapacity? Preview(CapacityInput input)
    {
        var baseline = BaselineAmount(input);
        if (baseline is not > 0 || input.Deductions.Any(x => x.Amount < 0) || input.StaffFte < 0)
        {
            return null;
        }

        return CapacityEngine.Calculate(EngineInputs(input, baseline.Value));
    }

    /// <summary>The baseline figure an input refers to, in the billable unit.</summary>
    public decimal? BaselineAmount(CapacityInput input) =>
        input.Baseline == CapacityBaseline.Stated
            ? input.StatedBaseline
            : Baselines.FirstOrDefault(x => x.Key == input.Baseline)?.Amount;

    private UsableCapacity? Usable(CapacityInput input, string name)
    {
        var errors = ModelState.ErrorCount;

        if (input.Baseline == CapacityBaseline.Stated)
        {
            if (input.StatedBaseline <= 0)
            {
                ModelState.AddModelError(string.Empty, $"{name}: enter the baseline capacity, greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(input.StatedBaselineNote))
            {
                ModelState.AddModelError(string.Empty, $"{name}: say where the stated baseline comes from.");
            }
        }
        else if (Baselines.All(x => x.Key != input.Baseline))
        {
            ModelState.AddModelError(
                string.Empty,
                Baselines.Count == 0
                    ? $"{name}: capacity in {Cycle.BillableUnit.ToLowerInvariant()} has no standard baseline. State one and say where it comes from."
                    : $"{name}: choose machine availability, staff availability or a stated baseline.");
        }

        foreach (var deduction in input.Deductions)
        {
            if (!RicCapacityDeduction.Kinds.Contains(deduction.Kind))
            {
                ModelState.AddModelError(string.Empty, $"{name}: unknown deduction.");
            }
            else if (deduction.Amount < 0)
            {
                ModelState.AddModelError(string.Empty, $"{name}: {deduction.Kind.ToLowerInvariant()} cannot be negative.");
            }
            else if (deduction.Amount > 0 && string.IsNullOrWhiteSpace(deduction.Note))
            {
                // US-07: each deduction takes a note explaining it.
                ModelState.AddModelError(string.Empty, $"{name}: explain the {deduction.Kind.ToLowerInvariant()} deduction.");
            }
        }

        if (input.IsStaffReliant)
        {
            if (StaffPerFte is null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"{name}: a staff cap is worked out in time, so it cannot be applied to {Cycle.BillableUnit.ToLowerInvariant()}. "
                    + "Take the staff limit off as a deduction instead.");
            }
            else if (input.StaffFte <= 0)
            {
                ModelState.AddModelError(string.Empty, $"{name}: enter the FTE allocated to this capability, such as 0.05.");
            }
        }

        if (ModelState.ErrorCount > errors || BaselineAmount(input) is not > 0)
        {
            return null;
        }

        var usable = CapacityEngine.Calculate(EngineInputs(input, BaselineAmount(input)!.Value));

        if (usable.Usable <= 0)
        {
            ModelState.AddModelError(
                string.Empty,
                $"{name}: the deductions take away all of the baseline, leaving no usable capacity.");
            return null;
        }

        return usable;
    }

    private void CheckForecast(CapacityInput input, string name, decimal usable)
    {
        if (new[] { input.UwaUse, input.ApfrUse, input.CommercialUse }.Any(x => x < 0))
        {
            ModelState.AddModelError(string.Empty, $"{name}: utilisation cannot be negative.");
        }

        // Forecast, not capacity. This is the input the workbook makes easy to confuse,
        // and the one the whole calculation divides by — so it is mandatory and it is
        // checked here rather than left to the engine to refuse later.
        if (input.Forecast <= 0)
        {
            ModelState.AddModelError(string.Empty, $"{name}: forecast utilisation must be greater than zero. Every rate is divided by it.");
        }

        // US-08: a forecast above capacity is allowed — a new grant can outrun last year's
        // figures — but it is never left unexplained.
        if (input.Forecast > usable && string.IsNullOrWhiteSpace(input.AboveCapacityReason))
        {
            var unit = Cycle.BillableUnit.ToLowerInvariant();
            ModelState.AddModelError(
                string.Empty,
                $"{name}: the forecast of {input.Forecast:N1} {unit} is above the usable capacity of {usable:N1} {unit}. "
                + "That is allowed, but say why it can be met.");
        }
    }

    private CapacityInputs EngineInputs(CapacityInput input, decimal baseline) => new()
    {
        Baseline = baseline,
        Deductions = input.Deductions.Where(x => x.Amount > 0).Select(x => new CapacityDeduction(x.Kind, x.Amount)).ToList(),
        StaffFte = input.IsStaffReliant && StaffPerFte is not null ? input.StaffFte : null,
        StaffBaseline = input.IsStaffReliant ? StaffPerFte : null
    };

    public class CapacityInput
    {
        public int Id { get; set; }

        /// <summary><see cref="CapacityBaseline.Machine"/>, <see cref="CapacityBaseline.Staff"/> or <see cref="CapacityBaseline.Stated"/>.</summary>
        public string Baseline { get; set; } = CapacityBaseline.Machine;

        public decimal StatedBaseline { get; set; }

        public string? StatedBaselineNote { get; set; }

        /// <summary>One row per kind of deduction the guide names, in its order.</summary>
        public List<DeductionInput> Deductions { get; set; } =
            RicCapacityDeduction.Kinds.Select(x => new DeductionInput { Kind = x }).ToList();

        public bool IsStaffReliant { get; set; }

        public decimal StaffFte { get; set; }

        public decimal UwaUse { get; set; }

        public decimal ApfrUse { get; set; }

        public decimal CommercialUse { get; set; }

        public string? AboveCapacityReason { get; set; }

        public decimal Forecast => UwaUse + ApfrUse + CommercialUse;

        public static CapacityInput From(RicCapability capability) => new()
        {
            Id = capability.Id,
            Baseline = string.IsNullOrEmpty(capability.CapacityBaseline) ? CapacityBaseline.Machine : capability.CapacityBaseline,
            StatedBaseline = capability.StatedBaseline,
            StatedBaselineNote = capability.StatedBaselineNote,
            Deductions = RicCapacityDeduction.Kinds
                .Select(kind => capability.CapacityDeductions.FirstOrDefault(x => x.Kind == kind) is { } saved
                    ? new DeductionInput { Kind = kind, Amount = saved.Amount, Note = saved.Note }
                    : new DeductionInput { Kind = kind })
                .ToList(),
            IsStaffReliant = capability.IsStaffReliant,
            StaffFte = capability.StaffFte,
            UwaUse = capability.ForecastUwaUse,
            ApfrUse = capability.ForecastApfrUse,
            CommercialUse = capability.ForecastCommercialUse,
            AboveCapacityReason = capability.AboveCapacityReason
        };
    }

    public class DeductionInput
    {
        public string Kind { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string? Note { get; set; }
    }

    /// <summary>"Inputs[1].Deductions[0].Amount" → "Cryo-EM: maintenance", and the plain fields likewise.</summary>
    private string? FieldLabel(string key)
    {
        var plain = IndexedFieldLabel(key, Inputs.Select(x => x.Id).ToList(), new Dictionary<string, string>
        {
            ["StatedBaseline"] = "stated baseline",
            ["StaffFte"] = "staff FTE",
            ["UwaUse"] = "UWA forecast use",
            ["ApfrUse"] = "APFR forecast use",
            ["CommercialUse"] = "commercial forecast use"
        });

        if (plain is not null)
        {
            return plain;
        }

        // Deductions are one level deeper: map the key onto its kind, then reuse the lookup.
        var marker = "].Deductions[";
        var at = key.IndexOf(marker, StringComparison.Ordinal);
        if (at < 0 || !key.EndsWith("].Amount", StringComparison.Ordinal))
        {
            return null;
        }

        var start = at + marker.Length;
        var end = key.IndexOf(']', start);
        if (end < 0 || !int.TryParse(key.AsSpan(start, end - start), out var index) || index >= RicCapacityDeduction.Kinds.Length)
        {
            return null;
        }

        return IndexedFieldLabel(
            key[..(at + 2)] + "Deduction",
            Inputs.Select(x => x.Id).ToList(),
            new Dictionary<string, string> { ["Deduction"] = RicCapacityDeduction.Kinds[index].ToLowerInvariant() });
    }
}
