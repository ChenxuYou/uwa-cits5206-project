namespace CostingTool.Models;

/// <summary>
/// The words that go beside a rate, in one place.
///
/// US-09 asks that every rate carry the guide's own eligibility wording, say which income
/// was deducted from it, and say whether the indirect cost uplift was applied — so that the
/// answer to "why this number?" is on the screen rather than in someone's memory. The
/// wording is the client's, from the costing &amp; pricing guide, Step 3 <b>[G]</b>; it
/// lives here rather than in the Razor view so that the screen and the sealed record cannot
/// describe the same rate differently.
///
/// Nothing here is a coefficient or a formula: the arithmetic stays in
/// <c>CostingTool.Engine</c>, and this file only explains it (N1).
/// </summary>
public static class RateCategories
{
    /// <summary>One of the three rates, and everything a custodian needs read beside it.</summary>
    /// <param name="Name">The category as the guide names it.</param>
    /// <param name="Eligibility">Who this rate applies to [G, Step 3].</param>
    /// <param name="IncomeDeducted">Which non-variable income lowers this rate.</param>
    /// <param name="UpliftApplies">Whether the indirect cost recovery factor is applied.</param>
    public sealed record Category(string Name, string Eligibility, string IncomeDeducted, bool UpliftApplies);

    public static readonly Category UwaResearcher = new(
        "UWA Researcher",
        "UWA-led research aligned with the HERDC definition of research.",
        "All non-variable income — UWA and non-UWA alike.",
        UpliftApplies: false);

    public static readonly Category Apfr = new(
        "APFR",
        "Australian publicly funded research: activity that meets the objectives of a publicly funded entity.",
        "Non-UWA income only. UWA GP and in-kind support is not deducted.",
        UpliftApplies: true);

    public static readonly Category Commercial = new(
        "Commercial",
        "All other activity, including non-research work and work for commercial parties.",
        "No income is deducted from this rate.",
        UpliftApplies: true);

    public static readonly Category[] All = [UwaResearcher, Apfr, Commercial];

    /// <summary>
    /// What the uplift is for, in the client's terms [K §2]. The percentage itself is read
    /// from the method configuration in force, never written here.
    /// </summary>
    public const string UpliftExplanation =
        "UWA's standard indirect cost recovery under the University Indirect Cost Recovery Policy: "
        + "insurance, legal, finance, library, buildings and IT infrastructure — not the equipment "
        + "or the capability itself.";

    /// <summary>Every rate on this screen is quoted without GST [G, GST].</summary>
    public const string GstLabel = "GST exclusive";

    /// <summary>The guide's own framing of what a calculated rate is for [G, Step 3].</summary>
    public const string ProposedRateFraming =
        "The calculated rates are a starting point for pricing decisions before considering market "
        + "conditions or strategic adjustments. A proposed rate may differ from them, but the "
        + "difference has to be explained and its effect on the balance has to be visible.";

    /// <summary>
    /// The obligation that applies when the commercial rate is moved [G, Step 3]. Shown only
    /// when it is moved, so that it reads as a prompt rather than as boilerplate.
    /// </summary>
    public const string CompetitiveNeutrality =
        "The commercial rate differs from the calculated one. The University's competitive "
        + "neutrality obligations apply: publicly funded facilities should not use that funding "
        + "to undercut commercial providers. Record why this rate is appropriate.";
}
