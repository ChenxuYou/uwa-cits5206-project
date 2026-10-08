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
/// US-07 and US-08 on the capacity step: usable capacity built from a baseline, less noted
/// deductions and capped by staff, and forecast use judged against it.
/// </summary>
public class CapacityStepTests
{
    private static CostingDbContext CreateDb(string unit = "Hours")
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
            BillableUnit = unit,
            Status = "Draft",
            CreatedBy = "entry",
            CreatedByDisplay = "Priya Lal",
            Capabilities = [new RicCapability { Name = "Cryo-EM" }]
        });

        db.SaveChanges();
        return db;
    }

    private static RicCycle Cycle(CostingDbContext db) =>
        db.RicCycles
            .Include(x => x.Capabilities).ThenInclude(x => x.CapacityDeductions)
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

    private static CapacityModel CapacityPage(CostingDbContext db, Action<CapacityModel.CapacityInput> edit)
    {
        var cycle = Cycle(db);
        var input = new CapacityModel.CapacityInput { Id = cycle.Capabilities.Single().Id, UwaUse = 500m };
        edit(input);

        return new CapacityModel(db, new MethodConfigProvider(db))
        {
            PageContext = SignedInAsEntry(),
            CycleId = cycle.Id,
            UtilisationAssumptions = "2025 bookings and one new ARC grant.",
            Inputs = [input]
        };
    }

    private static void Deduct(CapacityModel.CapacityInput input, string kind, decimal amount, string? note)
    {
        var row = input.Deductions.Single(x => x.Kind == kind);
        row.Amount = amount;
        row.Note = note;
    }

    // ---- US-07: usable capacity from a stated baseline ----------------------------------

    [Fact]
    public async Task CapacityIsTheBaselineLessItsDeductionsAndIsItemised()
    {
        await using var db = CreateDb();
        var page = CapacityPage(db, x =>
        {
            x.Baseline = CapacityBaseline.Machine;
            Deduct(x, "Maintenance", 112.5m, "Quarterly service, 15 days");
            Deduct(x, "Planned outages", 75m, "Building shutdown in January");
        });

        var result = await page.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
        var saved = Cycle(db).Capabilities.Single();
        Assert.Equal(CapacityBaseline.Machine, saved.CapacityBaseline);
        Assert.Equal(1_695m, saved.MaximumCapacity); // 1,882.5 − 112.5 − 75
        Assert.Equal(["Maintenance", "Planned outages"], saved.CapacityDeductions.Select(x => x.Kind).OrderBy(x => x).ToArray());
        Assert.Equal("Quarterly service, 15 days", saved.CapacityDeductions.Single(x => x.Kind == "Maintenance").Note);
    }

    [Fact]
    public async Task EachDeductionTakesANote()
    {
        await using var db = CreateDb();
        var page = CapacityPage(db, x => Deduct(x, "Downtime", 40m, note: " "));

        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains("Cryo-EM: explain the downtime deduction.", Errors(page));
    }

    [Fact]
    public async Task AStaffReliantCapabilityIsCappedAtItsFte()
    {
        // Capability 7 in the workbook: 0.05 FTE × 1,725 h = 86.25 h [W, sheet 2 row 18].
        await using var db = CreateDb();
        var page = CapacityPage(db, x =>
        {
            x.IsStaffReliant = true;
            x.StaffFte = 0.05m;
            x.UwaUse = 80m;
        });

        var result = await page.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
        var saved = Cycle(db).Capabilities.Single();
        Assert.Equal(86.25m, saved.MaximumCapacity);
        Assert.True(saved.IsStaffReliant);
    }

    [Fact]
    public async Task AStatedBaselineNeedsToSayWhereItComesFrom()
    {
        await using var db = CreateDb();
        var page = CapacityPage(db, x =>
        {
            x.Baseline = CapacityBaseline.Stated;
            x.StatedBaseline = 1_000m;
        });

        await page.OnPostAsync();

        Assert.Contains("Cryo-EM: say where the stated baseline comes from.", Errors(page));
    }

    [Fact]
    public async Task SamplesHaveNoStandardBaselineAndNoStaffCap()
    {
        await using var db = CreateDb(unit: "Samples");
        var page = CapacityPage(db, x =>
        {
            x.Baseline = CapacityBaseline.Machine;
            x.IsStaffReliant = true;
            x.StaffFte = 0.5m;
        });

        await page.OnPostAsync();

        Assert.Contains(Errors(page), x => x.Contains("has no standard baseline"));
        Assert.Contains(Errors(page), x => x.Contains("cannot be applied to samples"));
    }

    [Fact]
    public async Task DeductionsThatTakeAllOfTheBaselineAreRefused()
    {
        await using var db = CreateDb();
        var page = CapacityPage(db, x => Deduct(x, "Downtime", 2_000m, "Instrument off site"));

        await page.OnPostAsync();

        Assert.Contains("Cryo-EM: the deductions take away all of the baseline, leaving no usable capacity.", Errors(page));
    }

    [Fact]
    public async Task ReopeningTheStepShowsWhatWasSaved()
    {
        await using var db = CreateDb();
        await CapacityPage(db, x =>
        {
            x.Baseline = CapacityBaseline.Staff;
            Deduct(x, "Setup and pack-down", 25m, "Half an hour per session");
        }).OnPostAsync();

        var page = new CapacityModel(db, new MethodConfigProvider(db)) { PageContext = SignedInAsEntry() };
        await page.OnGetAsync(Cycle(db).Id);

        var input = page.Inputs.Single();
        Assert.Equal(CapacityBaseline.Staff, input.Baseline);
        Assert.Equal(RicCapacityDeduction.Kinds.Length, input.Deductions.Count);
        Assert.Equal(25m, input.Deductions.Single(x => x.Kind == "Setup and pack-down").Amount);
        Assert.Equal(1_700m, page.Preview(input)!.Usable); // 1,725 − 25
    }

    // ---- US-10: nothing is lost by going backwards --------------------------------------

    [Fact]
    public async Task GoingBackToFundingSavesWhatWasTypedFirst()
    {
        await using var db = CreateDb();
        var page = CapacityPage(db, x =>
        {
            x.Baseline = CapacityBaseline.Machine;
            Deduct(x, "Maintenance", 112.5m, "Quarterly service, 15 days");
        });

        var result = await page.OnPostBackAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Ric/Funding", redirect.PageName);
        var saved = Cycle(db).Capabilities.Single();
        Assert.Equal(1_770m, saved.MaximumCapacity); // 1,882.5 − 112.5
        Assert.Equal(500m, saved.ForecastUwaUse);
        Assert.Equal("2025 bookings and one new ARC grant.", Cycle(db).UtilisationAssumptions);
    }

    [Fact]
    public async Task GoingBackWithAnUnexplainedDeductionStopsAtTheSameQuestion()
    {
        await using var db = CreateDb();
        var page = CapacityPage(db, x => Deduct(x, "Downtime", 40m, note: null));

        var result = await page.OnPostBackAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains("Cryo-EM: explain the downtime deduction.", Errors(page));
        Assert.Equal(0m, Cycle(db).Capabilities.Single().MaximumCapacity);
    }
}
