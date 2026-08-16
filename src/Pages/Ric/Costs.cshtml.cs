using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Ric;

public class CostsModel(CostingDbContext db) : PageModel
{
    public RicCycle Cycle { get; private set; } = null!;
    [BindProperty] public int CycleId { get; set; }
    [BindProperty] public int? CapabilityId { get; set; }
    [BindProperty] public string Scope { get; set; } = "Capability";
    [BindProperty] public string EntryKind { get; set; } = "Cost";
    [BindProperty] public string Category { get; set; } = "Personnel";
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
    [BindProperty] public List<decimal> YearAmounts { get; set; } = [];
    public int YearCount => Cycle.EndYear - Cycle.StartYear + 1;

    public async Task<IActionResult> OnGetAsync(int cycleId) => await Load(cycleId) ? Page() : NotFound();

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (!await Load(CycleId)) return NotFound();
        if (Cycle.Status is not ("Draft" or "Returned")) return RedirectToPage("/Ric/Review", new { cycleId = CycleId });
        var allowedCosts = new[] { "Personnel", "Equipment", "Maintenance", "Travel", "Animal Cost", "Other" };
        var allowedIncome = new[] { "UWA GP / in-kind", "State", "Federal (incl. NCRIS)", "Other recurrent support" };
        if (EntryKind == "Cost" && !allowedCosts.Contains(Category)) ModelState.AddModelError(nameof(Category), "Select a valid cost category.");
        if (EntryKind == "Income" && !allowedIncome.Contains(Category)) ModelState.AddModelError(nameof(Category), "Select a valid income category.");
        if (Scope == "Capability" && CapabilityId is null) ModelState.AddModelError(nameof(CapabilityId), "Select a capability.");
        if (Category == "Personnel")
        {
            if (string.IsNullOrWhiteSpace(PersonnelName)) ModelState.AddModelError(nameof(PersonnelName), "Personnel name is required.");
            if (string.IsNullOrWhiteSpace(FundingType)) ModelState.AddModelError(nameof(FundingType), "Funding type is required.");
            if (FundingType == "ARC Fellow" && string.IsNullOrWhiteSpace(FellowshipType)) ModelState.AddModelError(nameof(FellowshipType), "Fellowship type is required for ARC Fellows.");
        }
        else if (string.IsNullOrWhiteSpace(Description)) ModelState.AddModelError(nameof(Description), "Description is required.");
        if (YearAmounts.Take(YearCount).Any(x => x < 0)) ModelState.AddModelError(string.Empty, "Year amounts cannot be negative.");
        if (!ModelState.IsValid) return Page();

        var amounts = Enumerable.Range(0, YearCount).Select(i => i < YearAmounts.Count ? YearAmounts[i] : 0).ToList();
        db.RicCostEntries.Add(new RicCostEntry
        {
            RicCycleId = CycleId, RicCapabilityId = Scope == "Capability" ? CapabilityId : null, Scope = Scope,
            CostType = EntryKind == "Income" ? "Non-variable income" : "Detailed cost", Category = Category,
            Amount = amounts.Average(), Notes = Notes, PersonnelName = Category == "Personnel" ? PersonnelName : null,
            FundingType = Category == "Personnel" ? FundingType : null,
            FellowshipType = Category == "Personnel" && FundingType == "ARC Fellow" ? FellowshipType : null,
            StepOption = Category == "Personnel" ? StepOption : null, WorkYears = Category == "Personnel" ? WorkYears : null,
            EmploymentType = Category == "Personnel" ? EmploymentType : null, PercentWorked = Category == "Personnel" ? PercentWorked : null,
            SuperannuationPercent = Category == "Personnel" ? SuperannuationPercent : null, StaffType = Category == "Personnel" ? StaffType : null,
            SalaryScale = Category == "Personnel" ? SalaryScale : null, SalaryStep = Category == "Personnel" ? SalaryStep : null,
            SchoolType = Category == "Personnel" ? SchoolType : null, BaseSalary = Category == "Personnel" ? BaseSalary : null,
            Description = Category == "Personnel" ? PersonnelName : Description, Supplier = Supplier,
            YearAmounts = amounts.Select((amount, i) => new RicCostYearAmount { ProjectYear = i + 1, Amount = amount }).ToList()
        });
        Cycle.UpdatedAtUtc = DateTime.UtcNow; await db.SaveChangesAsync();
        return RedirectToPage(new { cycleId = CycleId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var item = await db.RicCostEntries.Include(x => x.RicCycle).FirstOrDefaultAsync(x => x.Id == id && x.RicCycleId == CycleId && x.RicCycle.CreatedBy == User.Identity!.Name);
        if (item is null) return NotFound(); if (item.RicCycle.Status is "Draft" or "Returned") { db.Remove(item); await db.SaveChangesAsync(); }
        return RedirectToPage(new { cycleId = CycleId });
    }

    private async Task<bool> Load(int id)
    {
        Cycle = (await db.RicCycles.Include(x => x.Capabilities).Include(x => x.Costs).ThenInclude(x => x.Capability).Include(x => x.Costs).ThenInclude(x => x.YearAmounts).FirstOrDefaultAsync(x => x.Id == id && x.CreatedBy == User.Identity!.Name))!;
        CycleId = id; return Cycle is not null;
    }
}
