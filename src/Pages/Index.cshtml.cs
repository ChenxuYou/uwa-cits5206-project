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
        if (User.IsInRole("Approver")) return RedirectToPage("/Approvals/Index");
        var name = User.Identity!.Name!;
        Cycles = await db.RicCycles.AsNoTracking().Include(x => x.Capabilities)
            .Where(x => x.CreatedBy == name).OrderByDescending(x => x.UpdatedAtUtc).ToListAsync();
        RecentNotifications = await db.AppNotifications.AsNoTracking()
            .Where(x => x.RecipientName == name).OrderByDescending(x => x.CreatedAtUtc).Take(3).ToListAsync();
        UnreadCount = await db.AppNotifications.CountAsync(x => x.RecipientName == name && !x.IsRead);
        return Page();
    }
}
