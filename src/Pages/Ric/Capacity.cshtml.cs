using CostingTool.Data;
using Microsoft.AspNetCore.Mvc;

namespace CostingTool.Pages.Ric;

public class CapacityModel(CostingDbContext db) : RicPageModel(db)
{
    [BindProperty] public int CycleId { get; set; }

    [BindProperty] public List<CapacityInput> Inputs { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int cycleId)
    {
        if (!await LoadCycleAsync(cycleId))
        {
            return NotFound();
        }

        CycleId = cycleId;
        Inputs = Cycle.Capabilities
            .Select(x => new CapacityInput(x.Id, x.MaximumCapacity, x.ForecastUwaUse, x.ForecastApfrUse, x.ForecastCommercialUse))
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await LoadCycleAsync(CycleId))
        {
            return NotFound();
        }

        if (!Cycle.IsEditable)
        {
            return RedirectToPage("/Ric/Review", new { cycleId = CycleId });
        }

        foreach (var input in Inputs)
        {
            var capability = Cycle.Capabilities.FirstOrDefault(x => x.Id == input.Id);
            if (capability is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid capability.");
                continue;
            }

            var forecast = input.UwaUse + input.ApfrUse + input.CommercialUse;

            if (input.MaximumCapacity <= 0)
            {
                ModelState.AddModelError(string.Empty, $"{capability.Name}: maximum capacity must be greater than zero.");
            }

            if (new[] { input.UwaUse, input.ApfrUse, input.CommercialUse }.Any(x => x < 0))
            {
                ModelState.AddModelError(string.Empty, $"{capability.Name}: utilisation cannot be negative.");
            }

            // Forecast, not capacity. This is the input the workbook makes easy to confuse,
            // and the one the whole calculation divides by — so it is mandatory and it is
            // checked here rather than left to the engine to refuse later.
            if (forecast <= 0)
            {
                ModelState.AddModelError(string.Empty, $"{capability.Name}: forecast utilisation must be greater than zero.");
            }

            if (forecast > input.MaximumCapacity)
            {
                ModelState.AddModelError(string.Empty, $"{capability.Name}: forecast utilisation cannot exceed capacity.");
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        foreach (var input in Inputs)
        {
            var capability = Cycle.Capabilities.First(x => x.Id == input.Id);
            capability.MaximumCapacity = input.MaximumCapacity;
            capability.ForecastUwaUse = input.UwaUse;
            capability.ForecastApfrUse = input.ApfrUse;
            capability.ForecastCommercialUse = input.CommercialUse;
        }

        Cycle.UpdatedAtUtc = DateTime.UtcNow;
        await Db.SaveChangesAsync();

        return RedirectToPage("/Ric/Rates", new { cycleId = CycleId });
    }

    public class CapacityInput
    {
        public CapacityInput()
        {
        }

        public CapacityInput(int id, decimal maximumCapacity, decimal uwaUse, decimal apfrUse, decimal commercialUse) =>
            (Id, MaximumCapacity, UwaUse, ApfrUse, CommercialUse) = (id, maximumCapacity, uwaUse, apfrUse, commercialUse);

        public int Id { get; set; }

        public decimal MaximumCapacity { get; set; }

        public decimal UwaUse { get; set; }

        public decimal ApfrUse { get; set; }

        public decimal CommercialUse { get; set; }
    }
}
