using System.Security.Claims;
using System.Text.Json;
using CostingTool.Data;
using CostingTool.Engine;
using CostingTool.Models;
using CostingTool.Pages.Ric;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ApproverDetails = CostingTool.Pages.Approvals.DetailsModel;

namespace CostingTool.Web.Tests;

/// <summary>
/// US-13: room to explain the costs section as a whole, the guide's Step 5 checklist read
/// from what has been written, and the costing assumptions carried into the sealed record.
///
/// The cycle is the one in <see cref="SealTests"/>: complete, priced at the calculated rates,
/// so nothing on it needs a justification and it can be submitted as it stands.
/// </summary>
public class CostingAssumptionsTests
{
    private const string Assumptions = "2026 budget as approved in March; salaries at the current EBA step plus 17% on-costs.";

    private static CostingDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<CostingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static async Task<RicCycle> ReadyCycle(CostingDbContext db, string status = "Draft", string? costingAssumptions = null)
    {
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
            Status = status,
            CreatedBy = "entry",
            CreatedByDisplay = "Priya Lal",
            UtilisationAssumptions = "From the 2025 booking export.",
            CostingAssumptions = costingAssumptions,
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
        db.ChangeTracker.Clear();
        return cycle;
    }

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

    private static ClaimsPrincipal Person(string userName, string displayName, string role) => new(new ClaimsIdentity(
        [new Claim(CurrentUser.UserNameClaim, userName), new Claim(ClaimTypes.Name, displayName), new Claim(ClaimTypes.Role, role)],
        "TestAuth"));

    private static CostsModel CostsPage(CostingDbContext db, int cycleId, string? assumptions, string? leavingFor = null) =>
        new(db)
        {
            PageContext = Context(Person("entry", "Priya Lal", AppUser.Roles.DataEntry)),
            CycleId = cycleId,
            CostingAssumptions = assumptions,
            LeavingFor = leavingFor
        };

    private static ReviewModel ReviewPage(CostingDbContext db) =>
        new(db, new RicCalculationService(new MethodConfigProvider(db)))
        {
            PageContext = Context(Person("entry", "Priya Lal", AppUser.Roles.DataEntry))
        };

    private static ApproverDetails ApproverPage(CostingDbContext db) =>
        new(db, new RicCalculationService(new MethodConfigProvider(db)))
        {
            PageContext = Context(Person("approver", "Dr Mei Chen", AppUser.Roles.Approver))
        };

    private static Task<RicCycle> Stored(CostingDbContext db) => db.RicCycles.AsNoTracking().SingleAsync();

    // ---- Written on the costs step -------------------------------------------------------

    [Fact]
    public async Task CostingAssumptionsAreSavedOnTheCostsStepAndCountAsAnEdit()
    {
        await using var db = CreateDb();
        var cycle = await ReadyCycle(db);

        var result = await CostsPage(db, cycle.Id, "  " + Assumptions + "\n").OnPostAssumptionsAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Ric/Costs", redirect.PageName);
        var stored = await Stored(db);
        Assert.Equal(Assumptions, stored.CostingAssumptions);
        Assert.Equal("entry", stored.LastEditedBy);
    }

    [Fact]
    public async Task EmptyingTheBoxClearsThem()
    {
        await using var db = CreateDb();
        var cycle = await ReadyCycle(db, costingAssumptions: Assumptions);

        await CostsPage(db, cycle.Id, "   ").OnPostAssumptionsAsync();

        Assert.Null((await Stored(db)).CostingAssumptions);
    }

    [Fact]
    public async Task SavingTheSameTextAgainIsNotAnEdit()
    {
        await using var db = CreateDb();
        var cycle = await ReadyCycle(db, costingAssumptions: Assumptions);

        await CostsPage(db, cycle.Id, Assumptions).OnPostAssumptionsAsync();

        Assert.Equal(string.Empty, (await Stored(db)).LastEditedBy);
    }

    [Fact]
    public async Task LeavingByALinkSavesThemAndGoesThere()
    {
        await using var db = CreateDb();
        var cycle = await ReadyCycle(db);
        var fundingStep = $"/Ric/Funding?cycleId={cycle.Id}";

        var result = await CostsPage(db, cycle.Id, Assumptions, leavingFor: fundingStep).OnPostAssumptionsAsync();

        Assert.Equal(fundingStep, Assert.IsType<LocalRedirectResult>(result).Url);
        Assert.Equal(Assumptions, (await Stored(db)).CostingAssumptions);
    }

    [Fact]
    public async Task LeavingForAnotherSiteStaysOnTheCostsStep()
    {
        await using var db = CreateDb();
        var cycle = await ReadyCycle(db);

        var result = await CostsPage(db, cycle.Id, Assumptions, leavingFor: "//example.com/").OnPostAssumptionsAsync();

        Assert.Equal("/Ric/Costs", Assert.IsType<RedirectToPageResult>(result).PageName);
    }

    [Fact]
    public async Task ASubmittedCyclesAssumptionsCannotBeChanged()
    {
        await using var db = CreateDb();
        var cycle = await ReadyCycle(db, status: "Submitted", costingAssumptions: Assumptions);

        var result = await CostsPage(db, cycle.Id, "Something else").OnPostAssumptionsAsync();

        Assert.Equal("/Ric/Review", Assert.IsType<RedirectToPageResult>(result).PageName);
        Assert.Equal(Assumptions, (await Stored(db)).CostingAssumptions);
    }

    [Fact]
    public async Task ARefusedCostLineStillShowsTheSavedAssumptions()
    {
        await using var db = CreateDb();
        var cycle = await ReadyCycle(db, costingAssumptions: Assumptions);
        var page = new CostsModel(db)
        {
            PageContext = Context(Person("entry", "Priya Lal", AppUser.Roles.DataEntry)),
            CycleId = cycle.Id,
            Scope = CostEntry.Scopes.Platform,
            Category = "Materials and supplies",
            YearAmounts = [-1m, -1m]
        };

        Assert.IsType<PageResult>(await page.OnPostAddAsync());
        Assert.Equal(Assumptions, page.CostingAssumptions);
    }

    // ---- The guide's Step 5 checklist, on the review -------------------------------------

    [Fact]
    public async Task TheChecklistShowsWhatIsOutstandingAndWhereToWriteIt()
    {
        await using var db = CreateDb();
        var cycle = await ReadyCycle(db);

        var page = ReviewPage(db);
        await page.OnGetAsync(cycle.Id);

        Assert.Collection(
            page.Step5,
            costing =>
            {
                Assert.Equal("Costing assumptions documented", costing.Label);
                Assert.False(costing.Done);
                Assert.Equal("/Ric/Costs", costing.StepPage);
            },
            utilisation =>
            {
                Assert.Equal("Utilisation assumptions documented", utilisation.Label);
                Assert.True(utilisation.Done);
            },
            benchmarking =>
            {
                Assert.Equal("Benchmarking recorded", benchmarking.Label);
                Assert.False(benchmarking.Done);
                Assert.Equal("/Ric/Rates", benchmarking.StepPage);
            });
    }

    [Fact]
    public async Task TheChecklistReportsButDoesNotStopSubmission()
    {
        await using var db = CreateDb();
        var cycle = await ReadyCycle(db);

        var result = await ReviewPage(db).OnPostSubmitAsync(cycle.Id, confirmAccuracy: true);

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("Submitted", (await Stored(db)).Status);
    }

    [Fact]
    public async Task TheChecklistIsNotShownOnceTheCycleIsSubmitted()
    {
        await using var db = CreateDb();
        var cycle = await ReadyCycle(db, status: "Submitted");

        var page = ReviewPage(db);
        await page.OnGetAsync(cycle.Id);

        Assert.Empty(page.Step5);
    }

    // ---- Into the sealed record ----------------------------------------------------------

    [Fact]
    public async Task TheSealedSnapshotCarriesTheCostingAssumptions()
    {
        await using var db = CreateDb();
        var cycle = await ReadyCycle(db, status: "Submitted", costingAssumptions: Assumptions);

        Assert.IsType<RedirectToPageResult>(await ApproverPage(db).OnPostApproveAsync(cycle.Id, confirmApproval: true));

        using var snapshot = JsonDocument.Parse((await Stored(db)).SnapshotJson!);
        Assert.Equal("1.5", snapshot.RootElement.GetProperty("SchemaVersion").GetString());
        Assert.Equal(Assumptions, snapshot.RootElement.GetProperty("Cycle").GetProperty("CostingAssumptions").GetString());
    }

    private sealed class NullTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
