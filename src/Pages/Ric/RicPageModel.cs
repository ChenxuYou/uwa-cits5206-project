using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Ric;

/// <summary>
/// The one way a page in the guided workflow loads a cycle.
///
/// <b>Two things it makes impossible.</b> Every step used to write its own query, so each
/// carried its own <c>.Include()</c> list and its own ownership filter. A step that omitted
/// an <c>Include</c> would sum an unloaded collection to zero and quietly show different
/// figures from the step before it, and a step that omitted the filter would show one
/// custodian another's record. Loading through here means neither can happen by forgetting.
/// </summary>
public abstract class RicPageModel(CostingDbContext db) : PageModel
{
    protected CostingDbContext Db { get; } = db;

    public RicCycle Cycle { get; private set; } = null!;

    /// <summary>
    /// Load the cycle if it exists and belongs to the signed-in custodian.
    /// </summary>
    /// <returns>False when it does not exist or is not theirs — the page returns NotFound
    /// for both, so the URL cannot be used to find out which cycles exist.</returns>
    protected async Task<bool> LoadCycleAsync(int cycleId)
    {
        var owner = User.UserName();

        var cycle = await Db.RicCycles
            .Include(x => x.Capabilities)
            .Include(x => x.Costs).ThenInclude(x => x.Capability)
            .Include(x => x.Costs).ThenInclude(x => x.YearAmounts)
            .FirstOrDefaultAsync(x => x.Id == cycleId && x.CreatedBy == owner);

        if (cycle is null)
        {
            return false;
        }

        Cycle = cycle;
        return true;
    }
}
