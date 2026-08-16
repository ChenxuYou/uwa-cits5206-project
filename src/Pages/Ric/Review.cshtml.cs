using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CostingTool.Pages.Ric;

public class ReviewModel(CostingDbContext db, RicCalculationService calculator) : PageModel
{
    public RicCycle Cycle { get; private set; } = null!;
    public Dictionary<int, CapabilityRateResult> Results { get; private set; } = [];
    public bool IsEditable => Cycle.Status is "Draft" or "Returned";
    public bool IsPending => Cycle.Status == "Submitted";
    public bool IsSealed => Cycle.Status == "Sealed";
    public string? SuccessMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(int cycleId)
    {
        if (!await Load(cycleId)) return NotFound();
        SuccessMessage = TempData["Success"] as string;
        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(int cycleId, bool confirmAccuracy)
    {
        if (!await Load(cycleId)) return NotFound();
        if (!IsEditable)
        {
            ModelState.AddModelError(string.Empty, "Only Draft or Returned cycles can be submitted.");
            return Page();
        }
        if (!confirmAccuracy) ModelState.AddModelError(string.Empty, "Confirm that the assumptions and figures are complete and accurate.");
        if (Cycle.Capabilities.Count == 0 || Cycle.Costs.Count == 0) ModelState.AddModelError(string.Empty, "The cycle must contain capabilities and operating costs.");
        if (Cycle.Capabilities.Any(x => x.MaximumCapacity <= 0 || x.ForecastUwaUse + x.ForecastApfrUse + x.ForecastCommercialUse <= 0)) ModelState.AddModelError(string.Empty, "Every capability requires capacity and forecast utilisation.");
        if (Cycle.Capabilities.Any(x => x.ProposedUwaRate <= 0 || x.ProposedApfrRate <= 0 || x.ProposedCommercialRate <= 0)) ModelState.AddModelError(string.Empty, "Every capability requires three proposed rates.");
        if (!ModelState.IsValid) return Page();

        Cycle.Status = "Submitted";
        Cycle.SubmittedBy = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Unknown";
        Cycle.SubmittedAtUtc = DateTime.UtcNow;
        Cycle.ReturnedBy = null;
        Cycle.ReturnedAtUtc = null;
        Cycle.ReturnReason = null;
        Cycle.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        TempData["Success"] = "Costing cycle submitted for delegated authority approval.";
        return RedirectToPage(new { cycleId });
    }

    private async Task<bool> Load(int id)
    {
        Cycle = (await db.RicCycles.Include(x => x.Capabilities).Include(x => x.Costs).ThenInclude(x => x.YearAmounts).FirstOrDefaultAsync(x => x.Id == id && x.CreatedBy == User.Identity!.Name))!;
        if (Cycle is null) return false;
        Results = Cycle.Capabilities.ToDictionary(x => x.Id, x => calculator.Calculate(Cycle, x));
        return true;
    }
}
