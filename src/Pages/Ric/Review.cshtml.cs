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

    /// <summary>Why an export was refused, when one was. See <c>Export.cshtml.cs</c>.</summary>
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(int cycleId)
    {
        if (!await Load(cycleId))
        {
            return NotFound();
        }

        SuccessMessage = TempData["Success"] as string;
        ErrorMessage = TempData["Error"] as string;
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

        if (Cycle.Capabilities.Count == 0 || !Cycle.Costs.Any(x => !x.IsIncome))
        {
            ModelState.AddModelError(string.Empty, "The cycle must contain capabilities and operating costs.");
        }

        // Name the capabilities, so the custodian knows which screen to go back to (US-10).
        var noCapacity = Cycle.Capabilities.Where(x => x.MaximumCapacity <= 0 || x.ForecastUtilisation <= 0).Select(x => x.Name).ToList();
        if (noCapacity.Count > 0)
        {
            ModelState.AddModelError(string.Empty, $"Every capability requires capacity and forecast utilisation. Missing for: {string.Join(", ", noCapacity)}.");
        }

        var noRates = Cycle.Capabilities
            .Where(x => x.ProposedUwaRate <= 0 || x.ProposedApfrRate <= 0 || x.ProposedCommercialRate <= 0)
            .Select(x => x.Name)
            .ToList();
        if (noRates.Count > 0)
        {
            ModelState.AddModelError(string.Empty, $"Every capability requires three proposed rates above zero. Missing for: {string.Join(", ", noRates)}.");
        }

        // Nothing is submitted for approval while a capability still has no rates. The
        // approver would otherwise be asked to seal a record with a hole in it.
        foreach (var problem in Rates.Problems)
        {
            ModelState.AddModelError(string.Empty, problem);
        }

        // US-13: the explanations the tool requires are checked again here, against the cycle
        // as it now stands. A change made after the rates step — a new cost, say — can turn a
        // surplus into a deficit that was never explained.
        foreach (var missing in RequiredJustifications.Missing(Cycle, Rates))
        {
            ModelState.AddModelError(string.Empty, missing);
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
