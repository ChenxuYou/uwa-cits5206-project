using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CostingTool.Data;
using CostingTool.Engine;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Approvals;

public class DetailsModel(CostingDbContext db, RicCalculationService calculator) : PageModel
{
    public RicCycle Cycle { get; private set; } = null!;

    public CycleRates Rates { get; private set; } = null!;

    [BindProperty] public string? ReturnReason { get; set; }

    [BindProperty] public string? ApprovalComment { get; set; }

    [BindProperty] public DateTime? EffectiveDate { get; set; }

    public async Task<IActionResult> OnGetAsync(int id) =>
        await Load(id) ? Page() : NotFound();

    public async Task<IActionResult> OnPostReturnAsync(int id)
    {
        if (!await Load(id))
        {
            return NotFound();
        }

        if (Cycle.Status != "Submitted")
        {
            return RedirectToPage(new { id });
        }

        if (string.IsNullOrWhiteSpace(ReturnReason))
        {
            ModelState.AddModelError(nameof(ReturnReason), "Explain what must be changed before resubmission.");
            return Page();
        }

        Cycle.Status = "Returned";
        Cycle.ReturnedBy = User.DisplayName();
        Cycle.ReturnedAtUtc = DateTime.UtcNow;
        Cycle.ReturnReason = ReturnReason.Trim();
        Cycle.UpdatedAtUtc = DateTime.UtcNow;

        Notify("Returned", $"{Cycle.PlatformName} was returned for changes", Cycle.ReturnReason);

        await db.SaveChangesAsync();
        TempData["Success"] = "The cycle was returned to the submitter for changes.";

        // Back to this page, not the custodian's review: /Ric is restricted to data entry, so
        // an approver sent there lands on Access Denied after the decision has already been
        // saved, and reads it as a failure (#80).
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostApproveAsync(int id, bool confirmApproval)
    {
        if (!await Load(id))
        {
            return NotFound();
        }

        if (Cycle.Status != "Submitted")
        {
            return RedirectToPage(new { id });
        }

        if (!confirmApproval)
        {
            ModelState.AddModelError(string.Empty, "Confirm that you approve these rates and understand that, once sealed, the record cannot be edited or deleted.");
            return Page();
        }

        // A record is never sealed around a capability that has no rates. The seal is the
        // moment the figures stop being editable, so it is the last place this can be
        // caught — architecture.md §5.
        if (!Rates.IsComplete)
        {
            foreach (var problem in Rates.Problems)
            {
                ModelState.AddModelError(string.Empty, problem);
            }

            return Page();
        }

        var now = DateTime.UtcNow;
        var approver = User.DisplayName();

        Cycle.ApprovedBy = approver;
        Cycle.ApprovedAtUtc = now;
        Cycle.ApprovalComment = ApprovalComment?.Trim();
        Cycle.EffectiveDateUtc = EffectiveDate.HasValue
            ? DateTime.SpecifyKind(EffectiveDate.Value.Date, DateTimeKind.Utc)
            : now.Date;
        Cycle.SealedBy = approver;
        Cycle.SealedAtUtc = now;

        // Stamp the method version the figures were produced under, so the sealed record
        // reproduces its own numbers rather than a later version's — rule R6.
        Cycle.MethodVersion = Rates.Method.Version;
        Cycle.Status = "Sealed";
        Cycle.UpdatedAtUtc = now;

        // The record this one replaces is named in the snapshot, with its own hash, so the
        // chain from one approved rate to the next can be followed from the documents alone
        // (F22). The older record is read, never written.
        var replaced = Cycle.SupersedesCycleId is { } replacedId
            ? await db.RicCycles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == replacedId)
            : null;

        Cycle.SnapshotJson = BuildSnapshot(replaced);
        Cycle.SnapshotHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Cycle.SnapshotJson)));

        Notify(
            "Approved",
            $"{Cycle.PlatformName} was approved",
            string.IsNullOrWhiteSpace(Cycle.ApprovalComment)
                ? $"The costing cycle was approved and sealed, effective {Cycle.EffectiveDateUtc:dd MMM yyyy}."
                : Cycle.ApprovalComment);

        await db.SaveChangesAsync();
        TempData["Success"] = "The costing cycle was approved and sealed.";
        return RedirectToPage(new { id });
    }

    private void Notify(string type, string title, string message) =>
        db.AppNotifications.Add(new AppNotification
        {
            RecipientUserName = Cycle.CreatedBy,
            RicCycleId = Cycle.Id,
            Type = type,
            Title = title,
            Message = message
        });

    /// <summary>
    /// The immutable record.
    ///
    /// It stores the full inputs <b>and the workings</b>, not foreign keys to live rows: if
    /// a category is renamed in 2028, the 2026 record still says what it said
    /// (architecture.md §4). The workings are here because the client asked on 20 August
    /// 2026 that the record show them "for transparency and traceability" — a rate that
    /// cannot be re-derived from the document is not a defensible rate.
    /// </summary>
    private string BuildSnapshot(RicCycle? replaced)
    {
        var method = Rates.Method;

        var snapshot = new
        {
            SchemaVersion = "1.5",
            SealedAtUtc = Cycle.SealedAtUtc,
            MethodVersion = method.Version,
            Method = new
            {
                method.Version,
                method.IndirectCostRecovery,
                method.RateDecimals,
                MidpointRule = method.MidpointRule.ToString(),
                method.Source,
                Formulas = new
                {
                    UwaResearcher = "(C - I_total) / U",
                    Apfr = "((C - I_nonuwa) / U) * k",
                    Commercial = "(C / U) * k"
                }
            },
            Cycle = new
            {
                Cycle.Id,
                Cycle.PlatformName,
                Cycle.StartYear,
                Cycle.EndYear,
                Cycle.BillableUnit,
                Cycle.CreatedBy,
                Cycle.CreatedByDisplay,
                Cycle.UtilisationAssumptions,
                Cycle.CostingAssumptions,
                Cycle.BenchmarkNotes,
                Cycle.PricingJustification,
                Cycle.SubmittedBy,
                Cycle.SubmittedAtUtc,
                Cycle.ApprovedBy,
                Cycle.ApprovedAtUtc,
                Cycle.ApprovalComment,
                Cycle.EffectiveDateUtc,
                Cycle.SealedBy,
                Supersedes = replaced is null ? null : new
                {
                    replaced.Id,
                    replaced.PlatformName,
                    replaced.StartYear,
                    replaced.EndYear,
                    replaced.SealedAtUtc,
                    replaced.SnapshotHash
                }
            },
            Platform = new
            {
                TotalOperatingCost = Rates.TotalOperatingCost,
                TotalIncome = Rates.TotalIncome,
                NetCostToRecover = Rates.NetCostToRecover,
                GrossForecastRevenue = Rates.GrossForecastRevenue,
                OverheadsRecovered = Rates.OverheadsRecovered,
                ForecastRevenue = Rates.ForecastRevenue,
                ForecastBalance = Rates.ForecastBalance,
                FullEconomicCostBalance = Rates.FullEconomicCostBalance
            },
            Capabilities = Cycle.Capabilities.OrderBy(x => x.Id).Select(capability => new
            {
                capability.Id,
                capability.Name,
                capability.MaximumCapacity,
                capability.ForecastUwaUse,
                capability.ForecastApfrUse,
                capability.ForecastCommercialUse,
                capability.CapacityBaseline,
                BaselineCapacity = BaselineOf(capability),
                capability.StatedBaselineNote,
                capability.IsStaffReliant,
                capability.StaffFte,
                capability.AboveCapacityReason,
                CapacityDeductions = capability.CapacityDeductions.OrderBy(x => x.Id).Select(x => new { x.Kind, x.Amount, x.Note }),
                capability.ProposedUwaRate,
                capability.ProposedApfrRate,
                capability.ProposedCommercialRate,
                Result = Rates.For(capability.Id),
                Workings = Workings(Rates.For(capability.Id))
            }),
            Costs = Cycle.Costs.OrderBy(x => x.Id).Select(x => new
            {
                x.Id,
                x.RicCapabilityId,
                x.Scope,
                x.CostType,
                x.Category,
                x.Amount,
                x.Notes,
                x.PersonnelName,
                x.FundingType,
                x.FellowshipType,
                x.StepOption,
                x.WorkYears,
                x.EmploymentType,
                x.PercentWorked,
                x.SuperannuationPercent,
                x.StaffType,
                x.SalaryScale,
                x.SalaryStep,
                x.SchoolType,
                x.BaseSalary,
                x.Description,
                x.Supplier,
                x.Position,
                x.FloorArea,
                x.FloorAreaRate,
                YearAmounts = x.YearAmounts.OrderBy(y => y.ProjectYear).Select(y => new { y.ProjectYear, y.Amount })
            })
        };

        return JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// The figure the capability's deductions came off, as the capacity step worked it out:
    /// the custodian's own for a stated baseline, otherwise the method's working year in the
    /// billable unit. Null when the capacity step was never saved.
    /// </summary>
    private decimal? BaselineOf(RicCapability capability) =>
        capability.CapacityBaseline == CapacityBaseline.Stated
            ? capability.StatedBaseline
            : CapacityEngine.BaselinesFor(Rates.Method, Cycle.BillableUnit)
                .FirstOrDefault(x => x.Key == capability.CapacityBaseline)?.Amount;

    /// <summary>The arithmetic, written out with this capability's own numbers in it.</summary>
    private static object? Workings(CapabilityRateResult? r) => r is null ? null : new
    {
        C = $"{r.CapabilityOperatingCost} capability + {r.AllocatedPlatformCost} allocated platform = {r.TotalOperatingCost}",
        I_total = $"{r.UwaIncome} UWA + {r.NonUwaIncome} non-UWA = {r.TotalIncome}",
        I_nonuwa = $"{r.NonUwaIncome}",
        U = $"{r.ForecastUtilisation}",
        k = $"{r.IndirectCostRecovery}",
        UwaResearcher = $"({r.TotalOperatingCost} - {r.TotalIncome}) / {r.ForecastUtilisation} = {r.UwaRate}",
        Apfr = $"(({r.TotalOperatingCost} - {r.NonUwaIncome}) / {r.ForecastUtilisation}) * {r.IndirectCostRecovery} = {r.ApfrRate}",
        Commercial = $"({r.TotalOperatingCost} / {r.ForecastUtilisation}) * {r.IndirectCostRecovery} = {r.CommercialRate}"
    };

    /// <summary>
    /// Loads the cycle for review.
    ///
    /// There is deliberately no ownership filter here, unlike the custodian's pages: an
    /// approver is the delegated authority for records submitted to them, which the client
    /// confirmed on 20 August 2026 ("administrator is approver of the record"). The folder
    /// is restricted to the Approver role in Program.cs, and only Submitted records can be
    /// acted on.
    /// </summary>
    private async Task<bool> Load(int id)
    {
        var cycle = await db.RicCycles
            .Include(x => x.Capabilities).ThenInclude(x => x.CapacityDeductions)
            .Include(x => x.Costs).ThenInclude(x => x.Capability)
            .Include(x => x.Costs).ThenInclude(x => x.YearAmounts)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (cycle is null)
        {
            return false;
        }

        Cycle = cycle;
        Rates = Cycle.Status == "Sealed"
            ? SealedRates.Of(Cycle)
            : calculator.Calculate(Cycle);

        EffectiveDate ??= DateTime.Today;
        return true;
    }
}
