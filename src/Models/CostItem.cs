namespace CostingTool.Models;

public class CostItem
{
    public int Id { get; set; }
    public int CostingCycleId { get; set; }
    public CostingCycle CostingCycle { get; set; } = null!;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Supplier { get; set; }
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
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<CostItemYearAmount> YearAmounts { get; set; } = [];
}

public class CostItemYearAmount
{
    public int Id { get; set; }
    public int CostItemId { get; set; }
    public CostItem CostItem { get; set; } = null!;
    public int ProjectYear { get; set; }
    public decimal Amount { get; set; }
}
