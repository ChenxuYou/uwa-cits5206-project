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

    public async Task<IActionResult> OnGetAsync()
    {
        if (User.IsInRole(AppUser.Roles.Approver))
        {
            return RedirectToPage("/Approvals/Index");
        }

        var owner = User.UserName();

        Cycles = await db.RicCycles.AsNoTracking()
            .Include(x => x.Capabilities)
            .Where(x => x.CreatedBy == owner)
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToListAsync();

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
