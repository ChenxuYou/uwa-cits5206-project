using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Ric;

public class FundingModel(CostingDbContext db) : RicPageModel(db)
{
    [BindProperty] public int CycleId { get; set; }

    [BindProperty] public string Category { get; set; } = CostEntry.IncomeCategories.UwaGpInKind;

    [BindProperty] public string? Description { get; set; }

    [BindProperty] public string? FundingBody { get; set; }

    [BindProperty] public string? Justification { get; set; }

    [BindProperty] public List<decimal> YearAmounts { get; set; } = [];

    public int YearCount => Cycle.EndYear - Cycle.StartYear + 1;

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

        Validate();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var amounts = Enumerable.Range(0, YearCount)
            .Select(i => i < YearAmounts.Count ? YearAmounts[i] : 0m)
            .ToList();

        Db.RicCostEntries.Add(new RicCostEntry
        {
            RicCycleId = CycleId,
            RicCapabilityId = null,
            Scope = CostEntry.Scopes.Platform,
            CostType = CostEntry.Types.Income,
            Category = Category,
            Description = Description?.Trim(),
            Supplier = FundingBody?.Trim(),
            Notes = Justification?.Trim(),
            Amount = amounts.Average(),
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
                && x.CostType == CostEntry.Types.Income
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
        if (!CostEntry.IncomeCategories.All.Contains(Category))
        {
            ModelState.AddModelError(nameof(Category), "Select a valid funding source category.");
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            ModelState.AddModelError(nameof(Description), "Funding source name is required.");
        }

        if (string.IsNullOrWhiteSpace(Justification))
        {
            ModelState.AddModelError(
                nameof(Justification),
                "Justification is required: say where the funding comes from and how long it is committed for.");
        }

        if (YearAmounts.Take(YearCount).Any(x => x < 0))
        {
            ModelState.AddModelError(string.Empty, "Funding amounts cannot be negative.");
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
