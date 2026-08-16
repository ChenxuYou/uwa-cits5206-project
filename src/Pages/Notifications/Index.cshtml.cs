using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Notifications;

public class IndexModel(CostingDbContext db) : PageModel
{
    public List<AppNotification> Notifications { get; private set; } = [];
    public async Task OnGetAsync() => Notifications = await db.AppNotifications.AsNoTracking()
        .Where(x => x.RecipientName == User.Identity!.Name).OrderByDescending(x => x.CreatedAtUtc).ToListAsync();

    public async Task<IActionResult> OnPostReadAsync(int id)
    {
        var notice = await db.AppNotifications.FirstOrDefaultAsync(x => x.Id == id && x.RecipientName == User.Identity!.Name);
        if (notice is not null) { notice.IsRead = true; await db.SaveChangesAsync(); }
        return notice?.RicCycleId is int cycleId ? RedirectToPage("/Ric/Review", new { cycleId }) : RedirectToPage();
    }

    public async Task<IActionResult> OnPostReadAllAsync()
    {
        var notices = await db.AppNotifications.Where(x => x.RecipientName == User.Identity!.Name && !x.IsRead).ToListAsync();
        notices.ForEach(x => x.IsRead = true); await db.SaveChangesAsync(); return RedirectToPage();
    }
}
