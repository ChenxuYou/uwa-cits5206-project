using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Ric;

public class CapacityModel(CostingDbContext db) : PageModel
{
    public RicCycle Cycle { get; private set; } = null!;
    [BindProperty] public int CycleId { get; set; }
    [BindProperty] public List<CapacityInput> Inputs { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int cycleId)
    {
        if (!await Load(cycleId)) return NotFound();
        Inputs = Cycle.Capabilities.Select(x => new CapacityInput(x.Id, x.MaximumCapacity, x.ForecastUwaUse, x.ForecastApfrUse, x.ForecastCommercialUse)).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await Load(CycleId)) return NotFound();
        if (Cycle.Status != "Draft") return RedirectToPage("/Ric/Review", new { cycleId = CycleId });
        foreach (var input in Inputs)
        {
            var capability = Cycle.Capabilities.FirstOrDefault(x => x.Id == input.Id);
            if (capability is null) { ModelState.AddModelError(string.Empty, "Invalid capability."); continue; }
            var forecast = input.UwaUse + input.ApfrUse + input.CommercialUse;
            if (input.MaximumCapacity <= 0) ModelState.AddModelError(string.Empty, $"{capability.Name}: maximum capacity must be greater than zero.");
            if (forecast <= 0) ModelState.AddModelError(string.Empty, $"{capability.Name}: forecast utilisation must be greater than zero.");
            if (forecast > input.MaximumCapacity) ModelState.AddModelError(string.Empty, $"{capability.Name}: forecast utilisation cannot exceed capacity.");
            if (new[] { input.UwaUse, input.ApfrUse, input.CommercialUse }.Any(x => x < 0)) ModelState.AddModelError(string.Empty, $"{capability.Name}: utilisation cannot be negative.");
        }
        if (!ModelState.IsValid) return Page();
        foreach (var input in Inputs)
        {
            var c = Cycle.Capabilities.First(x => x.Id == input.Id); c.MaximumCapacity = input.MaximumCapacity; c.ForecastUwaUse = input.UwaUse; c.ForecastApfrUse = input.ApfrUse; c.ForecastCommercialUse = input.CommercialUse;
        }
        Cycle.UpdatedAtUtc = DateTime.UtcNow; await db.SaveChangesAsync();
        return RedirectToPage("/Ric/Rates", new { cycleId = CycleId });
    }

    private async Task<bool> Load(int id){Cycle=(await db.RicCycles.Include(x=>x.Capabilities).FirstOrDefaultAsync(x=>x.Id==id))!;CycleId=id;return Cycle is not null;}
    public class CapacityInput
    {
        public CapacityInput() { }
        public CapacityInput(int id, decimal maximumCapacity, decimal uwaUse, decimal apfrUse, decimal commercialUse) =>
            (Id, MaximumCapacity, UwaUse, ApfrUse, CommercialUse) = (id, maximumCapacity, uwaUse, apfrUse, commercialUse);
        public int Id { get; set; }
        public decimal MaximumCapacity { get; set; }
        public decimal UwaUse { get; set; }
        public decimal ApfrUse { get; set; }
        public decimal CommercialUse { get; set; }
    }
}
