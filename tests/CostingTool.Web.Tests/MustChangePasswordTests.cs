using System.Security.Claims;
using CostingTool.Data;
using CostingTool.Models;
using CostingTool.Pages.Account;
using CostingTool.Pages.Admin;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace CostingTool.Web.Tests;

/// <summary>
/// M4 in the audit: a password somebody else chose — an administrator, or the bootstrap
/// value in the server's environment — is replaced by its owner before anything else.
/// </summary>
public class MustChangePasswordTests
{
    private const string AdminSet = "Temporary-Pass-2026";
    private const string OwnChoice = "My-Own-Choice-2026";

    private static CostingDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<CostingDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static IPasswordHasher<AppUser> Hasher() =>
        new PasswordHasher<AppUser>(Options.Create(new PasswordHasherOptions { IterationCount = 1000 }));

    private static AppUser AddUser(CostingDbContext db, bool mustChange, string userName = "entry")
    {
        var user = new AppUser { UserName = userName, DisplayName = userName, Role = AppUser.Roles.DataEntry, MustChangePassword = mustChange };
        user.PasswordHash = Hasher().HashPassword(user, AdminSet);
        db.AppUsers.Add(user);
        db.SaveChanges();
        return user;
    }

    private static (HttpContext Http, FakeAuthenticationService Auth) Http(ClaimsPrincipal? user = null)
    {
        var auth = new FakeAuthenticationService();
        var http = new DefaultHttpContext
        {
            User = user ?? new ClaimsPrincipal(new ClaimsIdentity()),
            RequestServices = new ServiceCollection()
                .AddSingleton<IAuthenticationService>(auth)
                .AddSingleton<ITempDataProvider, NullTempDataProvider>()
                .AddSingleton<ITempDataDictionaryFactory, TempDataDictionaryFactory>()
                .BuildServiceProvider()
        };
        return (http, auth);
    }

    private static ClaimsPrincipal Principal(AppUser user, bool mustChange = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Role, user.Role),
            new(CurrentUser.UserNameClaim, user.UserName)
        };
        if (mustChange)
        {
            claims.Add(new Claim(CurrentUser.MustChangePasswordClaim, "true"));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    // ---- Where the flag is set ---------------------------------------------------------

    [Fact]
    public async Task AnAccountAnAdministratorCreatesMustChooseItsOwnPassword()
    {
        await using var db = CreateDb();
        var admin = AddUser(db, mustChange: false, userName: "admin");
        var page = new UsersModel(db, Hasher())
        {
            PageContext = new PageContext { HttpContext = Http(Principal(admin)).Http },
            NewUserName = "wenmin",
            NewDisplayName = "Wenmin Luo",
            NewRole = AppUser.Roles.DataEntry,
            NewPassword = AdminSet
        };

        await page.OnPostCreateAsync();

        Assert.True((await db.AppUsers.SingleAsync(x => x.UserName == "wenmin")).MustChangePassword);
    }

    [Fact]
    public async Task APasswordAnAdministratorResetsMustBeReplacedToo()
    {
        await using var db = CreateDb();
        var admin = AddUser(db, mustChange: false, userName: "admin");
        var user = AddUser(db, mustChange: false);
        var page = new UsersModel(db, Hasher())
        {
            PageContext = new PageContext { HttpContext = Http(Principal(admin)).Http },
            ResetPassword = AdminSet
        };

        await page.OnPostResetPasswordAsync(user.Id);

        Assert.True((await db.AppUsers.SingleAsync(x => x.Id == user.Id)).MustChangePassword);
    }

    // ---- Signing in and changing it ----------------------------------------------------

    [Fact]
    public async Task SigningInWithAnAdministratorsPasswordGoesStraightToChangingIt()
    {
        await using var db = CreateDb();
        AddUser(db, mustChange: true);
        var (http, auth) = Http();
        var page = new LoginModel(db, Hasher()) { PageContext = new PageContext { HttpContext = http }, UserName = "entry", Password = AdminSet };

        var result = await page.OnPostAsync();

        Assert.Equal("/Account/ChangePassword", Assert.IsType<LocalRedirectResult>(result).Url);
        Assert.True(auth.SignedInPrincipal!.MustChangePassword());
    }

    [Fact]
    public async Task AnOrdinarySignInCarriesNoSuchClaim()
    {
        await using var db = CreateDb();
        AddUser(db, mustChange: false);
        var (http, auth) = Http();
        var page = new LoginModel(db, Hasher()) { PageContext = new PageContext { HttpContext = http }, UserName = "entry", Password = AdminSet };

        await page.OnPostAsync();

        Assert.False(auth.SignedInPrincipal!.MustChangePassword());
    }

    [Fact]
    public async Task ChoosingANewPasswordClearsTheFlag()
    {
        await using var db = CreateDb();
        var user = AddUser(db, mustChange: true);
        var page = new ChangePasswordModel(db, Hasher())
        {
            PageContext = new PageContext { HttpContext = Http(Principal(user, mustChange: true)).Http },
            CurrentPassword = AdminSet,
            NewPassword = OwnChoice,
            ConfirmPassword = OwnChoice
        };

        Assert.IsType<RedirectToPageResult>(await page.OnPostAsync());
        Assert.False((await db.AppUsers.SingleAsync()).MustChangePassword);
    }

    // ---- Every other page stays closed until then ----------------------------------------

    private static async Task<IActionResult?> Filter(string page, bool mustChange)
    {
        var user = new AppUser { Id = 7, UserName = "entry", DisplayName = "entry", Role = AppUser.Roles.DataEntry };
        var http = new DefaultHttpContext { User = Principal(user, mustChange) };
        var descriptor = new CompiledPageActionDescriptor { ViewEnginePath = page };
        var pageContext = new PageContext(new ActionContext(http, new RouteData(), descriptor));
        var executing = new PageHandlerExecutingContext(pageContext, [], null!, new Dictionary<string, object?>(), new object());
        var reached = false;

        await new MustChangePasswordFilter().OnPageHandlerExecutionAsync(executing, () =>
        {
            reached = true;
            return Task.FromResult(new PageHandlerExecutedContext(pageContext, [], null!, new object()));
        });

        return reached ? null : executing.Result;
    }

    [Theory]
    [InlineData("/Index")]
    [InlineData("/Ric/Costs")]
    [InlineData("/Records/Index")]
    public async Task APageIsClosedUntilThePasswordIsChanged(string page)
    {
        var result = await Filter(page, mustChange: true);

        Assert.Equal("/Account/ChangePassword", Assert.IsType<RedirectToPageResult>(result).PageName);
    }

    [Theory]
    [InlineData("/Account/ChangePassword")]
    [InlineData("/Account/Logout")]
    [InlineData("/Error")]
    public async Task ChangingThePasswordAndSigningOutStayOpen(string page)
    {
        Assert.Null(await Filter(page, mustChange: true));
    }

    [Fact]
    public async Task AnOrdinarySessionIsNotAffected()
    {
        Assert.Null(await Filter("/Ric/Costs", mustChange: false));
    }

    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public ClaimsPrincipal? SignedInPrincipal { get; private set; }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme) => Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;

        public Task SignInAsync(HttpContext context, string? scheme, ClaimsPrincipal principal, AuthenticationProperties? properties)
        {
            SignedInPrincipal = principal;
            return Task.CompletedTask;
        }

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) => Task.CompletedTask;
    }

    private sealed class NullTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
