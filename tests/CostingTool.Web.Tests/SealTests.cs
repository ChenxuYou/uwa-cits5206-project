using System.Security.Claims;
using System.Text.Json;
using CostingTool.Data;
using CostingTool.Engine;
using CostingTool.Models;
using CostingTool.Pages.Ric;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ApproverDetails = CostingTool.Pages.Approvals.DetailsModel;

namespace CostingTool.Web.Tests;

/// <summary>
/// US-14 and US-15: review everything before submitting, then seal — after which the record
/// is final, on file, and shown as it was sealed.
///
/// The cycle here is complete and priced exactly at the calculated rates: $150,000 of cost
/// over 1,000 hours, no income, k = 1.35, so $150.00 / $202.50 / $202.50 and a balance of
/// nil — nothing that needs a justification.
/// </summary>
public class SealTests
{
    private static CostingDbContext CreateDb() =>
        new(new DbContextOptionsBuilder<CostingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static async Task<RicCycle> ReadyCycle(CostingDbContext db, string status = "Draft")
    {
        var capability = new RicCapability
        {
            Name = "Cryo-EM",
            MaximumCapacity = 1_000m,
            CapacityBaseline = CapacityBaseline.Stated,
            StatedBaseline = 1_100m,
            StatedBaselineNote = "Booking system, 2025",
            CapacityDeductions = [new RicCapacityDeduction { Kind = "Maintenance", Amount = 100m, Note = "Annual service" }],
            ForecastUwaUse = 600m,
            ForecastApfrUse = 250m,
            ForecastCommercialUse = 150m,
            ProposedUwaRate = 150m,
            ProposedApfrRate = 202.50m,
            ProposedCommercialRate = 202.50m
        };

        var cycle = new RicCycle
        {
            PlatformName = "Microscopy",
            StartYear = 2026,
            EndYear = 2027,
            BillableUnit = "Hours",
            Status = status,
            CreatedBy = "entry",
            CreatedByDisplay = "Priya Lal",
            UtilisationAssumptions = "From the 2025 booking export.",
            Capabilities = [capability],
            Costs =
            [
                new RicCostEntry
                {
                    Capability = capability,
                    Scope = CostEntry.Scopes.Capability,
                    CostType = CostEntry.Types.Cost,
                    Category = "Materials and supplies",
                    Description = "Grids and reagents",
                    Amount = 150_000m
                }
            ]
        };

        db.RicCycles.Add(cycle);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return cycle;
    }

    private static PageContext Context(ClaimsPrincipal user) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = user,
            RequestServices = new ServiceCollection()
                .AddSingleton<ITempDataProvider, NullTempDataProvider>()
                .AddSingleton<ITempDataDictionaryFactory, TempDataDictionaryFactory>()
                .BuildServiceProvider()
        }
    };

    private static ClaimsPrincipal Person(string userName, string displayName, string role) => new(new ClaimsIdentity(
        [new Claim(CurrentUser.UserNameClaim, userName), new Claim(ClaimTypes.Name, displayName), new Claim(ClaimTypes.Role, role)],
        "TestAuth"));

    private static ReviewModel ReviewPage(CostingDbContext db) =>
        new(db, new RicCalculationService(new MethodConfigProvider(db)))
        {
            PageContext = Context(Person("entry", "Priya Lal", AppUser.Roles.DataEntry))
        };

    private static ApproverDetails ApproverPage(CostingDbContext db) =>
        new(db, new RicCalculationService(new MethodConfigProvider(db)))
        {
            PageContext = Context(Person("approver", "Dr Mei Chen", AppUser.Roles.Approver))
        };

    private static async Task<int> Sealed(CostingDbContext db)
    {
        var cycle = await ReadyCycle(db, status: "Submitted");
        Assert.IsType<RedirectToPageResult>(await ApproverPage(db).OnPostApproveAsync(cycle.Id, confirmApproval: true));
        db.ChangeTracker.Clear();
        return cycle.Id;
    }

    // ---- US-14: one page, everything missing listed with its way back -------------------

    [Fact]
    public async Task AnEmptyDraftListsWhatIsMissingWithTheStepThatSuppliesIt()
    {
        await using var db = CreateDb();
        var cycle = new RicCycle { PlatformName = "Microscopy", StartYear = 2026, EndYear = 2027, CreatedBy = "entry", Capabilities = [new RicCapability { Name = "Cryo-EM" }] };
        db.RicCycles.Add(cycle);
        await db.SaveChangesAsync();

        var page = ReviewPage(db);
        await page.OnGetAsync(cycle.Id);

        Assert.False(page.IsReady);
        Assert.Contains(page.Missing, x => x.Step == 2 && x.StepPage == "/Ric/Costs");
        Assert.Contains(page.Missing, x => x.Step == 4 && x.Message.Contains("forecast utilisation", StringComparison.Ordinal));
        Assert.Contains(page.Missing, x => x.Step == 4 && x.Message.StartsWith("Explain how the forecast", StringComparison.Ordinal));
        Assert.Contains(page.Missing, x => x.Step == 5 && x.Message.Contains("propose all three rates", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ACompleteCycleHasNothingMissing()
    {
        await using var db = CreateDb();
        var cycle = await ReadyCycle(db);

        var page = ReviewPage(db);
        await page.OnGetAsync(cycle.Id);

        Assert.True(page.IsReady, string.Join("; ", page.Missing.Select(x => x.Message)));
    }

    [Fact]
    public async Task RatesThatDifferFromTheCalculatedOnesNeedAJustificationBeforeSubmission()
    {
        await using var db = CreateDb();
        var cycle = await ReadyCycle(db);
        var capability = await db.RicCapabilities.SingleAsync();
        capability.ProposedCommercialRate = 180m;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var page = ReviewPage(db);
        await page.OnGetAsync(cycle.Id);

        var item = Assert.Single(page.Missing);
        Assert.Equal(5, item.Step);
        Assert.Contains("differ from the calculated ones for Cryo-EM", item.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SubmittingIsRefusedWhileAnythingIsMissing()
    {
        await using var db = CreateDb();
        var cycle = await ReadyCycle(db);
        var stored = await db.RicCycles.SingleAsync();
        stored.UtilisationAssumptions = null;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var result = await ReviewPage(db).OnPostSubmitAsync(cycle.Id, confirmAccuracy: true);

        Assert.IsType<PageResult>(result);
        Assert.Equal("Draft", (await db.RicCycles.AsNoTracking().SingleAsync()).Status);
    }

    [Fact]
    public async Task ACompleteConfirmedCycleIsSubmitted()
    {
        await using var db = CreateDb();
        var cycle = await ReadyCycle(db);

        var result = await ReviewPage(db).OnPostSubmitAsync(cycle.Id, confirmAccuracy: true);

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("Submitted", (await db.RicCycles.AsNoTracking().SingleAsync()).Status);
    }

    // ---- US-15: the seal ----------------------------------------------------------------

    [Fact]
    public async Task SealingRecordsWhoSealedItWhenAndUnderWhichMethod()
    {
        await using var db = CreateDb();
        var id = await Sealed(db);

        var cycle = await db.RicCycles.AsNoTracking().SingleAsync(x => x.Id == id);
        Assert.Equal("Sealed", cycle.Status);
        Assert.Equal("Dr Mei Chen", cycle.SealedBy);
        Assert.NotNull(cycle.SealedAtUtc);
        Assert.Equal("2026.1", cycle.MethodVersion);

        using var snapshot = JsonDocument.Parse(cycle.SnapshotJson!);
        Assert.Equal("1.4", snapshot.RootElement.GetProperty("SchemaVersion").GetString());
        Assert.Equal("Dr Mei Chen", snapshot.RootElement.GetProperty("Cycle").GetProperty("SealedBy").GetString());

        var capability = snapshot.RootElement.GetProperty("Capabilities")[0];
        Assert.Equal(1_100m, capability.GetProperty("BaselineCapacity").GetDecimal());
        Assert.Equal("Annual service", capability.GetProperty("CapacityDeductions")[0].GetProperty("Note").GetString());
    }

    [Fact]
    public async Task ASealedCycleCannotBeChanged()
    {
        await using var db = CreateDb();
        var id = await Sealed(db);

        var cycle = await db.RicCycles.SingleAsync(x => x.Id == id);
        cycle.PricingJustification = "Rewritten afterwards.";

        await Assert.ThrowsAsync<SealedRecordException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task ASealedCycleCannotBeReopened()
    {
        await using var db = CreateDb();
        var id = await Sealed(db);

        var cycle = await db.RicCycles.SingleAsync(x => x.Id == id);
        cycle.Status = "Draft";

        await Assert.ThrowsAsync<SealedRecordException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task ASealedCycleCannotBeDeleted()
    {
        await using var db = CreateDb();
        var id = await Sealed(db);

        db.RicCycles.Remove(await db.RicCycles.SingleAsync(x => x.Id == id));

        await Assert.ThrowsAsync<SealedRecordException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task TheLinesUnderASealedCycleCannotBeChangedAddedOrRemoved()
    {
        await using var db = CreateDb();
        var id = await Sealed(db);

        var cost = await db.RicCostEntries.SingleAsync();
        cost.Amount = 1m;
        await Assert.ThrowsAsync<SealedRecordException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();

        db.RicCostEntries.Add(new RicCostEntry { RicCycleId = id, Category = "Other expenses", Amount = 5m });
        await Assert.ThrowsAsync<SealedRecordException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();

        db.RicCapacityDeductions.Remove(await db.RicCapacityDeductions.SingleAsync());
        await Assert.ThrowsAsync<SealedRecordException>(() => db.SaveChangesAsync());
        db.ChangeTracker.Clear();

        Assert.Equal(150_000m, (await db.RicCostEntries.AsNoTracking().SingleAsync()).Amount);
    }

    [Fact]
    public async Task ADraftCanStillBeChanged()
    {
        await using var db = CreateDb();
        await ReadyCycle(db);

        var cost = await db.RicCostEntries.SingleAsync();
        cost.Amount = 1m;
        await db.SaveChangesAsync();

        Assert.Equal(1m, (await db.RicCostEntries.AsNoTracking().SingleAsync()).Amount);
    }

    [Fact]
    public async Task ASealedRecordShowsItsSealedFiguresEvenWhenItsMethodVersionIsGone()
    {
        await using var db = CreateDb();

        // Sealed under the built-in 2026.1 method, which is not a row in the table.
        var id = await Sealed(db);

        // Later, a new method with a different k is made current. Recalculating would have
        // fallen back to it and shown $225.00 as the commercial rate.
        db.MethodConfigs.Add(new MethodConfig
        {
            Version = "2029.1",
            EffectiveFromUtc = new DateTime(2029, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            IndirectCostRecovery = 1.50m,
            IsCurrent = true
        });
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var page = ReviewPage(db);
        await page.OnGetAsync(id);

        var rates = page.Rates.For((await db.RicCapabilities.SingleAsync()).Id)!;
        Assert.Equal("2026.1", page.Rates.Method.Version);
        Assert.Equal(1.35m, rates.IndirectCostRecovery);
        Assert.Equal(202.50m, rates.DisplayCommercialRate);
        Assert.Equal(150_000m, page.Rates.TotalOperatingCost);
        Assert.Empty(page.Missing);
    }

    private sealed class NullTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
