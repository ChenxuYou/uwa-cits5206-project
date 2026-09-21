using CostingTool.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CostingTool.Data;

/// <summary>
/// The checks US-18 asks for that are about what a person typed rather than about the
/// method: a number that is not a number, a percentage outside 0–100, and an amount so
/// large for its category that it is more likely a slip of the finger than a real cost.
///
/// Every check here runs on the server. The <c>type="number"</c> and <c>min</c>
/// attributes in the views are a convenience; a request that bypasses them lands here
/// and is refused all the same.
/// </summary>
public static class EntryChecks
{
    /// <summary>
    /// Per-year amount above which one line needs a second look before it is saved.
    ///
    /// The named case is $200,000 typed where $20,000 was meant [K §5]. These figures are
    /// <b>placeholders to confirm with the client</b>: they are set so that an extra zero
    /// on an ordinary line is caught, and a genuine large line only costs one tick.
    /// Confirming never blocks a real figure; it only stops a mistyped one going in silently.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, decimal> ConfirmAbove =
        new Dictionary<string, decimal>
        {
            [CostEntry.CostCategories.Personnel] = 250_000m,
            ["Equipment"] = 500_000m,
            ["Maintenance"] = 100_000m,
            ["Travel"] = 50_000m,
            ["Animal Cost"] = 100_000m,
            ["Other"] = 100_000m
        };

    /// <summary>Income lines are usually larger than any single cost line.</summary>
    public const decimal ConfirmIncomeAbove = 1_000_000m;

    public static decimal ThresholdFor(string category) =>
        ConfirmAbove.TryGetValue(category, out var threshold) ? threshold : 100_000m;

    /// <summary>
    /// Messages for each year whose amount exceeds <paramref name="threshold"/>, naming the
    /// year, the amount and the usual ceiling.
    /// </summary>
    public static IReadOnlyList<string> UnusuallyLarge(
        IEnumerable<decimal> yearAmounts, int startYear, string what, decimal threshold) =>
        yearAmounts
            .Select((amount, i) => (amount, year: startYear + i))
            .Where(x => x.amount > threshold)
            .Select(x =>
                $"{x.year}: {x.amount:C0} is unusually large for {what} (usually under {threshold:C0}). "
                + "Check for an extra zero. If the amount is right, tick \"I have checked these amounts\" and add it again.")
            .ToList();

    /// <summary>Null when <paramref name="value"/> is a percentage, otherwise the message.</summary>
    public static string? NotAPercentage(decimal value, string label) =>
        value is < 0m or > 100m ? $"{label} must be between 0 and 100 percent." : null;

    /// <summary>
    /// Replace the framework's wording for a value that could not be read as a number
    /// ("The value 'abc' is not valid for YearAmounts[0].") with one that names the field
    /// in the words on the screen and says what is expected.
    ///
    /// Call it before adding the page's own errors: it rewrites every entry that failed
    /// model binding and has a label, and leaves the rest alone.
    /// </summary>
    /// <param name="labelFor">The on-screen name for a model-state key, or null to leave it.</param>
    /// <param name="expected">What a valid value looks like, e.g. "an amount in dollars, such as 20000.00".</param>
    public static void ExplainUnreadableNumbers(
        ModelStateDictionary modelState, Func<string, string?> labelFor, string expected)
    {
        foreach (var (key, entry) in modelState)
        {
            if (entry.Errors.Count == 0 || string.IsNullOrEmpty(entry.AttemptedValue))
            {
                continue;
            }

            var label = labelFor(key);
            if (label is null)
            {
                continue;
            }

            entry.Errors.Clear();
            entry.Errors.Add($"{label}: \"{entry.AttemptedValue}\" is not a number. Enter {expected}.");
        }
    }

    /// <summary>"YearAmounts[2]" → 2, or null for any other key.</summary>
    public static int? YearIndex(string key)
    {
        const string prefix = "YearAmounts[";
        return key.StartsWith(prefix, StringComparison.Ordinal)
               && key.EndsWith(']')
               && int.TryParse(key.AsSpan(prefix.Length, key.Length - prefix.Length - 1), out var index)
            ? index
            : null;
    }
}
