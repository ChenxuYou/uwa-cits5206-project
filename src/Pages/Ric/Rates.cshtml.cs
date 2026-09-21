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

    public Task<IActionResult> OnPostAsync() => SaveAsync("/Ric/Review");

    /// <summary>
    /// Going back a step saves first.
    ///
    /// US-10 asks that nothing be lost by navigating backwards, and the link that used to sit
    /// here discarded every proposed rate and every note typed on this screen. A custodian who
    /// goes back to change one number expects to find the rest of their work when they return.
    /// </summary>
    public Task<IActionResult> OnPostBackAsync() => SaveAsync("/Ric/Capacity");

    private async Task<IActionResult> SaveAsync(string nextPage)
    {
        if (!await Load(CycleId))
        {
            return NotFound();
        }

        if (!Cycle.IsEditable)
        {
            return RedirectToPage("/Ric/Review", new { cycleId = CycleId });
        }

        EntryChecks.ExplainUnreadableNumbers(ModelState, FieldLabel, "a rate in dollars, such as 162.00");

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

        if (!ModelState.IsValid)
        {
            return Page();
        }

        foreach (var input in Inputs)
        {
            var capability = Cycle.Capabilities.First(x => x.Id == input.Id);
            capability.ProposedUwaRate = input.Uwa;
            capability.ProposedApfrRate = input.Apfr;
            capability.ProposedCommercialRate = input.Commercial;
        }

        Cycle.BenchmarkNotes = BenchmarkNotes;
        Cycle.PricingJustification = PricingJustification;

        // Price again with what was just typed, so the variance and the balance below are
        // judged on this screen's figures rather than on the ones it was opened with.
        Rates = calculator.Calculate(Cycle);

        RequireJustification();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        Cycle.UpdatedAtUtc = DateTime.UtcNow;
        await Db.SaveChangesAsync();

        return RedirectToPage(nextPage, new { cycleId = CycleId });
    }

    /// <summary>
    /// The two places the tool insists on an explanation: a proposed rate that differs from
    /// the calculated one (US-11, F9) and a forecast deficit (US-12). Neither is refused —
    /// both are normal — but neither reaches the approver unexplained.
    /// </summary>
    private void RequireJustification()
    {
        if (!string.IsNullOrWhiteSpace(PricingJustification))
        {
            return;
        }

        var varied = Cycle.Capabilities
            .Where(x => Rates.For(x.Id)?.VariesFromCalculated == true)
            .Select(x => x.Name)
            .ToList();

        if (varied.Count > 0)
        {
            ModelState.AddModelError(
                nameof(PricingJustification),
                $"A pricing justification is required because the proposed rates differ from the "
                + $"calculated ones for {string.Join(", ", varied)}.");
            return;
        }

        if (Rates.IsComplete && Rates.ForecastBalance < 0)
        {
            ModelState.AddModelError(
                nameof(PricingJustification),
                "A pricing justification is required because these rates forecast a deficit. "
                + "A deficit does not stop the record being submitted; it has to be explained.");
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

    private string? FieldLabel(string key) => IndexedFieldLabel(key, Inputs.Select(x => x.Id).ToList(), new Dictionary<string, string>
    {
        ["Uwa"] = "proposed UWA rate",
        ["Apfr"] = "proposed APFR rate",
        ["Commercial"] = "proposed commercial rate"
    });
}
