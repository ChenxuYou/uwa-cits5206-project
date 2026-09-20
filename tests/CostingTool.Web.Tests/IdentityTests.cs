using System.Security.Claims;
using CostingTool.Data;
using CostingTool.Models;
using CostingTool.Pages.Account;
using CostingTool.Pages.Admin;
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
/// US-19. The rules that decide who can sign in and what they can see were, until this
/// file, held in place by nothing: the merge gate ran the engine tests and the sign-in
/// path had no coverage at all. These cover the parts that would be quiet if they broke —
/// a password policy that stops rejecting, an ownership filter that stops filtering.
/// </summary>
public class IdentityTests
{
    private static CostingDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<CostingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CostingDbContext(options);
    }

    private static IPasswordHasher<AppUser> Hasher() =>
        new PasswordHasher<AppUser>(Options.Create(new PasswordHasherOptions
        {
            // The application configures 210,000 iterations. The tests use the default so
            // that the suite is not spending seconds hashing; what is under test here is
            // the decision, not the work factor.
            CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3
        }));

    private static AppUser AddUser(
        CostingDbContext db,
        string userName,
        string password,
        string role = AppUser.Roles.DataEntry)
    {
        var user = new AppUser
        {
            UserName = userName,
            DisplayName = userName,
            Role = role
        };
        user.PasswordHash = Hasher().HashPassword(user, password);
        db.AppUsers.Add(user);
        db.SaveChanges();
        return user;
    }

    private static RicCycle AddCycle(CostingDbContext db, string owner, string platformName)
    {
        var cycle = new RicCycle
        {
            PlatformName = platformName,
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

    private static void SignIn(PageModel model, AppUser user)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.DisplayName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(CurrentUser.UserNameClaim, user.UserName)
            ],
            "TestAuth"));

        // The success paths write a message to TempData, which a bare DefaultHttpContext
        // cannot supply. Providing it here keeps those paths testable.
        var services = new ServiceCollection()
            .AddSingleton<ITempDataProvider, NullTempDataProvider>()
            .AddSingleton<ITempDataDictionaryFactory, TempDataDictionaryFactory>()
            .BuildServiceProvider();

        model.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext { User = principal, RequestServices = services }
        };
    }

    /// <summary>TempData that remembers nothing, which is all these tests need of it.</summary>
    private sealed class NullTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object?> LoadTempData(HttpContext context) =>
            new Dictionary<string, object?>();

        public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
        {
        }
    }

    // ---- Password policy -----------------------------------------------------------------

    [Theory]
    [InlineData("Short1!")]                 // under twelve characters
    [InlineData("alllowercase123!")]        // no uppercase
    [InlineData("ALLUPPERCASE123!")]        // no lowercase
    [InlineData("NoDigitsAtAllHere!")]      // no number
    [InlineData("NoSymbolsAtAll123")]       // no symbol
    [InlineData("")]                        // nothing at all
    public void PasswordPolicyRejectsWeakPasswords(string candidate) =>
        Assert.False(PasswordPolicy.IsAcceptable(candidate));

    [Theory]
    [InlineData("Costing&Pricing2026")]
    [InlineData("Wombat-Telescope-7")]
    public void PasswordPolicyAcceptsAStrongPassword(string candidate) =>
        Assert.True(PasswordPolicy.IsAcceptable(candidate));

    // ---- Account administration ----------------------------------------------------------

    [Fact]
    public async Task AdministratorCannotCreateAnAccountWithAWeakPassword()
    {
        await using var db = CreateDb();
        var admin = AddUser(db, "admin", "Costing&Pricing2026", AppUser.Roles.Administrator);

        var model = new UsersModel(db, Hasher())
        {
            NewUserName = "wenmin",
            NewDisplayName = "Wenmin Luo",
            NewRole = AppUser.Roles.DataEntry,
            NewPassword = "password1"
        };
        SignIn(model, admin);

        var result = await model.OnPostCreateAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(await db.AppUsers.AnyAsync(x => x.UserName == "wenmin"));
    }

    [Fact]
    public async Task AdministratorCannotReuseAnExistingUserName()
    {
        await using var db = CreateDb();
        var admin = AddUser(db, "admin", "Costing&Pricing2026", AppUser.Roles.Administrator);
        AddUser(db, "entry", "Costing&Pricing2026");

        var model = new UsersModel(db, Hasher())
        {
            NewUserName = "ENTRY",
            NewDisplayName = "Someone Else",
            NewRole = AppUser.Roles.DataEntry,
            NewPassword = "Wombat-Telescope-7"
        };
        SignIn(model, admin);

        var result = await model.OnPostCreateAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal(2, await db.AppUsers.CountAsync());
    }

    [Fact]
    public async Task DeactivatingAnAccountRotatesItsSecurityStamp()
    {
        await using var db = CreateDb();
        var admin = AddUser(db, "admin", "Costing&Pricing2026", AppUser.Roles.Administrator);
        var custodian = AddUser(db, "entry", "Costing&Pricing2026");
        var stampBefore = custodian.SecurityStamp;

        var model = new UsersModel(db, Hasher());
        SignIn(model, admin);

        await model.OnPostToggleActiveAsync(custodian.Id);

        var stored = await db.AppUsers.SingleAsync(x => x.Id == custodian.Id);
        Assert.False(stored.IsActive);
        Assert.NotEqual(stampBefore, stored.SecurityStamp);
    }

    [Fact]
    public async Task AnAdministratorCannotDeactivateTheirOwnAccount()
    {
        await using var db = CreateDb();
        var admin = AddUser(db, "admin", "Costing&Pricing2026", AppUser.Roles.Administrator);

        var model = new UsersModel(db, Hasher());
        SignIn(model, admin);

        var result = await model.OnPostToggleActiveAsync(admin.Id);

        Assert.IsType<PageResult>(result);
        Assert.True((await db.AppUsers.SingleAsync(x => x.Id == admin.Id)).IsActive);
    }

    [Fact]
    public async Task ResettingAPasswordSignsTheAccountOutAndClearsItsLockout()
    {
        await using var db = CreateDb();
        var admin = AddUser(db, "admin", "Costing&Pricing2026", AppUser.Roles.Administrator);
        var custodian = AddUser(db, "entry", "Costing&Pricing2026");
        custodian.AccessFailedCount = 5;
        custodian.LockoutEndUtc = DateTime.UtcNow.AddMinutes(15);
        await db.SaveChangesAsync();
        var stampBefore = custodian.SecurityStamp;

        var model = new UsersModel(db, Hasher()) { ResetPassword = "Wombat-Telescope-7" };
        SignIn(model, admin);

        await model.OnPostResetPasswordAsync(custodian.Id);

        var stored = await db.AppUsers.SingleAsync(x => x.Id == custodian.Id);
        Assert.NotEqual(stampBefore, stored.SecurityStamp);
        Assert.Null(stored.LockoutEndUtc);
        Assert.Equal(0, stored.AccessFailedCount);
        Assert.Equal(
            PasswordVerificationResult.Success,
            Hasher().VerifyHashedPassword(stored, stored.PasswordHash, "Wombat-Telescope-7"));
    }

    // ---- Who sees what -------------------------------------------------------------------

    [Fact]
    public async Task AnAdministratorSeesEveryCustodiansCycles()
    {
        await using var db = CreateDb();
        var admin = AddUser(db, "admin", "Costing&Pricing2026", AppUser.Roles.Administrator);
        AddCycle(db, "entry", "Microscopy");
        AddCycle(db, "wenmin", "Mass spectrometry");

        var model = new CyclesModel(db);
        SignIn(model, admin);

        await model.OnGetAsync(status: null, owner: null);

        Assert.Equal(2, model.Cycles.Count);
        Assert.Contains(model.Cycles, x => x.CreatedBy == "entry");
        Assert.Contains(model.Cycles, x => x.CreatedBy == "wenmin");
    }

    [Fact]
    public async Task TheAdministratorsCycleListCanBeNarrowedToOneCustodian()
    {
        await using var db = CreateDb();
        var admin = AddUser(db, "admin", "Costing&Pricing2026", AppUser.Roles.Administrator);
        AddCycle(db, "entry", "Microscopy");
        AddCycle(db, "wenmin", "Mass spectrometry");

        var model = new CyclesModel(db);
        SignIn(model, admin);

        await model.OnGetAsync(status: null, owner: "wenmin");

        Assert.Equal("Mass spectrometry", Assert.Single(model.Cycles).PlatformName);
        Assert.Equal(2, model.TotalCount);
    }

    [Fact]
    public async Task AnAdministratorCanOpenAnyCycleReadOnly()
    {
        await using var db = CreateDb();
        var admin = AddUser(db, "admin", "Costing&Pricing2026", AppUser.Roles.Administrator);
        var cycle = AddCycle(db, "entry", "Microscopy");

        var model = new CycleDetailsModel(db, new RicCalculationService(new MethodConfigProvider(db)));
        SignIn(model, admin);

        var result = await model.OnGetAsync(cycle.Id);

        Assert.IsType<PageResult>(result);
        Assert.Equal("entry", model.Cycle.CreatedBy);

        // Read-only means read-only: the page offers no way to write to the record.
        Assert.DoesNotContain(
            typeof(CycleDetailsModel).GetMethods(),
            method => method.Name.StartsWith("OnPost", StringComparison.Ordinal));
    }

    // ---- Changing a password -------------------------------------------------------------

    [Fact]
    public async Task ChangingAPasswordRequiresTheCurrentOne()
    {
        await using var db = CreateDb();
        var custodian = AddUser(db, "entry", "Costing&Pricing2026");
        var hashBefore = custodian.PasswordHash;

        var model = new ChangePasswordModel(db, Hasher())
        {
            CurrentPassword = "the-wrong-one",
            NewPassword = "Wombat-Telescope-7",
            ConfirmPassword = "Wombat-Telescope-7"
        };
        SignIn(model, custodian);

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal(hashBefore, (await db.AppUsers.SingleAsync()).PasswordHash);
    }

    [Fact]
    public async Task ANewPasswordMustSatisfyThePolicy()
    {
        await using var db = CreateDb();
        var custodian = AddUser(db, "entry", "Costing&Pricing2026");
        var hashBefore = custodian.PasswordHash;

        var model = new ChangePasswordModel(db, Hasher())
        {
            CurrentPassword = "Costing&Pricing2026",
            NewPassword = "short",
            ConfirmPassword = "short"
        };
        SignIn(model, custodian);

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal(hashBefore, (await db.AppUsers.SingleAsync()).PasswordHash);
    }
}
