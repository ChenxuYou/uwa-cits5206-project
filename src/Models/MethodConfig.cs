namespace CostingTool.Models;

/// <summary>
/// Versioned configuration for the University's costing method.
///
/// The indirect cost recovery factor <c>k</c>, the rounding rule and the capacity
/// baselines are <b>configuration, not constants</b>. The client told us at the kickoff
/// of 29 July 2026 that the method and its factors are reviewed, and a costing cycle runs
/// for three to five years — so a rate sealed in 2026 must still reproduce its own figures
/// in 2030 from the method version that produced it.
///
/// See <c>docs/spec/architecture.md</c> §3, engine rules R5 and R6.
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

    /// <summary>Decimal places a presented rate is rounded to. Stored values stay unrounded.</summary>
    public int RateDecimals { get; set; } = 2;

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
