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

    [BindProperty] public string EntryKind { get; set; } = "Cost";

    [BindProperty] public string Category { get; set; } = CostEntry.CostCategories.Personnel;

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

    public int YearCount => Cycle.EndYear - Cycle.StartYear + 1;

    public bool IsIncome => EntryKind == "Income";

    public async Task<IActionResult> OnGetAsync(int cycleId)
    {
        if (!await Load(cycleId))
        {
            return NotFound();
        }

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

        Validate();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var amounts = Enumerable.Range(0, YearCount)
            .Select(i => i < YearAmounts.Count ? YearAmounts[i] : 0)
            .ToList();

        var isPersonnel = !IsIncome && Category == CostEntry.CostCategories.Personnel;

        Db.RicCostEntries.Add(new RicCostEntry
        {
            RicCycleId = CycleId,

            // A line is booked either to one capability or to the platform, never to both.
            // The engine's four aggregates rely on this being exclusive — see the note on
            // RicCalculationService.InputsFor.
            RicCapabilityId = Scope == CostEntry.Scopes.Capability ? CapabilityId : null,
            Scope = Scope,
            CostType = IsIncome ? CostEntry.Types.Income : CostEntry.Types.Cost,
            Category = Category,
            Amount = amounts.Average(),
            Notes = Notes,
            Description = isPersonnel ? PersonnelName : Description,
            Supplier = Supplier,

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
            .FirstOrDefaultAsync(x => x.Id == id && x.RicCycleId == CycleId && x.RicCycle.CreatedBy == owner);

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
        var allowed = IsIncome ? CostEntry.IncomeCategories.All : CostEntry.CostCategories.All;

        if (!allowed.Contains(Category))
        {
            ModelState.AddModelError(nameof(Category), $"Select a valid {(IsIncome ? "income" : "cost")} category.");
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

        if (!IsIncome && Category == CostEntry.CostCategories.Personnel)
        {
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
        else if (string.IsNullOrWhiteSpace(Description))
        {
            ModelState.AddModelError(nameof(Description), "Description is required.");
        }

        if (YearAmounts.Take(YearCount).Any(x => x < 0))
        {
            ModelState.AddModelError(string.Empty, "Year amounts cannot be negative.");
        }
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
