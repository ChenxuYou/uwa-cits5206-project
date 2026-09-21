using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Ric;

public class CostsModel(CostingDbContext db) : RicPageModel(db)
{
    /// <summary>
    /// Placeholder salary shown while the field is read-only.
    ///
    /// Salary pre-fill from the pay scales is US-05, a Should, and is not built — so this
    /// is a stand-in, not a rate anyone should rely on. When US-05 lands, the figure comes
    /// from the pay scale table in <c>MethodConfig</c> and this constant goes.
    /// </summary>
    private const decimal PlaceholderBaseSalary = 122_876m;

    [BindProperty] public int CycleId { get; set; }

    [BindProperty] public int? CapabilityId { get; set; }

    [BindProperty] public string Scope { get; set; } = CostEntry.Scopes.Capability;

    [BindProperty] public string Category { get; set; } = CostEntry.CostCategories.EmployeeSalaryAndOnCosts;

    /// <summary>Platform leader or research officer, for a capability staff line.</summary>
    [BindProperty] public string? Position { get; set; }

    /// <summary>Square metres, for a floor-area line (US-04).</summary>
    [BindProperty] public decimal FloorArea { get; set; }

    /// <summary>Dollars per m² per annum, for a floor-area line.</summary>
    [BindProperty] public decimal FloorAreaRate { get; set; }

    [BindProperty] public string? PersonnelName { get; set; }

    [BindProperty] public string? FundingType { get; set; }

    [BindProperty] public string? FellowshipType { get; set; }

    [BindProperty] public string? StepOption { get; set; }

    [BindProperty] public int WorkYears { get; set; } = 1;

    [BindProperty] public string? EmploymentType { get; set; }

    [BindProperty] public decimal PercentWorked { get; set; } = 100;

    [BindProperty] public decimal SuperannuationPercent { get; set; } = 17;

    [BindProperty] public string? StaffType { get; set; }

    [BindProperty] public string? SalaryScale { get; set; }

    [BindProperty] public string? SalaryStep { get; set; }

    [BindProperty] public string? SchoolType { get; set; }

    [BindProperty] public decimal BaseSalary { get; set; } = PlaceholderBaseSalary;

    [BindProperty] public string? Description { get; set; }

    [BindProperty] public string? Supplier { get; set; }

    [BindProperty] public string? Notes { get; set; }

    [BindProperty] public List<decimal> YearAmounts { get; set; } = [];

    /// <summary>The custodian has looked at an unusually large amount and says it is right (US-18).</summary>
    [BindProperty] public bool ConfirmLargeAmounts { get; set; }

    /// <summary>True when the form should offer the "I have checked these amounts" tick.</summary>
    public bool NeedsLargeAmountConfirmation { get; private set; }

    public int YearCount => Cycle.EndYear - Cycle.StartYear + 1;

    /// <summary>
    /// The running totals the screen shows while costs are entered: each capability's own
    /// costs, its share of the platform's, and the reconciliation between them (US-03, US-04).
    /// The same roll-up the engine prices from.
    /// </summary>
    public CycleCosts Costs => RicCalculationService.CostsOf(Cycle);

    public async Task<IActionResult> OnGetAsync(int cycleId)
    {
        if (!await Load(cycleId))
        {
            return NotFound();
        }

        YearAmounts = Enumerable.Repeat(0m, YearCount).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!await Load(CycleId))
        {
            return NotFound();
        }

        if (!Cycle.IsEditable)
        {
            return RedirectToPage("/Ric/Review", new { cycleId = CycleId });
        }

        ExplainUnreadableNumbers();
        Validate();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var amounts = AmountsByYear();
        var isPersonnel = CostEntry.CostCategories.IsPersonnel(Category);
        var isFloorArea = CostEntry.CostCategories.IsFloorArea(Category);

        Db.RicCostEntries.Add(new RicCostEntry
        {
            RicCycleId = CycleId,

            // A line is booked either to one capability or to the platform, never to both.
            // The engine's four aggregates rely on this being exclusive — see the note on
            // RicCalculationService.InputsFor.
            RicCapabilityId = Scope == CostEntry.Scopes.Capability ? CapabilityId : null,
            Scope = Scope,
            CostType = CostEntry.Types.Cost,
            Category = Category,
            Amount = amounts.Average(),
            Notes = Notes,
            Description = isPersonnel ? PersonnelName
                : isFloorArea && string.IsNullOrWhiteSpace(Description) ? Category
                : Description,
            Supplier = Supplier,
            Position = Category == CostEntry.CostCategories.PlatformLeaderSalary
                ? CostEntry.Positions.PlatformLeader
                : isPersonnel ? Position : null,
            FloorArea = isFloorArea ? FloorArea : null,
            FloorAreaRate = isFloorArea ? FloorAreaRate : null,

            PersonnelName = isPersonnel ? PersonnelName : null,
            FundingType = isPersonnel ? FundingType : null,
            FellowshipType = isPersonnel && FundingType == "ARC Fellow" ? FellowshipType : null,
            StepOption = isPersonnel ? StepOption : null,
            WorkYears = isPersonnel ? WorkYears : null,
            EmploymentType = isPersonnel ? EmploymentType : null,
            PercentWorked = isPersonnel ? PercentWorked : null,
            SuperannuationPercent = isPersonnel ? SuperannuationPercent : null,
            StaffType = isPersonnel ? StaffType : null,
            SalaryScale = isPersonnel ? SalaryScale : null,
            SalaryStep = isPersonnel ? SalaryStep : null,
            SchoolType = isPersonnel ? SchoolType : null,
            BaseSalary = isPersonnel ? BaseSalary : null,

            YearAmounts = amounts
                .Select((amount, i) => new RicCostYearAmount { ProjectYear = i + 1, Amount = amount })
                .ToList()
        });

        Cycle.UpdatedAtUtc = DateTime.UtcNow;
        await Db.SaveChangesAsync();

        return RedirectToPage(new { cycleId = CycleId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var owner = User.UserName();

        var item = await Db.RicCostEntries
            .Include(x => x.RicCycle)
            .FirstOrDefaultAsync(x =>
                x.Id == id
                && x.RicCycleId == CycleId
                && x.CostType == CostEntry.Types.Cost
                && x.RicCycle.CreatedBy == owner);

        if (item is null)
        {
            return NotFound();
        }

        if (item.RicCycle.IsEditable)
        {
            Db.Remove(item);
            await Db.SaveChangesAsync();
        }

        return RedirectToPage(new { cycleId = CycleId });
    }

    private void Validate()
    {
        // The category list depends on where the cost sits: a capability takes directly
        // incurred costs, the platform directly allocated and indirect ones [W, sheet 1].
        if (!CostEntry.CostCategories.For(Scope).Contains(Category))
        {
            ModelState.AddModelError(
                nameof(Category),
                Scope == CostEntry.Scopes.Platform
                    ? "Select a platform-level cost category."
                    : "Select a cost category for the capability.");
        }

        if (Scope == CostEntry.Scopes.Capability)
        {
            if (CapabilityId is null)
            {
                ModelState.AddModelError(nameof(CapabilityId), "Select a capability.");
            }
            else if (Cycle.Capabilities.All(x => x.Id != CapabilityId))
            {
                // The capability list is rendered from this cycle, so a value that is not
                // in it arrived from somewhere other than the form.
                ModelState.AddModelError(nameof(CapabilityId), "That capability is not part of this cycle.");
            }
        }
        else if (Scope != CostEntry.Scopes.Platform)
        {
            ModelState.AddModelError(nameof(Scope), "Select a valid scope.");
        }

        if (CostEntry.CostCategories.IsPersonnel(Category))
        {
            if (Category == CostEntry.CostCategories.EmployeeSalaryAndOnCosts
                && !CostEntry.Positions.All.Contains(Position))
            {
                ModelState.AddModelError(nameof(Position), "Say whether this is the platform leader or a research officer.");
            }

            if (string.IsNullOrWhiteSpace(PersonnelName))
            {
                ModelState.AddModelError(nameof(PersonnelName), "Personnel name is required.");
            }

            if (string.IsNullOrWhiteSpace(FundingType))
            {
                ModelState.AddModelError(nameof(FundingType), "Funding type is required.");
            }

            if (FundingType == "ARC Fellow" && string.IsNullOrWhiteSpace(FellowshipType))
            {
                ModelState.AddModelError(nameof(FellowshipType), "Fellowship type is required for ARC Fellows.");
            }
        }
        else if (CostEntry.CostCategories.IsFloorArea(Category))
        {
            if (FloorArea <= 0)
            {
                ModelState.AddModelError(nameof(FloorArea), "Enter the floor area in square metres, greater than zero.");
            }

            if (FloorAreaRate <= 0)
            {
                ModelState.AddModelError(nameof(FloorAreaRate), "Enter the rate per m² per year, greater than zero.");
            }
        }
        else if (string.IsNullOrWhiteSpace(Description))
        {
            ModelState.AddModelError(nameof(Description), "Description is required.");
        }

        if (CostEntry.CostCategories.IsPersonnel(Category))
        {
            // The form offers these as fixed choices, but a posted value is not bound to
            // what the form offered — 500% FTE would otherwise be stored and costed.
            foreach (var problem in new[]
                     {
                         EntryChecks.NotAPercentage(PercentWorked, "Percent worked"),
                         EntryChecks.NotAPercentage(SuperannuationPercent, "Superannuation")
                     })
            {
                if (problem is not null)
                {
                    ModelState.AddModelError(string.Empty, problem);
                }
            }
        }

        var entered = AmountsByYear();

        if (entered.Any(x => x < 0))
        {
            ModelState.AddModelError(string.Empty, "Year amounts cannot be negative.");
        }

        // Only ask once everything else is right, so the tick is the last thing between
        // the custodian and a saved line rather than one more error among several.
        if (ModelState.IsValid && !ConfirmLargeAmounts)
        {
            var large = EntryChecks.UnusuallyLarge(
                entered, Cycle.StartYear, Category, EntryChecks.ThresholdFor(Category));

            foreach (var message in large)
            {
                ModelState.AddModelError(string.Empty, message);
            }

            NeedsLargeAmountConfirmation = large.Count > 0;
        }
    }

    /// <summary>
    /// The line's amount for each year of the cycle. A floor-area line is the same every
    /// year — area × rate — and is worked out here rather than trusted from the browser.
    /// </summary>
    private List<decimal> AmountsByYear() =>
        CostEntry.CostCategories.IsFloorArea(Category)
            ? Enumerable.Repeat(FloorArea * FloorAreaRate, YearCount).ToList()
            : Enumerable.Range(0, YearCount).Select(i => i < YearAmounts.Count ? YearAmounts[i] : 0).ToList();

    private void ExplainUnreadableNumbers()
    {
        EntryChecks.ExplainUnreadableNumbers(
            ModelState,
            key => EntryChecks.YearIndex(key) is { } i ? $"{Cycle.StartYear + i} cost" : null,
            "an amount in dollars, such as 20000.00");

        EntryChecks.ExplainUnreadableNumbers(
            ModelState,
            key => key switch
            {
                nameof(FloorArea) => "Floor area",
                nameof(FloorAreaRate) => "Rate per m²",
                _ => null
            },
            "a number, such as 120 or 450.00");
    }

    private async Task<bool> Load(int cycleId)
    {
        if (!await LoadCycleAsync(cycleId))
        {
            return false;
        }

        CycleId = cycleId;
        return true;
    }
}
