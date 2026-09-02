using CostingTool.Data;
using Microsoft.AspNetCore.Mvc;

namespace CostingTool.Pages.Ric;

public class ReviewModel(CostingDbContext db, RicCalculationService calculator) : RicPageModel(db)
{
    public CycleRates Rates { get; private set; } = null!;

    public bool IsEditable => Cycle.IsEditable;

    public bool IsPending => Cycle.Status == "Submitted";

    public bool IsSealed => Cycle.Status == "Sealed";

    public string? SuccessMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(int cycleId)
    {
        if (!await Load(cycleId))
        {
            return NotFound();
        }

        SuccessMessage = TempData["Success"] as string;
        return Page();
    }

    public async Task<IActionResult> OnPostSubmitAsync(int cycleId, bool confirmAccuracy)
    {
        if (!await Load(cycleId))
        {
            return NotFound();
        }

        if (!IsEditable)
        {
            ModelState.AddModelError(string.Empty, "Only Draft or Returned cycles can be submitted.");
            return Page();
        }

        if (!confirmAccuracy)
        {
            ModelState.AddModelError(string.Empty, "Confirm that the assumptions and figures are complete and accurate.");
        }

        if (Cycle.Capabilities.Count == 0 || Cycle.Costs.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "The cycle must contain capabilities and operating costs.");
        }

        if (Cycle.Capabilities.Any(x => x.MaximumCapacity <= 0 || x.ForecastUtilisation <= 0))
        {
            ModelState.AddModelError(string.Empty, "Every capability requires capacity and forecast utilisation.");
        }

        if (Cycle.Capabilities.Any(x => x.ProposedUwaRate <= 0 || x.ProposedApfrRate <= 0 || x.ProposedCommercialRate <= 0))
        {
            ModelState.AddModelError(string.Empty, "Every capability requires three proposed rates.");
        }

        // Nothing is submitted for approval while a capability still has no rates. The
        // approver would otherwise be asked to seal a record with a hole in it.
        foreach (var problem in Rates.Problems)
        {
            ModelState.AddModelError(string.Empty, problem);
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Cycle.Status = "Submitted";
        Cycle.SubmittedBy = User.DisplayName();
        Cycle.SubmittedAtUtc = DateTime.UtcNow;
        Cycle.ReturnedBy = null;
        Cycle.ReturnedAtUtc = null;
        Cycle.ReturnReason = null;
        Cycle.UpdatedAtUtc = DateTime.UtcNow;
        await Db.SaveChangesAsync();

        TempData["Success"] = "Costing cycle submitted for delegated authority approval.";
        return RedirectToPage(new { cycleId });
    }

    private async Task<bool> Load(int cycleId)
    {
        if (!await LoadCycleAsync(cycleId))
        {
            return false;
        }

        // A sealed record reproduces its own figures under the method version it was sealed
        // with, not today's — architecture.md §3 rule R6.
        Rates = IsSealed
            ? calculator.CalculateAsAt(Cycle, Cycle.MethodVersion)
            : calculator.Calculate(Cycle);

        return true;
    }
}
