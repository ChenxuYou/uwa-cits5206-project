using CostingTool.Data;
using CostingTool.Models;
using CostingTool.Pages.Ric;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;

namespace CostingTool.Web.Tests;

/// <summary>
/// "Update balance" on the Rates step (US-10, US-12): the balance is recalculated on the
/// server at the proposed rates the custodian has typed, and nothing is saved until they
/// continue. These tests pin the second half, because a "what if" that quietly writes to
/// a draft is worse than no preview.
/// </summary>
public class RatesPreviewTests
{
    private static DbContextOptions<CostingDbContext> CreateOptions()
    {
        var options = new DbContextOptionsBuilder<CostingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var db = new CostingDbContext(options);
        db.RicCycles.Add(new RicCycle
        {
            PlatformName = "Cytometry",
            StartYear = 2026,
            EndYear = 2028,
            BillableUnit = "Hours",
            Status = "Draft",
            CreatedBy = "entry",
            CreatedByDisplay = "Priya Lal",
            Capabilities =
            [
                new RicCapability
                {
                    Name = "Flow Cytometry",
                    MaximumCapacity = 1000m,
                    ForecastUwaUse = 500m,
                    ForecastApfrUse = 300m,
                    ForecastCommercialUse = 200m
                }
            ],
            Costs =
            [
                // One capability, so the whole platform line falls to it: C = $150,000.
                new RicCostEntry { Scope = CostEntry.Scopes.Platform, Category = "Other", Amount = 150000m }
            ]
        });
        db.SaveChanges();

        return options;
    }

    private static RatesModel CreateModel(CostingDbContext db, decimal uwa, decimal apfr, decimal commercial)
    {
        var cycle = db.RicCycles.Include(x => x.Capabilities).Single();

        var model = new RatesModel(db, new RicCalculationService(new MethodConfigProvider(db)))
        {
            CycleId = cycle.Id,
            Inputs = [new RatesModel.RateInput(cycle.Capabilities.Single().Id, uwa, apfr, commercial)]
        };

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(CurrentUser.UserNameClaim, "entry")],
            "TestAuth"));

        model.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        return model;
    }

    [Fact]
    public async Task PreviewRecalculatesAtTheTypedRatesWithoutSavingThem()
    {
        var options = CreateOptions();

        await using (var db = new CostingDbContext(options))
        {
            var model = CreateModel(db, 50m, 100m, 100m);

            var result = await model.OnPostPreviewAsync();

            Assert.IsType<PageResult>(result);
            Assert.True(model.IsPreview);

            // These rates vary from the calculated ones and no justification has been typed.
            // Saving would refuse that (RateProposalTests); a preview must not.
            Assert.True(model.ModelState.IsValid);

            // 500 x $50 + 300 x ($100 / 1.35) + 200 x ($100 / 1.35), against $150,000.
            Assert.Equal(62037.04m, Math.Round(model.Rates.ForecastRevenue, 2));
            Assert.Equal(-87962.96m, Math.Round(model.Rates.ForecastBalance, 2));
        }

        // A fresh context: the tracked entities in the first one were changed in memory.
        await using var reread = new CostingDbContext(options);
        var saved = reread.RicCycles.Include(x => x.Capabilities).Single().Capabilities.Single();

        Assert.Equal(0m, saved.ProposedUwaRate);
        Assert.Equal(0m, saved.ProposedApfrRate);
        Assert.Equal(0m, saved.ProposedCommercialRate);
    }

    [Fact]
    public async Task PreviewRefusesANegativeRateAndCalculatesNothing()
    {
        await using var db = new CostingDbContext(CreateOptions());
        var model = CreateModel(db, -1m, 100m, 100m);

        var result = await model.OnPostPreviewAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);
        Assert.False(model.IsPreview);
    }

    [Fact]
    public async Task PreviewOnASubmittedCycleGoesToTheReviewLikeASaveDoes()
    {
        var options = CreateOptions();

        await using var db = new CostingDbContext(options);
        db.RicCycles.Single().Status = "Submitted";
        await db.SaveChangesAsync();

        var model = CreateModel(db, 50m, 100m, 100m);

        var result = await model.OnPostPreviewAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Ric/Review", redirect.PageName);
    }

    [Fact]
    public async Task SavingStillPersistsTheProposedRates()
    {
        var options = CreateOptions();

        await using (var db = new CostingDbContext(options))
        {
            var model = CreateModel(db, 50m, 100m, 100m);

            // The rates vary from the calculated ones, so saving needs a justification (US-11).
            model.PricingJustification = "Priced to match the regional facility's published rates.";

            var result = await model.OnPostAsync();

            Assert.IsType<RedirectToPageResult>(result);
        }

        await using var reread = new CostingDbContext(options);
        var saved = reread.RicCycles.Include(x => x.Capabilities).Single().Capabilities.Single();

        Assert.Equal(50m, saved.ProposedUwaRate);
        Assert.Equal(100m, saved.ProposedApfrRate);
        Assert.Equal(100m, saved.ProposedCommercialRate);
    }
}
