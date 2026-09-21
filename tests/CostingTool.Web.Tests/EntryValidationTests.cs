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
/// US-18, "stop me breaking it": the entry mistakes the guided workflow must catch on the
/// server, whatever the browser did or did not check first.
/// </summary>
public class EntryValidationTests
{
    private static CostingDbContext CreateDb(int capabilities = 1)
    {
        var options = new DbContextOptionsBuilder<CostingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new CostingDbContext(options);
        db.RicCycles.Add(new RicCycle
        {
            PlatformName = "Microscopy",
            StartYear = 2026,
            EndYear = 2027,
            BillableUnit = "Hours",
            Status = "Draft",
            CreatedBy = "entry",
            CreatedByDisplay = "Priya Lal",
            Capabilities = Enumerable.Range(1, capabilities)
                .Select(i => new RicCapability
                {
                    Name = i == 1 ? "Cryo-EM" : $"Capability {i}",
                    MaximumCapacity = 1000m,
                    ForecastUwaUse = 500m,
                    ForecastApfrUse = 100m,
                    ForecastCommercialUse = 50m
                })
                .ToList()
        });

        db.SaveChanges();
        return db;
    }

    private static RicCycle Cycle(CostingDbContext db) =>
        db.RicCycles.Include(x => x.Capabilities).Single();

    private static PageContext SignedInAsEntry() => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(CurrentUser.UserNameClaim, "entry")], "TestAuth"))
        }
    };

    private static CostsModel CostLine(CostingDbContext db, string category, params decimal[] amounts)
    {
        var cycle = Cycle(db);
        return new CostsModel(db)
        {
            PageContext = SignedInAsEntry(),
            CycleId = cycle.Id,
            Scope = CostEntry.Scopes.Capability,
            CapabilityId = cycle.Capabilities.First().Id,
            Category = category,
            Description = "Service contract",
            PersonnelName = "Alex Tan",
            FundingType = "ARC Funded Position",
            YearAmounts = amounts.ToList()
        };
    }

    private static IEnumerable<string> Errors(PageModel page) =>
        page.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

    /// <summary>What model binding leaves behind when a field held text instead of a number.</summary>
    private static void PostedText(PageModel page, string key, string text)
    {
        page.ModelState.SetModelValue(key, text, text);
        page.ModelState.AddModelError(key, $"The value '{text}' is not valid for {key}.");
    }

    // ---- Text in a numeric field --------------------------------------------------------

    [Fact]
    public async Task TextInACostAmountIsRefusedNamingTheYearAndTheExpectedFormat()
    {
        await using var db = CreateDb();
        var page = CostLine(db, "Maintenance", 1_000m, 0m);
        PostedText(page, "YearAmounts[1]", "twenty thousand");

        var result = await page.OnPostAddAsync();

        Assert.IsType<PageResult>(result);
        var message = Assert.Single(Errors(page));
        Assert.Contains("2027 cost", message);
        Assert.Contains("\"twenty thousand\" is not a number", message);
        Assert.Contains("20000.00", message);
        Assert.Empty(db.RicCostEntries);
    }

    [Fact]
    public async Task TextInAFundingAmountIsRefusedNamingTheYear()
    {
        await using var db = CreateDb();
        var page = new FundingModel(db)
        {
            PageContext = SignedInAsEntry(),
            CycleId = Cycle(db).Id,
            Category = CostEntry.IncomeCategories.State,
            Description = "State grant",
            Justification = "Three-year commitment",
            YearAmounts = [10_000m, 0m]
        };
        PostedText(page, "YearAmounts[0]", "abc");

        var result = await page.OnPostAddAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(Errors(page), x => x.Contains("2026 funding") && x.Contains("\"abc\" is not a number"));
        Assert.Empty(db.RicCostEntries);
    }

    [Fact]
    public async Task TextInACapacityFieldNamesTheCapabilityAndTheField()
    {
        await using var db = CreateDb();
        var capability = Cycle(db).Capabilities.Single();
        var page = new CapacityModel(db)
        {
            PageContext = SignedInAsEntry(),
            CycleId = Cycle(db).Id,
            Inputs =
            [
                new CapacityModel.CapacityInput
                {
                    Id = capability.Id,
                    MaximumCapacity = 1000m,
                    UwaUse = 500m,
                    ApfrUse = 0m,
                    CommercialUse = 0m
                }
            ]
        };
        PostedText(page, "Inputs[0].ApfrUse", "lots");

        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(Errors(page), x => x.StartsWith("Cryo-EM: APFR forecast use: \"lots\" is not a number"));
    }

    // ---- Implausibly large amounts ------------------------------------------------------

    [Fact]
    public async Task TwoHundredThousandWhereTwentyThousandWasMeantAsksForConfirmation()
    {
        await using var db = CreateDb();
        var page = CostLine(db, "Maintenance", 20_000m, 200_000m);

        var result = await page.OnPostAddAsync();

        Assert.IsType<PageResult>(result);
        Assert.True(page.NeedsLargeAmountConfirmation);
        var message = Assert.Single(Errors(page));
        Assert.StartsWith("2027:", message);
        Assert.Contains("unusually large for Maintenance", message);
        Assert.Empty(db.RicCostEntries);
    }

    [Fact]
    public async Task AConfirmedLargeAmountIsSaved()
    {
        await using var db = CreateDb();
        var page = CostLine(db, "Maintenance", 20_000m, 200_000m);
        page.ConfirmLargeAmounts = true;

        var result = await page.OnPostAddAsync();

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal(110_000m, db.RicCostEntries.Single().Amount);
    }

    [Fact]
    public async Task AnOrdinaryAmountNeedsNoConfirmation()
    {
        await using var db = CreateDb();
        var page = CostLine(db, "Maintenance", 20_000m, 21_000m);

        var result = await page.OnPostAddAsync();

        Assert.IsType<RedirectToPageResult>(result);
        Assert.False(page.NeedsLargeAmountConfirmation);
        Assert.Single(db.RicCostEntries);
    }

    [Fact]
    public async Task ConfirmationIsNotOfferedWhileOtherErrorsRemain()
    {
        await using var db = CreateDb();
        var page = CostLine(db, "Maintenance", 200_000m, -1m);

        var result = await page.OnPostAddAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(page.NeedsLargeAmountConfirmation);
        Assert.Contains("Year amounts cannot be negative.", Errors(page));
    }

    [Fact]
    public async Task AnUnusuallyLargeFundingLineAsksForConfirmation()
    {
        await using var db = CreateDb();
        var page = new FundingModel(db)
        {
            PageContext = SignedInAsEntry(),
            CycleId = Cycle(db).Id,
            Category = CostEntry.IncomeCategories.State,
            Description = "State grant",
            Justification = "Three-year commitment",
            YearAmounts = [5_000_000m, 0m]
        };

        var result = await page.OnPostAddAsync();

        Assert.IsType<PageResult>(result);
        Assert.True(page.NeedsLargeAmountConfirmation);
        Assert.Empty(db.RicCostEntries);
    }

    // ---- Percentages and negative costs -------------------------------------------------

    [Theory]
    [InlineData(500, 17, "Percent worked must be between 0 and 100 percent.")]
    [InlineData(-10, 17, "Percent worked must be between 0 and 100 percent.")]
    [InlineData(100, 117, "Superannuation must be between 0 and 100 percent.")]
    public async Task APercentageOutsideZeroToOneHundredIsRefused(int percentWorked, int super, string expected)
    {
        await using var db = CreateDb();
        var page = CostLine(db, CostEntry.CostCategories.Personnel, 90_000m, 90_000m);
        page.PercentWorked = percentWorked;
        page.SuperannuationPercent = super;

        var result = await page.OnPostAddAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(expected, Errors(page));
        Assert.Empty(db.RicCostEntries);
    }

    [Fact]
    public async Task ANegativeCostIsRefused()
    {
        await using var db = CreateDb();
        var page = CostLine(db, "Travel", -500m, 0m);

        var result = await page.OnPostAddAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains("Year amounts cannot be negative.", Errors(page));
        Assert.Empty(db.RicCostEntries);
    }

    // ---- Aggregates over the same set ---------------------------------------------------

    [Fact]
    public void ThePlatformTotalIsSummedOverEveryLineExactlyOnce()
    {
        // N14: a platform total compared against capability figures must be built from the
        // same lines. Capability lines count once, against their own capability; a platform
        // line is shared out and must add back up to itself.
        using var db = CreateDb(capabilities: 3);
        var cycle = Cycle(db);
        var ids = cycle.Capabilities.Select(x => x.Id).ToList();

        db.RicCostEntries.AddRange(
            new RicCostEntry { RicCycleId = cycle.Id, RicCapabilityId = ids[0], Category = "Equipment", Amount = 10_000m },
            new RicCostEntry { RicCycleId = cycle.Id, RicCapabilityId = ids[1], Category = "Travel", Amount = 2_500m },
            new RicCostEntry { RicCycleId = cycle.Id, Scope = CostEntry.Scopes.Platform, Category = "Other", Amount = 100_000m });
        db.SaveChanges();

        var loaded = db.RicCycles
            .Include(x => x.Capabilities)
            .Include(x => x.Costs).ThenInclude(x => x.YearAmounts)
            .Single();

        var rates = new RicCalculationService(new MethodConfigProvider(db)).Calculate(loaded);

        Assert.True(rates.IsComplete);
        Assert.Equal(3, rates.All.Count);
        Assert.Equal(112_500m, Math.Round(rates.TotalOperatingCost, 2));
    }
}
