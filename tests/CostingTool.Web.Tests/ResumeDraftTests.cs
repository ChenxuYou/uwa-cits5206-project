using System.Security.Claims;
using CostingTool.Data;
using CostingTool.Engine;
using CostingTool.Models;
using CostingTool.Pages.Ric;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CostingTool.Web.Tests;

/// <summary>
/// US-02: a draft can be left half-finished and picked up again — at the step it was left
/// on, with what was typed saved, and saying when it was last changed and by whom.
/// </summary>
public class ResumeDraftTests
{
    private static CostingDbContext CreateDb()
    {
        var db = new CostingDbContext(new DbContextOptionsBuilder<CostingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.RicCycles.Add(new RicCycle
        {
            PlatformName = "Microscopy",
            StartYear = 2026,
            EndYear = 2026,
            BillableUnit = "Hours",
            Status = "Draft",
            CreatedBy = "entry",
            CreatedByDisplay = "Priya Lal",
            LastEditedAtUtc = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            Capabilities =
            [
                new RicCapability
                {
                    Name = "Cryo-EM",
                    CapacityBaseline = CapacityBaseline.Machine,
                    MaximumCapacity = 1_000m,
                    ForecastUwaUse = 1_000m
                }
            ]
        });
        db.SaveChanges();
        db.ChangeTracker.Clear();
        return db;
    }

    private static RicCycle Cycle(CostingDbContext db)
    {
        db.ChangeTracker.Clear();
        return db.RicCycles.Include(x => x.Capabilities).Include(x => x.Costs).Single();
    }

    private static PageContext SignedIn() => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(CurrentUser.UserNameClaim, "entry"), new Claim(ClaimTypes.Name, "Priya Lal")], "TestAuth"))
        }
    };

    private static RatesModel RatesPage(CostingDbContext db) =>
        new(db, new RicCalculationService(new MethodConfigProvider(db))) { PageContext = SignedIn() };

    /// <summary>The rates step, filled in and justified, ready to be saved.</summary>
    private static RatesModel FilledRates(CostingDbContext db, string? leavingFor)
    {
        var cycle = Cycle(db);
        var page = RatesPage(db);
        page.CycleId = cycle.Id;
        page.Inputs = [new RatesModel.RateInput(cycle.Capabilities.Single().Id, 90m, 150m, 190m)];
        page.PricingJustification = "Held at last year's rates while the new instrument beds in.";
        page.LeavingFor = leavingFor;
        return page;
    }

    [Fact]
    public async Task OpeningAStepIsRememberedSoTheDraftReopensThere()
    {
        await using var db = CreateDb();
        var id = Cycle(db).Id;

        var page = new CapacityModel(db, new MethodConfigProvider(db)) { PageContext = SignedIn() };
        await page.OnGetAsync(id);

        var cycle = Cycle(db);
        Assert.Equal(4, cycle.LastStep);
        Assert.Equal("/Ric/Capacity", RicSteps.Get(cycle.LastStep).Page);

        // Opening a step is not an edit.
        Assert.Equal(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc), cycle.LastEditedAtUtc);
    }

    [Fact]
    public async Task ASubmittedCycleDoesNotMoveItsResumePoint()
    {
        await using var db = CreateDb();
        var cycle = await db.RicCycles.SingleAsync();
        cycle.Status = "Submitted";
        cycle.LastStep = 5;
        await db.SaveChangesAsync();

        await new CapacityModel(db, new MethodConfigProvider(db)) { PageContext = SignedIn() }.OnGetAsync(cycle.Id);

        Assert.Equal(5, Cycle(db).LastStep);
    }

    [Fact]
    public void AStepOutOfRangeStillResumesSomewhereReal()
    {
        Assert.Equal("/Ric/Start", RicSteps.Get(0).Page);
        Assert.Equal("/Ric/Review", RicSteps.Get(99).Page);
    }

    [Fact]
    public async Task SavingAFigureRecordsWhenAndByWhom()
    {
        await using var db = CreateDb();

        var result = await FilledRates(db, leavingFor: null).OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
        var cycle = Cycle(db);
        Assert.Equal(90m, cycle.Capabilities.Single().ProposedUwaRate);
        Assert.Equal("entry", cycle.LastEditedBy);
        Assert.Equal("Priya Lal", cycle.LastEditedByDisplay);
        Assert.True(cycle.LastEditedAtUtc > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task DeletingACostLineCountsAsAnEdit()
    {
        await using var db = CreateDb();
        var cycle = Cycle(db);
        db.RicCostEntries.Add(new RicCostEntry
        {
            RicCycleId = cycle.Id,
            RicCapabilityId = cycle.Capabilities.Single().Id,
            Scope = CostEntry.Scopes.Capability,
            Category = "Materials and supplies",
            Description = "Grids",
            Amount = 1_000m
        });
        await db.SaveChangesAsync();
        var lineId = Cycle(db).Costs.Single().Id;

        var page = new CostsModel(db) { PageContext = SignedIn(), CycleId = cycle.Id };
        var result = await page.OnPostDeleteAsync(lineId);

        Assert.IsType<RedirectToPageResult>(result);
        var after = Cycle(db);
        Assert.Empty(after.Costs);
        Assert.Equal("Priya Lal", after.LastEditedByDisplay);
    }

    [Fact]
    public async Task FollowingALinkWithUnsavedFiguresSavesThemAndThenGoesThere()
    {
        await using var db = CreateDb();

        var result = await FilledRates(db, leavingFor: "/Ric/Funding?cycleId=1").OnPostAsync();

        var redirect = Assert.IsType<LocalRedirectResult>(result);
        Assert.Equal("/Ric/Funding?cycleId=1", redirect.Url);
        Assert.Equal(150m, Cycle(db).Capabilities.Single().ProposedApfrRate);
    }

    [Theory]
    [InlineData("//elsewhere.example/phish")]
    [InlineData("/\\elsewhere.example")]
    [InlineData("https://elsewhere.example/")]
    public async Task ALinkOffTheSiteIsNotFollowedAfterSaving(string leavingFor)
    {
        await using var db = CreateDb();

        var result = await FilledRates(db, leavingFor).OnPostAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Ric/Review", redirect.PageName);
    }

    [Fact]
    public async Task ASaveOnTheWayOutThatIsRefusedKeepsThePageOpen()
    {
        await using var db = CreateDb();
        var page = FilledRates(db, leavingFor: "/");
        page.Inputs[0].Uwa = -1m;

        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal(0m, Cycle(db).Capabilities.Single().ProposedUwaRate);
    }
}
