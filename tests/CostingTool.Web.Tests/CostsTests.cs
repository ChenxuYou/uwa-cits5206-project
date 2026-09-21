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
/// US-03 and US-04: costs by the client's workbook categories at capability and platform
/// level, floor area as an indirect cost, and running totals that reconcile.
/// </summary>
public class CostsTests
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
                .Select(i => new RicCapability { Name = i == 1 ? "Cryo-EM" : $"Capability {i}" })
                .ToList()
        });

        db.SaveChanges();
        return db;
    }

    private static RicCycle Cycle(CostingDbContext db) =>
        db.RicCycles
            .Include(x => x.Capabilities)
            .Include(x => x.Costs)
            .Single();

    private static PageContext SignedInAsEntry() => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(CurrentUser.UserNameClaim, "entry")], "TestAuth"))
        }
    };

    private static IEnumerable<string> Errors(PageModel page) =>
        page.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

    private static CostsModel CostLine(CostingDbContext db, string scope, string category, params decimal[] amounts)
    {
        var cycle = Cycle(db);
        return new CostsModel(db)
        {
            PageContext = SignedInAsEntry(),
            CycleId = cycle.Id,
            Scope = scope,
            CapabilityId = scope == CostEntry.Scopes.Capability ? cycle.Capabilities.First().Id : null,
            Category = category,
            Description = "Line",
            YearAmounts = amounts.ToList()
        };
    }

    // ---- US-03 / US-04: the client's categories, by where the cost sits -----------------

    [Fact]
    public void TheCategoriesAreTheWorkbooksForEachScope()
    {
        Assert.Equal(10, CostEntry.CostCategories.For(CostEntry.Scopes.Capability).Count);
        Assert.Contains("Decommissioning costs", CostEntry.CostCategories.For(CostEntry.Scopes.Capability));
        Assert.Contains("Cleaning and waste disposal", CostEntry.CostCategories.For(CostEntry.Scopes.Platform));
        Assert.Contains(CostEntry.CostCategories.LaboratoryFloorArea, CostEntry.CostCategories.For(CostEntry.Scopes.Platform));
        Assert.DoesNotContain("Cleaning and waste disposal", CostEntry.CostCategories.For(CostEntry.Scopes.Capability));
    }

    [Fact]
    public async Task APlatformLevelCategoryIsRefusedOnACapabilityLine()
    {
        await using var db = CreateDb();
        var page = CostLine(db, CostEntry.Scopes.Capability, "Cleaning and waste disposal", 5_000m, 5_000m);

        var result = await page.OnPostAddAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains("Select a cost category for the capability.", Errors(page));
        Assert.Empty(db.RicCostEntries);
    }

    [Fact]
    public async Task ACapabilityStaffLineSaysWhichRoleItIs()
    {
        await using var db = CreateDb();
        var page = CostLine(db, CostEntry.Scopes.Capability, CostEntry.CostCategories.EmployeeSalaryAndOnCosts, 90_000m, 90_000m);
        page.PersonnelName = "Alex Tan";
        page.FundingType = "ARC Funded Position";

        await page.OnPostAddAsync();
        Assert.Contains("Say whether this is the platform leader or a research officer.", Errors(page));

        page.ModelState.Clear();
        page.Position = CostEntry.Positions.ResearchOfficer;
        var result = await page.OnPostAddAsync();

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal(CostEntry.Positions.ResearchOfficer, db.RicCostEntries.Single().Position);
    }

    [Fact]
    public async Task FloorAreaIsCostedAsAreaTimesRateEveryYear()
    {
        // US-04: indirect cost, laboratory floor area at a rate per m² per annum [W, rows 40–41].
        await using var db = CreateDb();
        var page = CostLine(db, CostEntry.Scopes.Platform, CostEntry.CostCategories.LaboratoryFloorArea, 1m, 999_999m);
        page.FloorArea = 120m;
        page.FloorAreaRate = 450m;

        var result = await page.OnPostAddAsync();

        Assert.IsType<RedirectToPageResult>(result);
        var line = db.RicCostEntries.Include(x => x.YearAmounts).Single();
        Assert.Null(line.RicCapabilityId);
        Assert.Equal(54_000m, line.Amount);
        Assert.All(line.YearAmounts, x => Assert.Equal(54_000m, x.Amount));
        Assert.Equal(120m, line.FloorArea);
        Assert.Equal("Indirect", CostEntry.CostCategories.ClassOf(line.Scope, line.Category));
    }

    [Fact]
    public async Task FloorAreaNeedsBothTheAreaAndTheRate()
    {
        await using var db = CreateDb();
        var page = CostLine(db, CostEntry.Scopes.Platform, CostEntry.CostCategories.OfficeFloorArea);
        page.FloorAreaRate = 450m;

        await page.OnPostAddAsync();

        Assert.Contains("Enter the floor area in square metres, greater than zero.", Errors(page));
        Assert.Empty(db.RicCostEntries);
    }

    [Fact]
    public void RunningTotalsSplitPlatformCostsEvenlyAndReconcile()
    {
        // US-03 (N14) and US-04: each capability's own costs, its share of the platform's —
        // labelled as allocated — and capability totals that add back up to the platform's.
        using var db = CreateDb(capabilities: 3);
        var cycle = Cycle(db);
        var ids = cycle.Capabilities.Select(x => x.Id).ToList();

        db.RicCostEntries.AddRange(
            new RicCostEntry { RicCycleId = cycle.Id, RicCapabilityId = ids[0], Category = "Repairs and maintenance", Amount = 30_000m },
            new RicCostEntry { RicCycleId = cycle.Id, Scope = CostEntry.Scopes.Platform, Category = "Administration costs", Amount = 80_000m },
            new RicCostEntry { RicCycleId = cycle.Id, Scope = CostEntry.Scopes.Platform, Category = CostEntry.CostCategories.OfficeFloorArea, Amount = 20_000m },
            new RicCostEntry { RicCycleId = cycle.Id, Scope = CostEntry.Scopes.Platform, CostType = CostEntry.Types.Income, Category = CostEntry.IncomeCategories.State, Amount = 9_000m });
        db.SaveChanges();

        var costs = RicCalculationService.CostsOf(Cycle(db));

        Assert.Equal(3, costs.Divisor);
        Assert.Equal(80_000m, costs.DirectlyAllocated);
        Assert.Equal(20_000m, costs.Indirect);
        Assert.Equal(130_000m, costs.Total); // income is not an operating cost
        Assert.Equal(33_333.33m, Math.Round(costs.AllocatedToEach, 2));
        Assert.Equal(63_333.33m, Math.Round(costs.For(ids[0]).Total, 2));
        Assert.All(costs.Capabilities, x => Assert.Equal(costs.AllocatedToEach, x.Allocated));
        Assert.True(costs.Reconciles);
    }

    [Fact]
    public void TheEnginePricesFromTheSameRollUpTheCostsScreenShows()
    {
        using var db = CreateDb(capabilities: 2);
        var cycle = Cycle(db);
        foreach (var capability in cycle.Capabilities)
        {
            capability.ForecastUwaUse = 500m;
        }

        db.RicCostEntries.AddRange(
            new RicCostEntry { RicCycleId = cycle.Id, RicCapabilityId = cycle.Capabilities[0].Id, Category = "Utilities and rates", Amount = 12_000m },
            new RicCostEntry { RicCycleId = cycle.Id, Scope = CostEntry.Scopes.Platform, Category = CostEntry.CostCategories.LaboratoryFloorArea, Amount = 50_000m });
        db.SaveChanges();

        var loaded = Cycle(db);
        var costs = RicCalculationService.CostsOf(loaded);
        var rates = new RicCalculationService(new MethodConfigProvider(db)).Calculate(loaded);

        foreach (var capability in loaded.Capabilities)
        {
            var priced = rates.For(capability.Id)!;
            Assert.Equal(costs.For(capability.Id).DirectlyIncurred, priced.CapabilityOperatingCost);
            Assert.Equal(costs.For(capability.Id).Allocated, priced.AllocatedPlatformCost);
        }

        Assert.Equal(costs.Total, rates.TotalOperatingCost);
    }
}
