using System.Security.Claims;
using CostingTool.Data;
using CostingTool.Models;
using CostingTool.Pages.Ric;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CostingTool.Web.Tests;

/// <summary>
/// US-10, US-11 and US-13 on the rates screen: a proposed rate that differs from the
/// calculated one has to be explained, and going back a step must not throw away what was
/// typed to get there.
///
/// The cycle these tests build is the client's worked example — $150,000 of cost, $20,000
/// UWA and $30,000 non-UWA income, 1,000 forecast hours — so the calculated rates are
/// $100.00, $162.00 and $202.50 and a proposal can be compared against a known figure.
/// </summary>
public class RateProposalTests
{
    private static DbContextOptions<CostingDbContext> Options(string name) =>
        new DbContextOptionsBuilder<CostingDbContext>().UseInMemoryDatabase(name).Options;

    private static (CostingDbContext Db, string Name) Seed()
    {
        var name = Guid.NewGuid().ToString();
        var db = new CostingDbContext(Options(name));

        var capability = new RicCapability
        {
            Name = "Cryo-EM",
            MaximumCapacity = 1_000m,
            ForecastUwaUse = 600m,
            ForecastApfrUse = 250m,
            ForecastCommercialUse = 150m
        };

        db.RicCycles.Add(new RicCycle
        {
            PlatformName = "Microscopy",
            StartYear = 2026,
            EndYear = 2027,
            BillableUnit = "Hours",
            Status = "Draft",
            CreatedBy = "entry",
            CreatedByDisplay = "Priya Lal",
            Capabilities = [capability],
            Costs =
            [
                new RicCostEntry
                {
                    Capability = capability,
                    Scope = CostEntry.Scopes.Capability,
                    CostType = CostEntry.Types.Cost,
                    Category = "Maintenance",
                    Amount = 150_000m
                },
                new RicCostEntry
                {
                    Capability = capability,
                    Scope = CostEntry.Scopes.Capability,
                    CostType = CostEntry.Types.Income,
                    Category = CostEntry.IncomeCategories.UwaGpInKind,
                    Amount = 20_000m
                },
                new RicCostEntry
                {
                    Capability = capability,
                    Scope = CostEntry.Scopes.Capability,
                    CostType = CostEntry.Types.Income,
                    Category = CostEntry.IncomeCategories.State,
                    Amount = 30_000m
                }
            ]
        });

        db.SaveChanges();
        return (db, name);
    }

    private static PageContext SignedInAsEntry() => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(CurrentUser.UserNameClaim, "entry")], "TestAuth"))
        }
    };

    private static RatesModel RatesPage(CostingDbContext db, decimal uwa, decimal apfr, decimal commercial, string? justification = null)
    {
        var cycle = db.RicCycles.Include(x => x.Capabilities).Single();

        return new RatesModel(db, new RicCalculationService(new MethodConfigProvider(db)))
        {
            PageContext = SignedInAsEntry(),
            CycleId = cycle.Id,
            PricingJustification = justification,
            Inputs =
            [
                new RatesModel.RateInput(cycle.Capabilities.Single().Id, uwa, apfr, commercial)
            ]
        };
    }

    private static IEnumerable<string> Errors(PageModel page) =>
        page.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

    private static RicCapability SavedCapability(string name)
    {
        using var db = new CostingDbContext(Options(name));
        return db.RicCycles.Include(x => x.Capabilities).Single().Capabilities.Single();
    }

    // ---- US-11: a variance has to be explained ------------------------------------------

    [Fact]
    public async Task ARateProposedBelowTheCalculatedOneIsNotSavedUntilItIsExplained()
    {
        var (db, name) = Seed();
        await using var owned = db;

        var page = RatesPage(db, uwa: 90m, apfr: 150m, commercial: 190m);
        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(Errors(page), x => x.Contains("differ from the calculated ones") && x.Contains("Cryo-EM"));
        Assert.Equal(0m, SavedCapability(name).ProposedUwaRate);
    }

    [Fact]
    public async Task TheSameRatesAreSavedOnceTheJustificationIsThere()
    {
        var (db, name) = Seed();
        await using var owned = db;

        var page = RatesPage(db, 90m, 150m, 190m,
            justification: "Rates held at last cycle's level while the new detector is commissioned.");

        var result = await page.OnPostAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Ric/Review", redirect.PageName);
        Assert.Equal(90m, SavedCapability(name).ProposedUwaRate);
    }

    [Fact]
    public async Task ProposingExactlyTheCalculatedRatesNeedsNoJustification()
    {
        var (db, name) = Seed();
        await using var owned = db;

        var page = RatesPage(db, 100.00m, 162.00m, 202.50m);
        var result = await page.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Empty(Errors(page));
        Assert.Equal(202.50m, SavedCapability(name).ProposedCommercialRate);
    }

    // ---- US-10: nothing is lost by going backwards --------------------------------------

    [Fact]
    public async Task GoingBackToCapacitySavesWhatWasTypedFirst()
    {
        var (db, name) = Seed();
        await using var owned = db;

        var page = RatesPage(db, 100.00m, 162.00m, 202.50m);
        page.BenchmarkNotes = "ANFF and Monash published rates, checked 18 September.";

        var result = await page.OnPostBackAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Ric/Capacity", redirect.PageName);
        Assert.Equal(162.00m, SavedCapability(name).ProposedApfrRate);
    }

    [Fact]
    public async Task GoingBackWithAnUnexplainedVarianceStopsAtTheSameQuestion()
    {
        var (db, name) = Seed();
        await using var owned = db;

        var page = RatesPage(db, 90m, 150m, 190m);
        var result = await page.OnPostBackAsync();

        Assert.IsType<PageResult>(result);
        Assert.Equal(0m, SavedCapability(name).ProposedUwaRate);
    }

    // ---- US-13: the utilisation assumptions are recorded, not assumed -------------------

    [Fact]
    public async Task CapacityCannotBeSavedWithoutSayingWhereTheForecastCameFrom()
    {
        var (db, _) = Seed();
        await using var owned = db;

        var page = CapacityPage(db, assumptions: null);
        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(Errors(page), x => x.Contains("how these forecast figures were arrived at"));
    }

    [Fact]
    public async Task TheUtilisationAssumptionsAreSavedWithTheFigures()
    {
        var (db, name) = Seed();
        await using var owned = db;

        var page = CapacityPage(db, "2025 bookings, less 15 maintenance days per instrument.");
        var result = await page.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);

        using var fresh = new CostingDbContext(Options(name));
        Assert.Equal(
            "2025 bookings, less 15 maintenance days per instrument.",
            fresh.RicCycles.Single().UtilisationAssumptions);
    }

    private static CapacityModel CapacityPage(CostingDbContext db, string? assumptions)
    {
        var cycle = db.RicCycles.Include(x => x.Capabilities).Single();

        return new CapacityModel(db)
        {
            PageContext = SignedInAsEntry(),
            CycleId = cycle.Id,
            UtilisationAssumptions = assumptions,
            Inputs =
            [
                new CapacityModel.CapacityInput(cycle.Capabilities.Single().Id, 1_000m, 600m, 250m, 150m)
            ]
        };
    }
}
