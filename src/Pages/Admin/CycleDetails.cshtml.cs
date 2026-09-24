using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Admin;

/// <summary>
/// One cycle, read-only, for an administrator (US-19).
///
/// <b>Why this is not just the custodian's Review page with the filter relaxed.</b> Those
/// pages post: they submit, they edit, they write a name into the record. Reusing them
/// would have meant an administrator's click landing in the audit trail under whatever
/// name the page happened to stamp, which is precisely what US-02, US-15 and US-16 rely on
/// not happening. A separate page with no handler but OnGet cannot.
/// </summary>
public class CycleDetailsModel(CostingDbContext db, RicCalculationService calculator) : PageModel
{
    public RicCycle Cycle { get; private set; } = null!;

    public CycleRates Rates { get; private set; } = null!;

    public AppUser? Custodian { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var cycle = await db.RicCycles.AsNoTracking()
            .Include(x => x.Capabilities)
            .Include(x => x.Costs).ThenInclude(x => x.Capability)
            .Include(x => x.Costs).ThenInclude(x => x.YearAmounts)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (cycle is null)
        {
            return NotFound();
        }

        Cycle = cycle;

        // A sealed record shows the figures it was sealed with, read from its snapshot and
        // never recalculated — US-15, N6. See SealedRates.
        Rates = Cycle.Status == "Sealed"
            ? SealedRates.Of(Cycle)
            : calculator.Calculate(Cycle);

        Custodian = await db.AppUsers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserName == Cycle.CreatedBy);

        return Page();
    }
}
