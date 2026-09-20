using System.Security.Claims;
using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Admin;

/// <summary>
/// Account administration (US-19): create an account, deactivate one, reset a password.
///
/// Until this page existed there was no way to create a user at all outside development,
/// which made "accounts are provisioned deliberately" in <c>Program.cs</c> a statement with
/// nothing behind it.
///
/// <b>Deactivating, never deleting.</b> A user's name is written into the cycles they
/// created, submitted and sealed. Removing the row would leave those records pointing at
/// nobody, so an account that should no longer be used is switched off and keeps its
/// identity.
/// </summary>
public class UsersModel(CostingDbContext db, IPasswordHasher<AppUser> hasher) : PageModel
{
    public List<AppUser> Users { get; private set; } = [];

    /// <summary>How many cycles each username owns, so an account is never switched off blind.</summary>
    public Dictionary<string, int> CycleCounts { get; private set; } = [];

    public string? SuccessMessage { get; private set; }

    // Nullable on purpose. A non-nullable string property is implicitly required by model
    // binding, so posting the deactivate or reset form — neither of which carries these
    // fields — would fail validation on the create form's inputs and silently do nothing.
    [BindProperty] public string? NewUserName { get; set; }

    [BindProperty] public string? NewDisplayName { get; set; }

    [BindProperty] public string? NewRole { get; set; } = AppUser.Roles.DataEntry;

    [BindProperty] public string? NewPassword { get; set; }

    [BindProperty] public string? ResetPassword { get; set; }

    public async Task OnGetAsync()
    {
        SuccessMessage = TempData["Success"] as string;
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var userName = (NewUserName ?? string.Empty).Trim().ToLowerInvariant();
        var displayName = (NewDisplayName ?? string.Empty).Trim();

        if (userName.Length == 0)
        {
            ModelState.AddModelError(nameof(NewUserName), "Username is required.");
        }
        else if (userName.Any(char.IsWhiteSpace))
        {
            ModelState.AddModelError(nameof(NewUserName), "Username cannot contain spaces.");
        }
        else if (await db.AppUsers.AnyAsync(x => x.UserName == userName))
        {
            ModelState.AddModelError(nameof(NewUserName), "That username is already in use.");
        }

        if (displayName.Length == 0)
        {
            ModelState.AddModelError(nameof(NewDisplayName), "Display name is required — it is the name that appears on every record this person creates.");
        }

        if (NewRole is null || !AppUser.Roles.All.Contains(NewRole))
        {
            ModelState.AddModelError(nameof(NewRole), "Choose a role.");
        }

        foreach (var problem in PasswordPolicy.Problems(NewPassword))
        {
            ModelState.AddModelError(nameof(NewPassword), problem);
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        var user = new AppUser
        {
            UserName = userName,
            DisplayName = displayName,
            Role = NewRole!
        };
        user.PasswordHash = hasher.HashPassword(user, NewPassword!);

        db.AppUsers.Add(user);
        await db.SaveChangesAsync();

        TempData["Success"] = $"Account {userName} created. Give the person their password and ask them to change it.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int id)
    {
        var user = await db.AppUsers.FirstOrDefaultAsync(x => x.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        // An administrator switching off their own account would be the last act they could
        // perform, and on a single-administrator instance it locks everyone out for good.
        if (user.Id == SignedInUserId())
        {
            ModelState.AddModelError(string.Empty, "You cannot deactivate the account you are signed in with.");
            await LoadAsync();
            return Page();
        }

        user.IsActive = !user.IsActive;

        // Rotating the stamp ends any session the account still has open, so deactivating
        // takes effect on their next request rather than whenever their cookie expires.
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        user.AccessFailedCount = 0;
        user.LockoutEndUtc = null;
        await db.SaveChangesAsync();

        TempData["Success"] = user.IsActive
            ? $"Account {user.UserName} reactivated."
            : $"Account {user.UserName} deactivated and signed out.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(int id)
    {
        var user = await db.AppUsers.FirstOrDefaultAsync(x => x.Id == id);
        if (user is null)
        {
            return NotFound();
        }

        foreach (var problem in PasswordPolicy.Problems(ResetPassword))
        {
            ModelState.AddModelError(nameof(ResetPassword), problem);
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        user.PasswordHash = hasher.HashPassword(user, ResetPassword!);
        user.PasswordChangedAtUtc = DateTime.UtcNow;
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        user.AccessFailedCount = 0;
        user.LockoutEndUtc = null;
        await db.SaveChangesAsync();

        TempData["Success"] = $"Password reset for {user.UserName}. Their other sessions have been signed out.";
        return RedirectToPage();
    }

    private int SignedInUserId() =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    private async Task LoadAsync()
    {
        Users = await db.AppUsers.AsNoTracking()
            .OrderBy(x => x.Role)
            .ThenBy(x => x.DisplayName)
            .ToListAsync();

        CycleCounts = await db.RicCycles.AsNoTracking()
            .GroupBy(x => x.CreatedBy)
            .Select(group => new { UserName = group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.UserName, x => x.Count);
    }
}
