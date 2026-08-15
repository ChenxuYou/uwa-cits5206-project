namespace CostingTool.Models;

public class CostingCycle
{
    public int Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string? ProjectReference { get; set; }
    public int StartYear { get; set; }
    public int DurationYears { get; set; }
    public string Status { get; set; } = "Draft";
    public string? DraftCostJson { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<CostItem> CostItems { get; set; } = [];
}
