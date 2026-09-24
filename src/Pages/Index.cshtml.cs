using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages;

public class IndexModel(CostingDbContext db) : PageModel
{
    public List<RicCycle> Cycles { get; private set; } = [];

    public List<AppNotification> RecentNotifications { get; private set; } = [];

    public int UnreadCount { get; private set; }

    /// <summary>
    /// Sealed cycles that a later sealed cycle has replaced (F22). Worked out from the
    /// reference the newer cycle holds, because the older record is never written to.
    /// </summary>
    public HashSet<int> Superseded { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.IsInRole(AppUser.Roles.Approver))
        {
            return RedirectToPage("/Approvals/Index");
        }

        // The overview below is a custodian's own workspace — their cycles, their
        // notifications. An administrator owns neither, so they start where their work is.
        if (User.IsInRole(AppUser.Roles.Administrator))
        {
            return RedirectToPage("/Admin/Cycles");
        }

        var owner = User.UserName();

        Cycles = await db.RicCycles.AsNoTracking()
            .Include(x => x.Capabilities)
            .Where(x => x.CreatedBy == owner)
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToListAsync();

        Superseded = Cycles
            .Where(x => x.Status == "Sealed" && x.SupersedesCycleId is not null)
            .Select(x => x.SupersedesCycleId!.Value)
            .ToHashSet();

        RecentNotifications = await db.AppNotifications.AsNoTracking()
            .Where(x => x.RecipientUserName == owner)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(3)
            .ToListAsync();

        UnreadCount = await db.AppNotifications
            .CountAsync(x => x.RecipientUserName == owner && !x.IsRead);

        return Page();
    }
}
