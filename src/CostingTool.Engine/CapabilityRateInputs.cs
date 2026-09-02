namespace CostingTool.Engine;

/// <summary>
/// Everything the engine needs to price one capability, and nothing else.
///
/// <b>Why this type exists.</b> The engine used to take the Entity Framework entity graph
/// directly, which meant its answer depended on which <c>.Include()</c> the calling page
/// happened to have written: a page that forgot one would sum an unloaded collection to
/// zero and show a different rate for the same capability, silently. That is exactly the
/// failure mode this project was commissioned to remove (requirements §2, N14), so the
/// engine now takes a value object that has to be filled in deliberately. Assembling one
/// from the database is the job of <c>RicCalculationService</c>, in one place, where it
/// can be read.
///
/// Every figure here is an <b>annual</b> amount, per architecture.md §4 and requirements
/// assumption A11.
/// </summary>
public sealed record CapabilityRateInputs
{
    /// <summary>The capability being priced. Carried so that an error message can name it.</summary>
    public required string CapabilityName { get; init; }

    /// <summary>Operating cost booked directly against this capability.</summary>
    public decimal CapabilityOperatingCost { get; init; }

    /// <summary>
    /// This capability's share of the platform-level operating cost. The allocation rule —
    /// an even split across capabilities — is applied by the caller, because it is a
    /// property of the cycle rather than of one capability [W, sheet 1].
    /// </summary>
    public decimal AllocatedPlatformCost { get; init; }

    /// <summary>Non-variable income from UWA sources: GP and in-kind.</summary>
    public decimal UwaIncome { get; init; }

    /// <summary>Non-variable income from non-UWA sources: State, Federal (incl. NCRIS), Other.</summary>
    public decimal NonUwaIncome { get; init; }

    /// <summary>
    /// Forecast annual utilisation by each user category. <b>Forecast, not capacity</b> —
    /// the distinction the workbook makes easy to lose (requirements §4 Step 2).
    ///
    /// Their sum is <c>U</c>, the single divisor behind all three rates. The split itself
    /// drives only the revenue projection, which is how Q2 was closed.
    /// </summary>
    public decimal ForecastUwaUse { get; init; }

    public decimal ForecastApfrUse { get; init; }

    public decimal ForecastCommercialUse { get; init; }

    /// <summary>Rates the custodian proposes to charge, which need not equal the calculated ones.</summary>
    public decimal ProposedUwaRate { get; init; }

    public decimal ProposedApfrRate { get; init; }

    public decimal ProposedCommercialRate { get; init; }

    /// <summary><c>C</c> — total annual operating cost carried by this capability.</summary>
    public decimal TotalOperatingCost => CapabilityOperatingCost + AllocatedPlatformCost;

    /// <summary><c>U</c> — forecast annual utilisation, summed over the three user categories.</summary>
    public decimal ForecastUtilisation => ForecastUwaUse + ForecastApfrUse + ForecastCommercialUse;
}

/// <summary>
/// Raised when the inputs cannot produce a defensible rate.
///
/// The engine refuses rather than returning a number, because a plausible-looking wrong
/// figure is the specific harm this tool exists to prevent: a rate of $0.00 per hour reads
/// like an answer. Rule R4 in architecture.md §3 — never infinity, never a crash, never a
/// silent zero. The message is written to be shown to a custodian.
/// </summary>
public sealed class RateCalculationException(string message) : Exception(message);
