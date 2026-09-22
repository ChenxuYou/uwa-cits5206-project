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

    /// <summary>
    /// Where every operating cost in the cycle sits: against which capability, or at platform
    /// level and split across them (US-03, US-04).
    ///
    /// The costs screen shows this as a running total, and <see cref="InputsFor"/> prices
    /// from it, so the figure a custodian watches while typing is the figure the engine
    /// receives — never a second summation that could be taken over a different set of lines
    /// (N14, rule R8). It needs no capacity or utilisation, so it works before either exists.
    /// </summary>
    public static CycleCosts CostsOf(RicCycle cycle)
    {
        ArgumentNullException.ThrowIfNull(cycle);

        var costLines = cycle.Costs.Where(x => !x.IsIncome).ToList();
        var platformLines = costLines.Where(x => x.RicCapabilityId is null).ToList();

        return new CycleCosts(
            cycle.Capabilities
                .Select(capability => new CapabilityCosts(
                    capability.Id,
                    capability.Name,
                    costLines.Where(x => x.RicCapabilityId == capability.Id).Sum(x => x.Amount)))
                .ToList(),
            directlyAllocated: platformLines.Where(x => !CostEntry.CostCategories.IsFloorArea(x.Category)).Sum(x => x.Amount),
            indirect: platformLines.Where(x => CostEntry.CostCategories.IsFloorArea(x.Category)).Sum(x => x.Amount));
    }

    private static CycleRates Calculate(RicCycle cycle, MethodConfig method)
    {
        ArgumentNullException.ThrowIfNull(cycle);

        var results = new Dictionary<int, CapabilityRateResult>();
        var errors = new Dictionary<int, string>();
        var costs = CostsOf(cycle);

        foreach (var capability in cycle.Capabilities)
        {
            try
            {
                results[capability.Id] = RateEngine.Calculate(InputsFor(cycle, capability, costs), method);
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
    private static CapabilityRateInputs InputsFor(RicCycle cycle, RicCapability capability, CycleCosts costs)
    {
        // Platform-level amounts are split evenly across capability columns [W, sheet 1].
        // A cycle with no capabilities never reaches here — the loop above has nothing to
        // iterate — so the divisor cannot be zero.
        var capabilityCount = Math.Max(1, cycle.Capabilities.Count);

        decimal PlatformShare(bool income, Func<RicCostEntry, bool> also) => cycle.Costs
            .Where(x => x.RicCapabilityId is null && x.IsIncome == income && also(x))
            .Sum(x => x.Amount) / capabilityCount;

        return new CapabilityRateInputs
        {
            CapabilityName = capability.Name,

            // Cost comes from the same roll-up the costs screen shows, so the two agree.
            CapabilityOperatingCost = costs.For(capability.Id).DirectlyIncurred,
            AllocatedPlatformCost = costs.AllocatedToEach,

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
/// The operating costs of a cycle, by where they sit (US-03, US-04).
///
/// <b>The allocation rule is an even split</b> across every capability in the cycle
/// [W, sheet 1: <c>=$C$27/COUNTA($D$8:$J$8)</c>], one divisor for every platform-level line.
/// The workbook divides administration and the platform leader's salary by one more column —
/// Analysis/Consulting — than its other platform rows (requirements §4 Step 1). This tool does
/// not: an Analysis/Consulting line is costed as a capability, and takes the same share of
/// every platform-level line as the others. The costs screen says so in words.
/// </summary>
public sealed class CycleCosts(IReadOnlyList<CapabilityCosts> capabilities, decimal directlyAllocated, decimal indirect)
{
    public IReadOnlyList<CapabilityCosts> Capabilities { get; } =
        capabilities.Select(x => x with { Allocated = Share(directlyAllocated + indirect, capabilities.Count) }).ToList();

    /// <summary>Platform-level lines other than floor area [W, sheet 1 rows 27–36].</summary>
    public decimal DirectlyAllocated { get; } = directlyAllocated;

    /// <summary>Floor area at a rate per m² [W, sheet 1 rows 40–41].</summary>
    public decimal Indirect { get; } = indirect;

    public decimal PlatformLevel => DirectlyAllocated + Indirect;

    /// <summary>How many ways each platform-level line is split: every capability in the cycle.</summary>
    public int Divisor => Capabilities.Count;

    /// <summary>Each capability's share of the platform-level lines.</summary>
    public decimal AllocatedToEach => Share(PlatformLevel, Divisor);

    public decimal DirectlyIncurred => Capabilities.Sum(x => x.DirectlyIncurred);

    /// <summary>Every operating cost line, counted once.</summary>
    public decimal Total => DirectlyIncurred + PlatformLevel;

    /// <summary>
    /// True when the capability totals add back up to <see cref="Total"/> to the cent — the
    /// reconciliation US-03 asks the screen to show. A platform line split three ways leaves
    /// a fraction of a cent that no screen shows, so the comparison is made at the cent.
    /// With no capabilities there is nothing to split, and nothing reconciles.
    /// </summary>
    public bool Reconciles =>
        Divisor > 0 && Math.Round(Capabilities.Sum(x => x.Total), 2) == Math.Round(Total, 2);

    public CapabilityCosts For(int capabilityId) =>
        Capabilities.FirstOrDefault(x => x.CapabilityId == capabilityId)
        ?? new CapabilityCosts(capabilityId, string.Empty, 0m) { Allocated = AllocatedToEach };

    private static decimal Share(decimal amount, int ways) => ways == 0 ? 0m : amount / ways;
}

/// <summary>One capability's operating cost: its own lines, and its share of the platform's.</summary>
public sealed record CapabilityCosts(int CapabilityId, string Name, decimal DirectlyIncurred)
{
    /// <summary>Allocated from platform-level costs — never incurred by the capability directly (US-04).</summary>
    public decimal Allocated { get; init; }

    public decimal Total => DirectlyIncurred + Allocated;
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

    /// <summary>Non-variable income carried by the priced capabilities.</summary>
    public decimal TotalIncome => results.Values.Sum(x => x.TotalIncome);

    /// <summary>Operating cost less non-variable income — what usage has to recover.</summary>
    public decimal NetCostToRecover => results.Values.Sum(x => x.NetCostToRecover);

    /// <summary>Billed to users at the proposed rates, uplift included.</summary>
    public decimal GrossForecastRevenue => results.Values.Sum(x => x.GrossForecastRevenue);

    public decimal ForecastRevenue => results.Values.Sum(x => x.ForecastRevenue);

    /// <summary>The University's indirect cost recovery, on its own line (US-12).</summary>
    public decimal OverheadsRecovered => results.Values.Sum(x => x.OverheadsRecovered);

    /// <summary>
    /// Surplus or deficit at the proposed rates, against cost less income — the same
    /// measure the capability figures use, summed over the capabilities that priced.
    /// </summary>
    public decimal ForecastBalance => ForecastRevenue - NetCostToRecover;

    /// <summary>The same projection against full economic cost, with no income deducted.</summary>
    public decimal FullEconomicCostBalance => ForecastRevenue - TotalOperatingCost;

    /// <summary>Round a rolled-up figure by the same rule the engine applies to a rate.</summary>
    public decimal Round(decimal value) => Math.Round(value, Method.RateDecimals, Method.MidpointRule);
}
