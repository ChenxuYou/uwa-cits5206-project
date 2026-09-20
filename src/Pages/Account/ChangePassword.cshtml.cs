using System.Security.Claims;
using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Account;

public class ChangePasswordModel(CostingDbContext db, IPasswordHasher<AppUser> hasher) : PageModel
{
    [BindProperty] public string? CurrentPassword { get; set; }

    [BindProperty] public string? NewPassword { get; set; }

    [BindProperty] public string? ConfirmPassword { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var idText = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idText, out var userId))
        {
            return Challenge();
        }

        var user = await db.AppUsers.SingleOrDefaultAsync(x => x.Id == userId && x.IsActive);
        if (user is null)
        {
            return Challenge();
        }

        ValidateNewPassword();

        var current = CurrentPassword ?? string.Empty;
        if (hasher.VerifyHashedPassword(user, user.PasswordHash, current) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(nameof(CurrentPassword), "The current password is incorrect.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        user.PasswordHash = hasher.HashPassword(user, NewPassword!);
        user.PasswordChangedAtUtc = DateTime.UtcNow;
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        user.AccessFailedCount = 0;
        user.LockoutEndUtc = null;
        await db.SaveChangesAsync();

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["AuthenticationStatus"] = "Password changed successfully. Sign in with your new password.";
        return RedirectToPage("/Account/Login");
    }

    private void ValidateNewPassword()
    {
        var password = NewPassword ?? string.Empty;

        if (string.IsNullOrEmpty(CurrentPassword))
        {
            ModelState.AddModelError(nameof(CurrentPassword), "Current password is required.");
        }

        // The rules themselves live in PasswordPolicy, so that this page and the
        // administrator's create-and-reset screens cannot come to disagree about them.
        foreach (var problem in PasswordPolicy.Problems(password))
        {
            ModelState.AddModelError(nameof(NewPassword), problem);
        }

        if (password != ConfirmPassword)
        {
            ModelState.AddModelError(nameof(ConfirmPassword), "The new passwords do not match.");
        }

        if (!string.IsNullOrEmpty(CurrentPassword) && password == CurrentPassword)
        {
            ModelState.AddModelError(nameof(NewPassword), "The new password must be different from the current password.");
        }
    }
}
