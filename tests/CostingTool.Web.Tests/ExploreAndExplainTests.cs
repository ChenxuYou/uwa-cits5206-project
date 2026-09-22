using System.Security.Claims;
using System.Text.Json;
using CostingTool.Data;
using CostingTool.Engine;
using CostingTool.Models;
using CostingTool.Pages.Approvals;
using CostingTool.Pages.Ric;
using CostingTool.Pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CostingTool.Web.Tests;

/// <summary>
/// M3's remaining criteria. US-10: go back to any earlier section, change a value, and lose
/// nothing on the way. US-13: room to explain in every section, the explanations the tool
/// requires checked again at submission, and every explanation sealed beside its figures.
///
/// The cycle is the guide's worked example: $150,000 of cost, $20,000 UWA and $30,000
/// non-UWA income, 1,000 forecast hours — calculated rates $100.00, $162.00 and $202.50.
/// </summary>
public class ExploreAndExplainTests
{
    private static DbContextOptions<CostingDbContext> Options(string name) =>
        new DbContextOptionsBuilder<CostingDbContext>().UseInMemoryDatabase(name).Options;

    private static (CostingDbContext Db, string Name) Seed(decimal uwa = 100m, decimal apfr = 162m, decimal commercial = 202.5m)
    {
        var name = Guid.NewGuid().ToString();
        var db = new CostingDbContext(Options(name));

        var capability = new RicCapability
        {
            Name = "Cryo-EM",
            CapacityBaseline = CapacityBaseline.Stated,
            StatedBaseline = 1_100m,
            StatedBaselineNote = "The guide's worked example",
            MaximumCapacity = 1_000m,
            ForecastUwaUse = 600m,
            ForecastApfrUse = 250m,
            ForecastCommercialUse = 150m,
            ProposedUwaRate = uwa,
            ProposedApfrRate = apfr,
            ProposedCommercialRate = commercial,
            CapacityDeductions = [new RicCapacityDeduction { Kind = "Maintenance", Amount = 100m, Note = "Quarterly service" }]
        };

        db.RicCycles.Add(new RicCycle
        {
            PlatformName = "Microscopy",
            StartYear = 2026,
            EndYear = 2027,
            BillableUnit = "Hours",
            Status = "Draft",
            CreatedBy = "entry",
            CreatedByDisplay = "Priya Lal",
            UtilisationAssumptions = "2025 bookings.",
            CostingAssumptions = "2026 budget as approved in March.",
            Capabilities = [capability],
            Costs =
            [
                new RicCostEntry { Capability = capability, Scope = CostEntry.Scopes.Capability, Category = "Repairs and maintenance", Description = "Service contract", Amount = 150_000m, Notes = "Titan Krios, 2026 quote" },
                new RicCostEntry { Capability = capability, Scope = CostEntry.Scopes.Capability, CostType = CostEntry.Types.Income, Category = CostEntry.IncomeCategories.UwaGpInKind, Description = "School support", Amount = 20_000m, Notes = "Committed to 2028" },
                new RicCostEntry { Capability = capability, Scope = CostEntry.Scopes.Capability, CostType = CostEntry.Types.Income, Category = CostEntry.IncomeCategories.State, Description = "State grant", Amount = 30_000m, Notes = "Three-year grant" }
            ]
        });

        db.SaveChanges();
        return (db, name);
    }

    private static PageContext SignedInAs(string userName, string display = "Priya Lal", string role = AppUser.Roles.DataEntry) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(CurrentUser.UserNameClaim, userName),
                    new Claim(ClaimTypes.Name, display),
                    new Claim(ClaimTypes.Role, role)
                ],
                "TestAuth"))
        }
    };

    private static RicCalculationService Calculator(CostingDbContext db) => new(new MethodConfigProvider(db));

    private static IEnumerable<string> Errors(PageModel page) =>
        page.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

    private static int CycleId(CostingDbContext db) => db.RicCycles.Single().Id;

    // ---- US-13: required explanations are checked again at submission ------------------

    private static ReviewModel Review(CostingDbContext db) => new(db, Calculator(db))
    {
        PageContext = SignedInAs("entry"),
        TempData = new TempDataDictionary(new DefaultHttpContext(), new MemoryTempData())
    };

    [Fact]
    public async Task ACostAddedAfterTheRatesStepCannotLeaveTheRatesUnexplained()
    {
        // The rates were saved at the calculated figures, in surplus, so no justification was
        // asked for. Then $20,000 of cost was added: the calculated rates moved away from the
        // proposed ones and the same rates now forecast a deficit. Nobody was asked about it.
        var (db, _) = Seed();
        await using var owned = db;
        db.RicCostEntries.Add(new RicCostEntry
        {
            RicCycleId = CycleId(db),
            Scope = CostEntry.Scopes.Platform,
            Category = "Administration costs",
            Description = "Booking administration",
            Amount = 20_000m
        });
        db.SaveChanges();

        var page = Review(db);
        var result = await page.OnPostSubmitAsync(CycleId(db), confirmAccuracy: true);

        Assert.IsType<PageResult>(result);
        Assert.Contains(Errors(page), x => x.StartsWith("A pricing justification is required") && x.EndsWith("Add it on the rates step."));
        Assert.Equal("Draft", db.RicCycles.Single().Status);
    }

    [Fact]
    public async Task AVariedRateWithoutAJustificationCannotBeSubmitted()
    {
        var (db, _) = Seed(uwa: 90m);
        await using var owned = db;

        var page = Review(db);
        await page.OnPostSubmitAsync(CycleId(db), confirmAccuracy: true);

        Assert.Contains(Errors(page), x => x.Contains("differ from the calculated ones for Cryo-EM"));
    }

    [Fact]
    public async Task MissingUtilisationAssumptionsAreCaughtAtSubmission()
    {
        var (db, _) = Seed();
        await using var owned = db;
        db.RicCycles.Single().UtilisationAssumptions = null;
        db.SaveChanges();

        var page = Review(db);
        await page.OnPostSubmitAsync(CycleId(db), confirmAccuracy: true);

        Assert.Contains(Errors(page), x => x.Contains("utilisation assumptions are not recorded"));
    }

    [Fact]
    public async Task AnExplainedCycleIsSubmitted()
    {
        var (db, _) = Seed(uwa: 90m);
        await using var owned = db;
        db.RicCycles.Single().PricingJustification = "Held below cost for the first year while a second instrument is commissioned.";
        db.SaveChanges();

        var result = await Review(db).OnPostSubmitAsync(CycleId(db), confirmAccuracy: true);

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("Submitted", db.RicCycles.Single().Status);
    }

    // ---- US-13: room to explain the costs section; the guide's Step 5 checklist ---------

    [Fact]
    public async Task CostingAssumptionsAreSavedForTheCostsSectionAsAWhole()
    {
        var (db, _) = Seed();
        await using var owned = db;

        var page = new CostsModel(db)
        {
            PageContext = SignedInAs("entry"),
            TempData = new TempDataDictionary(new DefaultHttpContext(), new MemoryTempData()),
            CycleId = CycleId(db),
            CostingAssumptions = "  Salaries at the 2026 EBA step plus 17% on-costs.  "
        };

        var result = await page.OnPostAssumptionsAsync();

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("Salaries at the 2026 EBA step plus 17% on-costs.", db.RicCycles.Single().CostingAssumptions);
    }

    [Fact]
    public void TheStepFiveChecklistReadsWhatHasBeenWritten()
    {
        var cycle = new RicCycle { CostingAssumptions = "Budget 2026", UtilisationAssumptions = "Bookings" };

        var items = Step5Checklist.For(cycle);

        Assert.Equal(["Costing assumptions documented", "Utilisation assumptions documented", "Benchmarking recorded"], items.Select(x => x.Label));
        Assert.Equal([true, true, false], items.Select(x => x.Done));
        Assert.Equal("/Ric/Rates", items.Single(x => !x.Done).Page);
    }

    // ---- US-10: return to any earlier section and change any value ---------------------

    private static StartModel Platform(CostingDbContext db) => new(db) { PageContext = SignedInAs("entry") };

    [Fact]
    public async Task ThePlatformStepReopensWithTheCyclesValues()
    {
        var (db, _) = Seed();
        await using var owned = db;

        var page = Platform(db);
        await page.OnGetAsync(CycleId(db));

        Assert.True(page.IsEditing);
        Assert.Equal("Microscopy", page.PlatformName);
        Assert.Equal("Cryo-EM", Assert.Single(page.Existing).Name);
        Assert.True(page.PeriodIsFixed);
        Assert.True(page.UnitIsFixed);
    }

    [Fact]
    public async Task ACapabilityCanBeRenamedAndAnotherAddedWithoutLosingItsFigures()
    {
        var (db, _) = Seed();
        await using var owned = db;

        var page = Platform(db);
        await page.OnGetAsync(CycleId(db));
        page.PlatformName = "Microscopy & Characterisation";
        page.Existing[0].Name = "Cryo-electron microscope";
        page.CapabilityNames = "Confocal";

        var result = await page.OnPostAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Ric/Costs", redirect.PageName);
        var cycle = db.RicCycles.Include(x => x.Capabilities).Include(x => x.Costs).Single();
        Assert.Equal("Microscopy & Characterisation", cycle.PlatformName);
        Assert.Equal(["Confocal", "Cryo-electron microscope"], cycle.Capabilities.Select(x => x.Name).OrderBy(x => x).ToArray());
        Assert.Equal(3, cycle.Costs.Count(x => x.Capability!.Name == "Cryo-electron microscope"));
    }

    [Fact]
    public async Task ACapabilityWithLinesBookedToItIsNotRemovedSilently()
    {
        var (db, _) = Seed();
        await using var owned = db;

        var page = Platform(db);
        await page.OnGetAsync(CycleId(db));
        page.Existing[0].Remove = true;
        page.CapabilityNames = "Confocal";

        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(Errors(page), x => x.Contains("Cryo-EM still has cost or funding lines"));
        Assert.Single(db.RicCapabilities);
    }

    [Fact]
    public async Task AnEmptyCapabilityCanBeRemoved()
    {
        var (db, _) = Seed();
        await using var owned = db;
        db.RicCycles.Include(x => x.Capabilities).Single().Capabilities.Add(new RicCapability { Name = "Spare" });
        db.SaveChanges();

        var page = Platform(db);
        await page.OnGetAsync(CycleId(db));
        page.Existing.Single(x => x.Name == "Spare").Remove = true;

        var result = await page.OnPostAsync();

        Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("Cryo-EM", db.RicCapabilities.Single().Name);
    }

    [Fact]
    public async Task ThePeriodAndUnitCannotMoveUnderFiguresExpressedInThem()
    {
        var (db, _) = Seed();
        await using var owned = db;

        var page = Platform(db);
        await page.OnGetAsync(CycleId(db));
        page.EndYear = 2029;
        page.BillableUnit = "Days";

        var result = await page.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.Contains(Errors(page), x => x.StartsWith("The pricing period cannot change once cost or funding lines are entered"));
        Assert.Contains(Errors(page), x => x.StartsWith("The billable unit cannot change once capacity and forecast use are entered in hours"));
        var cycle = db.RicCycles.Single();
        Assert.Equal(2027, cycle.EndYear);
        Assert.Equal("Hours", cycle.BillableUnit);
    }

    [Fact]
    public async Task AnotherCustodiansCycleCannotBeReopened()
    {
        var (db, _) = Seed();
        await using var owned = db;

        var page = new StartModel(db) { PageContext = SignedInAs("someone-else") };

        Assert.IsType<NotFoundResult>(await page.OnGetAsync(CycleId(db)));
    }

    // ---- US-10: nothing is lost by navigating backwards ---------------------------------

    private static CapacityModel CapacityPage(CostingDbContext db)
    {
        var cycle = db.RicCycles.Include(x => x.Capabilities).ThenInclude(x => x.CapacityDeductions).Single();
        var input = CapacityModel.CapacityInput.From(cycle.Capabilities.Single());
        input.UwaUse = 550m; // 950 hours forecast against 1,000 usable

        return new CapacityModel(db, new MethodConfigProvider(db))
        {
            PageContext = SignedInAs("entry"),
            CycleId = cycle.Id,
            UtilisationAssumptions = cycle.UtilisationAssumptions,
            Inputs = [input]
        };
    }

    [Fact]
    public async Task GoingBackFromCapacitySavesWhatWasTypedFirst()
    {
        var (db, _) = Seed();
        await using var owned = db;

        var result = await CapacityPage(db).OnPostBackAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Ric/Funding", redirect.PageName);
        Assert.Equal(550m, db.RicCapabilities.Single().ForecastUwaUse);
    }

    [Theory]
    [InlineData("/Ric/Start", "/Ric/Start")]
    [InlineData("/Ric/Costs", "/Ric/Costs")]
    [InlineData("/Ric/Review", "/Ric/Funding")]
    [InlineData("https://example.com/", "/Ric/Funding")]
    public async Task AStepLinkFromCapacitySavesAndGoesOnlyBackwards(string to, string expected)
    {
        var (db, _) = Seed();
        await using var owned = db;

        var result = await CapacityPage(db).OnPostLeaveAsync(to);

        Assert.Equal(expected, Assert.IsType<RedirectToPageResult>(result).PageName);
        Assert.Equal(550m, db.RicCapabilities.Single().ForecastUwaUse);
    }

    [Fact]
    public async Task AStepLinkFromRatesSavesTheTypedRatesFirst()
    {
        var (db, _) = Seed();
        await using var owned = db;
        var cycle = db.RicCycles.Include(x => x.Capabilities).Single();

        var page = new RatesModel(db, Calculator(db))
        {
            PageContext = SignedInAs("entry"),
            CycleId = cycle.Id,
            PricingJustification = "Rounded for researchers.",
            Inputs = [new RatesModel.RateInput(cycle.Capabilities.Single().Id, 95m, 162m, 202.5m)]
        };

        var result = await page.OnPostLeaveAsync("/Ric/Start");

        Assert.Equal("/Ric/Start", Assert.IsType<RedirectToPageResult>(result).PageName);
        Assert.Equal(95m, db.RicCapabilities.Single().ProposedUwaRate);
    }

    // ---- US-13: every explanation is sealed beside the figures it explains --------------

    [Fact]
    public async Task TheSealedRecordCarriesEveryExplanationAndThePdfPrintsThem()
    {
        var (db, _) = Seed(uwa: 90m);
        await using var owned = db;
        var cycle = db.RicCycles.Include(x => x.Capabilities).Single();
        cycle.Status = "Submitted";
        cycle.PricingJustification = "Held below cost for the first year.";
        cycle.BenchmarkNotes = "Within 8% of two Group of Eight platforms.";
        cycle.Capabilities.Single().AboveCapacityReason = "Not above capacity; never shown.";
        db.SaveChanges();

        var approver = new DetailsModel(db, Calculator(db))
        {
            PageContext = SignedInAs("approver", "Dr Chen", AppUser.Roles.Approver),
            TempData = new TempDataDictionary(new DefaultHttpContext(), new MemoryTempData())
        };

        var result = await approver.OnPostApproveAsync(cycle.Id, confirmApproval: true);

        // Back to the approval page for this record — not the custodian's /Ric/Review, which
        // an approver is not allowed to open.
        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Null(redirect.PageName);
        Assert.Equal(cycle.Id, redirect.RouteValues!["id"]);

        var sealedCycle = db.RicCycles.Single();
        using var json = JsonDocument.Parse(sealedCycle.SnapshotJson!);
        var root = json.RootElement;
        Assert.Equal("1.3", root.GetProperty("SchemaVersion").GetString());
        Assert.Equal("2026 budget as approved in March.", root.GetProperty("Cycle").GetProperty("CostingAssumptions").GetString());
        var capability = root.GetProperty("Capabilities")[0];
        Assert.Equal(1_100m, capability.GetProperty("CapacityBaselineAmount").GetDecimal());
        Assert.Equal("Quarterly service", capability.GetProperty("CapacityDeductions")[0].GetProperty("Note").GetString());

        var record = SealedRecord.Parse(sealedCycle.SnapshotJson!);
        var text = PdfText(SealedRecordPdf.Build(record, sealedCycle.SnapshotHash));
        foreach (var expected in new[]
                 {
                     "2026 budget as approved in March.",
                     "Note: Titan Krios, 2026 quote",
                     "Justification: Three-year grant",
                     "2025 bookings.",
                     "Stated baseline",
                     "The guide's worked example",
                     "Less maintenance",
                     "Quarterly service",
                     "Held below cost for the first year.",
                     "Within 8% of two Group of Eight platforms."
                 })
        {
            Assert.Contains(expected, text, StringComparison.Ordinal);
        }
    }

    private static string PdfText(MigraDoc.DocumentObjectModel.Document document)
    {
        var builder = new System.Text.StringBuilder();
        void Collect(System.Collections.IEnumerable elements)
        {
            foreach (var element in elements)
            {
                switch (element)
                {
                    case MigraDoc.DocumentObjectModel.Paragraph p:
                        foreach (var child in p.Elements)
                        {
                            if (child is MigraDoc.DocumentObjectModel.Text t) builder.Append(t.Content);
                            else if (child is MigraDoc.DocumentObjectModel.FormattedText f) Collect(f.Elements);
                        }
                        builder.AppendLine();
                        break;
                    case MigraDoc.DocumentObjectModel.Text t:
                        builder.Append(t.Content);
                        break;
                    case MigraDoc.DocumentObjectModel.Tables.Table table:
                        foreach (MigraDoc.DocumentObjectModel.Tables.Row row in table.Rows)
                        {
                            foreach (MigraDoc.DocumentObjectModel.Tables.Cell cell in row.Cells)
                            {
                                Collect(cell.Elements);
                            }
                        }

                        break;
                }
            }
        }

        foreach (MigraDoc.DocumentObjectModel.Section section in document.Sections)
        {
            Collect(section.Elements);
        }

        return builder.ToString();
    }

    /// <summary>TempData that lives for the test, so a handler that writes a message can run.</summary>
    private sealed class MemoryTempData : ITempDataProvider
    {
        private IDictionary<string, object> values = new Dictionary<string, object>();

        public IDictionary<string, object> LoadTempData(HttpContext context) => values;

        public void SaveTempData(HttpContext context, IDictionary<string, object> values) => this.values = values;
    }
}
