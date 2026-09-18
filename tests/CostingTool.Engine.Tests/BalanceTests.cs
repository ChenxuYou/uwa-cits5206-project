using Xunit;

namespace CostingTool.Engine.Tests;

/// <summary>
/// US-12: what the proposed rates do to the balance.
///
/// The rates themselves are held to the client's worked example in
/// <see cref="GoldenFileTests"/>. These tests hold the projection built on top of them: what
/// the platform bills, what it keeps once the University's uplift is separated out, and what
/// that leaves against the cost usage has to recover.
///
/// <b>The measure changed on 18 September 2026.</b> The balance used to compare revenue with
/// the full operating cost; US-12 asks for it against "total cost less income", because
/// non-variable income is money the platform already holds and charging for it twice
/// overstates the shortfall. The old measure is kept as
/// <c>FullEconomicCostBalance</c> and asserted here too, so neither can drift unnoticed.
/// </summary>
public class BalanceTests
{
    /// <summary>
    /// The guide's worked example, priced at exactly the rates it produces:
    /// $150,000 cost, $20,000 UWA and $30,000 non-UWA income, 1,000 hours split 600/250/150.
    /// </summary>
    private static CapabilityRateInputs PricedAtTheCalculatedRates() => new()
    {
        CapabilityName = "Guide worked example",
        CapabilityOperatingCost = 150_000m,
        UwaIncome = 20_000m,
        NonUwaIncome = 30_000m,
        ForecastUwaUse = 600m,
        ForecastApfrUse = 250m,
        ForecastCommercialUse = 150m,
        ProposedUwaRate = 100.00m,
        ProposedApfrRate = 162.00m,
        ProposedCommercialRate = 202.50m
    };

    private static CapabilityRateResult Result(CapabilityRateInputs inputs) =>
        RateEngine.Calculate(inputs, MethodConfig.Fallback);

    [Fact]
    public void CostToRecoverIsTheOperatingCostLessNonVariableIncome()
    {
        var result = Result(PricedAtTheCalculatedRates());

        // 150,000 - (20,000 + 30,000)
        Assert.Equal(100_000m, result.NetCostToRecover);
    }

    [Fact]
    public void WhatUsersAreBilledIsSeparateFromWhatThePlatformKeeps()
    {
        var result = Result(PricedAtTheCalculatedRates());

        // 600 x 100 + 250 x 162 + 150 x 202.50
        Assert.Equal(130_875m, result.GrossForecastRevenue);

        // The same use at the rates with the uplift divided back out.
        Assert.Equal(112_500m, result.ForecastRevenue);

        // The difference is the University's indirect cost recovery, on its own line.
        Assert.Equal(18_375m, result.OverheadsRecovered);
    }

    [Fact]
    public void TheBalanceIsMeasuredAgainstCostLessIncome()
    {
        var result = Result(PricedAtTheCalculatedRates());

        // 112,500 retained against 100,000 to recover.
        Assert.Equal(12_500m, result.ForecastBalance);
    }

    [Fact]
    public void TheSameRevenueAgainstFullEconomicCostIsKeptAsItsOwnFigure()
    {
        var result = Result(PricedAtTheCalculatedRates());

        // 112,500 retained against the whole 150,000 — negative by design, because the
        // income deducted from the UWA rate is carrying the rest.
        Assert.Equal(-37_500m, result.FullEconomicCostBalance);
    }

    [Fact]
    public void RatesProposedBelowTheCalculatedOnesTurnTheSurplusIntoADeficit()
    {
        var result = Result(PricedAtTheCalculatedRates() with
        {
            ProposedUwaRate = 80.00m,
            ProposedApfrRate = 120.00m,
            ProposedCommercialRate = 150.00m
        });

        Assert.True(result.ForecastBalance < 0);
        Assert.True(result.VariesFromCalculated);
    }

    [Fact]
    public void ARateStillLeftAtZeroIsAnUnansweredQuestionRatherThanAVariance()
    {
        var result = Result(PricedAtTheCalculatedRates() with
        {
            ProposedUwaRate = 0m,
            ProposedApfrRate = 0m,
            ProposedCommercialRate = 0m
        });

        // Nothing has been proposed yet, so there is nothing to justify. Submission is what
        // insists on three rates (US-14), not this screen.
        Assert.False(result.VariesFromCalculated);
    }

    [Fact]
    public void AFractionOfACentTheScreenNeverShowedIsNotAVariance()
    {
        // The custodian accepts the rate the screen showed them, to the cent.
        var result = Result(PricedAtTheCalculatedRates() with
        {
            CapabilityOperatingCost = 150_000.004m,
            ProposedUwaRate = 100.00m,
            ProposedApfrRate = 162.00m,
            ProposedCommercialRate = 202.50m
        });

        Assert.False(result.VariesFromCalculated);
    }

    [Fact]
    public void TheVarianceIsReportedInDollarsAndAsAPercentage()
    {
        var result = Result(PricedAtTheCalculatedRates() with { ProposedUwaRate = 120.00m });

        Assert.Equal(20.00m, result.UwaRateVariance);
        Assert.Equal(20m, result.UwaRateVariancePercent);
        Assert.True(result.VariesFromCalculated);
        Assert.False(result.CommercialRateVaries);
    }

    [Fact]
    public void AVariancePercentageIsNotInventedWhenThereIsNoRateToVaryFrom()
    {
        // Income equal to cost leaves a calculated UWA rate of zero; a percentage against it
        // would be a division by zero dressed up as an answer.
        var result = Result(PricedAtTheCalculatedRates() with
        {
            UwaIncome = 120_000m,
            NonUwaIncome = 30_000m
        });

        Assert.Equal(0m, result.UwaRate);
        Assert.Null(result.UwaRateVariancePercent);
    }
}
