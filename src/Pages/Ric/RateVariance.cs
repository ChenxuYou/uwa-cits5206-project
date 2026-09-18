using System.Globalization;

namespace CostingTool.Pages.Ric;

/// <summary>
/// How far a proposed rate sits from the calculated one (US-11: "the variance between
/// calculated and proposed, in dollars and as a percentage").
///
/// This is presentation arithmetic, not costing arithmetic: it takes two figures that are
/// already on the screen and subtracts them. The calculated rate passed in is the
/// <b>displayed</b> one (already rounded by the engine's single rule, R3), so the variance a
/// reader works out by hand from the page always agrees with the page. That choice is ours
/// rather than the client's and is worth confirming with the calculation owner.
///
/// The live update in Rates.cshtml repeats the same subtraction in the browser, in whole
/// cents, using only numbers the page already shows — no formula or coefficient is sent
/// (US-09, N1). The two must produce the same words; RateVarianceTests pins the server side.
/// </summary>
/// <param name="Amount">Proposed minus calculated: positive when the proposed rate is higher.</param>
/// <param name="Percent">Amount as a percentage of the calculated rate; null when that rate is zero.</param>
public readonly record struct RateVariance(decimal Amount, decimal? Percent)
{
    /// <summary>True when the proposed rate differs from the calculated one at all.</summary>
    public bool IsVaried => Amount != 0;

    /// <param name="calculated">The calculated rate as displayed.</param>
    /// <param name="proposed">The proposed rate.</param>
    public static RateVariance Between(decimal calculated, decimal proposed)
    {
        var amount = proposed - calculated;

        // A calculated rate of zero (income covers the whole cost) has no percentage. The
        // magnitude is used as the divisor so the sign of the percentage matches the amount.
        decimal? percent = calculated == 0 ? null : amount / Math.Abs(calculated) * 100m;

        return new RateVariance(amount, percent);
    }

    /// <summary>
    /// The sentence shown under a proposed-rate box. Direction is stated in words, so it does
    /// not depend on colour or on reading a minus sign.
    /// </summary>
    public static string Describe(decimal calculated, decimal proposed)
    {
        // Proposed rates are stored as decimals, not nulls, so zero stands for "not entered".
        // Submission already refuses a zero rate.
        if (proposed <= 0)
        {
            return "Enter a proposed rate to compare with the calculated rate.";
        }

        var variance = Between(calculated, proposed);

        if (!variance.IsVaried)
        {
            return "Matches the calculated rate.";
        }

        var text = Math.Abs(variance.Amount).ToString("C");

        if (variance.Percent is { } percent)
        {
            text += $" ({Math.Abs(percent).ToString("0.0", CultureInfo.CurrentCulture)}%)";
        }

        return $"{text} {(variance.Amount > 0 ? "above" : "below")} the calculated rate.";
    }
}
