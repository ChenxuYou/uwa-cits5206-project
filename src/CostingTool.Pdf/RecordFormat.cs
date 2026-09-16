using System.Globalization;

namespace CostingTool.Pdf;

/// <summary>
/// How a figure is written in the sealed record.
///
/// Pulled out of the renderer so it can be tested without producing a PDF, and pinned to
/// <c>en-AU</c> for the same reason <c>Program.cs</c> pins the request culture: on a
/// stock Linux server the invariant culture renders $1,250.00 as "¤1,250.00", and a
/// sealed record that formats differently depending on which machine exported it is not
/// a sealed record.
/// </summary>
public static class RecordFormat
{
    /// <summary>Perth reads these documents; the server may be anywhere.</summary>
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-AU");

    /// <summary>An amount of money, to the cent: <c>$150,000.00</c>.</summary>
    public static string Money(decimal value) => value.ToString("C2", Culture);

    /// <summary>
    /// A rate per billable unit. Rounding already happened in the engine, at
    /// presentation, once (architecture.md §3 rule R3) — this only writes it down.
    /// </summary>
    public static string Rate(decimal value, string? billableUnit)
    {
        var unit = string.IsNullOrWhiteSpace(billableUnit) ? "unit" : Singular(billableUnit.Trim());
        return $"{Money(value)} per {unit.ToLower(Culture)}";
    }

    /// <summary>A quantity of billable units: <c>1,000 hours</c>.</summary>
    public static string Quantity(decimal value, string? billableUnit)
    {
        var unit = string.IsNullOrWhiteSpace(billableUnit) ? "units" : billableUnit.Trim();
        return $"{value.ToString("#,##0.##", Culture)} {unit.ToLower(Culture)}";
    }

    /// <summary>Long-form and day-first: <c>13 September 2026</c>. Never 09/13/26.</summary>
    public static string Date(DateTime? value) =>
        value is null ? "—" : value.Value.ToString("d MMMM yyyy", Culture);

    /// <summary>A timestamp, said in words, and labelled UTC because it is stored as UTC.</summary>
    public static string Timestamp(DateTime? value) =>
        value is null ? "—" : value.Value.ToString("d MMMM yyyy, HH:mm 'UTC'", Culture);

    /// <summary>
    /// A surplus or a deficit.
    ///
    /// Never colour alone: the word and the parentheses carry the meaning, so the figure
    /// survives a monochrome print and a reader who cannot distinguish the two colours.
    /// </summary>
    public static string Balance(decimal value) =>
        value < 0
            ? $"({Money(Math.Abs(value))}) deficit"
            : $"{Money(value)} surplus";

    /// <summary>"Hours" → "hour", so a rate reads "per hour" rather than "per hours".</summary>
    private static string Singular(string unit) =>
        unit.EndsWith("ies", StringComparison.OrdinalIgnoreCase) ? unit[..^3] + "y"
        : unit.EndsWith('s') ? unit[..^1]
        : unit;
}
