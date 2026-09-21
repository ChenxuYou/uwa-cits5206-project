using CostingTool.Data;
using CostingTool.Engine;
using CostingTool.Models;
using CostingTool.Pages.Ric;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using System.Security.Claims;
using Xunit;

namespace CostingTool.Web.Tests;

public class ValidationTests
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
                    MaximumCapacity = 1000m,
                    ForecastUwaUse = 0m,
                    ForecastApfrUse = 0m,
                    ForecastCommercialUse = 0m
                }
            ]
        });

        db.SaveChanges();
        return db;
    }

    private static CapacityModel CreateModel(CostingDbContext db, RicCycle cycle)
    {
        var capability = cycle.Capabilities.Single();
        var model = new CapacityModel(db, new MethodConfigProvider(db))
        {
            CycleId = cycle.Id,
            UtilisationAssumptions = "2025 bookings",
            Inputs =
            [
                new CapacityModel.CapacityInput
                {
                    Id = capability.Id,
                    Baseline = CapacityBaseline.Stated,
                    StatedBaseline = 1000m,
                    StatedBaselineNote = "Booking system maximum",
                    UwaUse = capability.ForecastUwaUse,
                    ApfrUse = capability.ForecastApfrUse,
                    CommercialUse = capability.ForecastCommercialUse
                }
            ]
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
    public async Task AForecastAboveCapacityIsNotSavedUntilItIsExplained()
    {
        // US-08: warned about and explained, not blocked. This test used to assert the
        // opposite — "cannot exceed capacity" — which contradicted the story's criteria.
        await using var db = CreateDb();
        var cycle = db.RicCycles.Include(x => x.Capabilities).Single();
        var model = CreateModel(db, cycle);
        model.Inputs[0].UwaUse = 400m;
        model.Inputs[0].ApfrUse = 350m;
        model.Inputs[0].CommercialUse = 300m;

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(model.ModelState, kvp => kvp.Key == string.Empty && kvp.Value.Errors.Any(e =>
            e.ErrorMessage.Contains("above the usable capacity of 1,000.0 hours", StringComparison.Ordinal)
            && e.ErrorMessage.Contains("That is allowed, but say why", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task AForecastAboveCapacityIsSavedWithItsReason()
    {
        await using var db = CreateDb();
        var cycle = db.RicCycles.Include(x => x.Capabilities).Single();
        var model = CreateModel(db, cycle);
        model.Inputs[0].UwaUse = 400m;
        model.Inputs[0].ApfrUse = 350m;
        model.Inputs[0].CommercialUse = 300m;
        model.Inputs[0].AboveCapacityReason = "A second shift starts in March under the new ARC grant.";

        var result = await model.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
        var saved = db.RicCapabilities.Single();
        Assert.Equal(1_050m, saved.ForecastUtilisation);
        Assert.Equal(1_000m, saved.MaximumCapacity);
        Assert.Equal("A second shift starts in March under the new ARC grant.", saved.AboveCapacityReason);
    }

    [Fact]
    public async Task CapacityModelRejectsZeroForecastUtilisation()
    {
        await using var db = CreateDb();
        var cycle = db.RicCycles.Include(x => x.Capabilities).Single();
        var model = CreateModel(db, cycle);

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(model.ModelState, kvp => kvp.Key == string.Empty && kvp.Value.Errors.Any(e => e.ErrorMessage.Contains("forecast utilisation must be greater than zero", StringComparison.OrdinalIgnoreCase)));
    }
}
