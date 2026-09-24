using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Ric;

public class ReviewModel(CostingDbContext db, RicCalculationService calculator) : RicPageModel(db)
{
    public CycleRates Rates { get; private set; } = null!;

    public bool IsEditable => Cycle.IsEditable;

    public bool IsPending => Cycle.Status == "Submitted";

    public bool IsSealed => Cycle.Status == "Sealed";

    public string? SuccessMessage { get; private set; }

    /// <summary>The sealed record this cycle replaces, if it replaces one (US-01).</summary>
    public PreviousRecord? Previous { get; private set; }

    /// <summary>
    /// The cycle that replaces this one, once one has been started. This record is
    /// superseded when that cycle is sealed; until then it holds the current rates (F22).
    /// </summary>
    public RicCycle? Successor { get; private set; }

    public bool IsSuperseded => Successor?.Status == "Sealed";

    /// <summary>
    /// What the cycle still needs before it can be submitted, each with the step that
    /// supplies it (US-14). Empty once it is complete, and for a cycle no longer editable.
    /// </summary>
    public IReadOnlyList<MissingItem> Missing { get; private set; } = [];

    public bool IsReady => Missing.Count == 0;

    /// <summary>Where each operating cost sits (US-03, US-04), for the inputs on the page.</summary>
    public CycleCosts Costs { get; private set; } = null!;

    /// <summary>Why an export was refused, when one was. See <c>Export.cshtml.cs</c>.</summary>
    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(int cycleId)
    {
        if (!await Load(cycleId))
        {
            return NotFound();
        }

        await RememberStepAsync(RicSteps.Review);
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

        // Nothing goes for approval with a hole in it: the list the page already shows, with
        // its links, is the list the submission refuses on (US-14, N3). The page lists the
        // items themselves, so the summary only says why the button did nothing.
        if (!IsReady)
        {
            ModelState.AddModelError(
                string.Empty,
                $"The cycle cannot be submitted yet: {Missing.Count} item{(Missing.Count == 1 ? " is" : "s are")} missing. Each is listed above with a link to where it is entered.");
        }

        if (!confirmAccuracy)
        {
            ModelState.AddModelError(string.Empty, "Confirm that the assumptions and figures are complete and accurate.");
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

        // A sealed record shows the figures it was sealed with, read from its snapshot and
        // never recalculated — US-15, N6. See SealedRates.
        Rates = IsSealed
            ? SealedRates.Of(Cycle)
            : calculator.Calculate(Cycle);

        Costs = RicCalculationService.CostsOf(Cycle);
        Missing = IsEditable ? SubmissionChecks.For(Cycle, Rates) : [];

        Previous = await LoadReplacedRecordAsync();

        if (IsSealed)
        {
            var owner = User.UserName();
            Successor = await Db.RicCycles.AsNoTracking()
                .FirstOrDefaultAsync(x => x.SupersedesCycleId == Cycle.Id && x.CreatedBy == owner);
        }

        return true;
    }
}
