using System.Security.Claims;
using CostingTool.Data;
using CostingTool.Models;
using CostingTool.Pages.Approvals;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CostingTool.Web.Tests;

/// <summary>
/// The delegated authority's decision, and where it leaves the approver afterwards.
///
/// #80: approving saved the seal and then redirected to the custodian's review page, which
/// the Approver role cannot open. The approver saw Access Denied for a decision that had in
/// fact gone through, and found it already processed on signing in again. Both decisions
/// must land somewhere an approver is allowed to be.
/// </summary>
public class ApprovalTests
{
    private static CostingDbContext Seed()
    {
        var db = new CostingDbContext(new DbContextOptionsBuilder<CostingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        var capability = new RicCapability
        {
            Name = "Cryo-EM",
            MaximumCapacity = 1_000m,
            ForecastUwaUse = 600m,
            ForecastApfrUse = 250m,
            ForecastCommercialUse = 150m,
            ProposedUwaRate = 100m,
            ProposedApfrRate = 162m,
            ProposedCommercialRate = 202.50m
        };

        db.RicCycles.Add(new RicCycle
        {
            PlatformName = "Microscopy",
            StartYear = 2026,
            EndYear = 2027,
            BillableUnit = "Hours",
            Status = "Submitted",
            CreatedBy = "entry",
            CreatedByDisplay = "Priya Lal",
            Capabilities = [capability],
            Costs =
            [
                new RicCostEntry
                {
                    Capability = capability,
                    Scope = CostEntry.Scopes.Capability,
                    CostType = CostEntry.Types.Cost,
                    Category = "Maintenance",
                    Amount = 150_000m
                }
            ]
        });

        db.SaveChanges();
        return db;
    }

    private static DetailsModel ApproverPage(CostingDbContext db)
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, "Dana Approver"),
                new Claim(ClaimTypes.Role, AppUser.Roles.Approver),
                new Claim(CurrentUser.UserNameClaim, "approver")
            ],
            "TestAuth"));

        var services = new ServiceCollection()
            .AddSingleton<ITempDataProvider, NullTempDataProvider>()
            .AddSingleton<ITempDataDictionaryFactory, TempDataDictionaryFactory>()
            .BuildServiceProvider();

        return new DetailsModel(db, new RicCalculationService(new MethodConfigProvider(db)))
        {
            PageContext = new PageContext
            {
                HttpContext = new DefaultHttpContext { User = principal, RequestServices = services }
            }
        };
    }

    /// <summary>
    /// A null page name is the current page, /Approvals/Details; anything under /Ric is the
    /// custodian's folder and answers an approver with Access Denied.
    /// </summary>
    private static void AssertStaysOnTheApproversPage(IActionResult result, int cycleId)
    {
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.True(
            redirect.PageName is null || redirect.PageName.StartsWith("/Approvals/", StringComparison.Ordinal),
            $"An approver was sent to {redirect.PageName}, which the Approver role cannot open.");
        Assert.Equal(cycleId, redirect.RouteValues?["id"]);
    }

    [Fact]
    public async Task ApprovingSealsTheRecordAndReturnsTheApproverToTheirOwnPage()
    {
        await using var db = Seed();
        var cycle = await db.RicCycles.SingleAsync();

        var result = await ApproverPage(db).OnPostApproveAsync(cycle.Id, confirmApproval: true);

        AssertStaysOnTheApproversPage(result, cycle.Id);

        var sealedCycle = await db.RicCycles.AsNoTracking().SingleAsync();
        Assert.Equal("Sealed", sealedCycle.Status);
        Assert.Equal("Dana Approver", sealedCycle.SealedBy);
    }

    [Fact]
    public async Task ReturningACycleLeavesTheApproverOnTheirOwnPage()
    {
        await using var db = Seed();
        var cycle = await db.RicCycles.SingleAsync();

        var page = ApproverPage(db);
        page.ReturnReason = "Add the service contract to costs.";

        var result = await page.OnPostReturnAsync(cycle.Id);

        AssertStaysOnTheApproversPage(result, cycle.Id);
        Assert.Equal("Returned", (await db.RicCycles.AsNoTracking().SingleAsync()).Status);
    }

    /// <summary>TempData that remembers nothing, which is all these tests need of it.</summary>
    private sealed class NullTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
