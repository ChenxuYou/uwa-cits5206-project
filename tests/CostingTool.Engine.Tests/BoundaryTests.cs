using CostingTool.Engine;
using Xunit;

namespace CostingTool.Engine.Tests;

/// <summary>
/// The edges, where a costing tool does its real damage.
///
/// A wrong rate that looks wrong gets caught by the custodian. A wrong rate that looks
/// plausible gets published for three to five years and then defended in a Freedom of
/// Information response — which is the risk the whole project exists to remove
/// (risks.md §1). These tests are about the second kind.
/// </summary>
public class BoundaryTests
{
    private static MethodConfig Method() => MethodConfig.Fallback;

    private static CapabilityRateInputs Baseline() => new()
    {
        CapabilityName = "Cryo-EM",
        CapabilityOperatingCost = 150_000m,
        UwaIncome = 20_000m,
        NonUwaIncome = 30_000m,
        ForecastUwaUse = 1_000m
    };

    [Fact]
    public void ZeroForecastUtilisationIsRefused()
    {
        // Rule R4. The old engine returned 0.00 for all three rates here, and $0.00 an hour
        // reads like an answer.
        var inputs = Baseline() with { ForecastUwaUse = 0m };

        var error = Assert.Throws<RateCalculationException>(() => RateEngine.Calculate(inputs, Method()));

        Assert.Contains("Cryo-EM", error.Message);
        Assert.Contains("forecast", error.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NegativeForecastUtilisationIsRefused()
    {
        var inputs = Baseline() with { ForecastUwaUse = -1m };

        Assert.Throws<RateCalculationException>(() => RateEngine.Calculate(inputs, Method()));
    }

    [Fact]
    public void NegativeCostIsRefusedRatherThanTreatedAsIncome()
    {
        // A cost typed as a negative number would otherwise behave like income that is
        // deducted from every rate equally — which is wrong, because the three formulas
        // deduct different subsets of income.
        var inputs = Baseline() with { CapabilityOperatingCost = -5_000m };

        Assert.Throws<RateCalculationException>(() => RateEngine.Calculate(inputs, Method()));
    }

    [Fact]
    public void IncomeExceedingCostProducesANegativeUwaRateRatherThanAnError()
    {
        // This is a real situation, not a mistake: a heavily grant-funded capability can
        // cost a UWA researcher nothing. The tool must show the figure and let the
        // custodian propose a rate of zero, rather than refusing to calculate.
        var inputs = Baseline() with { UwaIncome = 400_000m };

        var result = RateEngine.Calculate(inputs, Method());

        Assert.True(result.UwaRate < 0);

        // The commercial rate deducts no income at all, so it is unaffected.
        Assert.Equal(202.50m, result.DisplayCommercialRate);
    }

    [Fact]
    public void TheSplitAcrossUserCategoriesDoesNotChangeTheRates()
    {
        // Q2, closed against the client's workbook: a single U per capability drives the
        // rates, and the per-category split drives only the balance projection.
        var allUwa = Baseline() with
        {
            ForecastUwaUse = 1_000m, ForecastApfrUse = 0m, ForecastCommercialUse = 0m
        };

        var spread = Baseline() with
        {
            ForecastUwaUse = 400m, ForecastApfrUse = 350m, ForecastCommercialUse = 250m
        };

        Assert.Equal(
            RateEngine.Calculate(allUwa, Method()).UwaRate,
            RateEngine.Calculate(spread, Method()).UwaRate);
    }

    [Fact]
    public void ChangingTheMethodVersionChangesTheUpliftButNotTheUwaRate()
    {
        // k is versioned configuration, not a constant (rule R5). Only the two external
        // rates carry the uplift, so a change to k must leave the UWA rate alone.
        var inputs = Baseline();
        var revised = new MethodConfig { Version = "2027.1", IndirectCostRecovery = 1.40m, RateDecimals = 2 };

        var before = RateEngine.Calculate(inputs, Method());
        var after = RateEngine.Calculate(inputs, revised);

        Assert.Equal(before.UwaRate, after.UwaRate);
        Assert.Equal(210.00m, after.DisplayCommercialRate);
        Assert.Equal("2027.1", after.MethodVersion);
    }

    [Fact]
    public void StoredValuesStayUnroundedAndRoundingHappensOnceAtPresentation()
    {
        // Rule R3. $100,000 over 3 hours is 33,333.333... — the stored figure keeps its
        // tail so that totals reconcile, and only the presented figure is rounded.
        var inputs = Baseline() with
        {
            CapabilityOperatingCost = 100_000m,
            UwaIncome = 0m,
            NonUwaIncome = 0m,
            ForecastUwaUse = 3m
        };

        var result = RateEngine.Calculate(inputs, Method());

        Assert.NotEqual(result.UwaRate, result.DisplayUwaRate);
        Assert.Equal(33_333.33m, result.DisplayUwaRate);
    }

    [Fact]
    public void AHalfCentRoundsAwayFromZero()
    {
        // $1.005 per unit. The convention is ours, not the client's — MethodConfig says so
        // — but it is applied consistently and it is versioned, so a sealed record keeps
        // the rule it was sealed under.
        var inputs = Baseline() with
        {
            CapabilityOperatingCost = 10.05m,
            UwaIncome = 0m,
            NonUwaIncome = 0m,
            ForecastUwaUse = 10m
        };

        var result = RateEngine.Calculate(inputs, Method());

        Assert.Equal(1.01m, result.DisplayUwaRate);
    }

    [Fact]
    public void VeryLargeAmountsDoNotOverflowOrLosePrecision()
    {
        // A national-scale facility. decimal carries 28 significant digits, so this is
        // comfortably inside range — asserted so that a future change to the numeric type
        // cannot pass unnoticed.
        var inputs = Baseline() with
        {
            CapabilityOperatingCost = 999_999_999.99m,
            UwaIncome = 0m,
            NonUwaIncome = 0m,
            ForecastUwaUse = 1m
        };

        var result = RateEngine.Calculate(inputs, Method());

        Assert.Equal(999_999_999.99m, result.DisplayUwaRate);
    }

    [Fact]
    public void TheSameInputsAlwaysProduceTheSameResult()
    {
        // Rule R1, and the reason a 2026 record still reproduces its figures in 2030.
        var inputs = Baseline();

        var first = RateEngine.Calculate(inputs, Method());
        var second = RateEngine.Calculate(inputs, Method());

        Assert.Equal(first, second);
    }
}
