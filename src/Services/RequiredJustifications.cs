using CostingTool.Models;

namespace CostingTool.Data;

/// <summary>
/// The three places the tool insists on an explanation rather than merely offering room for
/// one (US-13): the utilisation assumptions, a proposed rate that differs from the calculated
/// one (US-11, F9), and a forecast deficit (US-12).
///
/// One definition, used both where the figure is entered and again at submission. Checking
/// only where it is entered was not enough: a custodian who saved surplus rates and then went
/// back and added a cost reached the review with a deficit nobody had been asked to explain.
/// </summary>
public static class RequiredJustifications
{
    /// <summary>
    /// Why a pricing justification is needed and missing for these rates, or null when it is
    /// given or not needed. A variance is reported before a deficit, naming the capabilities.
    /// </summary>
    public static string? PricingProblem(RicCycle cycle, CycleRates rates, string? justification)
    {
        ArgumentNullException.ThrowIfNull(cycle);
        ArgumentNullException.ThrowIfNull(rates);

        if (!string.IsNullOrWhiteSpace(justification))
        {
            return null;
        }

        var varied = cycle.Capabilities
            .Where(x => rates.For(x.Id)?.VariesFromCalculated == true)
            .Select(x => x.Name)
            .ToList();

        if (varied.Count > 0)
        {
            return "A pricing justification is required because the proposed rates differ from the "
                   + $"calculated ones for {string.Join(", ", varied)}.";
        }

        if (rates.IsComplete && rates.ForecastBalance < 0)
        {
            return "A pricing justification is required because these rates forecast a deficit. "
                   + "A deficit does not stop the record being submitted; it has to be explained.";
        }

        return null;
    }

    /// <summary>Every required explanation missing from the cycle as saved, in the order of the steps.</summary>
    public static IReadOnlyList<string> Missing(RicCycle cycle, CycleRates rates)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(cycle.UtilisationAssumptions))
        {
            missing.Add("The utilisation assumptions are not recorded. Say on the capacity step how the forecast figures were arrived at.");
        }

        if (PricingProblem(cycle, rates, cycle.PricingJustification) is { } pricing)
        {
            missing.Add(pricing + " Add it on the rates step.");
        }

        return missing;
    }
}
