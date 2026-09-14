using CostingTool.Data;
using CostingTool.Models;
using CostingTool.Pages.Ric;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CostingTool.Engine.Tests;

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

    [Fact]
    public async Task CapacityModelRejectsForecastUtilisationAboveCapacity()
    {
        await using var db = CreateDb();
        var model = new CapacityModel(db)
        {
            CycleId = db.RicCycles.Single().Id,
            Inputs =
            [
                new CapacityModel.CapacityInput
                {
                    Id = db.RicCycles.Include(x => x.Capabilities).Single().Capabilities.Single().Id,
                    MaximumCapacity = 1000m,
                    UwaUse = 400m,
                    ApfrUse = 350m,
                    CommercialUse = 300m
                }
            ]
        };

        var user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
            [new System.Security.Claims.Claim(CostingTool.Data.CurrentUser.UserNameClaim, "entry")],
            "TestAuth"));
        model.PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user }
        };

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(model.ModelState, kvp => kvp.Key == string.Empty && kvp.Value.Errors.Any(e => e.ErrorMessage.Contains("forecast utilisation cannot exceed capacity", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task CapacityModelRejectsMissingForecastUtilisation()
    {
        await using var db = CreateDb();
        var cycle = db.RicCycles.Include(x => x.Capabilities).Single();
        var capability = cycle.Capabilities.Single();

        var model = new CapacityModel(db)
        {
            CycleId = cycle.Id,
            Inputs =
            [
                new CapacityModel.CapacityInput
                {
                    Id = capability.Id,
                    MaximumCapacity = 1000m,
                    UwaUse = 0m,
                    ApfrUse = 0m,
                    CommercialUse = 0m
                }
            ]
        };

        var user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
            [new System.Security.Claims.Claim(CostingTool.Data.CurrentUser.UserNameClaim, "entry")],
            "TestAuth"));
        model.PageContext = new Microsoft.AspNetCore.Mvc.RazorPages.PageContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user }
        };

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(model.ModelState, kvp => kvp.Key == string.Empty && kvp.Value.Errors.Any(e => e.ErrorMessage.Contains("forecast utilisation must be greater than zero", StringComparison.OrdinalIgnoreCase)));
    }
}
