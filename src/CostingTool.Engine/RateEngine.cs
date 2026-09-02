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

            ProposedUwaRate = inputs.ProposedUwaRate,
            ProposedApfrRate = inputs.ProposedApfrRate,
            ProposedCommercialRate = inputs.ProposedCommercialRate,
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

    // ---- The answers -----------------------------------------------------------------

    public required decimal UwaRate { get; init; }

    public required decimal ApfrRate { get; init; }

    public required decimal CommercialRate { get; init; }

    public required decimal ProposedUwaRate { get; init; }

    public required decimal ProposedApfrRate { get; init; }

    public required decimal ProposedCommercialRate { get; init; }

    public required decimal ForecastRevenue { get; init; }

    /// <summary>Surplus, or deficit if negative, at the proposed rates.</summary>
    public decimal ForecastBalance => ForecastRevenue - TotalOperatingCost;

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
