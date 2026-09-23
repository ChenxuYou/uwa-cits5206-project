using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;

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

    /// <summary>
    /// The cost line being changed, or null while adding one (US-10: change any value). The
    /// form is the same either way; saving replaces the line's figures instead of adding a line.
    /// </summary>
    [BindProperty] public int? EditId { get; set; }

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

    /// <param name="edit">A cost line of this cycle to open in the form for changing.</param>
    public async Task<IActionResult> OnGetAsync(int cycleId, int? edit = null)
    {
        if (!await Load(cycleId))
        {
            return NotFound();
        }

        await RememberStepAsync(2);
        YearAmounts = Enumerable.Repeat(0m, YearCount).ToList();

        if (edit is not null)
        {
            if (!Cycle.IsEditable)
            {
                return RedirectToPage("/Ric/Review", new { cycleId });
            }

            if (Editable(edit.Value) is not { } item)
            {
                return NotFound();
            }

            FillFormFrom(item);
        }

        return Page();
    }

    public Task<IActionResult> OnPostAddAsync() => SaveAsync();

    /// <summary>Save changes to the line in <see cref="EditId"/>, held to the same checks as a new one.</summary>
    public Task<IActionResult> OnPostUpdateAsync() => SaveAsync();

    private async Task<IActionResult> SaveAsync()
    {
        if (!await Load(CycleId))
        {
            return NotFound();
        }

        if (!Cycle.IsEditable)
        {
            return RedirectToPage("/Ric/Review", new { cycleId = CycleId });
        }

        RicCostEntry? existing = null;
        if (EditId is { } editId && (existing = Editable(editId)) is null)
        {
            return NotFound();
        }

        ExplainUnreadableNumbers();
        Validate();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (existing is null)
        {
            existing = new RicCostEntry { RicCycleId = CycleId, CostType = CostEntry.Types.Cost };
            Db.RicCostEntries.Add(existing);
        }
        else
        {
            Db.RicCostYearAmounts.RemoveRange(existing.YearAmounts);
        }

        FillEntry(existing);

        RecordEdit();
        await Db.SaveChangesAsync();

        return RedirectToPage(new { cycleId = CycleId });
    }

    /// <summary>A cost line of the loaded cycle — never an income line, never another cycle's.</summary>
    private RicCostEntry? Editable(int id) => Cycle.Costs.FirstOrDefault(x => x.Id == id && !x.IsIncome);

    /// <summary>Write what the form holds onto a line, new or existing.</summary>
    private void FillEntry(RicCostEntry entry)
    {
        var amounts = AmountsByYear();
        var isPersonnel = CostEntry.CostCategories.IsPersonnel(Category);
        var isFloorArea = CostEntry.CostCategories.IsFloorArea(Category);

        // A line is booked either to one capability or to the platform, never to both.
        // The engine's four aggregates rely on this being exclusive — see the note on
        // RicCalculationService.InputsFor.
        entry.RicCapabilityId = Scope == CostEntry.Scopes.Capability ? CapabilityId : null;
        entry.Capability = Scope == CostEntry.Scopes.Capability ? Cycle.Capabilities.First(x => x.Id == CapabilityId) : null;
        entry.Scope = Scope;
        entry.Category = Category;
        entry.Amount = amounts.Average();
        entry.Notes = Notes;
        entry.Description = isPersonnel ? PersonnelName
            : isFloorArea && string.IsNullOrWhiteSpace(Description) ? Category
            : Description;
        entry.Supplier = Supplier;
        entry.Position = Category == CostEntry.CostCategories.PlatformLeaderSalary
            ? CostEntry.Positions.PlatformLeader
            : isPersonnel ? Position : null;
        entry.FloorArea = isFloorArea ? FloorArea : null;
        entry.FloorAreaRate = isFloorArea ? FloorAreaRate : null;

        entry.PersonnelName = isPersonnel ? PersonnelName : null;
        entry.FundingType = isPersonnel ? FundingType : null;
        entry.FellowshipType = isPersonnel && FundingType == "ARC Fellow" ? FellowshipType : null;
        entry.StepOption = isPersonnel ? StepOption : null;
        entry.WorkYears = isPersonnel ? WorkYears : null;
        entry.EmploymentType = isPersonnel ? EmploymentType : null;
        entry.PercentWorked = isPersonnel ? PercentWorked : null;
        entry.SuperannuationPercent = isPersonnel ? SuperannuationPercent : null;
        entry.StaffType = isPersonnel ? StaffType : null;
        entry.SalaryScale = isPersonnel ? SalaryScale : null;
        entry.SalaryStep = isPersonnel ? SalaryStep : null;
        entry.SchoolType = isPersonnel ? SchoolType : null;
        entry.BaseSalary = isPersonnel ? BaseSalary : null;

        entry.YearAmounts = amounts
            .Select((amount, i) => new RicCostYearAmount { ProjectYear = i + 1, Amount = amount })
            .ToList();
    }

    /// <summary>Put a saved line back into the form, as it was entered.</summary>
    private void FillFormFrom(RicCostEntry item)
    {
        EditId = item.Id;
        Scope = item.Scope;
        CapabilityId = item.RicCapabilityId;
        Category = item.Category;
        Position = item.Position;
        FloorArea = item.FloorArea ?? 0m;
        FloorAreaRate = item.FloorAreaRate ?? 0m;
        Description = CostEntry.CostCategories.IsPersonnel(item.Category) ? null : item.Description;
        Supplier = item.Supplier;
        Notes = item.Notes;

        PersonnelName = item.PersonnelName;
        FundingType = item.FundingType;
        FellowshipType = item.FellowshipType;
        StepOption = item.StepOption;
        WorkYears = item.WorkYears ?? WorkYears;
        EmploymentType = item.EmploymentType;
        PercentWorked = item.PercentWorked ?? PercentWorked;
        SuperannuationPercent = item.SuperannuationPercent ?? SuperannuationPercent;
        StaffType = item.StaffType;
        SalaryScale = item.SalaryScale;
        SalaryStep = item.SalaryStep;
        SchoolType = item.SchoolType;
        BaseSalary = item.BaseSalary ?? BaseSalary;

        var saved = item.YearAmounts.OrderBy(x => x.ProjectYear).Select(x => x.Amount).ToList();
        YearAmounts = Enumerable.Range(0, YearCount).Select(i => i < saved.Count ? saved[i] : 0m).ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (!await Load(CycleId) || Editable(id) is not { } item)
        {
            return NotFound();
        }

        if (Cycle.IsEditable)
        {
            Db.Remove(item);
            RecordEdit();
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
