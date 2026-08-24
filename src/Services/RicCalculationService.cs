using CostingTool.Models;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Data;

/// <summary>
/// The calculation engine.
///
/// Pure: the same inputs and the same <see cref="MethodConfig"/> produce the same
/// outputs, always. No database, no HTTP, no clock, no randomness — which is what makes
/// it testable against the client's worked example and what makes a 2026 record
/// reproducible in 2030. See <c>docs/spec/architecture.md</c> §3, rules R1, R5, R6, R7.
///
/// Nothing in here reads a constant. <c>k</c> arrives as configuration.
/// </summary>
public static class RateEngine
{
    public static CapabilityRateResult Calculate(RicCycle cycle, RicCapability capability, MethodConfig method)
    {
        ArgumentNullException.ThrowIfNull(cycle);
        ArgumentNullException.ThrowIfNull(capability);
        ArgumentNullException.ThrowIfNull(method);

        // k — the indirect cost recovery factor, from versioned method configuration.
        var k = method.IndirectCostRecovery;

        var capabilityCosts = cycle.Costs
            .Where(x => x.RicCapabilityId == capability.Id && x.CostType != "Non-variable income")
            .Sum(x => x.Amount);

        var platformCosts = cycle.Costs
            .Where(x => x.Scope == "Platform" && x.CostType != "Non-variable income")
            .Sum(x => x.Amount);

        var capabilityCount = Math.Max(1, cycle.Capabilities.Count);
        var allocatedPlatformCost = platformCosts / capabilityCount;
        var totalOperatingCost = capabilityCosts + allocatedPlatformCost;

        var uwaIncome = cycle.Costs
            .Where(x => x.CostType == "Non-variable income" && x.Category == "UWA GP / in-kind")
            .Sum(x => x.Amount) / capabilityCount;
        var nonUwaIncome = cycle.Costs
            .Where(x => x.CostType == "Non-variable income" && x.Category != "UWA GP / in-kind")
            .Sum(x => x.Amount) / capabilityCount;

        var uwaUse = capability.ForecastUwaUse;
        var apfrUse = capability.ForecastApfrUse;
        var commercialUse = capability.ForecastCommercialUse;
        var totalForecastUse = uwaUse + apfrUse + commercialUse;

        return new CapabilityRateResult(
            totalOperatingCost,
            totalForecastUse > 0 ? (totalOperatingCost - uwaIncome - nonUwaIncome) / totalForecastUse : 0,
            totalForecastUse > 0 ? ((totalOperatingCost - nonUwaIncome) / totalForecastUse) * k : 0,
            totalForecastUse > 0 ? (totalOperatingCost / totalForecastUse) * k : 0,
            uwaUse * capability.ProposedUwaRate
                + apfrUse * (capability.ProposedApfrRate / k)
                + commercialUse * (capability.ProposedCommercialRate / k),
            method.Version);
    }
}

/// <summary>
/// Resolves the method configuration in force. Kept separate from the engine so that the
/// engine itself never touches a database row.
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
/// Thin application-facing wrapper: picks up the configuration in force and hands it to
/// the pure engine. Page models depend on this; the engine stays independent of them.
/// </summary>
public class RicCalculationService(MethodConfigProvider methods)
{
    /// <summary>The method configuration these results were produced under.</summary>
    public MethodConfig Method => methods.Current;

    public CapabilityRateResult Calculate(RicCycle cycle, RicCapability capability) =>
        RateEngine.Calculate(cycle, capability, methods.Current);

    /// <summary>Recalculate under a named method version — used when reopening a sealed record.</summary>
    public CapabilityRateResult Calculate(RicCycle cycle, RicCapability capability, string? methodVersion) =>
        RateEngine.Calculate(cycle, capability, methods.ForVersion(methodVersion));
}

public record CapabilityRateResult(
    decimal TotalOperatingCost,
    decimal UwaRate,
    decimal ApfrRate,
    decimal CommercialRate,
    decimal ForecastPlatformRevenue,
    string MethodVersion = "")
{
    public decimal ForecastBalance => ForecastPlatformRevenue - TotalOperatingCost;
}
