using System.Security.Claims;
using CostingTool.Data;
using CostingTool.Models;
using CostingTool.Pages.Account;
using CostingTool.Pages.Ric;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace CostingTool.Web.Tests;

/// <summary>
/// US-19, the two rules the sign-in page exists to enforce: nobody gets in without the
/// right password, and nobody sees a record that is not theirs.
///
/// Neither had any automated coverage. The engine tests are the merge gate, so a change
/// that quietly stopped counting failed attempts, or dropped the owner from the cycle
/// query, would have reached main green.
/// </summary>
public class SignInTests
{
    private const string GoodPassword = "Costing&Pricing2026";

    private static CostingDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<CostingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CostingDbContext(options);
    }

    /// <summary>
    /// The application hashes at 210,000 iterations. These tests hash repeatedly and are
    /// not testing the work factor, so they use a cheap one to stay fast.
    /// </summary>
    private static IPasswordHasher<AppUser> Hasher() =>
        new PasswordHasher<AppUser>(Options.Create(new PasswordHasherOptions { IterationCount = 1000 }));

    private static AppUser AddUser(
        CostingDbContext db,
        string userName = "entry",
        string password = GoodPassword,
        string role = AppUser.Roles.DataEntry,
        bool isActive = true)
    {
        var user = new AppUser
        {
            UserName = userName,
            DisplayName = userName,
            Role = role,
            IsActive = isActive
        };
        user.PasswordHash = Hasher().HashPassword(user, password);
        db.AppUsers.Add(user);
        db.SaveChanges();
        return user;
    }

    private static (LoginModel Page, FakeAuthenticationService Auth) CreateLoginPage(CostingDbContext db)
    {
        var auth = new FakeAuthenticationService();
        var services = new ServiceCollection()
            .AddSingleton<IAuthenticationService>(auth)
            .BuildServiceProvider();

        var page = new LoginModel(db, Hasher())
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext { RequestServices = services }
            }
        };

        return (page, auth);
    }

    private static async Task<IActionResult> AttemptSignIn(LoginModel page, string userName, string password)
    {
        page.UserName = userName;
        page.Password = password;
        return await page.OnPostAsync();
    }

    private static bool WasRejected(LoginModel page) =>
        page.ModelState.TryGetValue(string.Empty, out var entry) && entry.Errors.Count > 0;

    // ---- Signing in ----------------------------------------------------------------------

    [Fact]
    public async Task TheRightPasswordSignsTheUserIn()
    {
        await using var db = CreateDb();
        AddUser(db);
        var (page, auth) = CreateLoginPage(db);

        var result = await AttemptSignIn(page, "entry", GoodPassword);

        Assert.IsType<LocalRedirectResult>(result);
        Assert.NotNull(auth.SignedInPrincipal);
        Assert.Equal("entry", auth.SignedInPrincipal!.FindFirstValue(CurrentUser.UserNameClaim));
        Assert.Equal(AppUser.Roles.DataEntry, auth.SignedInPrincipal.FindFirstValue(ClaimTypes.Role));
    }

    [Fact]
    public async Task TheUserNameIsNotCaseSensitive()
    {
        await using var db = CreateDb();
        AddUser(db);
        var (page, auth) = CreateLoginPage(db);

        var result = await AttemptSignIn(page, "  ENTRY  ", GoodPassword);

        Assert.IsType<LocalRedirectResult>(result);
        Assert.NotNull(auth.SignedInPrincipal);
    }

    [Fact]
    public async Task AnApproverLandsOnTheApprovalQueue()
    {
        await using var db = CreateDb();
        AddUser(db, "approver", role: AppUser.Roles.Approver);
        var (page, _) = CreateLoginPage(db);

        var result = await AttemptSignIn(page, "approver", GoodPassword);

        Assert.Equal("/Approvals", Assert.IsType<LocalRedirectResult>(result).Url);
    }

    [Fact]
    public async Task TheWrongPasswordIsRejectedAndNobodyIsSignedIn()
    {
        await using var db = CreateDb();
        AddUser(db);
        var (page, auth) = CreateLoginPage(db);

        var result = await AttemptSignIn(page, "entry", "not-the-password");

        Assert.IsType<PageResult>(result);
        Assert.True(WasRejected(page));
        Assert.Null(auth.SignedInPrincipal);
        Assert.Equal(1, (await db.AppUsers.SingleAsync()).AccessFailedCount);
    }

    [Fact]
    public async Task AnUnknownUserNameIsRefusedInTheSameWordsAsAWrongPassword()
    {
        // The message must not distinguish the two, or the form becomes a way to find out
        // which usernames exist.
        await using var db = CreateDb();
        AddUser(db);

        var (knownUser, _) = CreateLoginPage(db);
        await AttemptSignIn(knownUser, "entry", "not-the-password");

        var (unknownUser, _) = CreateLoginPage(db);
        await AttemptSignIn(unknownUser, "nobody", "not-the-password");

        Assert.Equal(
            knownUser.ModelState[string.Empty]!.Errors.Single().ErrorMessage,
            unknownUser.ModelState[string.Empty]!.Errors.Single().ErrorMessage);
    }

    [Fact]
    public async Task ADeactivatedAccountCannotSignInEvenWithTheRightPassword()
    {
        await using var db = CreateDb();
        AddUser(db, isActive: false);
        var (page, auth) = CreateLoginPage(db);

        var result = await AttemptSignIn(page, "entry", GoodPassword);

        Assert.IsType<PageResult>(result);
        Assert.Null(auth.SignedInPrincipal);
    }

    // ---- Lockout -------------------------------------------------------------------------

    [Fact]
    public async Task FiveFailedAttemptsLockTheAccount()
    {
        await using var db = CreateDb();
        AddUser(db);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var (page, _) = CreateLoginPage(db);
            await AttemptSignIn(page, "entry", "not-the-password");
        }

        var stored = await db.AppUsers.SingleAsync();
        Assert.Equal(5, stored.AccessFailedCount);
        Assert.NotNull(stored.LockoutEndUtc);
        Assert.True(stored.LockoutEndUtc > DateTime.UtcNow);
    }

    [Fact]
    public async Task FourFailedAttemptsDoNotLockTheAccount()
    {
        // The boundary matters in both directions: a lockout that fired one attempt early
        // would be a support problem, and one that fired late would be a security one.
        await using var db = CreateDb();
        AddUser(db);

        for (var attempt = 0; attempt < 4; attempt++)
        {
            var (page, _) = CreateLoginPage(db);
            await AttemptSignIn(page, "entry", "not-the-password");
        }

        var stored = await db.AppUsers.SingleAsync();
        Assert.Equal(4, stored.AccessFailedCount);
        Assert.Null(stored.LockoutEndUtc);
    }

    [Fact]
    public async Task ALockedAccountIsRefusedEvenWithTheRightPassword()
    {
        await using var db = CreateDb();
        var user = AddUser(db);
        user.AccessFailedCount = 5;
        user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(15);
        await db.SaveChangesAsync();

        var (page, auth) = CreateLoginPage(db);
        var result = await AttemptSignIn(page, "entry", GoodPassword);

        Assert.IsType<PageResult>(result);
        Assert.Null(auth.SignedInPrincipal);
    }

    [Fact]
    public async Task OnceTheLockoutHasExpiredTheRightPasswordWorksAgain()
    {
        await using var db = CreateDb();
        var user = AddUser(db);
        user.AccessFailedCount = 5;
        user.LockoutEndUtc = DateTime.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();

        var (page, auth) = CreateLoginPage(db);
        var result = await AttemptSignIn(page, "entry", GoodPassword);

        Assert.IsType<LocalRedirectResult>(result);
        Assert.NotNull(auth.SignedInPrincipal);

        var stored = await db.AppUsers.SingleAsync();
        Assert.Null(stored.LockoutEndUtc);
        Assert.Equal(0, stored.AccessFailedCount);
    }

    [Fact]
    public async Task ASuccessfulSignInClearsEarlierFailedAttempts()
    {
        await using var db = CreateDb();
        AddUser(db);

        var (failed, _) = CreateLoginPage(db);
        await AttemptSignIn(failed, "entry", "not-the-password");

        var (succeeded, _) = CreateLoginPage(db);
        await AttemptSignIn(succeeded, "entry", GoodPassword);

        var stored = await db.AppUsers.SingleAsync();
        Assert.Equal(0, stored.AccessFailedCount);
        Assert.NotNull(stored.LastLoginAtUtc);
    }

    // ---- Ownership -----------------------------------------------------------------------

    private static RicCycle AddCycle(CostingDbContext db, string owner)
    {
        var cycle = new RicCycle
        {
            PlatformName = "Microscopy",
            StartYear = 2026,
            EndYear = 2028,
            BillableUnit = "Hours",
            Status = "Draft",
            CreatedBy = owner,
            CreatedByDisplay = owner
        };
        db.RicCycles.Add(cycle);
        db.SaveChanges();
        return cycle;
    }

    private static ReviewModel CreateReviewPage(CostingDbContext db, string signedInAs)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, signedInAs),
                new Claim(ClaimTypes.Role, AppUser.Roles.DataEntry),
                new Claim(CurrentUser.UserNameClaim, signedInAs)
            ],
            "TestAuth"));

        var services = new ServiceCollection()
            .AddSingleton<ITempDataProvider, NullTempDataProvider>()
            .AddSingleton<ITempDataDictionaryFactory, TempDataDictionaryFactory>()
            .BuildServiceProvider();

        return new ReviewModel(db, new RicCalculationService(new MethodConfigProvider(db)))
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext { User = principal, RequestServices = services }
            }
        };
    }

    [Fact]
    public async Task ACustodianCanOpenTheirOwnCycle()
    {
        await using var db = CreateDb();
        var cycle = AddCycle(db, owner: "entry");

        var result = await CreateReviewPage(db, signedInAs: "entry").OnGetAsync(cycle.Id);

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task ACustodianCannotOpenAnotherCustodiansCycle()
    {
        // NotFound, not Forbid: the same answer as a cycle that does not exist, so the URL
        // cannot be used to find out which cycles other people have.
        await using var db = CreateDb();
        var cycle = AddCycle(db, owner: "wenmin");

        var result = await CreateReviewPage(db, signedInAs: "entry").OnGetAsync(cycle.Id);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AMissingCycleAndSomeoneElsesCycleAnswerTheSameWay()
    {
        await using var db = CreateDb();
        var theirs = AddCycle(db, owner: "wenmin");

        var notMine = await CreateReviewPage(db, signedInAs: "entry").OnGetAsync(theirs.Id);
        var notThere = await CreateReviewPage(db, signedInAs: "entry").OnGetAsync(theirs.Id + 999);

        Assert.Equal(notThere.GetType(), notMine.GetType());
    }

    // ---- Test doubles --------------------------------------------------------------------

    /// <summary>
    /// Records the principal the page signs in, instead of writing a cookie. Everything
    /// else is a no-op: these tests are about the decision, not the cookie middleware.
    /// </summary>
    private sealed class FakeAuthenticationService : IAuthenticationService
    {
        public ClaimsPrincipal? SignedInPrincipal { get; private set; }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme) =>
            Task.FromResult(AuthenticateResult.NoResult());

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) =>
            Task.CompletedTask;

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) =>
            Task.CompletedTask;

        public Task SignInAsync(
            HttpContext context,
            string? scheme,
            ClaimsPrincipal principal,
            AuthenticationProperties? properties)
        {
            SignedInPrincipal = principal;
            return Task.CompletedTask;
        }

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties) =>
            Task.CompletedTask;
    }

    private sealed class NullTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object?> LoadTempData(HttpContext context) =>
            new Dictionary<string, object?>();

        public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
        {
        }
    }
}
