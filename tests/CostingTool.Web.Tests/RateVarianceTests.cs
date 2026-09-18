using CostingTool.Pages.Ric;
using Xunit;

namespace CostingTool.Web.Tests;

/// <summary>
/// The variance shown under each proposed-rate box (US-11). The sentence itself is built
/// with the current culture's currency format, so these tests pin the numbers and the
/// culture-independent wording; the exact dollar text is checked in the browser.
/// </summary>
public class RateVarianceTests
{
    [Fact]
    public void ProposedAboveCalculatedIsPositiveInDollarsAndPercent()
    {
        // The client's workbook proposes round rates against calculated ones such as $36.64.
        var variance = RateVariance.Between(36.64m, 50m);

        Assert.Equal(13.36m, variance.Amount);
        Assert.Equal(36.46m, Math.Round(variance.Percent!.Value, 2));
        Assert.True(variance.IsVaried);
    }

    [Fact]
    public void ProposedBelowCalculatedIsNegative()
    {
        var variance = RateVariance.Between(100m, 90m);

        Assert.Equal(-10m, variance.Amount);
        Assert.Equal(-10m, variance.Percent);
    }

    [Fact]
    public void ProposedEqualToCalculatedIsNotVaried()
    {
        Assert.False(RateVariance.Between(36.64m, 36.64m).IsVaried);
    }

    [Fact]
    public void CalculatedRateOfZeroHasNoPercentage()
    {
        // Income can cover the whole cost, leaving a calculated rate of zero.
        var variance = RateVariance.Between(0m, 5m);

        Assert.Equal(5m, variance.Amount);
        Assert.Null(variance.Percent);
    }

    [Fact]
    public void PercentageKeepsTheSignOfTheAmountWhenTheCalculatedRateIsNegative()
    {
        var variance = RateVariance.Between(-10m, -5m);

        Assert.Equal(5m, variance.Amount);
        Assert.Equal(50m, variance.Percent);
    }

    [Fact]
    public void ZeroProposedRateIsTreatedAsNotEntered()
    {
        Assert.StartsWith("Enter a proposed rate", RateVariance.Describe(36.64m, 0m));
    }

    [Fact]
    public void DescriptionStatesDirectionInWords()
    {
        Assert.Contains("above", RateVariance.Describe(36.64m, 50m));
        Assert.Contains("below", RateVariance.Describe(100m, 90m));
        Assert.Equal("Matches the calculated rate.", RateVariance.Describe(36.64m, 36.64m));
    }
}
