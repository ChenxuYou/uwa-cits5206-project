using System.Security.Claims;
using System.Text.Json;
using CostingTool.Data;
using CostingTool.Models;
using CostingTool.Pages.Approvals;
using CostingTool.Pages.Ric;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CostingTool.Web.Tests;

/// <summary>
/// US-01 and F22: a new cycle is set against the last sealed record for its platform, and
/// replaces it by reference — never by editing it.
///
/// The sealed record here is sealed the way the application seals one, through the
/// approver's page, so the snapshot the new cycle reads is the snapshot the application
/// actually writes.
/// </summary>
public class SupersessionTests
{
    private static CostingDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<CostingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    /// <summary>A submitted cycle for <paramref name="owner"/>, then sealed by the approver.</summary>
    private static async Task<int> SealedCycle(CostingDbContext db, string owner = "entry", int? supersedes = null)
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
            PlatformName = "Microscopy",
            StartYear = 2026,
            EndYear = 2027,
            BillableUnit = "Hours",
            Status = "Submitted",
            CreatedBy = owner,
            CreatedByDisplay = owner,
            SupersedesCycleId = supersedes,
            Capabilities = [capability],
            Costs =
            [
                new RicCostEntry
                {
                    Capability = capability,
                    Scope = CostEntry.Scopes.Capability,
                    CostType = CostEntry.Types.Cost,
                    Category = "Materials and supplies",
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

    private static ClaimsPrincipal Custodian(string owner = "entry") => new(new ClaimsIdentity(
        [new Claim(CurrentUser.UserNameClaim, owner), new Claim(ClaimTypes.Name, "Priya Lal")], "TestAuth"));

    private static StartModel StartPage(CostingDbContext db) =>
        new(db) { PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = Custodian() } } };

    private static DetailsModel ApproverPage(CostingDbContext db)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, "Dana Approver"),
                new Claim(ClaimTypes.Role, AppUser.Roles.Approver),
                new Claim(CurrentUser.UserNameClaim, "approver")
            ],
            "TestAuth"));

        var services = new ServiceCollection()
            .AddSingleton<ITempDataProvider, NullTempDataProvider>()
            .AddSingleton<ITempDataDictionaryFactory, TempDataDictionaryFactory>()
            .BuildServiceProvider();

        return new DetailsModel(db, new RicCalculationService(new MethodConfigProvider(db)))
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext { User = principal, RequestServices = services }
            }
        };
    }

    private static IEnumerable<string> Errors(PageModel page) =>
        page.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

    /// <summary>Start the replacement of <paramref name="replaced"/> as the custodian would: open it, then continue.</summary>
    private static async Task<IActionResult> Replace(CostingDbContext db, int replaced)
    {
        var opened = StartPage(db);
        opened.Supersedes = replaced;
        await opened.OnGetAsync();

        var page = StartPage(db);
        page.Supersedes = replaced;
        page.PlatformName = opened.PlatformName;
        page.StartYear = opened.StartYear;
        page.EndYear = opened.EndYear;
        page.BillableUnit = opened.BillableUnit;
        page.CapabilityNames = opened.CapabilityNames;
        return await page.OnPostAsync();
    }

    [Fact]
    public async Task StartingFromASealedRecordCarriesItOverAndShowsItsFigures()
    {
        await using var db = CreateDb();
        var sealedId = await SealedCycle(db);

        var page = StartPage(db);
        page.Supersedes = sealedId;
        var result = await page.OnGetAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal("Microscopy", page.PlatformName);
        Assert.Equal("Hours", page.BillableUnit);
        Assert.Equal("Cryo-EM", page.CapabilityNames);
        Assert.Equal((2028, 2029), (page.StartYear, page.EndYear));

        // The figures alongside are the sealed ones, read from the snapshot.
        var previous = Assert.IsType<PreviousRecord>(page.Previous);
        Assert.Null(previous.Problem);
        Assert.Equal(150_000m, previous.TotalOperatingCost);
        var rates = Assert.IsType<PreviousRates>(previous.For("cryo-em"));
        Assert.Equal((100m, 162m, 202.50m), (rates.Uwa, rates.Apfr, rates.Commercial));
    }

    [Fact]
    public async Task TheNewCycleRefersToTheOldOneAndLeavesItUntouched()
    {
        await using var db = CreateDb();
        var sealedId = await SealedCycle(db);
        var before = await db.RicCycles.AsNoTracking().SingleAsync(x => x.Id == sealedId);

        var result = await Replace(db, sealedId);

        Assert.IsType<RedirectToPageResult>(result);
        var created = await db.RicCycles.AsNoTracking().SingleAsync(x => x.Id != sealedId);
        Assert.Equal(sealedId, created.SupersedesCycleId);
        Assert.Equal("Draft", created.Status);

        var after = await db.RicCycles.AsNoTracking().SingleAsync(x => x.Id == sealedId);
        Assert.Equal("Sealed", after.Status);
        Assert.Equal(before.SnapshotJson, after.SnapshotJson);
        Assert.Equal(before.SnapshotHash, after.SnapshotHash);
        Assert.Equal(before.UpdatedAtUtc, after.UpdatedAtUtc);
    }

    [Fact]
    public async Task ARecordIsNotReplacedTwiceAtOnce()
    {
        await using var db = CreateDb();
        var sealedId = await SealedCycle(db);
        Assert.IsType<RedirectToPageResult>(await Replace(db, sealedId));
        var underWay = await db.RicCycles.AsNoTracking().SingleAsync(x => x.SupersedesCycleId == sealedId);

        var page = StartPage(db);
        page.Supersedes = sealedId;
        await page.OnGetAsync();

        Assert.Equal(underWay.Id, page.ReplacementUnderWayId);
        Assert.Contains(Errors(page), x => x.Contains("already under way", StringComparison.Ordinal));
        Assert.Equal(2, await db.RicCycles.CountAsync());
    }

    [Theory]
    [InlineData("someone-else", "Sealed")]
    [InlineData("entry", "Draft")]
    public async Task OnlyTheCustodiansOwnSealedRecordCanBeReplaced(string owner, string status)
    {
        await using var db = CreateDb();

        // A sealed record cannot be turned back into a draft (US-15), so the draft is made as one.
        int id;
        if (status == "Sealed")
        {
            id = await SealedCycle(db, owner);
        }
        else
        {
            var draft = new RicCycle { PlatformName = "Microscopy", StartYear = 2026, EndYear = 2027, Status = status, CreatedBy = owner };
            db.RicCycles.Add(draft);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            id = draft.Id;
        }

        var page = StartPage(db);
        page.Supersedes = id;
        page.PlatformName = "Microscopy";
        page.CapabilityNames = "Cryo-EM";
        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains("A new cycle can only replace one of your own sealed records.", Errors(page));
        Assert.Equal(1, await db.RicCycles.CountAsync());
    }

    [Fact]
    public async Task ANewCycleForAPlatformWithASealedRecordIsAskedWhetherItReplacesIt()
    {
        await using var db = CreateDb();
        var sealedId = await SealedCycle(db);

        var page = StartPage(db);
        page.PlatformName = " microscopy ";
        page.CapabilityNames = "Cryo-EM";
        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.True(page.NeedsReplacementDecision);
        Assert.Equal(sealedId, page.Previous?.Id);
        Assert.Equal(1, await db.RicCycles.CountAsync());

        var confirmed = StartPage(db);
        confirmed.PlatformName = "Microscopy";
        confirmed.CapabilityNames = "Cryo-EM";
        confirmed.ConfirmNotReplacing = true;

        Assert.IsType<RedirectToPageResult>(await confirmed.OnPostAsync());
        Assert.Null((await db.RicCycles.AsNoTracking().SingleAsync(x => x.Id != sealedId)).SupersedesCycleId);
    }

    [Fact]
    public async Task SealingTheReplacementNamesTheOldRecordAndSupersedesIt()
    {
        await using var db = CreateDb();
        var oldId = await SealedCycle(db);
        var oldHash = (await db.RicCycles.AsNoTracking().SingleAsync(x => x.Id == oldId)).SnapshotHash;

        var newId = await SealedCycle(db, supersedes: oldId);

        var sealedNew = await db.RicCycles.AsNoTracking().SingleAsync(x => x.Id == newId);
        using var snapshot = JsonDocument.Parse(sealedNew.SnapshotJson!);
        Assert.Equal("1.5", snapshot.RootElement.GetProperty("SchemaVersion").GetString());
        var reference = snapshot.RootElement.GetProperty("Cycle").GetProperty("Supersedes");
        Assert.Equal(oldId, reference.GetProperty("Id").GetInt32());
        Assert.Equal(oldHash, reference.GetProperty("SnapshotHash").GetString());

        var overview = new CostingTool.Pages.IndexModel(db) { PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = Custodian() } } };
        await overview.OnGetAsync();
        Assert.Equal([oldId], overview.Superseded.ToArray());

        // Superseded, not replaceable: the next cycle replaces the newer record instead.
        var page = StartPage(db);
        await page.OnGetAsync();
        Assert.Equal([newId], page.Replaceable.Select(x => x.Id).ToArray());
    }

    /// <summary>TempData that remembers nothing, which is all these tests need of it.</summary>
    private sealed class NullTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
