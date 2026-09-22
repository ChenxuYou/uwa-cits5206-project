using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;

namespace CostingTool.Pages.Ric;

/// <summary>
/// Step 1: the platform, its pricing period, its billable unit and its capabilities.
///
/// It creates a cycle, and since US-10 it also reopens one: "I can return to any earlier
/// section, change any value, and return to the rates with the figures updated." Two values
/// are guarded rather than silently rewritten, because later figures are expressed in them:
/// the pricing period, once a cost or funding line holds an amount for each of its years, and
/// the billable unit, once capacity or forecast use has been entered in it. The page says why
/// and what to do instead.
/// </summary>
public class StartModel(CostingDbContext db) : RicPageModel(db)
{
    public static readonly string[] BillableUnits = ["Hours", "Days", "Samples"];

    /// <summary>Set when an existing cycle is being edited; zero when a new one is being started.</summary>
    [BindProperty] public int CycleId { get; set; }

    [BindProperty] public string PlatformName { get; set; } = string.Empty;

    [BindProperty] public int StartYear { get; set; } = DateTime.Now.Year;

    [BindProperty] public int EndYear { get; set; } = DateTime.Now.Year + 2;

    [BindProperty] public string BillableUnit { get; set; } = "Hours";

    /// <summary>New capabilities, one per line or separated by commas.</summary>
    [BindProperty] public string CapabilityNames { get; set; } = string.Empty;

    /// <summary>The cycle's capabilities, when editing: rename or remove.</summary>
    [BindProperty] public List<ExistingCapability> Existing { get; set; } = [];

    public bool IsEditing => CycleId > 0;

    /// <summary>True when cost or funding lines hold per-year amounts, so the period is fixed.</summary>
    public bool PeriodIsFixed => IsEditing && Cycle.Costs.Count > 0;

    /// <summary>True when capacity or forecast use has been entered in the unit, so the unit is fixed.</summary>
    public bool UnitIsFixed => IsEditing && Cycle.Capabilities.Any(HasCapacityFigures);

    public async Task<IActionResult> OnGetAsync(int? cycleId)
    {
        if (cycleId is null)
        {
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

        CycleId = Cycle.Id;
        PlatformName = Cycle.PlatformName;
        StartYear = Cycle.StartYear;
        EndYear = Cycle.EndYear;
        BillableUnit = Cycle.BillableUnit;
        Existing = Cycle.Capabilities
            .OrderBy(x => x.Id)
            .Select(x => new ExistingCapability { Id = x.Id, Name = x.Name })
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (IsEditing)
        {
            return await UpdateAsync();
        }

        var names = NewNames();
        ValidateCommon();

        if (names.Count == 0)
        {
            ModelState.AddModelError(nameof(CapabilityNames), "Add at least one capability.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var cycle = new RicCycle
        {
            PlatformName = PlatformName.Trim(),
            StartYear = StartYear,
            EndYear = EndYear,
            BillableUnit = BillableUnit,
            CreatedBy = User.UserName(),
            CreatedByDisplay = User.DisplayName(),
            Capabilities = names.Select(x => new RicCapability { Name = x }).ToList()
        };

        Db.RicCycles.Add(cycle);
        await Db.SaveChangesAsync();

        return RedirectToPage("/Ric/Costs", new { cycleId = cycle.Id });
    }

    private async Task<IActionResult> UpdateAsync()
    {
        if (!await LoadCycleAsync(CycleId))
        {
            return NotFound();
        }

        if (!Cycle.IsEditable)
        {
            return RedirectToPage("/Ric/Review", new { cycleId = CycleId });
        }

        ValidateCommon();

        if ((StartYear != Cycle.StartYear || EndYear != Cycle.EndYear) && PeriodIsFixed)
        {
            ModelState.AddModelError(
                nameof(StartYear),
                "The pricing period cannot change once cost or funding lines are entered: each holds an amount "
                + "for every year of the period. Remove those lines first, or start a new cycle for the new period.");
        }

        if (BillableUnit != Cycle.BillableUnit && UnitIsFixed)
        {
            ModelState.AddModelError(
                nameof(BillableUnit),
                $"The billable unit cannot change once capacity and forecast use are entered in {Cycle.BillableUnit.ToLowerInvariant()}. "
                + "Start a new cycle to price in a different unit.");
        }

        var kept = new List<(RicCapability Capability, string Name)>();
        var removed = new List<RicCapability>();

        foreach (var row in Existing)
        {
            var capability = Cycle.Capabilities.FirstOrDefault(x => x.Id == row.Id);
            if (capability is null)
            {
                ModelState.AddModelError(string.Empty, "That capability is not part of this cycle.");
                continue;
            }

            if (row.Remove)
            {
                // A capability's lines would go with it (cascade delete), which would move every
                // rate without the custodian having seen a figure change. Ask for them first.
                if (Cycle.Costs.Any(x => x.RicCapabilityId == capability.Id))
                {
                    ModelState.AddModelError(
                        string.Empty,
                        $"{capability.Name} still has cost or funding lines booked to it. Delete those lines first, then remove the capability.");
                }
                else
                {
                    removed.Add(capability);
                }

                continue;
            }

            if (string.IsNullOrWhiteSpace(row.Name))
            {
                ModelState.AddModelError(string.Empty, $"Give {capability.Name} a name, or tick remove.");
                continue;
            }

            kept.Add((capability, row.Name.Trim()));
        }

        var added = NewNames();
        var allNames = kept.Select(x => x.Name).Concat(added).ToList();

        if (allNames.Count == 0)
        {
            ModelState.AddModelError(nameof(CapabilityNames), "A cycle needs at least one capability.");
        }

        var duplicate = allNames.GroupBy(x => x, StringComparer.OrdinalIgnoreCase).FirstOrDefault(x => x.Count() > 1);
        if (duplicate is not null)
        {
            ModelState.AddModelError(string.Empty, $"Two capabilities are called {duplicate.Key}. Give each a different name.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Cycle.PlatformName = PlatformName.Trim();
        Cycle.StartYear = StartYear;
        Cycle.EndYear = EndYear;
        Cycle.BillableUnit = BillableUnit;

        foreach (var (capability, name) in kept)
        {
            capability.Name = name;
        }

        Db.RicCapabilities.RemoveRange(removed);

        foreach (var name in added)
        {
            Cycle.Capabilities.Add(new RicCapability { Name = name });
        }

        Cycle.UpdatedAtUtc = DateTime.UtcNow;
        await Db.SaveChangesAsync();

        return RedirectToPage("/Ric/Costs", new { cycleId = Cycle.Id });
    }

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

    private List<string> NewNames() =>
        (CapabilityNames ?? string.Empty)
            .Split(['\n', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static bool HasCapacityFigures(RicCapability x) =>
        x.MaximumCapacity > 0 || x.ForecastUtilisation > 0 || !string.IsNullOrEmpty(x.CapacityBaseline);

    public class ExistingCapability
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public bool Remove { get; set; }
    }
}
