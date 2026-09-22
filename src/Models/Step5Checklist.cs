namespace CostingTool.Models;

/// <summary>
/// The guide's Step 5 documentation checklist, read from what the custodian has written
/// (US-13): "costing assumptions documented, utilisation assumptions documented,
/// benchmarking recorded" [G, Step 5].
///
/// It reports rather than blocks. Utilisation assumptions are already required on the
/// capacity step; costing assumptions and benchmarking are shown as outstanding so the
/// custodian sees what the guide expects before the approver does.
/// </summary>
public static class Step5Checklist
{
    /// <param name="Label">The checklist item, in the guide's words.</param>
    /// <param name="Done">Whether the record holds something for it.</param>
    /// <param name="Page">The step where it is written.</param>
    public sealed record Item(string Label, bool Done, string Page);

    public static IReadOnlyList<Item> For(RicCycle cycle)
    {
        ArgumentNullException.ThrowIfNull(cycle);

        return
        [
            new("Costing assumptions documented", !string.IsNullOrWhiteSpace(cycle.CostingAssumptions), "/Ric/Costs"),
            new("Utilisation assumptions documented", !string.IsNullOrWhiteSpace(cycle.UtilisationAssumptions), "/Ric/Capacity"),
            new("Benchmarking recorded", !string.IsNullOrWhiteSpace(cycle.BenchmarkNotes), "/Ric/Rates")
        ];
    }
}
