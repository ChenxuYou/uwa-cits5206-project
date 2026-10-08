using System.Globalization;
using Xunit;

namespace CostingTool.Pdf.Tests;

/// <summary>
/// How figures are written down.
///
/// These look trivial and are not. The application pins <c>en-AU</c> in
/// <c>Program.cs</c> because a stock Linux server renders $1,250.00 as "¤1,250.00" under
/// the invariant culture — and a sealed record that formats differently depending on
/// which machine exported it is not a sealed record. The renderer does not inherit the
/// web request's culture, so it pins its own, and this is what holds it there.
/// </summary>
public class RecordFormatTests
{
    [Fact]
    public void Money_is_australian_currency_to_the_cent_whatever_the_machine_is_set_to()
    {
        var thread = CultureInfo.CurrentCulture;
        try
        {
            // A German developer's laptop, or a server nobody configured.
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

            Assert.Equal("$150,000.00", RecordFormat.Money(150000m));
            Assert.Equal("$202.50", RecordFormat.Money(202.5m));
        }
        finally
        {
            CultureInfo.CurrentCulture = thread;
        }
    }

    [Theory]
    [InlineData("Hours", "$100.00 per hour")]
    [InlineData("Days", "$100.00 per day")]
    [InlineData("Samples", "$100.00 per sample")]
    [InlineData(null, "$100.00 per unit")]
    public void A_rate_names_the_unit_it_is_charged_in(string? billableUnit, string expected) =>
        Assert.Equal(expected, RecordFormat.Rate(100m, billableUnit));

    [Fact]
    public void A_quantity_keeps_its_unit_plural()
    {
        Assert.Equal("1,000 hours", RecordFormat.Quantity(1000m, "Hours"));
        Assert.Equal("12.5 hours", RecordFormat.Quantity(12.5m, "Hours"));
    }

    [Fact]
    public void A_deficit_says_so_in_words_and_in_brackets()
    {
        // Colour must never be the only carrier of meaning: this has to survive a
        // monochrome print and a reader who cannot separate red from green.
        Assert.Equal("($37,500.00) deficit", RecordFormat.Balance(-37500m));
        Assert.Equal("$1,200.00 surplus", RecordFormat.Balance(1200m));
    }

    [Fact]
    public void Dates_are_long_form_and_day_first()
    {
        // 13/09 and 09/13 are different days on either side of the Pacific; the record is
        // read in Perth and may be read anywhere.
        Assert.Equal("16 September 2026", RecordFormat.Date(new DateTime(2026, 9, 16, 0, 0, 0, DateTimeKind.Utc)));
        Assert.Equal("—", RecordFormat.Date(null));
    }

    [Theory]
    [InlineData(100, 100, "matches")]
    [InlineData(270, 240, "$30.00 below (11.1%)")]
    [InlineData(162, 170, "$8.00 above (4.9%)")]
    [InlineData(0, 5, "$5.00 above")]
    public void Variance_is_said_in_words_with_its_direction(decimal calculated, decimal proposed, string expected)
    {
        Assert.Equal(expected, RecordFormat.Variance(calculated, proposed));
    }
}
