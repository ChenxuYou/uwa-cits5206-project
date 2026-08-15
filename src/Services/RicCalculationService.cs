using CostingTool.Models;

namespace CostingTool.Data;

public class RicCalculationService
{
    public CapabilityRateResult Calculate(RicCycle cycle, RicCapability capability)
    {
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
            totalForecastUse > 0 ? ((totalOperatingCost - nonUwaIncome) / totalForecastUse) * 1.35m : 0,
            totalForecastUse > 0 ? (totalOperatingCost / totalForecastUse) * 1.35m : 0,
            uwaUse * capability.ProposedUwaRate + apfrUse * (capability.ProposedApfrRate / 1.35m) + commercialUse * (capability.ProposedCommercialRate / 1.35m));
    }
}

public record CapabilityRateResult(
    decimal TotalOperatingCost,
    decimal UwaRate,
    decimal ApfrRate,
    decimal CommercialRate,
    decimal ForecastPlatformRevenue)
{
    public decimal ForecastBalance => ForecastPlatformRevenue - TotalOperatingCost;
}
