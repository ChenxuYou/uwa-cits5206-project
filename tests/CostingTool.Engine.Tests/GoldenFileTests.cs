using CostingTool.Engine;
using Xunit;

namespace CostingTool.Engine.Tests;

/// <summary>
/// The client's own worked example, asserted to the cent.
///
/// This is the first test on the project and the one the CI merge gate exists for
/// (plan.md M1, ADR-001 follow-on action 4). If it fails, a rate this tool would publish
/// no longer matches the University's guide, and nothing merges.
///
/// Source: the client's costing &amp; pricing guide, Step 3 — <b>[G]</b>. Figures
/// transcribed from the guide, never from the recorded walkthrough; the walkthrough
/// figures were withdrawn from the repository for exactly that reason
/// (architecture.md §3, "Withdrawn fixtures").
/// </summary>
public class GoldenFileTests
{
    /// <summary>
    /// The guide's example, expressed as engine inputs.
    ///
    /// $150,000 operating costs · $20,000 UWA in-kind · $30,000 WA Government support ·
    /// 1,000 forecast hours.
    /// </summary>
    private static CapabilityRateInputs GuideWorkedExample() => new()
    {
        CapabilityName = "Guide worked example",
        CapabilityOperatingCost = 150_000m,
        AllocatedPlatformCost = 0m,
        UwaIncome = 20_000m,
        NonUwaIncome = 30_000m,

        // A single capability with 1,000 forecast hours. The split across user categories
        // does not change the rates — it drives only the revenue projection (Q2).
        ForecastUwaUse = 600m,
        ForecastApfrUse = 250m,
        ForecastCommercialUse = 150m
    };

    private static MethodConfig Method2026() => MethodConfig.Fallback;

    [Fact]
    public void UwaResearcherRateIsOneHundredDollarsPerHour()
    {
        var result = RateEngine.Calculate(GuideWorkedExample(), Method2026());

        // (150,000 - 50,000) / 1,000
        Assert.Equal(100.00m, result.DisplayUwaRate);
    }

    [Fact]
    public void ApfrRateIsOneHundredAndSixtyTwoDollarsPerHour()
    {
        var result = RateEngine.Calculate(GuideWorkedExample(), Method2026());

        // ((150,000 - 30,000) / 1,000) x 1.35
        Assert.Equal(162.00m, result.DisplayApfrRate);
    }

    [Fact]
    public void CommercialRateIsTwoHundredAndTwoDollarsFiftyPerHour()
    {
        var result = RateEngine.Calculate(GuideWorkedExample(), Method2026());

        // (150,000 / 1,000) x 1.35
        Assert.Equal(202.50m, result.DisplayCommercialRate);
    }

    [Fact]
    public void TheWorkingsBehindEachRateAreReturned()
    {
        // The client asked that the sealed record show the workings, not only the answers
        // (requirements §9, Q5). A record cannot show a working the engine never returned.
        var result = RateEngine.Calculate(GuideWorkedExample(), Method2026());

        Assert.Equal(150_000m, result.TotalOperatingCost);
        Assert.Equal(50_000m, result.TotalIncome);
        Assert.Equal(30_000m, result.NonUwaIncome);
        Assert.Equal(1_000m, result.ForecastUtilisation);
        Assert.Equal(1.35m, result.IndirectCostRecovery);
        Assert.Equal("2026.1", result.MethodVersion);
    }

    [Fact]
    public void PlatformCostsAreCarriedIntoTheCapabilityTotal()
    {
        // Platform-level costs are split across capabilities by the caller; the engine adds
        // this capability's share to its own costs. Here, $30,000 of platform cost split
        // four ways adds $7,500, so C becomes $157,500 and the UWA rate $107.50.
        var inputs = GuideWorkedExample() with { AllocatedPlatformCost = 7_500m };

        var result = RateEngine.Calculate(inputs, Method2026());

        Assert.Equal(157_500m, result.TotalOperatingCost);
        Assert.Equal(107.50m, result.DisplayUwaRate);
    }

    [Fact]
    public void ArithmeticIsDecimalRatherThanBinaryFloatingPoint()
    {
        // Rule R2. With binary floating point the three tenths below would not sum to a
        // figure that divides cleanly, and a cent would appear or disappear in an FOI
        // response. Asserted rather than assumed, because it is a claim we make in writing.
        var inputs = GuideWorkedExample() with
        {
            CapabilityOperatingCost = 0.10m + 0.20m,
            UwaIncome = 0m,
            NonUwaIncome = 0m,
            ForecastUwaUse = 0.30m,
            ForecastApfrUse = 0m,
            ForecastCommercialUse = 0m
        };

        var result = RateEngine.Calculate(inputs, Method2026());

        Assert.Equal(1.00m, result.DisplayUwaRate);
    }
}
