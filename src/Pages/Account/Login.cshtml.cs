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

    /// <summary>
    /// A real hash, of a password nobody holds, verified against when there is no user to
    /// verify against. See <see cref="SpendTheSameTime"/>. Built once per process from the
    /// configured hasher so that it carries the configured work factor; a hash made with
    /// default settings would verify in a different time and defeat the purpose.
    /// </summary>
    private static string? decoyHash;

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
            SpendTheSameTime(suppliedPassword);
            AddInvalidLoginError();
            return Page();
        }

        if (user.LockoutEndUtc is not null && user.LockoutEndUtc > now)
        {
            SpendTheSameTime(suppliedPassword);
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

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Role, user.Role),
            new(CurrentUser.UserNameClaim, user.UserName),
            new(CurrentUser.SecurityStampClaim, user.SecurityStamp)
        };

        if (user.MustChangePassword)
        {
            claims.Add(new Claim(CurrentUser.MustChangePasswordClaim, "true"));
        }

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = false });

        // A password someone else chose is replaced before anything else is done with it
        // (M4). MustChangePasswordFilter holds every other page closed until then.
        if (user.MustChangePassword)
        {
            return LocalRedirect("/Account/ChangePassword");
        }

        if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return LocalRedirect(HomeFor(user.Role));
    }

    /// <summary>
    /// Verify the supplied password against a decoy hash, and discard the answer.
    ///
    /// The identical error message above stops the form naming which usernames exist. The
    /// clock would have told anyway: verifying a real hash is deliberately slow, so a reply
    /// that skipped it came back measurably sooner, and "no such user" and "locked" were
    /// both distinguishable from "wrong password" by timing alone. Every path now pays the
    /// same cost.
    /// </summary>
    private void SpendTheSameTime(string suppliedPassword)
    {
        var decoy = new AppUser { UserName = "decoy" };
        decoyHash ??= hasher.HashPassword(decoy, Guid.NewGuid().ToString("N"));
        hasher.VerifyHashedPassword(decoy, decoyHash, suppliedPassword);
    }

    private void AddInvalidLoginError() =>
        ModelState.AddModelError(
            string.Empty,
            "Invalid username or password. Repeated failed attempts temporarily lock the account.");

    private string Home() => HomeFor(
        User.IsInRole(AppUser.Roles.Approver) ? AppUser.Roles.Approver
        : User.IsInRole(AppUser.Roles.Administrator) ? AppUser.Roles.Administrator
        : AppUser.Roles.DataEntry);

    private static string HomeFor(string role) => role switch
    {
        AppUser.Roles.Approver => "/Approvals",
        AppUser.Roles.Administrator => "/Admin/Cycles",
        _ => "/"
    };
}
