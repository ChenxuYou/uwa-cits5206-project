using CostingTool.Data;
using Microsoft.AspNetCore.Mvc;

namespace CostingTool.Pages.Ric;

public class RatesModel(CostingDbContext db, RicCalculationService calculator) : RicPageModel(db)
{
    public CycleRates Rates { get; private set; } = null!;

    [BindProperty] public int CycleId { get; set; }

    [BindProperty] public List<RateInput> Inputs { get; set; } = [];

    [BindProperty] public string? BenchmarkNotes { get; set; }

    [BindProperty] public string? PricingJustification { get; set; }

    /// <summary>
    /// True when the page is showing the balance for rates the custodian has typed but not
    /// saved (<see cref="OnPostPreviewAsync"/>), so the view can say so.
    /// </summary>
    public bool IsPreview { get; private set; }

    public async Task<IActionResult> OnGetAsync(int cycleId)
    {
        if (!await Load(cycleId))
        {
            return NotFound();
        }

        Inputs = Cycle.Capabilities
            .Select(x => new RateInput(x.Id, x.ProposedUwaRate, x.ProposedApfrRate, x.ProposedCommercialRate))
            .ToList();

        BenchmarkNotes = Cycle.BenchmarkNotes;
        PricingJustification = Cycle.PricingJustification;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await Load(CycleId))
        {
            return NotFound();
        }

        if (!Cycle.IsEditable)
        {
            return RedirectToPage("/Ric/Review", new { cycleId = CycleId });
        }

        ValidateInputs();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        ApplyInputs();

        Cycle.BenchmarkNotes = BenchmarkNotes;
        Cycle.PricingJustification = PricingJustification;
        Cycle.UpdatedAtUtc = DateTime.UtcNow;
        await Db.SaveChangesAsync();

        return RedirectToPage("/Ric/Review", new { cycleId = CycleId });
    }

    /// <summary>
    /// "What if" (US-10, US-12): recalculate the balance at the proposed rates typed on the
    /// page, without saving them. The calculation stays on the server — no formula or
    /// coefficient is sent to the browser (US-09, N1) — so seeing a new balance costs a round
    /// trip, and nothing reaches the database until the custodian continues to the review.
    /// </summary>
    public async Task<IActionResult> OnPostPreviewAsync()
    {
        if (!await Load(CycleId))
        {
            return NotFound();
        }

        if (!Cycle.IsEditable)
        {
            return RedirectToPage("/Ric/Review", new { cycleId = CycleId });
        }

        ValidateInputs();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // In memory only: this handler never calls SaveChanges, and the context is scoped to
        // the request, so the tracked entities are discarded with it.
        ApplyInputs();
        Rates = calculator.Calculate(Cycle);
        IsPreview = true;

        return Page();
    }

    private void ValidateInputs()
    {
        foreach (var input in Inputs)
        {
            if (Cycle.Capabilities.All(x => x.Id != input.Id))
            {
                ModelState.AddModelError(string.Empty, "Invalid capability.");
            }
            else if (new[] { input.Uwa, input.Apfr, input.Commercial }.Any(x => x < 0))
            {
                ModelState.AddModelError(string.Empty, "Proposed rates cannot be negative.");
            }
        }
    }

    private void ApplyInputs()
    {
        foreach (var input in Inputs)
        {
            var capability = Cycle.Capabilities.First(x => x.Id == input.Id);
            capability.ProposedUwaRate = input.Uwa;
            capability.ProposedApfrRate = input.Apfr;
            capability.ProposedCommercialRate = input.Commercial;
        }
    }

    private async Task<bool> Load(int cycleId)
    {
        if (!await LoadCycleAsync(cycleId))
        {
            return false;
        }

        CycleId = cycleId;
        Rates = calculator.Calculate(Cycle);
        return true;
    }

    public class RateInput
    {
        public RateInput()
        {
        }

        public RateInput(int id, decimal uwa, decimal apfr, decimal commercial) =>
            (Id, Uwa, Apfr, Commercial) = (id, uwa, apfr, commercial);

        public int Id { get; set; }

        public decimal Uwa { get; set; }

        public decimal Apfr { get; set; }

        public decimal Commercial { get; set; }
    }
}
