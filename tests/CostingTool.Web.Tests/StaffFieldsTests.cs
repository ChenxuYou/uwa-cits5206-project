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
/// The staff fields of a cost line, as changed after the client tested staging (feedback of
/// 9 October 2026): professional Levels 1–10 alongside academic Levels A–E, "LG funded" and
/// "GP funded" as funding types, no low or high cost school, and a base salary the custodian
/// enters, from which the form fills each year.
/// </summary>
public class StaffFieldsTests
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
            Capabilities = [new RicCapability { Name = "Cryo-EM", MaximumCapacity = 1000m }]
        });

        db.SaveChanges();
        return db;
    }

    private static PageContext SignedInAsEntry() => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(CurrentUser.UserNameClaim, "entry")], "TestAuth"))
        }
    };

    private static CostsModel StaffLine(CostingDbContext db, string staffType, string salaryScale, string fundingType = "ARC Funded Position")
    {
        var cycle = db.RicCycles.Include(x => x.Capabilities).Single();
        return new CostsModel(db)
        {
            PageContext = SignedInAsEntry(),
            CycleId = cycle.Id,
            Scope = CostEntry.Scopes.Capability,
            CapabilityId = cycle.Capabilities.Single().Id,
            Category = CostEntry.CostCategories.EmployeeSalaryAndOnCosts,
            Position = CostEntry.Positions.ResearchOfficer,
            PersonnelName = "Alex Tan",
            FundingType = fundingType,
            StaffType = staffType,
            SalaryScale = salaryScale,
            SalaryStep = "02",
            YearAmounts = [90_000m, 90_000m]
        };
    }

    private static IEnumerable<string> Errors(PageModel page) =>
        page.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

    [Fact]
    public void ProfessionalStaffAreOfferedLevelsOneToTenAndAcademicStaffLevelsAToE()
    {
        Assert.Equal(
            ["LVL1", "LVL2", "LVL3", "LVL4", "LVL5", "LVL6", "LVL7", "LVL8", "LVL9", "LVL10"],
            CostEntry.SalaryScales.For(CostEntry.SalaryScales.Professional).ToArray());
        Assert.Equal(
            ["LVLA", "LVLB", "LVLC", "LVLD", "LVLE"],
            CostEntry.SalaryScales.For(CostEntry.SalaryScales.Academic).ToArray());
    }

    [Fact]
    public async Task AProfessionalStaffLineAtLevelSevenIsSaved()
    {
        await using var db = CreateDb();
        var page = StaffLine(db, CostEntry.SalaryScales.Professional, "LVL7");

        var result = await page.OnPostAddAsync();

        Assert.IsType<RedirectToPageResult>(result);
        var line = db.RicCostEntries.Single();
        Assert.Equal(CostEntry.SalaryScales.Professional, line.StaffType);
        Assert.Equal("LVL7", line.SalaryScale);
    }

    [Theory]
    [InlineData("Academic", "LVL7", "Academic staff take a salary level from A to E.")]
    [InlineData("Professional", "LVLA", "Professional staff take a salary level from 1 to 10.")]
    [InlineData("Professional", "LVL11", "Professional staff take a salary level from 1 to 10.")]
    public async Task ALevelFromTheOtherScaleIsRefused(string staffType, string salaryScale, string message)
    {
        await using var db = CreateDb();
        var page = StaffLine(db, staffType, salaryScale);

        var result = await page.OnPostAddAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(message, Errors(page));
        Assert.Empty(db.RicCostEntries);
    }

    [Theory]
    [InlineData("LG funded")]
    [InlineData("GP funded")]
    public async Task LgAndGpFundedPositionsAreAccepted(string fundingType)
    {
        await using var db = CreateDb();
        var page = StaffLine(db, CostEntry.SalaryScales.Academic, "LVLB", fundingType);

        var result = await page.OnPostAddAsync();

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal(fundingType, db.RicCostEntries.Single().FundingType);
    }

    [Fact]
    public async Task AFundingTypeTheFormDoesNotOfferIsRefused()
    {
        await using var db = CreateDb();
        var page = StaffLine(db, CostEntry.SalaryScales.Academic, "LVLB", "Philanthropic");

        var result = await page.OnPostAddAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains("Select one of the listed funding types.", Errors(page));
        Assert.Empty(db.RicCostEntries);
    }

    [Fact]
    public async Task ALineSavedWithASchoolTypeLosesItWhenChanged()
    {
        await using var db = CreateDb();
        await StaffLine(db, CostEntry.SalaryScales.Academic, "LVLB").OnPostAddAsync();
        var line = db.RicCostEntries.Single();
        line.SchoolType = "High cost school";
        db.SaveChanges();

        var page = new CostsModel(db) { PageContext = SignedInAsEntry() };
        await page.OnGetAsync(line.RicCycleId, line.Id);
        page.YearAmounts = [95_000m, 95_000m];
        var result = await page.OnPostUpdateAsync();

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Null(db.RicCostEntries.Single().SchoolType);
    }

    [Fact]
    public async Task ANewStaffLineStartsWithNoBaseSalary()
    {
        await using var db = CreateDb();
        var page = new CostsModel(db) { PageContext = SignedInAsEntry() };

        await page.OnGetAsync(db.RicCycles.Single().Id);

        Assert.Null(page.BaseSalary);
    }

    [Fact]
    public async Task AStaffLineWithoutABaseSalaryIsSavedWithoutOne()
    {
        await using var db = CreateDb();

        await StaffLine(db, CostEntry.SalaryScales.Academic, "LVLB").OnPostAddAsync();

        Assert.Null(db.RicCostEntries.Single().BaseSalary);
    }

    [Fact]
    public async Task AnEnteredBaseSalaryIsSavedAndComesBackWhenTheLineIsChanged()
    {
        await using var db = CreateDb();
        var add = StaffLine(db, CostEntry.SalaryScales.Professional, "LVL6");
        add.BaseSalary = 92_500m;
        await add.OnPostAddAsync();
        var line = db.RicCostEntries.Single();

        var page = new CostsModel(db) { PageContext = SignedInAsEntry() };
        await page.OnGetAsync(line.RicCycleId, line.Id);

        Assert.Equal(92_500m, line.BaseSalary);
        Assert.Equal(92_500m, page.BaseSalary);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ABaseSalaryOfZeroOrLessIsRefused(decimal baseSalary)
    {
        await using var db = CreateDb();
        var page = StaffLine(db, CostEntry.SalaryScales.Academic, "LVLB");
        page.BaseSalary = baseSalary;

        var result = await page.OnPostAddAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains("Enter the base salary in dollars, greater than zero, or leave it blank.", Errors(page));
        Assert.Empty(db.RicCostEntries);
    }

    [Fact]
    public async Task ABaseSalaryIsNotKeptOnALineThatIsNotStaff()
    {
        await using var db = CreateDb();
        var page = StaffLine(db, CostEntry.SalaryScales.Academic, "LVLB");
        page.Category = "Materials and supplies";
        page.Description = "Reagents";
        page.BaseSalary = 92_500m;

        await page.OnPostAddAsync();

        Assert.Null(db.RicCostEntries.Single().BaseSalary);
    }
}
