using System.Security.Claims;
using CostingTool.Data;
using CostingTool.Engine;
using CostingTool.Models;
using CostingTool.Pages.Records;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ApproverDetails = CostingTool.Pages.Approvals.DetailsModel;
using RecordDetails = CostingTool.Pages.Records.DetailsModel;
using RecordsIndex = CostingTool.Pages.Records.IndexModel;

namespace CostingTool.Web.Tests;

/// <summary>
/// US-17: "why does it cost $50 an hour?", answered from a record sealed years ago.
///
/// Records are sealed the way the application seals them, through the approver's page, so
/// the snapshot the register reads is the snapshot the application actually writes.
/// </summary>
public class SealedRecordsTests
{
    private static CostingDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<CostingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    /// <summary>A submitted cycle, then sealed by the approver. No income, so C / U = 150 an hour before k.</summary>
    private static async Task<int> Sealed(
        CostingDbContext db,
        string platform = "Microscopy",
        int startYear = 2026,
        string owner = "entry",
        int? supersedes = null)
    {
        var capability = new RicCapability
        {
            Name = "Cryo-EM",
            MaximumCapacity = 1_000m,
            ForecastUwaUse = 600m,
            ForecastApfrUse = 250m,
            ForecastCommercialUse = 150m,
            ProposedUwaRate = 100m,
            ProposedApfrRate = 162m,
            ProposedCommercialRate = 202.50m
        };

        var cycle = new RicCycle
        {
            PlatformName = platform,
            StartYear = startYear,
            EndYear = startYear + 1,
            BillableUnit = "Hours",
            Status = "Submitted",
            CreatedBy = owner,
            CreatedByDisplay = owner,
            SupersedesCycleId = supersedes,
            PricingJustification = "Set at full cost so the instrument can be replaced.",
            Capabilities = [capability],
            Costs =
            [
                new RicCostEntry
                {
                    Capability = capability,
                    Scope = CostEntry.Scopes.Capability,
                    CostType = CostEntry.Types.Cost,
                    Category = "Materials and supplies",
                    Description = "Grids and reagents",
                    Amount = 150_000m
                }
            ]
        };

        db.RicCycles.Add(cycle);
        await db.SaveChangesAsync();

        var approve = await ApproverPage(db).OnPostApproveAsync(cycle.Id, confirmApproval: true);
        Assert.IsType<RedirectToPageResult>(approve);
        db.ChangeTracker.Clear();

        return cycle.Id;
    }

    private static ClaimsPrincipal Person(string userName, string? role = null)
    {
        var claims = new List<Claim> { new(CurrentUser.UserNameClaim, userName), new(ClaimTypes.Name, userName) };
        if (role is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static ClaimsPrincipal Custodian(string owner = "entry") => Person(owner, AppUser.Roles.DataEntry);

    private static ClaimsPrincipal Administrator() => Person("admin", AppUser.Roles.Administrator);

    private static PageContext Context(ClaimsPrincipal user) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = user,
            RequestServices = new ServiceCollection()
                .AddSingleton<ITempDataProvider, NullTempDataProvider>()
                .AddSingleton<ITempDataDictionaryFactory, TempDataDictionaryFactory>()
                .BuildServiceProvider()
        }
    };

    private static ApproverDetails ApproverPage(CostingDbContext db) =>
        new(db, new RicCalculationService(new MethodConfigProvider(db)))
        {
            PageContext = Context(Person("approver", AppUser.Roles.Approver))
        };

    private static async Task<RecordsIndex> Register(CostingDbContext db, ClaimsPrincipal user, string? platform = null)
    {
        var page = new RecordsIndex(db) { PageContext = Context(user) };
        await page.OnGetAsync(platform);
        return page;
    }

    private static RecordDetails RecordPage(CostingDbContext db, ClaimsPrincipal user) =>
        new(db) { PageContext = Context(user) };

    [Fact]
    public async Task RecordsAreListedByPlatformAndPeriodWithTheirSealedDateAndState()
    {
        await using var db = CreateDb();
        var first = await Sealed(db, startYear: 2026);
        var second = await Sealed(db, startYear: 2028, supersedes: first);
        var genomics = await Sealed(db, platform: "Genomics", startYear: 2027);

        db.RicCycles.Add(new RicCycle { PlatformName = "Microscopy", StartYear = 2030, EndYear = 2031, CreatedBy = "entry" });
        await db.SaveChangesAsync();

        var page = await Register(db, Custodian());

        Assert.Equal(["Genomics", "Microscopy"], page.Platforms.Select(x => x.Name));

        var microscopy = page.Platforms.Single(x => x.Name == "Microscopy").Records;
        Assert.Equal([second, first], microscopy.Select(x => x.Id));
        Assert.All(microscopy, x => Assert.NotNull(x.SealedAtUtc));
        Assert.False(microscopy[0].IsSuperseded);
        Assert.True(microscopy[1].IsSuperseded);

        // The draft is not a record, so it is not in the register.
        Assert.Equal(3, page.Count);
        Assert.Contains(genomics, page.Platforms.Single(x => x.Name == "Genomics").Records.Select(x => x.Id));
    }

    [Fact]
    public async Task TheRegisterCanBeNarrowedToOnePlatform()
    {
        await using var db = CreateDb();
        await Sealed(db);
        await Sealed(db, platform: "Genomics");

        var page = await Register(db, Custodian(), platform: "genomics");

        Assert.Equal(["Genomics"], page.Platforms.Select(x => x.Name));
        Assert.Equal(["Genomics", "Microscopy"], page.PlatformNames);
    }

    [Fact]
    public async Task ACustodianSeesTheirOwnRecordsAndAnAdministratorSeesEveryone()
    {
        await using var db = CreateDb();
        var mine = await Sealed(db, owner: "entry");
        var theirs = await Sealed(db, owner: "other");

        var custodian = await Register(db, Custodian());
        var administrator = await Register(db, Administrator());

        Assert.Equal([mine], custodian.Platforms.SelectMany(x => x.Records).Select(x => x.Id));
        Assert.Equal(2, administrator.Count);

        Assert.IsType<NotFoundResult>(await RecordPage(db, Custodian()).OnGetAsync(theirs));
        Assert.IsType<PageResult>(await RecordPage(db, Administrator()).OnGetAsync(theirs));
    }

    [Fact]
    public async Task AnUnsealedCycleCannotBeOpenedAsARecord()
    {
        await using var db = CreateDb();
        var draft = new RicCycle { PlatformName = "Microscopy", StartYear = 2026, EndYear = 2027, CreatedBy = "entry" };
        db.RicCycles.Add(draft);
        await db.SaveChangesAsync();

        Assert.IsType<NotFoundResult>(await RecordPage(db, Custodian()).OnGetAsync(draft.Id));
    }

    [Fact]
    public async Task TheRecordShowsTheNumbersTheReasoningAndTheMethodVersion()
    {
        await using var db = CreateDb();
        var id = await Sealed(db);

        var page = RecordPage(db, Custodian());
        Assert.IsType<PageResult>(await page.OnGetAsync(id));

        var record = page.Record!;
        var capability = Assert.Single(record.Capabilities);

        Assert.Equal("2026.1", record.MethodVersion);
        Assert.Equal(1.35m, record.Method!.IndirectCostRecovery);
        Assert.Equal(150m, capability.Result!.DisplayUwaRate);
        Assert.Equal(202.50m, capability.Result.DisplayCommercialRate);
        Assert.Equal("(150000 / 1000) * 1.35 = 202.50", capability.Workings!.Commercial);
        Assert.Equal("Set at full cost so the instrument can be replaced.", record.Cycle!.PricingJustification);
        Assert.Equal("Grids and reagents", Assert.Single(record.Costs).Description);
        Assert.True(page.HashMatches);
    }

    [Fact]
    public async Task TheRecordIsUnchangedWhenTheMethodAndKChangeAfterwards()
    {
        await using var db = CreateDb();
        var id = await Sealed(db);

        // Years later: a new method with a different k becomes current. It may not reach a
        // record that was sealed before it [N6, N7]. (The rows themselves cannot be edited
        // once sealed — see SealTests.)
        db.MethodConfigs.Add(new MethodConfig
        {
            Version = "2029.1",
            EffectiveFromUtc = new DateTime(2029, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IndirectCostRecovery = 1.50m,
            RateDecimals = 2,
            IsCurrent = true
        });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var page = RecordPage(db, Custodian());
        await page.OnGetAsync(id);

        var capability = Assert.Single(page.Record!.Capabilities);
        Assert.Equal("2026.1", page.Record.MethodVersion);
        Assert.Equal(1.35m, page.Record.Method!.IndirectCostRecovery);
        Assert.Equal(202.50m, capability.Result!.DisplayCommercialRate);
        Assert.Equal("(150000 / 1000) * 1.35 = 202.50", capability.Workings!.Commercial);
        Assert.Equal(150_000m, Assert.Single(page.Record.Costs).Amount);
    }

    [Fact]
    public async Task ASupersededRecordNamesTheOneThatReplacedItAndStillOpens()
    {
        await using var db = CreateDb();
        var first = await Sealed(db, startYear: 2026);
        var second = await Sealed(db, startYear: 2028, supersedes: first);

        var old = RecordPage(db, Custodian());
        Assert.IsType<PageResult>(await old.OnGetAsync(first));
        Assert.True(old.IsSuperseded);
        Assert.Equal(second, old.ReplacedBy!.Id);

        var current = RecordPage(db, Custodian());
        await current.OnGetAsync(second);
        Assert.False(current.IsSuperseded);
        Assert.Equal(first, current.ReplacesId);
    }

    [Fact]
    public async Task ADamagedSnapshotIsReportedRatherThanRendered()
    {
        await using var db = CreateDb();
        // Damage happens beneath the application, which refuses to write to a sealed record,
        // so the damaged record is stored as such rather than edited into that state.
        var damaged = new RicCycle
        {
            PlatformName = "Microscopy",
            StartYear = 2026,
            EndYear = 2027,
            Status = "Sealed",
            CreatedBy = "entry",
            SealedAtUtc = DateTime.UtcNow,
            SnapshotJson = "{ not json",
            SnapshotHash = "0000"
        };
        db.RicCycles.Add(damaged);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var id = damaged.Id;

        var page = RecordPage(db, Custodian());
        Assert.IsType<PageResult>(await page.OnGetAsync(id));

        Assert.Null(page.Record);
        Assert.NotNull(page.ReadError);
        Assert.False(page.HashMatches);
    }

    [Fact]
    public async Task AnAdministratorCanDownloadTheSealedPdf()
    {
        await using var db = CreateDb();
        var id = await Sealed(db, owner: "other");

        var result = await RecordPage(db, Administrator()).OnGetPdfAsync(id);

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", file.ContentType);
        Assert.StartsWith("costing-record-microscopy-2026-2027-sealed-", file.FileDownloadName);
        Assert.NotEmpty(file.FileContents);
    }

    private sealed class NullTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
