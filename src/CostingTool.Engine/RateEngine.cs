namespace CostingTool.Engine;

/// <summary>
/// The calculation engine — the most important lines in the project.
///
/// Pure: the same <see cref="CapabilityRateInputs"/> and the same <see cref="MethodConfig"/>
/// produce the same outputs, always. No database, no HTTP, no clock, no randomness. That is
/// what makes it testable against the client's worked example and what makes a 2026 record
/// reproducible in 2030. See <c>docs/spec/architecture.md</c> §3, rules R1 to R8.
///
/// Nothing in here reads a constant. <c>k</c> arrives as configuration.
/// </summary>
public static class RateEngine
{
    /// <summary>
    /// Price one capability.
    /// </summary>
    /// <exception cref="RateCalculationException">
    /// When forecast utilisation is zero or negative, or a cost or income figure is
    /// negative. Rule R4: refuse, do not return a plausible number.
    /// </exception>
    public static CapabilityRateResult Calculate(CapabilityRateInputs inputs, MethodConfig method)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(method);

        Guard(inputs);

        var k = method.IndirectCostRecovery;
        var cost = inputs.TotalOperatingCost;
        var utilisation = inputs.ForecastUtilisation;
        var totalIncome = inputs.UwaIncome + inputs.NonUwaIncome;

        // The client's guide, Step 3. Written so that each line reads like the formula in
        // requirements §4 rather than like an optimisation of it.
        var uwaRate = (cost - totalIncome) / utilisation;
        var apfrRate = (cost - inputs.NonUwaIncome) / utilisation * k;
        var commercialRate = cost / utilisation * k;

        return new CapabilityRateResult
        {
            CapabilityName = inputs.CapabilityName,
            MethodVersion = method.Version,
            IndirectCostRecovery = k,
            RateDecimals = method.RateDecimals,
            MidpointRule = method.MidpointRule,

            CapabilityOperatingCost = inputs.CapabilityOperatingCost,
            AllocatedPlatformCost = inputs.AllocatedPlatformCost,
            TotalOperatingCost = cost,
            UwaIncome = inputs.UwaIncome,
            NonUwaIncome = inputs.NonUwaIncome,
            ForecastUtilisation = utilisation,

            UwaRate = uwaRate,
            ApfrRate = apfrRate,
            CommercialRate = commercialRate,

            ForecastUwaUse = inputs.ForecastUwaUse,
            ForecastApfrUse = inputs.ForecastApfrUse,
            ForecastCommercialUse = inputs.ForecastCommercialUse,

            ProposedUwaRate = inputs.ProposedUwaRate,
            ProposedApfrRate = inputs.ProposedApfrRate,
            ProposedCommercialRate = inputs.ProposedCommercialRate,
            GrossForecastRevenue = GrossForecastRevenue(inputs),
            ForecastRevenue = ForecastRevenue(inputs, k)
        };
    }

    /// <summary>
    /// Revenue the platform expects to retain at the proposed rates.
    ///
    /// <b>Read the division carefully.</b> The APFR and commercial rates are charged
    /// inclusive of indirect cost recovery, and the uplift is not the platform's to keep,
    /// so the projection divides it back out before adding the money to the platform's
    /// side of the ledger. This is the behaviour the application has had since the spike
    /// and it is preserved deliberately rather than quietly corrected — but it carries no
    /// source marker in any client document, which means it is <b>our</b> reading of how
    /// the uplift flows, not theirs. It is on the list to confirm; see the note in
    /// requirements §4.
    /// </summary>
    private static decimal ForecastRevenue(CapabilityRateInputs inputs, decimal k) =>
        inputs.ForecastUwaUse * inputs.ProposedUwaRate
        + inputs.ForecastApfrUse * (inputs.ProposedApfrRate / k)
        + inputs.ForecastCommercialUse * (inputs.ProposedCommercialRate / k);

    /// <summary>
    /// What users are billed at the proposed rates, before the indirect cost uplift is
    /// separated out.
    ///
    /// The difference between this and the retained <c>ForecastRevenue</c> is the University's
    /// overhead recovery, which US-12 asks to see on a line of its own rather than folded
    /// into the platform's own revenue [W, sheet 3 rows 39-42].
    /// </summary>
    private static decimal GrossForecastRevenue(CapabilityRateInputs inputs) =>
        inputs.ForecastUwaUse * inputs.ProposedUwaRate
        + inputs.ForecastApfrUse * inputs.ProposedApfrRate
        + inputs.ForecastCommercialUse * inputs.ProposedCommercialRate;

    private static void Guard(CapabilityRateInputs inputs)
    {
        var name = string.IsNullOrWhiteSpace(inputs.CapabilityName) ? "This capability" : inputs.CapabilityName;

        if (inputs.ForecastUtilisation <= 0)
        {
            throw new RateCalculationException(
                $"{name} has no forecast utilisation, so a rate per unit cannot be worked out. " +
                "Enter the hours, days or samples you expect to be used in a year — the forecast, " +
                "not the full capacity.");
        }

        if (inputs.CapabilityOperatingCost < 0 || inputs.AllocatedPlatformCost < 0)
        {
            throw new RateCalculationException(
                $"{name} has a negative operating cost. Record income on an income line rather " +
                "than as a negative cost, so that the UWA and non-UWA split stays correct.");
        }

        if (inputs.UwaIncome < 0 || inputs.NonUwaIncome < 0)
        {
            throw new RateCalculationException($"{name} has a negative non-variable income figure.");
        }
    }
}

/// <summary>
/// Three rates, and the figures behind them.
///
/// The intermediate values are part of the result rather than a debugging convenience:
/// the client asked on 20 August 2026 that the sealed PDF show "the workings for the
/// calculator (for transparency and traceability)", and a record cannot show a working
/// the engine never returned. Requirements §9, Q5.
/// </summary>
public sealed record CapabilityRateResult
{
    public required string CapabilityName { get; init; }

    /// <summary>The method version that produced these figures — rule R6.</summary>
    public required string MethodVersion { get; init; }

    public required decimal IndirectCostRecovery { get; init; }

    public required int RateDecimals { get; init; }

    public required MidpointRounding MidpointRule { get; init; }

    // ---- The workings ----------------------------------------------------------------

    public required decimal CapabilityOperatingCost { get; init; }

    public required decimal AllocatedPlatformCost { get; init; }

    /// <summary><c>C</c></summary>
    public required decimal TotalOperatingCost { get; init; }

    public required decimal UwaIncome { get; init; }

    public required decimal NonUwaIncome { get; init; }

    /// <summary><c>I_total</c> — everything deducted for a UWA researcher.</summary>
    public decimal TotalIncome => UwaIncome + NonUwaIncome;

    /// <summary><c>U</c></summary>
    public required decimal ForecastUtilisation { get; init; }

    /// <summary>
    /// The forecast split across the three user categories. It does not move a rate — the
    /// divisor is their sum — but it is what the revenue projection is built from, so a
    /// record that shows a balance has to carry it (requirements §9, Q2).
    /// </summary>
    public required decimal ForecastUwaUse { get; init; }

    public required decimal ForecastApfrUse { get; init; }

    public required decimal ForecastCommercialUse { get; init; }

    // ---- The answers -----------------------------------------------------------------

    public required decimal UwaRate { get; init; }

    public required decimal ApfrRate { get; init; }

    public required decimal CommercialRate { get; init; }

    public required decimal ProposedUwaRate { get; init; }

    public required decimal ProposedApfrRate { get; init; }

    public required decimal ProposedCommercialRate { get; init; }

    /// <summary>Billed to users at the proposed rates, uplift included.</summary>
    public required decimal GrossForecastRevenue { get; init; }

    /// <summary>Retained by the platform: the billed amount less the University's uplift.</summary>
    public required decimal ForecastRevenue { get; init; }

    /// <summary>
    /// The University's indirect cost recovery, shown separately from the recovery of full
    /// economic cost because the workbook shows it separately and US-12 asks for the same.
    /// </summary>
    public decimal OverheadsRecovered => GrossForecastRevenue - ForecastRevenue;

    /// <summary>
    /// <c>C - I_total</c> — what usage has to recover once non-variable income is counted.
    /// </summary>
    public decimal NetCostToRecover => TotalOperatingCost - TotalIncome;

    /// <summary>
    /// Surplus, or deficit if negative, at the proposed rates.
    ///
    /// <b>This is measured against cost less income</b>, which is what US-12 asks for:
    /// "proposed rates x forecast utilisation by user type, against total cost less income".
    /// Non-variable income is money the platform already holds, so measuring against the
    /// full cost counts it twice and overstates the shortfall. Before 18 September 2026 this
    /// compared revenue with the full operating cost; that figure is kept as
    /// <see cref="FullEconomicCostBalance"/> rather than dropped, because the workbook
    /// reports both and an approver reads them together.
    /// </summary>
    public decimal ForecastBalance => ForecastRevenue - NetCostToRecover;

    /// <summary>
    /// The same projection measured against the platform's <b>full</b> economic cost, with
    /// no income deducted. Negative by design wherever income carries part of the cost.
    /// </summary>
    public decimal FullEconomicCostBalance => ForecastRevenue - TotalOperatingCost;

    /// <summary>
    /// What the custodian proposes, less what the method calculates. Positive means they
    /// propose to charge more than the minimum sustainable rate (US-11).
    /// </summary>
    public decimal UwaRateVariance => ProposedUwaRate - UwaRate;

    public decimal ApfrRateVariance => ProposedApfrRate - ApfrRate;

    public decimal CommercialRateVariance => ProposedCommercialRate - CommercialRate;

    /// <summary>The same variance as a percentage, or null when there is no rate to vary from.</summary>
    public decimal? UwaRateVariancePercent => Percent(UwaRateVariance, UwaRate);

    public decimal? ApfrRateVariancePercent => Percent(ApfrRateVariance, ApfrRate);

    public decimal? CommercialRateVariancePercent => Percent(CommercialRateVariance, CommercialRate);

    /// <summary>
    /// True when a rate the custodian has actually entered differs from the calculated one.
    ///
    /// A rate still left at zero is not a variance — it is an unanswered question, caught
    /// at submission instead. The comparison is made on the rounded figures, so a fraction
    /// of a cent the screen never showed does not demand a justification (US-11, F9).
    /// </summary>
    public bool VariesFromCalculated =>
        Varies(ProposedUwaRate, DisplayUwaRate)
        || Varies(ProposedApfrRate, DisplayApfrRate)
        || Varies(ProposedCommercialRate, DisplayCommercialRate);

    /// <summary>
    /// True when the commercial rate in particular is varied, which is where the University's
    /// competitive neutrality obligations apply [G, Step 3].
    /// </summary>
    public bool CommercialRateVaries => Varies(ProposedCommercialRate, DisplayCommercialRate);

    private bool Varies(decimal proposed, decimal calculated) =>
        proposed > 0 && Round(proposed) != calculated;

    private static decimal? Percent(decimal variance, decimal calculated) =>
        calculated == 0 ? null : variance / calculated * 100m;

    /// <summary>
    /// The single rounding rule, applied once, on the way to a screen or a PDF — rule R3.
    /// Stored values stay unrounded, so a total never drifts from the sum of its parts.
    /// </summary>
    public decimal Round(decimal value) => Math.Round(value, RateDecimals, MidpointRule);

    /// <summary>The three calculated rates as they should be presented.</summary>
    public decimal DisplayUwaRate => Round(UwaRate);

    public decimal DisplayApfrRate => Round(ApfrRate);

    public decimal DisplayCommercialRate => Round(CommercialRate);
}
