namespace CostingTool.Models;

public class RicCycle
{
    public int Id { get; set; }
    public string PlatformName { get; set; } = string.Empty;
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    public string BillableUnit { get; set; } = "Hours";
    public string Status { get; set; } = "Draft";
    public string? BenchmarkNotes { get; set; }
    public string? PricingJustification { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<RicCapability> Capabilities { get; set; } = [];
    public List<RicCostEntry> Costs { get; set; } = [];
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
