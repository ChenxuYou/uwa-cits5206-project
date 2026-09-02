using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Notifications;

public class IndexModel(CostingDbContext db) : PageModel
{
    public List<AppNotification> Notifications { get; private set; } = [];

    public async Task OnGetAsync() =>
        Notifications = await db.AppNotifications.AsNoTracking()
            .Where(x => x.RecipientUserName == User.UserName())
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

    public async Task<IActionResult> OnPostReadAsync(int id)
    {
        var owner = User.UserName();
        var notice = await db.AppNotifications
            .FirstOrDefaultAsync(x => x.Id == id && x.RecipientUserName == owner);

        if (notice is null)
        {
            return NotFound();
        }

        notice.IsRead = true;
        await db.SaveChangesAsync();

        return notice.RicCycleId is int cycleId
            ? RedirectToPage("/Ric/Review", new { cycleId })
            : RedirectToPage();
    }

    public async Task<IActionResult> OnPostReadAllAsync()
    {
        var owner = User.UserName();
        var notices = await db.AppNotifications
            .Where(x => x.RecipientUserName == owner && !x.IsRead)
            .ToListAsync();

        notices.ForEach(x => x.IsRead = true);
        await db.SaveChangesAsync();

        return RedirectToPage();
    }
}
