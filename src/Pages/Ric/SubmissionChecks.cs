using CostingTool.Data;
using CostingTool.Models;

namespace CostingTool.Pages.Ric;

/// <summary>One thing a cycle still needs before it can go for approval, and the step that supplies it.</summary>
public sealed record MissingItem(string Message, int Step)
{
    public string StepName => RicSteps.Get(Step).Name;

    public string StepPage => RicSteps.Get(Step).Page;
}

/// <summary>
/// What must be in a cycle before it is submitted, and so before it can be sealed (US-14, N3).
///
/// <b>One list, used twice.</b> The review page shows it the moment the page opens, each
/// item linked to the step that fixes it, and the submit handler refuses on exactly the same
/// list. Before, the page learned what was missing only by submitting and failing, and the
/// messages carried no way back to the field. A rule added here reaches both, so the page
/// can never say "ready" about a cycle the handler would refuse.
///
/// The rules are the ones each step already enforces when it is saved. They are checked
/// again here because a step that was never opened was never saved, and because a later
/// change — a capability added on step 1, say — can leave an earlier answer incomplete.
/// </summary>
public static class SubmissionChecks
{
    public static IReadOnlyList<MissingItem> For(RicCycle cycle, CycleRates rates)
    {
        ArgumentNullException.ThrowIfNull(cycle);
        ArgumentNullException.ThrowIfNull(rates);

        var missing = new List<MissingItem>();
        var unit = cycle.BillableUnit.ToLowerInvariant();

        if (cycle.Capabilities.Count == 0)
        {
            missing.Add(new("Add at least one capability to price.", 1));
        }

        if (!cycle.Costs.Any(x => !x.IsIncome))
        {
            missing.Add(new("Record the platform's operating costs.", 2));
        }

        foreach (var capability in cycle.Capabilities)
        {
            if (capability.MaximumCapacity <= 0)
            {
                missing.Add(new($"{capability.Name}: work out its usable capacity.", 4));
            }

            if (capability.ForecastUtilisation <= 0)
            {
                missing.Add(new($"{capability.Name}: enter its forecast utilisation. Every rate is divided by it.", 4));
            }
            else if (capability.MaximumCapacity > 0
                     && capability.ForecastUtilisation > capability.MaximumCapacity
                     && string.IsNullOrWhiteSpace(capability.AboveCapacityReason))
            {
                missing.Add(new(
                    $"{capability.Name}: the forecast of {capability.ForecastUtilisation:N1} {unit} is above the usable "
                    + $"capacity of {capability.MaximumCapacity:N1} {unit}. Say why it can be met.",
                    4));
            }
        }

        if (cycle.Capabilities.Count > 0 && string.IsNullOrWhiteSpace(cycle.UtilisationAssumptions))
        {
            missing.Add(new("Explain how the forecast utilisation was arrived at.", 4));
        }

        // A capability with no forecast already has its item above; anything else the engine
        // refuses comes from the figures entered on the costs and funding steps.
        foreach (var capability in cycle.Capabilities.Where(x => x.ForecastUtilisation > 0))
        {
            if (rates.ProblemFor(capability.Id) is { } problem)
            {
                missing.Add(new(problem, 2));
            }
        }

        foreach (var capability in cycle.Capabilities)
        {
            if (capability.ProposedUwaRate <= 0 || capability.ProposedApfrRate <= 0 || capability.ProposedCommercialRate <= 0)
            {
                missing.Add(new($"{capability.Name}: propose all three rates.", 5));
            }
        }

        if (JustificationRequiredBecause(cycle, rates) is { } reason && string.IsNullOrWhiteSpace(cycle.PricingJustification))
        {
            missing.Add(new($"Give a pricing justification: {reason}.", 5));
        }

        return missing;
    }

    /// <summary>
    /// Why a pricing justification is needed, or null when it is not: the proposed rates
    /// differ from the calculated ones (US-11), or they forecast a deficit (US-12). The rates
    /// step asks on save for the same reasons.
    /// </summary>
    public static string? JustificationRequiredBecause(RicCycle cycle, CycleRates rates)
    {
        var varied = cycle.Capabilities
            .Where(x => rates.For(x.Id)?.VariesFromCalculated == true)
            .Select(x => x.Name)
            .ToList();

        if (varied.Count > 0)
        {
            return $"the proposed rates differ from the calculated ones for {string.Join(", ", varied)}";
        }

        if (rates.IsComplete && cycle.Capabilities.Count > 0 && rates.ForecastBalance < 0)
        {
            return "these rates forecast a deficit";
        }

        return null;
    }
}
