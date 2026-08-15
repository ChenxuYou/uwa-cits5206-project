using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CostingTool.Pages.Costs;

public class CreateModel(CostingDbContext db) : PageModel
{
    [BindProperty] public int? CycleId { get; set; }
    [BindProperty] public string ProjectName { get; set; } = string.Empty;
    [BindProperty] public string? ProjectReference { get; set; }
    [BindProperty] public int StartYear { get; set; } = DateTime.Now.Year;
    [BindProperty] public int ProjectDuration { get; set; } = 3;
    [BindProperty] public string? CostCategory { get; set; }

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
    [BindProperty] public decimal BaseSalary { get; set; } = 122876;

    [BindProperty] public string? Description { get; set; }
    [BindProperty] public string? Supplier { get; set; }
    [BindProperty] public string? Notes { get; set; }
    [BindProperty] public List<decimal> PersonnelYearAmounts { get; set; } = [];
    [BindProperty] public List<decimal> GenericYearAmounts { get; set; } = [];

    public CostingCycle? CurrentCycle { get; private set; }
    public string? SuccessMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? cycleId)
    {
        if (cycleId is null) return Page();
        await LoadCycleAsync(cycleId.Value);
        if (CurrentCycle is null) return NotFound();
        if (CurrentCycle.Status != "Draft")
            return RedirectToPage("/Costs/Review", new { cycleId = CurrentCycle.Id });
        CycleId = CurrentCycle.Id;
        ProjectName = CurrentCycle.ProjectName;
        ProjectReference = CurrentCycle.ProjectReference;
        StartYear = CurrentCycle.StartYear;
        ProjectDuration = CurrentCycle.DurationYears;
        RestoreDraftCost(CurrentCycle.DraftCostJson);
        SuccessMessage = TempData["SuccessMessage"] as string;
        return Page();
    }

    public async Task<IActionResult> OnPostSaveDraftAsync()
    {
        if (!await IsEditableAsync()) return RedirectToReview();
        if (!ValidateProject())
        {
            await ReloadCurrentCycleAsync();
            return Page();
        }
        var cycle = await SaveCycleAsync();
        cycle.DraftCostJson = JsonSerializer.Serialize(new DraftCostData(
            CostCategory, PersonnelName, FundingType,
            FundingType == "arc-fellow" ? FellowshipType : null,
            StepOption, WorkYears, EmploymentType, PercentWorked, SuperannuationPercent,
            StaffType, SalaryScale, SalaryStep, SchoolType, BaseSalary,
            Description, Supplier, Notes, PersonnelYearAmounts, GenericYearAmounts));
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "Draft saved successfully.";
        return RedirectToPage(new { cycleId = cycle.Id });
    }

    public async Task<IActionResult> OnPostAddCostAsync()
    {
        if (!await IsEditableAsync()) return RedirectToReview();
        ValidateProject();
        ValidateCostItem();
        if (!ModelState.IsValid)
        {
            await ReloadCurrentCycleAsync();
            return Page();
        }

        var cycle = await SaveCycleAsync();
        var isPersonnel = CostCategory == "personnel";
        var amounts = isPersonnel ? PersonnelYearAmounts : GenericYearAmounts;

        var item = new CostItem
        {
            CostingCycleId = cycle.Id,
            Category = CostCategory!,
            Description = isPersonnel ? PersonnelName : Description,
            Supplier = isPersonnel ? null : Supplier,
            Notes = Notes,
            PersonnelName = isPersonnel ? PersonnelName : null,
            FundingType = isPersonnel ? FundingType : null,
            FellowshipType = isPersonnel && FundingType == "arc-fellow" ? FellowshipType : null,
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
            YearAmounts = amounts.Take(ProjectDuration).Select((amount, index) => new CostItemYearAmount
            {
                ProjectYear = index + 1,
                Amount = amount
            }).ToList()
        };

        db.CostItems.Add(item);
        cycle.DraftCostJson = null;
        cycle.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        TempData["SuccessMessage"] = "Cost item added successfully.";
        return RedirectToPage(new { cycleId = cycle.Id });
    }

    private bool ValidateProject()
    {
        if (string.IsNullOrWhiteSpace(ProjectName)) ModelState.AddModelError(nameof(ProjectName), "Project name is required.");
        if (StartYear is < 2000 or > 2200) ModelState.AddModelError(nameof(StartYear), "Enter a valid start year.");
        if (ProjectDuration is < 1 or > 5) ModelState.AddModelError(nameof(ProjectDuration), "Project duration must be between 1 and 5 years.");
        return ModelState.IsValid;
    }

    private void ValidateCostItem()
    {
        var categories = new[] { "personnel", "equipment", "maintenance", "travel", "animal", "other" };
        if (CostCategory is null || !categories.Contains(CostCategory))
            ModelState.AddModelError(nameof(CostCategory), "Select a valid cost category.");

        if (CostCategory == "personnel")
        {
            if (string.IsNullOrWhiteSpace(PersonnelName)) ModelState.AddModelError(nameof(PersonnelName), "Name is required.");
            if (string.IsNullOrWhiteSpace(FundingType)) ModelState.AddModelError(nameof(FundingType), "Funding type is required.");
            if (FundingType == "arc-fellow" && string.IsNullOrWhiteSpace(FellowshipType))
                ModelState.AddModelError(nameof(FellowshipType), "Fellowship type is required for an ARC Fellow.");
        }
        else if (string.IsNullOrWhiteSpace(Description))
        {
            ModelState.AddModelError(nameof(Description), "Description is required.");
        }

        var amounts = CostCategory == "personnel" ? PersonnelYearAmounts : GenericYearAmounts;
        if (amounts.Any(x => x < 0)) ModelState.AddModelError(string.Empty, "Cost amounts cannot be negative.");
    }

    private async Task<CostingCycle> SaveCycleAsync()
    {
        CostingCycle cycle;
        if (CycleId is int id)
        {
            cycle = await db.CostingCycles.FindAsync(id) ?? new CostingCycle();
            if (cycle.Id == 0) db.CostingCycles.Add(cycle);
        }
        else
        {
            cycle = new CostingCycle();
            db.CostingCycles.Add(cycle);
        }

        cycle.ProjectName = ProjectName.Trim();
        cycle.ProjectReference = ProjectReference?.Trim();
        cycle.StartYear = StartYear;
        cycle.DurationYears = ProjectDuration;
        cycle.Status = "Draft";
        cycle.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        CycleId = cycle.Id;
        return cycle;
    }

    private async Task LoadCycleAsync(int id)
    {
        CurrentCycle = await db.CostingCycles
            .Include(x => x.CostItems)
            .ThenInclude(x => x.YearAmounts)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    private async Task ReloadCurrentCycleAsync()
    {
        if (CycleId is int id) await LoadCycleAsync(id);
    }

    private async Task<bool> IsEditableAsync()
    {
        if (CycleId is not int id) return true;
        return await db.CostingCycles.AnyAsync(x => x.Id == id && x.Status == "Draft");
    }

    private IActionResult RedirectToReview() =>
        RedirectToPage("/Costs/Review", new { cycleId = CycleId });

    private void RestoreDraftCost(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return;
        var draft = JsonSerializer.Deserialize<DraftCostData>(json);
        if (draft is null) return;
        CostCategory = draft.CostCategory;
        PersonnelName = draft.PersonnelName;
        FundingType = draft.FundingType;
        FellowshipType = draft.FellowshipType;
        StepOption = draft.StepOption;
        WorkYears = draft.WorkYears;
        EmploymentType = draft.EmploymentType;
        PercentWorked = draft.PercentWorked;
        SuperannuationPercent = draft.SuperannuationPercent;
        StaffType = draft.StaffType;
        SalaryScale = draft.SalaryScale;
        SalaryStep = draft.SalaryStep;
        SchoolType = draft.SchoolType;
        BaseSalary = draft.BaseSalary;
        Description = draft.Description;
        Supplier = draft.Supplier;
        Notes = draft.Notes;
        PersonnelYearAmounts = draft.PersonnelYearAmounts;
        GenericYearAmounts = draft.GenericYearAmounts;
    }

    private sealed record DraftCostData(
        string? CostCategory, string? PersonnelName, string? FundingType, string? FellowshipType,
        string? StepOption, int WorkYears, string? EmploymentType, decimal PercentWorked,
        decimal SuperannuationPercent, string? StaffType, string? SalaryScale, string? SalaryStep,
        string? SchoolType, decimal BaseSalary, string? Description, string? Supplier, string? Notes,
        List<decimal> PersonnelYearAmounts, List<decimal> GenericYearAmounts);
}
