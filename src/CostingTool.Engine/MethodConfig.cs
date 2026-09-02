namespace CostingTool.Engine;

/// <summary>
/// Versioned configuration for the University's costing method.
///
/// The indirect cost recovery factor <c>k</c> and the rounding rule are
/// <b>configuration, not constants</b>. The client told us at the kickoff of 29 July 2026
/// that the method and its factors are reviewed, and a costing cycle runs for three to
/// five years — so a rate sealed in 2026 must still reproduce its own figures in 2030
/// from the method version that produced it.
///
/// See <c>docs/spec/architecture.md</c> §3, engine rules R5 and R6.
///
/// This type lives in the engine project rather than beside the other entities because
/// the engine must be able to calculate without a database present — a unit test
/// constructs one directly. Entity Framework maps it from the web project all the same;
/// a POCO does not need to live next to its <c>DbContext</c>.
/// </summary>
public class MethodConfig
{
    public int Id { get; set; }

    /// <summary>Identifies the formula set and its constants, e.g. "2026.1".</summary>
    public string Version { get; set; } = string.Empty;

    public DateTime EffectiveFromUtc { get; set; }

    /// <summary>
    /// <c>k</c> — UWA's standard indirect cost recovery, applied whenever an external
    /// party engages UWA services. 1.35 as at 2026: a 0.35 uplift covering insurance,
    /// legal, finance, library, buildings and IT infrastructure, but not the equipment
    /// or the capability itself.
    /// </summary>
    public decimal IndirectCostRecovery { get; set; } = 1.35m;

    /// <summary>
    /// Decimal places a presented rate is rounded to. Stored values stay unrounded, and
    /// the rounding happens once, at presentation — architecture.md §3 rule R3.
    /// </summary>
    public int RateDecimals { get; set; } = 2;

    /// <summary>
    /// What to do with an exact half at the last decimal place.
    ///
    /// <b>This is an assumption, not a quotation.</b> The client's guide states no
    /// half-cent convention, so we apply the ordinary commercial one — round half away
    /// from zero — and record here that it is ours rather than theirs. It is a candidate
    /// for the next batch of questions; changing it means a new method version, never an
    /// edit to this one.
    /// </summary>
    public MidpointRounding MidpointRule { get; set; } = MidpointRounding.AwayFromZero;

    /// <summary>Where the figures in this version came from, so a reader can check them.</summary>
    public string Source { get; set; } = string.Empty;

    public string? Notes { get; set; }

    /// <summary>Exactly one version is current; older versions are retained, never edited.</summary>
    public bool IsCurrent { get; set; }

    /// <summary>
    /// Last-resort defaults, used only if the configuration table is empty — for example
    /// in a unit test that constructs the engine directly. Never a silent substitute for
    /// a stored version in a running application.
    /// </summary>
    public static MethodConfig Fallback => new()
    {
        Version = "2026.1",
        EffectiveFromUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        IndirectCostRecovery = 1.35m,
        RateDecimals = 2,
        Source = "UWA Costing & Pricing Guide, Step 3; University Indirect Cost Recovery Policy",
        IsCurrent = true
    };
}
