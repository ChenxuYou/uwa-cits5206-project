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
    [BindProperty] public string UserName { get; set; } = string.Empty;

    [BindProperty] public string Password { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public IActionResult OnGet() =>
        User.Identity?.IsAuthenticated == true ? LocalRedirect(Home()) : Page();

    public async Task<IActionResult> OnPostAsync()
    {
        var userName = UserName.Trim().ToLowerInvariant();
        var user = await db.AppUsers.SingleOrDefaultAsync(x => x.UserName == userName && x.IsActive);

        // One message for both "no such user" and "wrong password", so the form cannot be
        // used to find out which usernames exist.
        if (user is null ||
            hasher.VerifyHashedPassword(user, user.PasswordHash, Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(CurrentUser.UserNameClaim, user.UserName)
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

    private string Home() => User.IsInRole(AppUser.Roles.Approver) ? "/Approvals" : "/";
}
