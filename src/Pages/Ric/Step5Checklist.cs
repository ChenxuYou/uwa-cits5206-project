using CostingTool.Models;

namespace CostingTool.Pages.Ric;

/// <summary>
/// The guide's Step 5 documentation checklist, read from what the custodian has written
/// (US-13): "costing assumptions documented, utilisation assumptions documented,
/// benchmarking recorded" [G, Step 5].
///
/// <b>It reports rather than blocks.</b> <see cref="SubmissionChecks"/> is the list that
/// refuses a submission, and utilisation assumptions are already on it. Costing assumptions
/// and benchmarking are not required by the tool, so they are shown here as outstanding:
/// the custodian sees what the guide expects before the approver does.
/// </summary>
public static class Step5Checklist
{
    /// <param name="Label">The checklist item, in the guide's words.</param>
    /// <param name="Done">Whether the record holds something for it.</param>
    /// <param name="Step">The step where it is written.</param>
    public sealed record Item(string Label, bool Done, int Step)
    {
        public string StepName => RicSteps.Get(Step).Name;

        public string StepPage => RicSteps.Get(Step).Page;
    }

    public static IReadOnlyList<Item> For(RicCycle cycle)
    {
        ArgumentNullException.ThrowIfNull(cycle);

        return
        [
            new("Costing assumptions documented", !string.IsNullOrWhiteSpace(cycle.CostingAssumptions), 2),
            new("Utilisation assumptions documented", !string.IsNullOrWhiteSpace(cycle.UtilisationAssumptions), 4),
            new("Benchmarking recorded", !string.IsNullOrWhiteSpace(cycle.BenchmarkNotes), 5)
        ];
    }
}
