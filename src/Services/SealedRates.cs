using CostingTool.Engine;
using CostingTool.Models;
using CostingTool.Pdf;

namespace CostingTool.Data;

/// <summary>
/// A sealed cycle's figures, read back from its snapshot — never recalculated (US-15, N6).
///
/// <b>Why not recalculate under the sealed method version?</b> That used to be how a sealed
/// cycle was shown, and it rests on two things staying true forever: that the method
/// version is still in the table, and that the rows under the cycle have not changed. When
/// the version was missing, <see cref="MethodConfigProvider.ForVersion"/> fell back to
/// today's method, so a record could quietly show figures nobody approved. The snapshot
/// holds the figures as they were computed at the seal; showing those needs neither.
///
/// The result is an ordinary <see cref="CycleRates"/>, so the Review, approver and
/// administrator pages render a sealed record with the same markup as a live one. A
/// snapshot that cannot be read gives a rate set with every capability unpriced and the
/// reason stated — a gap on the page, never a recalculated number in its place.
/// </summary>
public static class SealedRates
{
    public static CycleRates Of(RicCycle cycle)
    {
        ArgumentNullException.ThrowIfNull(cycle);

        SealedRecord record;
        try
        {
            record = SealedRecord.Parse(cycle.SnapshotJson ?? string.Empty);
        }
        catch (SealedRecordFormatException error)
        {
            return Unreadable(cycle, error.Message);
        }

        var method = MethodOf(record);
        var results = new Dictionary<int, CapabilityRateResult>();
        var errors = new Dictionary<int, string>();

        foreach (var capability in record.Capabilities)
        {
            if (capability.Result is { } result)
            {
                results[capability.Id] = ResultOf(capability, result, method);
            }
            else
            {
                errors[capability.Id] = $"{capability.Name ?? "This capability"} has no rates in the sealed snapshot.";
            }
        }

        return new CycleRates(method, results, errors);
    }

    private static CycleRates Unreadable(RicCycle cycle, string reason) =>
        new(
            new MethodConfig { Version = cycle.MethodVersion },
            [],
            cycle.Capabilities.ToDictionary(x => x.Id, _ => reason));

    private static MethodConfig MethodOf(SealedRecord record)
    {
        var method = record.Method;
        return new MethodConfig
        {
            Version = method?.Version ?? record.MethodVersion ?? string.Empty,
            IndirectCostRecovery = method?.IndirectCostRecovery ?? 0m,
            RateDecimals = method?.RateDecimals ?? 2,
            MidpointRule = Enum.TryParse<MidpointRounding>(method?.MidpointRule, out var rule)
                ? rule
                : MidpointRounding.AwayFromZero,
            Source = method?.Source ?? string.Empty
        };
    }

    /// <summary>
    /// The engine's answer, rebuilt field for field from what was stored. The unrounded rates
    /// are in every snapshot the application has written; the displayed ones stand in only
    /// if a snapshot lacks them, which rounds to the same figure on the page.
    /// </summary>
    private static CapabilityRateResult ResultOf(SealedCapability capability, SealedResult result, MethodConfig method) => new()
    {
        CapabilityName = result.CapabilityName ?? capability.Name ?? string.Empty,
        MethodVersion = method.Version,
        IndirectCostRecovery = method.IndirectCostRecovery,
        RateDecimals = method.RateDecimals,
        MidpointRule = method.MidpointRule,

        CapabilityOperatingCost = result.CapabilityOperatingCost,
        AllocatedPlatformCost = result.AllocatedPlatformCost,
        TotalOperatingCost = result.TotalOperatingCost,
        UwaIncome = result.UwaIncome,
        NonUwaIncome = result.NonUwaIncome,
        ForecastUtilisation = result.ForecastUtilisation,

        ForecastUwaUse = capability.ForecastUwaUse,
        ForecastApfrUse = capability.ForecastApfrUse,
        ForecastCommercialUse = capability.ForecastCommercialUse,

        UwaRate = result.UwaRate ?? result.DisplayUwaRate,
        ApfrRate = result.ApfrRate ?? result.DisplayApfrRate,
        CommercialRate = result.CommercialRate ?? result.DisplayCommercialRate,

        ProposedUwaRate = result.ProposedUwaRate,
        ProposedApfrRate = result.ProposedApfrRate,
        ProposedCommercialRate = result.ProposedCommercialRate,

        // A schema 1.1 record did not store the gross figure; overheads then read as zero
        // rather than being worked out again.
        GrossForecastRevenue = result.GrossForecastRevenue ?? result.ForecastRevenue,
        ForecastRevenue = result.ForecastRevenue
    };
}
