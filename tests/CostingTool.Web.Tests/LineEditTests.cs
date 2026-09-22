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
/// US-10 on the costs and funding steps: a recorded line can be opened, changed and saved in
/// place, rather than deleted and typed again.
/// </summary>
public class LineEditTests
{
    private static CostingDbContext CreateDb()
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
            Capabilities = [new RicCapability { Name = "Cryo-EM" }, new RicCapability { Name = "Light microscopy" }]
        });
        db.SaveChanges();

        var cycle = db.RicCycles.Include(x => x.Capabilities).Single();
        db.RicCostEntries.AddRange(
            new RicCostEntry
            {
                RicCycleId = cycle.Id,
                RicCapabilityId = cycle.Capabilities.First(x => x.Name == "Cryo-EM").Id,
                Scope = CostEntry.Scopes.Capability,
                CostType = CostEntry.Types.Cost,
                Category = "Materials and supplies",
                Description = "Grids",
                Supplier = "ProSciTech",
                Notes = "Two boxes a month",
                Amount = 11_000m,
                YearAmounts = [new() { ProjectYear = 1, Amount = 10_000m }, new() { ProjectYear = 2, Amount = 12_000m }]
            },
            new RicCostEntry
            {
                RicCycleId = cycle.Id,
                Scope = CostEntry.Scopes.Platform,
                CostType = CostEntry.Types.Income,
                Category = CostEntry.IncomeCategories.Federal,
                Description = "NCRIS operations",
                Supplier = "Australian Government",
                Notes = "Committed to 2028",
                Amount = 20_000m,
                YearAmounts = [new() { ProjectYear = 1, Amount = 20_000m }, new() { ProjectYear = 2, Amount = 20_000m }]
            });
        db.SaveChanges();
        db.ChangeTracker.Clear();

        return db;
    }

    private static RicCycle Cycle(CostingDbContext db)
    {
        db.ChangeTracker.Clear();
        return db.RicCycles
            .Include(x => x.Capabilities)
            .Include(x => x.Costs).ThenInclude(x => x.YearAmounts)
            .Single();
    }

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

    private static RicCostEntry CostLine(CostingDbContext db) => Cycle(db).Costs.Single(x => !x.IsIncome);

    private static RicCostEntry FundingLine(CostingDbContext db) => Cycle(db).Costs.Single(x => x.IsIncome);

    // ---- Costs -----------------------------------------------------------------------------

    [Fact]
    public async Task OpeningACostLineForChangePutsItsFiguresBackInTheForm()
    {
        await using var db = CreateDb();
        var line = CostLine(db);
        var page = new CostsModel(db) { PageContext = SignedInAsEntry() };

        var result = await page.OnGetAsync(line.RicCycleId, line.Id);

        Assert.IsType<PageResult>(result);
        Assert.Equal(line.Id, page.EditId);
        Assert.Equal(CostEntry.Scopes.Capability, page.Scope);
        Assert.Equal(line.RicCapabilityId, page.CapabilityId);
        Assert.Equal("Materials and supplies", page.Category);
        Assert.Equal("Grids", page.Description);
        Assert.Equal("Two boxes a month", page.Notes);
        Assert.Equal([10_000m, 12_000m], page.YearAmounts.ToArray());
    }

    [Fact]
    public async Task SavingAChangedCostLineReplacesItsFiguresInPlace()
    {
        await using var db = CreateDb();
        var line = CostLine(db);
        var other = Cycle(db).Capabilities.First(x => x.Name == "Light microscopy").Id;
        var page = new CostsModel(db) { PageContext = SignedInAsEntry() };
        await page.OnGetAsync(line.RicCycleId, line.Id);

        page.CapabilityId = other;
        page.YearAmounts = [14_000m, 16_000m];
        page.Notes = "Price rise from July";
        var result = await page.OnPostUpdateAsync();

        Assert.IsType<RedirectToPageResult>(result);
        var saved = CostLine(db);
        Assert.Equal(line.Id, saved.Id);
        Assert.Equal(other, saved.RicCapabilityId);
        Assert.Equal(15_000m, saved.Amount);
        Assert.Equal([14_000m, 16_000m], saved.YearAmounts.OrderBy(x => x.ProjectYear).Select(x => x.Amount).ToArray());
        Assert.Equal("Price rise from July", saved.Notes);
        Assert.Equal(2, db.RicCostYearAmounts.Count(x => x.RicCostEntryId == line.Id));
    }

    [Fact]
    public async Task AChangedCostLineIsHeldToTheSameChecksAsANewOne()
    {
        await using var db = CreateDb();
        var line = CostLine(db);
        var page = new CostsModel(db) { PageContext = SignedInAsEntry() };
        await page.OnGetAsync(line.RicCycleId, line.Id);

        page.Description = " ";
        page.YearAmounts = [-5m, 12_000m];
        var result = await page.OnPostUpdateAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains("Description is required.", Errors(page));
        Assert.Contains("Year amounts cannot be negative.", Errors(page));
        Assert.Equal(11_000m, CostLine(db).Amount);
    }

    [Fact]
    public async Task AFundingLineCannotBeOpenedAsACostLine()
    {
        await using var db = CreateDb();
        var funding = FundingLine(db);
        var page = new CostsModel(db) { PageContext = SignedInAsEntry() };

        Assert.IsType<NotFoundResult>(await page.OnGetAsync(funding.RicCycleId, funding.Id));
    }

    // ---- Funding ---------------------------------------------------------------------------

    [Fact]
    public async Task SavingAChangedFundingLineReplacesItsFiguresInPlace()
    {
        await using var db = CreateDb();
        var line = FundingLine(db);
        var page = new FundingModel(db) { PageContext = SignedInAsEntry() };
        await page.OnGetAsync(line.RicCycleId, line.Id);

        Assert.Equal(line.Id, page.EditId);
        Assert.Equal(CostEntry.IncomeCategories.Federal, page.Category);
        Assert.Equal("Committed to 2028", page.Justification);

        page.Category = CostEntry.IncomeCategories.State;
        page.YearAmounts = [25_000m, 15_000m];
        var result = await page.OnPostUpdateAsync();

        Assert.IsType<RedirectToPageResult>(result);
        var saved = FundingLine(db);
        Assert.Equal(line.Id, saved.Id);
        Assert.Equal(CostEntry.IncomeCategories.State, saved.Category);
        Assert.Equal(20_000m, saved.Amount);
        Assert.Equal([25_000m, 15_000m], saved.YearAmounts.OrderBy(x => x.ProjectYear).Select(x => x.Amount).ToArray());
        Assert.Single(Cycle(db).Costs, x => x.IsIncome);
    }

    [Fact]
    public async Task AChangedFundingLineStillNeedsAJustification()
    {
        await using var db = CreateDb();
        var line = FundingLine(db);
        var page = new FundingModel(db) { PageContext = SignedInAsEntry() };
        await page.OnGetAsync(line.RicCycleId, line.Id);

        page.Justification = "";
        var result = await page.OnPostUpdateAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(Errors(page), x => x.StartsWith("Justification is required"));
        Assert.Equal("Committed to 2028", FundingLine(db).Notes);
    }

    [Fact]
    public async Task ACostLineCannotBeSavedThroughTheFundingStep()
    {
        await using var db = CreateDb();
        var cost = CostLine(db);
        var page = new FundingModel(db)
        {
            PageContext = SignedInAsEntry(),
            CycleId = cost.RicCycleId,
            EditId = cost.Id,
            Category = CostEntry.IncomeCategories.State,
            Description = "Not funding",
            Justification = "x",
            YearAmounts = [1m, 1m]
        };

        Assert.IsType<NotFoundResult>(await page.OnPostUpdateAsync());
        Assert.False(CostLine(db).IsIncome);
    }
}
