using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Ric;

/// <summary>
/// Step 1: the platform, its pricing period, its billable unit and its capabilities.
///
/// Opened without a cycle it creates one. Opened with a <c>cycleId</c> it edits that cycle's
/// answers (US-10: return to any earlier section and change any value). Editing is careful
/// about the three answers later steps are built on:
/// <list type="bullet">
/// <item><b>Removing a capability</b> removes the cost lines booked to it and its capacity,
/// so it has to be confirmed when there is anything to lose.</item>
/// <item><b>Changing the billable unit</b> makes every capacity, forecast and proposed rate
/// a figure in the wrong unit, so those are cleared — after confirmation — rather than left
/// to price in hours what was entered in days.</item>
/// <item><b>Changing the length of the pricing period</b> is refused once cost or funding
/// lines exist. Each line holds one amount per year and the engine prices from their mean,
/// so adding a year would mean inventing its amount and removing one would silently change
/// the mean. Moving the period without changing its length is allowed.</item>
/// </list>
///
/// <b>Replacing a sealed record</b> (US-01, F22). Opened with <c>?supersedes=</c> it starts
/// the cycle that replaces one of the custodian's sealed records: the platform, unit and
/// capabilities are carried over, the period starts where the old one ended, the old
/// record's key figures sit alongside, and the new cycle keeps a reference to it. The old
/// record itself is not touched. A new cycle for a platform that already has a sealed
/// record is asked whether it replaces it, rather than being left to start unconnected.
/// </summary>
public class StartModel(CostingDbContext db) : RicPageModel(db)
{
    public static readonly string[] BillableUnits = ["Hours", "Days", "Samples"];

    /// <summary>Null while creating a cycle; the cycle being edited otherwise.</summary>
    [BindProperty] public int? CycleId { get; set; }

    [BindProperty] public string PlatformName { get; set; } = string.Empty;

    [BindProperty] public int StartYear { get; set; } = DateTime.Now.Year;

    [BindProperty] public int EndYear { get; set; } = DateTime.Now.Year + 2;

    [BindProperty] public string BillableUnit { get; set; } = "Hours";

    /// <summary>Capabilities to add: all of them while creating, new ones while editing.</summary>
    [BindProperty] public string? CapabilityNames { get; set; }

    /// <summary>The cycle's capabilities as they stand, each renamable or removable. Editing only.</summary>
    [BindProperty] public List<CapabilityEdit> Existing { get; set; } = [];

    /// <summary>The custodian accepts that removing capabilities removes their costs and capacity.</summary>
    [BindProperty] public bool ConfirmRemovals { get; set; }

    /// <summary>The custodian accepts that changing the unit clears capacity, forecasts and proposed rates.</summary>
    [BindProperty] public bool ConfirmUnitChange { get; set; }

    /// <summary>The sealed record a new cycle replaces. Creating only; a cycle's reference is fixed once made.</summary>
    [BindProperty(SupportsGet = true)] public int? Supersedes { get; set; }

    /// <summary>The custodian says a new cycle for a platform with a sealed record does not replace it.</summary>
    [BindProperty] public bool ConfirmNotReplacing { get; set; }

    public bool IsEditing => CycleId is not null;

    /// <summary>
    /// The sealed record whose figures sit alongside the form: the one being replaced, or
    /// the one the platform name matched.
    /// </summary>
    public PreviousRecord? Previous { get; private set; }

    /// <summary>The custodian's sealed records that nothing has replaced yet, offered on a new cycle.</summary>
    public List<RicCycle> Replaceable { get; private set; } = [];

    /// <summary>
    /// A cycle already under way that replaces the record asked for, when there is one — so
    /// the page can send the custodian to it rather than start a second.
    /// </summary>
    public int? ReplacementUnderWayId { get; private set; }

    /// <summary>True when the form should ask whether the new cycle replaces the matching sealed record.</summary>
    public bool NeedsReplacementDecision { get; private set; }

    /// <summary>True when the form should offer the "remove them anyway" tick.</summary>
    public bool NeedsRemovalConfirmation { get; private set; }

    /// <summary>True when the form should offer the "change the unit anyway" tick.</summary>
    public bool NeedsUnitChangeConfirmation { get; private set; }

    /// <summary>
    /// A blank form for a new cycle — or one carried over from the sealed record in
    /// <see cref="Supersedes"/> — or, given a <paramref name="cycleId"/>, an existing cycle's
    /// first step with its answers filled in.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(int? cycleId = null)
    {
        if (cycleId is null)
        {
            await LoadReplaceableAsync();

            if (Supersedes is { } replacing && await ReplacedAsync(replacing) is { } replaced)
            {
                CarryOver(replaced);
            }

            return Page();
        }

        if (!await LoadCycleAsync(cycleId.Value))
        {
            return NotFound();
        }

        if (!Cycle.IsEditable)
        {
            return RedirectToPage("/Ric/Review", new { cycleId });
        }

        await RememberStepAsync(1);
        Previous = await LoadReplacedRecordAsync();
        CycleId = cycleId;
        PlatformName = Cycle.PlatformName;
        StartYear = Cycle.StartYear;
        EndYear = Cycle.EndYear;
        BillableUnit = Cycle.BillableUnit;
        Existing = Cycle.Capabilities
            .OrderBy(x => x.Id)
            .Select(x => new CapabilityEdit { Id = x.Id, Name = x.Name })
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (CycleId is { } cycleId)
        {
            if (!await LoadCycleAsync(cycleId))
            {
                return NotFound();
            }

            if (!Cycle.IsEditable)
            {
                return RedirectToPage("/Ric/Review", new { cycleId });
            }

            Previous = await LoadReplacedRecordAsync();
        }
        else
        {
            await LoadReplaceableAsync();
        }

        var added = NamesToAdd();
        ValidateCommon();

        if (IsEditing)
        {
            ValidateEdit(added);
        }
        else
        {
            if (added.Count == 0)
            {
                ModelState.AddModelError(nameof(CapabilityNames), "Add at least one capability.");
            }

            await ValidateReplacementAsync();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!IsEditing)
        {
            return await CreateAsync(added);
        }

        ApplyEdit(added);
        RecordEdit();
        await Db.SaveChangesAsync();

        return RedirectToPage("/Ric/Costs", new { cycleId = Cycle.Id });
    }

    private async Task<IActionResult> CreateAsync(List<string> names)
    {
        var cycle = new RicCycle
        {
            PlatformName = PlatformName.Trim(),
            StartYear = StartYear,
            EndYear = EndYear,
            BillableUnit = BillableUnit,
            CreatedBy = User.UserName(),
            CreatedByDisplay = User.DisplayName(),
            LastEditedBy = User.UserName(),
            LastEditedByDisplay = User.DisplayName(),
            SupersedesCycleId = Supersedes,
            Capabilities = names.Select(x => new RicCapability { Name = x }).ToList()
        };

        Db.RicCycles.Add(cycle);
        await Db.SaveChangesAsync();

        return RedirectToPage("/Ric/Costs", new { cycleId = cycle.Id });
    }

    /// <summary>The custodian's sealed records that no cycle refers to yet.</summary>
    private async Task LoadReplaceableAsync()
    {
        var owner = User.UserName();

        var referenced = await Db.RicCycles
            .Where(x => x.CreatedBy == owner && x.SupersedesCycleId != null)
            .Select(x => x.SupersedesCycleId!.Value)
            .ToListAsync();

        Replaceable = await Db.RicCycles.AsNoTracking()
            .Where(x => x.CreatedBy == owner && x.Status == "Sealed" && !referenced.Contains(x.Id))
            .OrderByDescending(x => x.SealedAtUtc)
            .ToListAsync();
    }

    /// <summary>
    /// The sealed record <paramref name="id"/>, if this custodian may start a cycle that
    /// replaces it; otherwise null, with the reason in <see cref="PageModel.ModelState"/>.
    ///
    /// A record is replaced once. A second cycle pointing at the same record would leave two
    /// candidates for "the current rates" when both were sealed, which is the ambiguity the
    /// reference exists to remove — so a custodian who has already started the replacement
    /// is sent back to it.
    /// </summary>
    private async Task<RicCycle?> ReplacedAsync(int id)
    {
        var owner = User.UserName();

        var replaced = await Db.RicCycles.AsNoTracking()
            .Include(x => x.Capabilities)
            .FirstOrDefaultAsync(x => x.Id == id && x.CreatedBy == owner && x.Status == "Sealed");

        if (replaced is null)
        {
            ModelState.AddModelError(nameof(Supersedes), "A new cycle can only replace one of your own sealed records.");
            return null;
        }

        var successor = await Db.RicCycles.AsNoTracking()
            .Where(x => x.SupersedesCycleId == id)
            .Select(x => new { x.Id, x.Status, x.StartYear, x.EndYear })
            .FirstOrDefaultAsync();

        if (successor is not null)
        {
            if (successor.Status == "Sealed")
            {
                ModelState.AddModelError(
                    nameof(Supersedes),
                    $"The {replaced.StartYear}–{replaced.EndYear} {replaced.PlatformName} record has already been replaced, "
                    + $"by the {successor.StartYear}–{successor.EndYear} record. Replace that one instead.");
            }
            else
            {
                ReplacementUnderWayId = successor.Id;
                ModelState.AddModelError(
                    nameof(Supersedes),
                    $"A cycle replacing the {replaced.StartYear}–{replaced.EndYear} {replaced.PlatformName} record is already "
                    + "under way. Carry on with that one rather than starting a second.");
            }

            return null;
        }

        Previous = PreviousRecord.From(replaced);
        return replaced;
    }

    /// <summary>
    /// Start the replacement from the record it replaces: the same platform, unit and
    /// capabilities, and a period of the same length beginning the year after it ended.
    /// Every one of these can be changed before continuing.
    /// </summary>
    private void CarryOver(RicCycle replaced)
    {
        PlatformName = replaced.PlatformName;
        BillableUnit = replaced.BillableUnit;
        StartYear = replaced.EndYear + 1;
        EndYear = StartYear + (replaced.EndYear - replaced.StartYear);
        CapabilityNames = string.Join('\n', replaced.Capabilities.OrderBy(x => x.Id).Select(x => x.Name));
    }

    /// <summary>
    /// A new cycle either names the record it replaces, which must be replaceable, or — when
    /// its platform already has a sealed record — says that it does not replace it. Asked
    /// last, like the other confirmations, so the tick is not one error among many.
    /// </summary>
    private async Task ValidateReplacementAsync()
    {
        if (Supersedes is { } id)
        {
            await ReplacedAsync(id);
            return;
        }

        if (!ModelState.IsValid || ConfirmNotReplacing)
        {
            return;
        }

        var match = Replaceable.FirstOrDefault(x =>
            string.Equals(x.PlatformName.Trim(), PlatformName.Trim(), StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            return;
        }

        Previous = PreviousRecord.From(match);
        NeedsReplacementDecision = true;
        ModelState.AddModelError(
            string.Empty,
            $"{match.PlatformName} already has a sealed record for {match.StartYear}–{match.EndYear}, shown below. "
            + "If this cycle replaces it, start from that record so the new cycle refers to it. "
            + "If it is a separate cycle, say so.");
    }

    private List<string> NamesToAdd() =>
        (CapabilityNames ?? string.Empty)
            .Split(['\n', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private void ValidateCommon()
    {
        if (string.IsNullOrWhiteSpace(PlatformName))
        {
            ModelState.AddModelError(nameof(PlatformName), "Platform name is required.");
        }

        if (EndYear < StartYear)
        {
            ModelState.AddModelError(nameof(EndYear), "End year must be after the start year.");
        }

        if (!BillableUnits.Contains(BillableUnit))
        {
            ModelState.AddModelError(nameof(BillableUnit), "Select a valid billable unit.");
        }
    }

    private void ValidateEdit(List<string> added)
    {
        // The rows are rendered from this cycle, so an id that is not in it — or a
        // capability missing from the post — arrived from somewhere other than the form.
        var ids = Cycle.Capabilities.Select(x => x.Id).ToHashSet();
        if (Existing.Count != ids.Count || Existing.Any(x => !ids.Contains(x.Id)) || Existing.Select(x => x.Id).Distinct().Count() != Existing.Count)
        {
            ModelState.AddModelError(string.Empty, "The capability list has changed since this page was opened. Reload it and try again.");
            return;
        }

        var kept = Existing.Where(x => !x.Remove).ToList();

        foreach (var row in kept.Where(x => string.IsNullOrWhiteSpace(x.Name)))
        {
            var was = Cycle.Capabilities.First(x => x.Id == row.Id).Name;
            ModelState.AddModelError(string.Empty, $"Give \"{was}\" a name, or tick it to be removed.");
        }

        var names = kept.Select(x => x.Name?.Trim() ?? string.Empty).Where(x => x.Length > 0).Concat(added).ToList();
        foreach (var duplicate in names.GroupBy(x => x, StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1))
        {
            ModelState.AddModelError(string.Empty, $"\"{duplicate.Key}\" appears more than once. Each capability needs its own name.");
        }

        if (kept.Count + added.Count == 0)
        {
            ModelState.AddModelError(nameof(CapabilityNames), "Keep or add at least one capability.");
        }

        var lines = Cycle.Costs.Count;
        var years = EndYear - StartYear + 1;
        var yearsNow = Cycle.EndYear - Cycle.StartYear + 1;
        if (lines > 0 && EndYear >= StartYear && years != yearsNow)
        {
            ModelState.AddModelError(
                nameof(EndYear),
                $"The pricing period is {yearsNow} {Plural(yearsNow, "year")} long, and {lines} cost or funding "
                + $"{Plural(lines, "line")} already hold an amount for each of those years. Keep it {yearsNow} "
                + $"{Plural(yearsNow, "year")} long — it can start in a different year — or delete those lines first.");
        }

        // Ask about what would be lost only once everything else is right, so the tick is
        // the last thing between the custodian and the change rather than one error among many.
        if (!ModelState.IsValid)
        {
            return;
        }

        var losing = Existing
            .Where(x => x.Remove)
            .Select(x => Cycle.Capabilities.First(c => c.Id == x.Id))
            .Where(HasWork)
            .ToList();

        if (losing.Count > 0 && !ConfirmRemovals)
        {
            foreach (var capability in losing)
            {
                var costs = Cycle.Costs.Count(x => x.RicCapabilityId == capability.Id);
                ModelState.AddModelError(
                    string.Empty,
                    $"Removing \"{capability.Name}\" also removes its {costs} cost {Plural(costs, "line")}, its capacity "
                    + "and its proposed rates, and every platform-level cost is then split one way fewer.");
            }

            NeedsRemovalConfirmation = true;
        }

        if (BillableUnit != Cycle.BillableUnit && Cycle.Capabilities.Any(HasCapacityOrRates) && !ConfirmUnitChange)
        {
            ModelState.AddModelError(
                nameof(BillableUnit),
                $"Capacity, forecast use and proposed rates have been entered in {Cycle.BillableUnit.ToLowerInvariant()}. "
                + $"Changing to {BillableUnit.ToLowerInvariant()} clears them, to be entered again in the new unit.");
            NeedsUnitChangeConfirmation = true;
        }
    }

    private void ApplyEdit(List<string> added)
    {
        var unitChanged = BillableUnit != Cycle.BillableUnit;

        Cycle.PlatformName = PlatformName.Trim();
        Cycle.StartYear = StartYear;
        Cycle.EndYear = EndYear;
        Cycle.BillableUnit = BillableUnit;

        foreach (var row in Existing)
        {
            var capability = Cycle.Capabilities.First(x => x.Id == row.Id);

            if (row.Remove)
            {
                // The cost lines booked to it and its deductions go with it (cascade in
                // CostingDbContext), so none is left behind to be counted as platform-level.
                Db.RicCostEntries.RemoveRange(Cycle.Costs.Where(x => x.RicCapabilityId == capability.Id));
                Db.RicCapabilities.Remove(capability);
                continue;
            }

            capability.Name = row.Name!.Trim();

            if (unitChanged)
            {
                ClearUnitFigures(capability);
            }
        }

        foreach (var name in added)
        {
            Cycle.Capabilities.Add(new RicCapability { Name = name });
        }
    }

    /// <summary>
    /// Every figure held in the billable unit. The staff FTE is not one of them — it is a
    /// share of a person — so it stays.
    /// </summary>
    private void ClearUnitFigures(RicCapability capability)
    {
        capability.CapacityBaseline = string.Empty;
        capability.StatedBaseline = 0m;
        capability.StatedBaselineNote = null;
        capability.MaximumCapacity = 0m;
        capability.ForecastUwaUse = 0m;
        capability.ForecastApfrUse = 0m;
        capability.ForecastCommercialUse = 0m;
        capability.AboveCapacityReason = null;
        capability.ProposedUwaRate = 0m;
        capability.ProposedApfrRate = 0m;
        capability.ProposedCommercialRate = 0m;
        Db.RicCapacityDeductions.RemoveRange(capability.CapacityDeductions);
        capability.CapacityDeductions.Clear();
    }

    private bool HasWork(RicCapability capability) =>
        Cycle.Costs.Any(x => x.RicCapabilityId == capability.Id) || HasCapacityOrRates(capability);

    private static bool HasCapacityOrRates(RicCapability capability) =>
        !string.IsNullOrEmpty(capability.CapacityBaseline)
        || capability.MaximumCapacity > 0
        || capability.ForecastUtilisation > 0
        || capability.ProposedUwaRate > 0
        || capability.ProposedApfrRate > 0
        || capability.ProposedCommercialRate > 0;

    private static string Plural(int count, string word) => count == 1 ? word : word + "s";

    public class CapabilityEdit
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public bool Remove { get; set; }
    }
}
