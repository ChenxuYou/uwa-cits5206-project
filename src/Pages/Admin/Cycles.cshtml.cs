using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Admin;

/// <summary>
/// Every costing cycle in the application, whoever created it (US-19).
///
/// This is the one query in the application that deliberately does not filter by owner.
/// Everywhere else, omitting the filter is the bug that shows one custodian another's
/// record; here the whole point is to show them all, so the absence is stated rather than
/// left to look like an oversight. The page is read-only — see <see cref="CycleDetailsModel"/>.
/// </summary>
public class CyclesModel(CostingDbContext db) : PageModel
{
    public List<RicCycle> Cycles { get; private set; } = [];

    public List<AppUser> Custodians { get; private set; } = [];

    public string? Status { get; private set; }

    public string? Owner { get; private set; }

    public int TotalCount { get; private set; }

    public async Task OnGetAsync(string? status, string? owner)
    {
        Status = string.IsNullOrWhiteSpace(status) ? null : status;
        Owner = string.IsNullOrWhiteSpace(owner) ? null : owner;

        var query = db.RicCycles.AsNoTracking().Include(x => x.Capabilities).AsQueryable();

        if (Status is not null)
        {
            query = query.Where(x => x.Status == Status);
        }

        if (Owner is not null)
        {
            query = query.Where(x => x.CreatedBy == Owner);
        }

        Cycles = await query.OrderByDescending(x => x.UpdatedAtUtc).ToListAsync();
        TotalCount = await db.RicCycles.CountAsync();

        Custodians = await db.AppUsers.AsNoTracking()
            .Where(x => x.Role == AppUser.Roles.DataEntry)
            .OrderBy(x => x.DisplayName)
            .ToListAsync();
    }
}
