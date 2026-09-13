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

public class LoginModel(CostingDbContext db, IPasswordHasher<AppUser> hasher) : PageModel
{
    private const int MaximumFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    [BindProperty] public string UserName { get; set; } = string.Empty;

    [BindProperty] public string Password { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public IActionResult OnGet() =>
        User.Identity?.IsAuthenticated == true ? LocalRedirect(Home()) : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        var userName = (UserName ?? string.Empty).Trim().ToLowerInvariant();
        var suppliedPassword = Password ?? string.Empty;
        var user = await db.AppUsers.SingleOrDefaultAsync(x => x.UserName == userName && x.IsActive);
        var now = DateTime.UtcNow;

        // One message for both "no such user" and "wrong password", so the form cannot be
        // used to find out which usernames exist.
        if (user is null)
        {
            AddInvalidLoginError();
            return Page();
        }

        if (user.LockoutEndUtc is not null && user.LockoutEndUtc > now)
        {
            AddInvalidLoginError();
            return Page();
        }

        if (user.LockoutEndUtc is not null)
        {
            // The lockout period has expired. Start a fresh attempt window instead of
            // immediately locking the account again after one more mistake.
            user.LockoutEndUtc = null;
            user.AccessFailedCount = 0;
        }

        var verification = hasher.VerifyHashedPassword(user, user.PasswordHash, suppliedPassword);
        if (verification == PasswordVerificationResult.Failed)
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= MaximumFailedAttempts)
            {
                user.LockoutEndUtc = now.Add(LockoutDuration);
            }

            await db.SaveChangesAsync();
            AddInvalidLoginError();
            return Page();
        }

        // The encoded hash stores its salt, PRF and iteration count. When the configured
        // work factor increases, a correct login transparently upgrades the stored hash.
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = hasher.HashPassword(user, suppliedPassword);
        }

        user.AccessFailedCount = 0;
        user.LockoutEndUtc = null;
        user.LastLoginAtUtc = now;
        await db.SaveChangesAsync();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(CurrentUser.UserNameClaim, user.UserName),
            new Claim(CurrentUser.SecurityStampClaim, user.SecurityStamp)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = false });

        if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return LocalRedirect(user.Role == AppUser.Roles.Approver ? "/Approvals" : "/");
    }

    private void AddInvalidLoginError() =>
        ModelState.AddModelError(
            string.Empty,
            "Invalid username or password. Repeated failed attempts temporarily lock the account.");

    private string Home() => User.IsInRole(AppUser.Roles.Approver) ? "/Approvals" : "/";
}
