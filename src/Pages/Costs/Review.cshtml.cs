using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Costs;

public class ReviewModel(CostingDbContext db) : PageModel
{
    public CostingCycle Cycle { get; private set; } = null!;
    public decimal GrandTotal => Cycle.CostItems.SelectMany(x => x.YearAmounts).Sum(x => x.Amount);
    public bool Submitted => Cycle.Status == "Submitted";
    public string? SuccessMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(int cycleId)
    {
        var cycle = await LoadCycleAsync(cycleId);
        if (cycle is null) return NotFound();
        Cycle = cycle;
        SuccessMessage = TempData["SuccessMessage"] as string;
        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(int cycleId, bool confirmAccuracy)
    {
        var cycle = await db.CostingCycles
            .Include(x => x.CostItems)
            .ThenInclude(x => x.YearAmounts)
            .FirstOrDefaultAsync(x => x.Id == cycleId);

        if (cycle is null) return NotFound();
        Cycle = cycle;

        if (cycle.Status != "Draft")
        {
            ModelState.AddModelError(string.Empty, "This costing cycle has already been submitted.");
            return Page();
        }
        if (cycle.CostItems.Count == 0)
            ModelState.AddModelError(string.Empty, "Add at least one cost item before submitting.");
        if (!confirmAccuracy)
            ModelState.AddModelError(string.Empty, "Confirm that the project information is complete and accurate.");
        if (cycle.CostItems.SelectMany(x => x.YearAmounts).Any(x => x.Amount < 0))
            ModelState.AddModelError(string.Empty, "Cost amounts cannot be negative.");

        if (!ModelState.IsValid) return Page();

        cycle.Status = "Submitted";
        cycle.UpdatedAtUtc = DateTime.UtcNow;
        cycle.DraftCostJson = null;
        await db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Costing cycle submitted successfully.";
        return RedirectToPage(new { cycleId });
    }

    private Task<CostingCycle?> LoadCycleAsync(int id) => db.CostingCycles
        .Include(x => x.CostItems)
        .ThenInclude(x => x.YearAmounts)
        .AsNoTracking()
        .FirstOrDefaultAsync(x => x.Id == id);
}
