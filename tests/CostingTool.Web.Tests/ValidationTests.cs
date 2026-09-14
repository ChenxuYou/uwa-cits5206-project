using CostingTool.Data;
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
        var model = new CapacityModel(db)
        {
            CycleId = cycle.Id,
            Inputs =
            [
                new CapacityModel.CapacityInput
                {
                    Id = capability.Id,
                    MaximumCapacity = capability.MaximumCapacity,
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
    public async Task CapacityModelRejectsForecastUtilisationAboveCapacity()
    {
        await using var db = CreateDb();
        var cycle = db.RicCycles.Include(x => x.Capabilities).Single();
        var model = CreateModel(db, cycle);
        model.Inputs[0] = new CapacityModel.CapacityInput
        {
            Id = cycle.Capabilities.Single().Id,
            MaximumCapacity = 1000m,
            UwaUse = 400m,
            ApfrUse = 350m,
            CommercialUse = 300m
        };

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(model.ModelState, kvp => kvp.Key == string.Empty && kvp.Value.Errors.Any(e => e.ErrorMessage.Contains("forecast utilisation cannot exceed capacity", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task CapacityModelRejectsZeroForecastUtilisation()
    {
        await using var db = CreateDb();
        var cycle = db.RicCycles.Include(x => x.Capabilities).Single();
        var model = CreateModel(db, cycle);
        model.Inputs[0] = new CapacityModel.CapacityInput
        {
            Id = cycle.Capabilities.Single().Id,
            MaximumCapacity = 1000m,
            UwaUse = 0m,
            ApfrUse = 0m,
            CommercialUse = 0m
        };

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(model.ModelState, kvp => kvp.Key == string.Empty && kvp.Value.Errors.Any(e => e.ErrorMessage.Contains("forecast utilisation must be greater than zero", StringComparison.OrdinalIgnoreCase)));
    }
}
