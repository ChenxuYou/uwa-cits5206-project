namespace CostingTool.Models;

public class RicCycle
{
    public int Id { get; set; }
    public string PlatformName { get; set; } = string.Empty;
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    public string BillableUnit { get; set; } = "Hours";
    public string Status { get; set; } = "Draft";
    public string CreatedBy { get; set; } = string.Empty;
    public string? BenchmarkNotes { get; set; }
    public string? PricingJustification { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public string? ReturnedBy { get; set; }
    public DateTime? ReturnedAtUtc { get; set; }
    public string? ReturnReason { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public string? ApprovalComment { get; set; }
    public string? SealedBy { get; set; }
    public DateTime? SealedAtUtc { get; set; }
    public string? SnapshotJson { get; set; }
    public string? SnapshotHash { get; set; }
    public DateTime? EffectiveDateUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<RicCapability> Capabilities { get; set; } = [];
    public List<RicCostEntry> Costs { get; set; } = [];
}

public class AppUser
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "DataEntry";
    public bool IsActive { get; set; } = true;
}

public class AppNotification
{
    public int Id { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public int? RicCycleId { get; set; }
    public string Type { get; set; } = "Info";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class RicCapability
{
    public int Id { get; set; }
    public int RicCycleId { get; set; }
    public RicCycle RicCycle { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public decimal MaximumCapacity { get; set; }
    public decimal ForecastUwaUse { get; set; }
    public decimal ForecastApfrUse { get; set; }
    public decimal ForecastCommercialUse { get; set; }
    public decimal ProposedUwaRate { get; set; }
    public decimal ProposedApfrRate { get; set; }
    public decimal ProposedCommercialRate { get; set; }
}

public class RicCostEntry
{
    public int Id { get; set; }
    public int RicCycleId { get; set; }
    public RicCycle RicCycle { get; set; } = null!;
    public int? RicCapabilityId { get; set; }
    public RicCapability? Capability { get; set; }
    public string Scope { get; set; } = "Capability";
    public string CostType { get; set; } = "Directly incurred";
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public string? PersonnelName { get; set; }
    public string? FundingType { get; set; }
    public string? FellowshipType { get; set; }
    public string? StepOption { get; set; }
    public int? WorkYears { get; set; }
    public string? EmploymentType { get; set; }
    public decimal? PercentWorked { get; set; }
    public decimal? SuperannuationPercent { get; set; }
    public string? StaffType { get; set; }
    public string? SalaryScale { get; set; }
    public string? SalaryStep { get; set; }
    public string? SchoolType { get; set; }
    public decimal? BaseSalary { get; set; }
    public string? Description { get; set; }
    public string? Supplier { get; set; }
    public List<RicCostYearAmount> YearAmounts { get; set; } = [];
}

public class RicCostYearAmount
{
    public int Id { get; set; }
    public int RicCostEntryId { get; set; }
    public RicCostEntry RicCostEntry { get; set; } = null!;
    public int ProjectYear { get; set; }
    public decimal Amount { get; set; }
}
