using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CostingTool.Pages.Ric;

public class StartModel(CostingDbContext db) : PageModel
{
    [BindProperty] public string PlatformName { get; set; } = string.Empty;
    [BindProperty] public int StartYear { get; set; } = DateTime.Now.Year;
    [BindProperty] public int EndYear { get; set; } = DateTime.Now.Year + 2;
    [BindProperty] public string BillableUnit { get; set; } = "Hours";
    [BindProperty] public string CapabilityNames { get; set; } = string.Empty;

    public async Task<IActionResult> OnPostAsync()
    {
        var names = CapabilityNames.Split(['\n', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Distinct().ToList();
        if (string.IsNullOrWhiteSpace(PlatformName)) ModelState.AddModelError(nameof(PlatformName), "Platform name is required.");
        if (EndYear < StartYear) ModelState.AddModelError(nameof(EndYear), "End year must be after the start year.");
        if (!new[] { "Hours", "Days", "Samples" }.Contains(BillableUnit)) ModelState.AddModelError(nameof(BillableUnit), "Select a valid billable unit.");
        if (names.Count == 0) ModelState.AddModelError(nameof(CapabilityNames), "Add at least one capability.");
        if (!ModelState.IsValid) return Page();

        var cycle = new RicCycle
        {
            PlatformName = PlatformName.Trim(), StartYear = StartYear, EndYear = EndYear,
            BillableUnit = BillableUnit,
            Capabilities = names.Select(x => new RicCapability { Name = x }).ToList()
        };
        db.RicCycles.Add(cycle);
        await db.SaveChangesAsync();
        return RedirectToPage("/Ric/Costs", new { cycleId = cycle.Id });
    }
}
