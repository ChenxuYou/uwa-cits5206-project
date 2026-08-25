using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CostingTool.Pages.Approvals;

public class DetailsModel(CostingDbContext db, RicCalculationService calculator) : PageModel
{
    public RicCycle Cycle { get; private set; } = null!;
    public Dictionary<int, CapabilityRateResult> Results { get; private set; } = [];
    [BindProperty] public string? ReturnReason { get; set; }
    [BindProperty] public string? ApprovalComment { get; set; }
    [BindProperty] public DateTime? EffectiveDate { get; set; }

    public async Task<IActionResult> OnGetAsync(int id) => await Load(id) ? Page() : NotFound();

    public async Task<IActionResult> OnPostReturnAsync(int id)
    {
        if (!await Load(id)) return NotFound();
        if (Cycle.Status != "Submitted") return RedirectToPage(new { id });
        if (string.IsNullOrWhiteSpace(ReturnReason))
        {
            ModelState.AddModelError(nameof(ReturnReason), "Explain what must be changed before resubmission.");
            return Page();
        }
        Cycle.Status = "Returned";
        Cycle.ReturnedBy = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Unknown";
        Cycle.ReturnedAtUtc = DateTime.UtcNow;
        Cycle.ReturnReason = ReturnReason.Trim();
        Cycle.UpdatedAtUtc = DateTime.UtcNow;
        db.AppNotifications.Add(new AppNotification
        {
            RecipientName = Cycle.CreatedBy, RicCycleId = Cycle.Id, Type = "Returned",
            Title = $"{Cycle.PlatformName} was returned for changes",
            Message = Cycle.ReturnReason
        });
        await db.SaveChangesAsync();
        TempData["Success"] = "The cycle was returned to the submitter for changes.";
        return RedirectToPage("/Ric/Review", new { cycleId = id });
    }

    public async Task<IActionResult> OnPostApproveAsync(int id, bool confirmApproval)
    {
        if (!await Load(id)) return NotFound();
        if (Cycle.Status != "Submitted") return RedirectToPage(new { id });
        if (!confirmApproval)
        {
            ModelState.AddModelError(string.Empty, "Confirm delegated authority approval before sealing the record.");
            return Page();
        }

        var now = DateTime.UtcNow;
        Cycle.ApprovedBy = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? "Unknown";
        Cycle.ApprovedAtUtc = now;
        Cycle.ApprovalComment = ApprovalComment?.Trim();
        Cycle.EffectiveDateUtc = EffectiveDate.HasValue
            ? DateTime.SpecifyKind(EffectiveDate.Value.Date, DateTimeKind.Utc)
            : now.Date;
        Cycle.SealedBy = Cycle.ApprovedBy;
        Cycle.SealedAtUtc = now;
        // Stamp the method version the figures were produced under, so the sealed record
        // reproduces its own numbers rather than a later version's.
        Cycle.MethodVersion = calculator.Method.Version;
        Cycle.Status = "Sealed";
        Cycle.UpdatedAtUtc = now;

        Cycle.SnapshotJson = BuildSnapshot();
        Cycle.SnapshotHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Cycle.SnapshotJson)));
        db.AppNotifications.Add(new AppNotification
        {
            RecipientName = Cycle.CreatedBy, RicCycleId = Cycle.Id, Type = "Approved",
            Title = $"{Cycle.PlatformName} was approved",
            Message = string.IsNullOrWhiteSpace(Cycle.ApprovalComment)
                ? $"The costing cycle was approved and sealed, effective {Cycle.EffectiveDateUtc:dd MMM yyyy}."
                : Cycle.ApprovalComment
        });
        await db.SaveChangesAsync();
        TempData["Success"] = "The costing cycle was approved and sealed.";
        return RedirectToPage("/Ric/Review", new { cycleId = id });
    }

    private string BuildSnapshot()
    {
        var snapshot = new
        {
            SchemaVersion = "1.0",
            MethodVersion = calculator.Method.Version,
            CalculationMethod = $"RIC sustainable rate, method version {calculator.Method.Version}; "
                + $"indirect cost recovery k = {calculator.Method.IndirectCostRecovery} "
                + $"({calculator.Method.Source})",
            Cycle = new { Cycle.Id, Cycle.PlatformName, Cycle.StartYear, Cycle.EndYear, Cycle.BillableUnit, Cycle.CreatedBy, Cycle.BenchmarkNotes, Cycle.PricingJustification, Cycle.SubmittedBy, Cycle.SubmittedAtUtc, Cycle.ApprovedBy, Cycle.ApprovedAtUtc, Cycle.ApprovalComment, Cycle.EffectiveDateUtc },
            Capabilities = Cycle.Capabilities.OrderBy(x => x.Id).Select(x => new { x.Id, x.Name, x.MaximumCapacity, x.ForecastUwaUse, x.ForecastApfrUse, x.ForecastCommercialUse, x.ProposedUwaRate, x.ProposedApfrRate, x.ProposedCommercialRate, Result = Results[x.Id] }),
            Costs = Cycle.Costs.OrderBy(x => x.Id).Select(x => new { x.Id, x.RicCapabilityId, x.Scope, x.CostType, x.Category, x.Amount, x.Notes, x.PersonnelName, x.FundingType, x.FellowshipType, x.StepOption, x.WorkYears, x.EmploymentType, x.PercentWorked, x.SuperannuationPercent, x.StaffType, x.SalaryScale, x.SalaryStep, x.SchoolType, x.BaseSalary, x.Description, x.Supplier, YearAmounts = x.YearAmounts.OrderBy(y => y.ProjectYear).Select(y => new { y.ProjectYear, y.Amount }) })
        };
        return JsonSerializer.Serialize(snapshot, new JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<bool> Load(int id)
    {
        Cycle = (await db.RicCycles.Include(x => x.Capabilities).Include(x => x.Costs).ThenInclude(x => x.YearAmounts).FirstOrDefaultAsync(x => x.Id == id))!;
        if (Cycle is null) return false;
        Results = Cycle.Capabilities.ToDictionary(x => x.Id, x => calculator.Calculate(Cycle, x));
        EffectiveDate ??= DateTime.Today;
        return true;
    }
}
