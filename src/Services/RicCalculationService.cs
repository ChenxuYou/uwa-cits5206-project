using CostingTool.Engine;
using CostingTool.Models;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Data;

/// <summary>
/// Resolves the method configuration in force. Kept separate from the engine so that the
/// engine itself never touches a database row — architecture.md §3 rule R7.
/// </summary>
public class MethodConfigProvider(CostingDbContext db)
{
    private MethodConfig? current;

    /// <summary>The version in force for new and in-progress cycles.</summary>
    public MethodConfig Current => current ??= db.MethodConfigs
        .Where(x => x.IsCurrent)
        .OrderByDescending(x => x.EffectiveFromUtc)
        .FirstOrDefault() ?? MethodConfig.Fallback;

    /// <summary>
    /// The version a sealed record was calculated under, so an old record reproduces its
    /// own figures rather than today's.
    /// </summary>
    public MethodConfig ForVersion(string? version) =>
        string.IsNullOrWhiteSpace(version)
            ? Current
            : db.MethodConfigs.FirstOrDefault(x => x.Version == version) ?? Current;
}

/// <summary>
/// Turns a stored cycle into engine inputs, and the engine's answers into something a page
/// can render.
///
/// <b>This class is the seam, and it is the only place allowed to know both sides.</b> The
/// engine takes value objects and knows nothing about Entity Framework; the pages take
/// results and know nothing about the formulas. Everything that translates between them —
/// the platform-cost allocation rule, which rows count as income, how a capability's share
/// is worked out — happens here, once, where it can be read and reviewed.
/// </summary>
public class RicCalculationService(MethodConfigProvider methods)
{
    /// <summary>The method configuration currently in force.</summary>
    public MethodConfig Method => methods.Current;

    /// <summary>
    /// Price every capability in the cycle under the method in force.
    /// </summary>
    /// <remarks>
    /// The cycle must be loaded with <c>Capabilities</c> and <c>Costs</c>; the loader
    /// throws rather than quietly pricing an unloaded collection at zero.
    /// </remarks>
    public CycleRates Calculate(RicCycle cycle) => Calculate(cycle, methods.Current);

    /// <summary>
    /// Price every capability under a named method version — used when reopening a sealed
    /// record, so it reproduces its own figures rather than today's (rule R6).
    /// </summary>
    public CycleRates CalculateAsAt(RicCycle cycle, string? methodVersion) =>
        Calculate(cycle, methods.ForVersion(methodVersion));

    private static CycleRates Calculate(RicCycle cycle, MethodConfig method)
    {
        ArgumentNullException.ThrowIfNull(cycle);

        var results = new Dictionary<int, CapabilityRateResult>();
        var errors = new Dictionary<int, string>();

        foreach (var capability in cycle.Capabilities)
        {
            try
            {
                results[capability.Id] = RateEngine.Calculate(InputsFor(cycle, capability), method);
            }
            catch (RateCalculationException error)
            {
                errors[capability.Id] = error.Message;
            }
        }

        return new CycleRates(method, results, errors);
    }

    /// <summary>
    /// Assemble one capability's inputs from the stored cycle.
    ///
    /// The four predicates below are deliberately <b>mutually exclusive</b>: a row is a
    /// capability cost, a platform cost, a capability income or a platform income, and
    /// never two of those. That is rule R8 — a total that could be summed over a different
    /// set from the figures it is compared against is the workbook's defect, and the fix is
    /// to make it unrepresentable rather than merely unlikely.
    /// </summary>
    private static CapabilityRateInputs InputsFor(RicCycle cycle, RicCapability capability)
    {
        // Platform-level amounts are split evenly across capability columns [W, sheet 1].
        // A cycle with no capabilities never reaches here — the loop above has nothing to
        // iterate — so the divisor cannot be zero.
        var capabilityCount = Math.Max(1, cycle.Capabilities.Count);

        decimal CapabilityLines(bool income) => cycle.Costs
            .Where(x => x.RicCapabilityId == capability.Id && x.IsIncome == income)
            .Sum(x => x.Amount);

        decimal PlatformShare(bool income, Func<RicCostEntry, bool> also) => cycle.Costs
            .Where(x => x.RicCapabilityId is null && x.IsIncome == income && also(x))
            .Sum(x => x.Amount) / capabilityCount;

        return new CapabilityRateInputs
        {
            CapabilityName = capability.Name,

            CapabilityOperatingCost = CapabilityLines(income: false),
            AllocatedPlatformCost = PlatformShare(income: false, _ => true),

            // Income is scoped exactly as cost is. A grant booked against one capability
            // belongs to that capability; only platform-level income is shared out. Before
            // this was fixed, every income line was divided across every capability
            // regardless of how it had been entered, which moved all three rates.
            UwaIncome = cycle.Costs
                .Where(x => x.RicCapabilityId == capability.Id && x.IsIncome && x.IsUwaIncome)
                .Sum(x => x.Amount)
                + PlatformShare(income: true, x => x.IsUwaIncome),

            NonUwaIncome = cycle.Costs
                .Where(x => x.RicCapabilityId == capability.Id && x.IsIncome && !x.IsUwaIncome)
                .Sum(x => x.Amount)
                + PlatformShare(income: true, x => !x.IsUwaIncome),

            ForecastUwaUse = capability.ForecastUwaUse,
            ForecastApfrUse = capability.ForecastApfrUse,
            ForecastCommercialUse = capability.ForecastCommercialUse,

            ProposedUwaRate = capability.ProposedUwaRate,
            ProposedApfrRate = capability.ProposedApfrRate,
            ProposedCommercialRate = capability.ProposedCommercialRate
        };
    }
}

/// <summary>
/// Every capability's rates for one cycle, plus the platform roll-up.
///
/// A capability that could not be priced has an explanation instead of a number. Pages ask
/// for one or the other; there is no path on which a page receives a zero and mistakes it
/// for an answer.
/// </summary>
public sealed class CycleRates(
    MethodConfig method,
    Dictionary<int, CapabilityRateResult> results,
    Dictionary<int, string> errors)
{
    public MethodConfig Method { get; } = method;

    /// <summary>Every capability priced successfully. Aggregates iterate this — rule R8.</summary>
    public IReadOnlyCollection<CapabilityRateResult> All => results.Values;

    /// <summary>True when every capability in the cycle produced rates.</summary>
    public bool IsComplete => errors.Count == 0;

    /// <summary>The reasons capabilities could not be priced, in the order they were found.</summary>
    public IReadOnlyCollection<string> Problems => errors.Values;

    public CapabilityRateResult? For(int capabilityId) =>
        results.TryGetValue(capabilityId, out var result) ? result : null;

    public string? ProblemFor(int capabilityId) =>
        errors.TryGetValue(capabilityId, out var message) ? message : null;

    // The platform figure is a roll-up over the capability collection, never a stored
    // total — architecture.md §4.
    public decimal TotalOperatingCost => results.Values.Sum(x => x.TotalOperatingCost);

    public decimal ForecastRevenue => results.Values.Sum(x => x.ForecastRevenue);

    public decimal ForecastBalance => ForecastRevenue - TotalOperatingCost;

    /// <summary>Round a rolled-up figure by the same rule the engine applies to a rate.</summary>
    public decimal Round(decimal value) => Math.Round(value, Method.RateDecimals, Method.MidpointRule);
}
