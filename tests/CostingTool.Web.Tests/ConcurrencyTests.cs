using System.Security.Claims;
using CostingTool.Data;
using CostingTool.Engine;
using CostingTool.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ApproverDetails = CostingTool.Pages.Approvals.DetailsModel;

namespace CostingTool.Web.Tests;

/// <summary>
/// C3 in the audit: a request that read a cycle before someone else sealed it must not be
/// able to write to it afterwards.
///
/// These run on SQLite, not the in-memory provider, because what is under test is the
/// UPDATE's WHERE clause on the concurrency stamp. Two contexts share one connection, so
/// each is a separate request looking at the same database.
/// </summary>
public sealed class ConcurrencyTests : IDisposable
{
    private readonly SqliteConnection connection = new("DataSource=:memory:");

    public ConcurrencyTests()
    {
        connection.Open();
        using var db = Context();
        db.Database.EnsureCreated();
    }

    public void Dispose() => connection.Dispose();

    private CostingDbContext Context() =>
        new(new DbContextOptionsBuilder<CostingDbContext>().UseSqlite(connection).Options);

    private async Task<int> SubmittedCycle()
    {
        await using var db = Context();
        var capability = new RicCapability
        {
            Name = "Cryo-EM",
            MaximumCapacity = 1_000m,
            CapacityBaseline = CapacityBaseline.Stated,
            StatedBaseline = 1_000m,
            ForecastUwaUse = 600m,
            ForecastApfrUse = 250m,
            ForecastCommercialUse = 150m,
            ProposedUwaRate = 150m,
            ProposedApfrRate = 202.50m,
            ProposedCommercialRate = 202.50m
        };
        var cycle = new RicCycle
        {
            PlatformName = "Microscopy",
            StartYear = 2026,
            EndYear = 2027,
            BillableUnit = "Hours",
            Status = "Submitted",
            CreatedBy = "entry",
            CreatedByDisplay = "Priya Lal",
            UtilisationAssumptions = "From the 2025 booking export.",
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
        return cycle.Id;
    }

    private static PageContext Signed(string userName, string displayName) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(CurrentUser.UserNameClaim, userName), new Claim(ClaimTypes.Name, displayName), new Claim(ClaimTypes.Role, AppUser.Roles.Approver)],
                "TestAuth")),
            RequestServices = new ServiceCollection()
                .AddSingleton<ITempDataProvider, NullTempDataProvider>()
                .AddSingleton<ITempDataDictionaryFactory, TempDataDictionaryFactory>()
                .BuildServiceProvider()
        }
    };

    private static ApproverDetails ApproverPage(CostingDbContext db, string userName = "approver", string displayName = "Dr Mei Chen") =>
        new(db, new RicCalculationService(new MethodConfigProvider(db))) { PageContext = Signed(userName, displayName) };

    private async Task<RicCycle> Stored(int id)
    {
        await using var db = Context();
        return await db.RicCycles.AsNoTracking().SingleAsync(x => x.Id == id);
    }

    [Fact]
    public async Task EverySaveOfACycleChangesItsStamp()
    {
        var id = await SubmittedCycle();
        var before = (await Stored(id)).ConcurrencyStamp;

        await using (var db = Context())
        {
            var cycle = await db.RicCycles.SingleAsync(x => x.Id == id);
            cycle.ApprovalComment = "noted";
            await db.SaveChangesAsync();
        }

        Assert.NotEqual(before, (await Stored(id)).ConcurrencyStamp);
    }

    [Fact]
    public async Task ARequestThatReadTheCycleBeforeItWasSealedCannotOverwriteTheSeal()
    {
        var id = await SubmittedCycle();

        await using var late = Context();
        var stale = await late.RicCycles.SingleAsync(x => x.Id == id);   // read while Submitted

        await using (var first = Context())
        {
            Assert.IsType<RedirectToPageResult>(await ApproverPage(first).OnPostApproveAsync(id, confirmApproval: true));
        }

        var sealedRecord = await Stored(id);
        stale.SnapshotJson = "{}";
        stale.SnapshotHash = "OVERWRITTEN";
        stale.SealedBy = "Someone else";

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => late.SaveChangesAsync());

        var after = await Stored(id);
        Assert.Equal("Sealed", after.Status);
        Assert.Equal(sealedRecord.SnapshotHash, after.SnapshotHash);
        Assert.Equal("Dr Mei Chen", after.SealedBy);
    }

    [Fact]
    public async Task ASecondApproverReturningASealedCycleIsToldAndChangesNothing()
    {
        var id = await SubmittedCycle();

        // The second approver's request has already read the cycle, as Submitted, when the
        // first approver's seal lands. A tracked entity is not refreshed by a later query, so
        // the handler below sees what the request originally read.
        await using var second = Context();
        await second.RicCycles.SingleAsync(x => x.Id == id);

        await using (var first = Context())
        {
            Assert.IsType<RedirectToPageResult>(await ApproverPage(first).OnPostApproveAsync(id, confirmApproval: true));
        }

        var page = ApproverPage(second, "approver2", "Prof Ana Ruiz");
        page.ReturnReason = "Please check the capacity figures.";
        var result = await page.OnPostReturnAsync(id);

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Contains("was not saved", page.TempData["Error"] as string, StringComparison.Ordinal);
        var after = await Stored(id);
        Assert.Equal("Sealed", after.Status);
        Assert.Null(after.ReturnReason);

        await using var check = Context();
        Assert.Empty(await check.AppNotifications.Where(x => x.Type == "Returned").ToListAsync());
    }

    [Fact]
    public async Task ADoubleClickedApprovalSealsOnce()
    {
        var id = await SubmittedCycle();

        await using var secondClick = Context();
        await secondClick.RicCycles.SingleAsync(x => x.Id == id);

        await using (var firstClick = Context())
        {
            await ApproverPage(firstClick).OnPostApproveAsync(id, confirmApproval: true);
        }

        var sealedOnce = await Stored(id);
        var page = ApproverPage(secondClick);
        await page.OnPostApproveAsync(id, confirmApproval: true);

        var after = await Stored(id);
        Assert.Equal(sealedOnce.SnapshotHash, after.SnapshotHash);
        Assert.Equal(sealedOnce.SealedAtUtc, after.SealedAtUtc);
        Assert.NotNull(page.TempData["Error"]);
    }

    private sealed class NullTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
