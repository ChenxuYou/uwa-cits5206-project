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
/// US-10 on step 1: the platform, period, unit and capabilities can be changed after the
/// cycle exists, without silently losing or distorting what later steps hold.
/// </summary>
public class StartEditTests
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
            Capabilities =
            [
                new RicCapability
                {
                    Name = "Cryo-EM",
                    CapacityBaseline = CapacityBaseline.Machine,
                    MaximumCapacity = 1_770m,
                    ForecastUwaUse = 500m,
                    ProposedUwaRate = 100m,
                    StaffFte = 0.5m,
                    CapacityDeductions = [new RicCapacityDeduction { Kind = "Maintenance", Amount = 112.5m, Note = "Service" }]
                },
                new RicCapability { Name = "Light microscopy" }
            ]
        });
        db.SaveChanges();

        var cycle = db.RicCycles.Include(x => x.Capabilities).Single();
        db.RicCostEntries.Add(new RicCostEntry
        {
            RicCycleId = cycle.Id,
            RicCapabilityId = cycle.Capabilities.First(x => x.Name == "Cryo-EM").Id,
            Scope = CostEntry.Scopes.Capability,
            Category = "Materials and supplies",
            Description = "Grids",
            Amount = 10_000m,
            YearAmounts = [new() { ProjectYear = 1, Amount = 10_000m }, new() { ProjectYear = 2, Amount = 10_000m }]
        });
        db.SaveChanges();
        db.ChangeTracker.Clear();

        return db;
    }

    private static RicCycle Cycle(CostingDbContext db)
    {
        db.ChangeTracker.Clear();
        return db.RicCycles
            .Include(x => x.Capabilities).ThenInclude(x => x.CapacityDeductions)
            .Include(x => x.Costs)
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

    /// <summary>The step as it opens on the cycle, for a test to change and post.</summary>
    private static async Task<StartModel> Opened(CostingDbContext db)
    {
        var page = new StartModel(db) { PageContext = SignedInAsEntry() };
        var result = await page.OnGetAsync(Cycle(db).Id);
        Assert.IsType<PageResult>(result);
        return page;
    }

    private static int CryoEm(CostingDbContext db) => Cycle(db).Capabilities.First(x => x.Name == "Cryo-EM").Id;

    [Fact]
    public async Task OpeningStepOneOnACycleShowsItsAnswers()
    {
        await using var db = CreateDb();

        var page = await Opened(db);

        Assert.True(page.IsEditing);
        Assert.Equal("Microscopy", page.PlatformName);
        Assert.Equal(2026, page.StartYear);
        Assert.Equal("Hours", page.BillableUnit);
        Assert.Equal(["Cryo-EM", "Light microscopy"], page.Existing.Select(x => x.Name ?? string.Empty).ToArray());
    }

    [Fact]
    public async Task RenamingAndAddingKeepsWhatWasEntered()
    {
        await using var db = CreateDb();
        var page = await Opened(db);
        page.PlatformName = "Centre for Microscopy";
        page.Existing.First(x => x.Name == "Cryo-EM").Name = "Cryo-electron microscopy";
        page.CapabilityNames = "Analysis / Consulting";

        var result = await page.OnPostAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Ric/Costs", redirect.PageName);
        var cycle = Cycle(db);
        Assert.Equal("Centre for Microscopy", cycle.PlatformName);
        Assert.Equal(3, cycle.Capabilities.Count);
        var renamed = cycle.Capabilities.Single(x => x.Name == "Cryo-electron microscopy");
        Assert.Equal(1_770m, renamed.MaximumCapacity);
        Assert.Single(cycle.Costs, x => x.RicCapabilityId == renamed.Id);
    }

    [Fact]
    public async Task RemovingACapabilityWithWorkAgainstItAsksFirst()
    {
        await using var db = CreateDb();
        var page = await Opened(db);
        page.Existing.First(x => x.Name == "Cryo-EM").Remove = true;

        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.True(page.NeedsRemovalConfirmation);
        Assert.Contains(Errors(page), x => x.StartsWith("Removing \"Cryo-EM\" also removes its 1 cost line"));
        Assert.Equal(2, Cycle(db).Capabilities.Count);
    }

    [Fact]
    public async Task RemovingACapabilityOnceConfirmedTakesItsCostLinesWithIt()
    {
        await using var db = CreateDb();
        var page = await Opened(db);
        page.Existing.First(x => x.Name == "Cryo-EM").Remove = true;
        page.ConfirmRemovals = true;

        var result = await page.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
        var cycle = Cycle(db);
        Assert.Equal(["Light microscopy"], cycle.Capabilities.Select(x => x.Name).ToArray());
        Assert.Empty(cycle.Costs);
        Assert.Empty(db.RicCapacityDeductions);
    }

    [Fact]
    public async Task RemovingACapabilityWithNothingAgainstItNeedsNoConfirmation()
    {
        await using var db = CreateDb();
        var page = await Opened(db);
        page.Existing.First(x => x.Name == "Light microscopy").Remove = true;

        var result = await page.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal(["Cryo-EM"], Cycle(db).Capabilities.Select(x => x.Name).ToArray());
    }

    [Fact]
    public async Task EveryCapabilityCannotBeRemoved()
    {
        await using var db = CreateDb();
        var page = await Opened(db);
        page.Existing.ForEach(x => x.Remove = true);
        page.ConfirmRemovals = true;

        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains("Keep or add at least one capability.", Errors(page));
    }

    [Fact]
    public async Task TwoCapabilitiesCannotShareAName()
    {
        await using var db = CreateDb();
        var page = await Opened(db);
        page.CapabilityNames = "cryo-em";

        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(Errors(page), x => x.Contains("appears more than once"));
    }

    [Fact]
    public async Task ThePeriodCanMoveButNotChangeLengthOnceLinesExist()
    {
        await using var db = CreateDb();

        var longer = await Opened(db);
        longer.EndYear = 2028;
        Assert.IsType<PageResult>(await longer.OnPostAsync());
        Assert.Contains(Errors(longer), x => x.StartsWith("The pricing period is 2 years long, and 1 cost or funding line"));

        var moved = await Opened(db);
        moved.StartYear = 2027;
        moved.EndYear = 2028;
        Assert.IsType<RedirectToPageResult>(await moved.OnPostAsync());
        Assert.Equal(2027, Cycle(db).StartYear);
        Assert.Equal(10_000m, Cycle(db).Costs.Single().Amount);
    }

    [Fact]
    public async Task ChangingTheUnitAsksFirstAndThenClearsFiguresInTheOldUnit()
    {
        await using var db = CreateDb();

        var unconfirmed = await Opened(db);
        unconfirmed.BillableUnit = "Days";
        Assert.IsType<PageResult>(await unconfirmed.OnPostAsync());
        Assert.True(unconfirmed.NeedsUnitChangeConfirmation);
        Assert.Equal("Hours", Cycle(db).BillableUnit);

        var confirmed = await Opened(db);
        confirmed.BillableUnit = "Days";
        confirmed.ConfirmUnitChange = true;
        Assert.IsType<RedirectToPageResult>(await confirmed.OnPostAsync());

        var cycle = Cycle(db);
        var capability = cycle.Capabilities.Single(x => x.Id == CryoEm(db));
        Assert.Equal("Days", cycle.BillableUnit);
        Assert.Equal(0m, capability.MaximumCapacity);
        Assert.Equal(0m, capability.ForecastUtilisation);
        Assert.Equal(0m, capability.ProposedUwaRate);
        Assert.Empty(capability.CapacityDeductions);
        Assert.Equal(0.5m, capability.StaffFte);
        Assert.Single(cycle.Costs);
    }

    [Fact]
    public async Task ASubmittedCycleIsNotReopenedForEditing()
    {
        await using var db = CreateDb();
        var cycle = db.RicCycles.Single();
        cycle.Status = "Submitted";
        db.SaveChanges();

        var page = new StartModel(db) { PageContext = SignedInAsEntry() };
        var result = await page.OnGetAsync(cycle.Id);

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Ric/Review", redirect.PageName);
    }

    [Fact]
    public async Task AnotherCustodiansCycleIsNotFound()
    {
        await using var db = CreateDb();
        var page = new StartModel(db)
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(CurrentUser.UserNameClaim, "someone-else")], "TestAuth"))
                }
            }
        };

        Assert.IsType<NotFoundResult>(await page.OnGetAsync(Cycle(db).Id));
    }
}
