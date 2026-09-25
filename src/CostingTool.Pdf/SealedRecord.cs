using System.Text.Json;
using System.Text.Json.Serialization;

namespace CostingTool.Pdf;

/// <summary>
/// The sealed snapshot, read back.
///
/// <b>The PDF is rendered from the snapshot, never from the live rows.</b> That is the
/// single most important decision in this project and the reason this type exists at
/// all. The snapshot is what the approver sealed; the rows can be edited, superseded or
/// renamed afterwards. If the export read the database it could hand a custodian a
/// document that disagrees with the record it claims to be — and a rate that cannot be
/// re-derived from the document is exactly the failure this tool exists to remove
/// (architecture.md §4, requirements §9 Q5).
///
/// The shape below mirrors <c>Pages/Approvals/Details.cshtml.cs</c> — <c>BuildSnapshot()</c>,
/// schema 1.5. The two must move together: adding a field to the snapshot without adding
/// it here means the PDF silently stops showing it, so <see cref="SupportedSchemaVersions"/>
/// is checked on the way in rather than trusted.
///
/// <b>Older snapshots keep rendering.</b> Schema 1.2 added the balance lines US-12 asks for;
/// a record sealed under 1.1 does not carry them, so they are nullable here and the document
/// leaves them out rather than printing a zero the approver never saw. Schema 1.3 added
/// <see cref="SealedCycle.Supersedes"/>, the record a cycle replaces (F22), null before it and
/// on a record that replaces nothing. Schema 1.4 added who sealed the record
/// (<see cref="SealedCycle.SealedBy"/>, US-15 and US-16) and the capacity inputs behind each
/// capability's forecast (US-16, "every input"); both are absent, and left out rather than
/// invented, on an older record. Schema 1.5 added the costing assumptions
/// (<see cref="SealedCycle.CostingAssumptions"/>, US-13), which a record sealed before it
/// never held. A snapshot is never rewritten — the renderer is what
/// learns the older shape.
/// </summary>
public sealed class SealedRecord
{
    /// <summary>The schema new snapshots are written in.</summary>
    public const string CurrentSchemaVersion = "1.5";

    /// <summary>Every schema this renderer can read, oldest first.</summary>
    public static readonly string[] SupportedSchemaVersions = ["1.1", "1.2", "1.3", "1.4", CurrentSchemaVersion];

    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public string? SchemaVersion { get; set; }

    public DateTime? SealedAtUtc { get; set; }

    public string? MethodVersion { get; set; }

    public SealedMethod? Method { get; set; }

    public SealedCycle? Cycle { get; set; }

    public SealedPlatform? Platform { get; set; }

    public List<SealedCapability> Capabilities { get; set; } = [];

    /// <summary>
    /// The cost and income lines behind the totals, as they stood when sealed. The PDF prints
    /// the totals only; the record page (US-17) lists the lines, so "why is C that much?" is
    /// answered from the record too.
    /// </summary>
    public List<SealedCost> Costs { get; set; } = [];

    /// <summary>
    /// Parse a snapshot.
    /// </summary>
    /// <exception cref="SealedRecordFormatException">
    /// When the JSON is not a snapshot, or is a schema version this renderer does not
    /// know. Refusing is the same rule the engine follows: a document assembled from
    /// half-understood JSON would look finished and be wrong.
    /// </exception>
    public static SealedRecord Parse(string snapshotJson)
    {
        if (string.IsNullOrWhiteSpace(snapshotJson))
        {
            throw new SealedRecordFormatException(
                "This costing cycle has no sealed snapshot, so there is nothing to export. " +
                "A record can only be exported once the delegated authority has approved it.");
        }

        SealedRecord? record;
        try
        {
            record = JsonSerializer.Deserialize<SealedRecord>(snapshotJson, ReadOptions);
        }
        catch (JsonException error)
        {
            throw new SealedRecordFormatException(
                "The sealed snapshot could not be read as JSON. The stored record may be " +
                "damaged; do not re-seal it — raise it, because the integrity hash is the " +
                "evidence of what it used to say.", error);
        }

        if (record is null)
        {
            throw new SealedRecordFormatException("The sealed snapshot is empty.");
        }

        if (record.SchemaVersion is null || !SupportedSchemaVersions.Contains(record.SchemaVersion))
        {
            throw new SealedRecordFormatException(
                $"The sealed snapshot is schema version '{record.SchemaVersion ?? "(none)"}', and this " +
                $"renderer understands {string.Join(" and ", SupportedSchemaVersions)}. Snapshots are " +
                "never rewritten, so the renderer is what has to learn the older shape.");
        }

        return record;
    }
}

public sealed class SealedMethod
{
    public string? Version { get; set; }

    public decimal IndirectCostRecovery { get; set; }

    public int RateDecimals { get; set; }

    public string? MidpointRule { get; set; }

    public string? Source { get; set; }

    public SealedFormulas? Formulas { get; set; }
}

public sealed class SealedFormulas
{
    public string? UwaResearcher { get; set; }

    public string? Apfr { get; set; }

    public string? Commercial { get; set; }
}

public sealed class SealedCycle
{
    public int Id { get; set; }

    public string? PlatformName { get; set; }

    public int StartYear { get; set; }

    public int EndYear { get; set; }

    public string? BillableUnit { get; set; }

    public string? CreatedByDisplay { get; set; }

    public string? UtilisationAssumptions { get; set; }

    /// <summary>
    /// What the cost figures rest on as a whole (US-13; the guide's Step 5 checklist).
    /// Schema 1.5 onwards; null before it, and when none was written.
    /// </summary>
    public string? CostingAssumptions { get; set; }

    public string? BenchmarkNotes { get; set; }

    public string? PricingJustification { get; set; }

    public string? SubmittedBy { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAtUtc { get; set; }

    public string? ApprovalComment { get; set; }

    public DateTime? EffectiveDateUtc { get; set; }

    /// <summary>
    /// Who sealed the record, from their signed-in identity (US-15, F15). Schema 1.4 onwards.
    /// Sealing is the approver's act of approval, so before 1.4 it is the approver.
    /// </summary>
    public string? SealedBy { get; set; }

    /// <summary>The sealed record this one replaces (F22). Schema 1.3 onwards; null when it replaces none.</summary>
    public SealedReference? Supersedes { get; set; }
}

/// <summary>Another sealed record, named by what identifies it — including its own hash.</summary>
public sealed class SealedReference
{
    public int Id { get; set; }

    public string? PlatformName { get; set; }

    public int StartYear { get; set; }

    public int EndYear { get; set; }

    public DateTime? SealedAtUtc { get; set; }

    public string? SnapshotHash { get; set; }
}

public sealed class SealedPlatform
{
    public decimal TotalOperatingCost { get; set; }

    /// <summary>Schema 1.2 onwards; null on a record sealed under 1.1.</summary>
    public decimal? TotalIncome { get; set; }

    /// <summary>Operating cost less non-variable income — what usage has to recover. 1.2 onwards.</summary>
    public decimal? NetCostToRecover { get; set; }

    /// <summary>Billed to users at the proposed rates, uplift included. 1.2 onwards.</summary>
    public decimal? GrossForecastRevenue { get; set; }

    /// <summary>The University's indirect cost recovery, on its own line (US-12). 1.2 onwards.</summary>
    public decimal? OverheadsRecovered { get; set; }

    public decimal ForecastRevenue { get; set; }

    /// <summary>
    /// Surplus or deficit. Against cost less income from schema 1.2; against full operating
    /// cost in a record sealed under 1.1, which is how that record was approved.
    /// </summary>
    public decimal ForecastBalance { get; set; }

    /// <summary>The same projection against full economic cost. 1.2 onwards.</summary>
    public decimal? FullEconomicCostBalance { get; set; }
}

public sealed class SealedCapability
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public decimal MaximumCapacity { get; set; }

    public decimal ForecastUwaUse { get; set; }

    public decimal ForecastApfrUse { get; set; }

    public decimal ForecastCommercialUse { get; set; }

    /// <summary>Machine, Staff or Stated; schema 1.4 onwards, null before.</summary>
    public string? CapacityBaseline { get; set; }

    /// <summary>The baseline the deductions came off, in the billable unit. 1.4 onwards.</summary>
    public decimal? BaselineCapacity { get; set; }

    public string? StatedBaselineNote { get; set; }

    public bool? IsStaffReliant { get; set; }

    public decimal? StaffFte { get; set; }

    /// <summary>Why a forecast above usable capacity can be met (US-08). 1.4 onwards.</summary>
    public string? AboveCapacityReason { get; set; }

    public List<SealedDeduction> CapacityDeductions { get; set; } = [];

    public SealedResult? Result { get; set; }

    public SealedWorkings? Workings { get; set; }
}

/// <summary>One reason a capability is unavailable for part of its baseline (US-07).</summary>
public sealed class SealedDeduction
{
    public string? Kind { get; set; }

    public decimal Amount { get; set; }

    public string? Note { get; set; }
}

/// <summary>One cost or income line, in the words the custodian entered it.</summary>
public sealed class SealedCost
{
    public int Id { get; set; }

    /// <summary>The capability it is booked to; null for a platform-level line.</summary>
    public int? RicCapabilityId { get; set; }

    public string? Scope { get; set; }

    public string? CostType { get; set; }

    public string? Category { get; set; }

    /// <summary>The annual figure the engine used — the mean of the per-year amounts.</summary>
    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public string? PersonnelName { get; set; }

    public string? Notes { get; set; }

    public string? Supplier { get; set; }

    public string? FundingType { get; set; }

    public decimal? FloorArea { get; set; }

    public decimal? FloorAreaRate { get; set; }

    /// <summary>
    /// Worded as <c>CostEntry.Types.Income</c> in the web project, which this assembly does not
    /// reference. A snapshot stores the words, so the words are what is compared.
    /// </summary>
    public bool IsIncome => CostType == "Non-variable income";
}

/// <summary>
/// One capability's figures, exactly as the engine returned them when the record was
/// sealed. <c>Display*</c> are the rounded rates: the same values the custodian saw on
/// screen, so the PDF cannot round differently from the page that was approved.
/// </summary>
public sealed class SealedResult
{
    public string? CapabilityName { get; set; }

    public decimal CapabilityOperatingCost { get; set; }

    public decimal AllocatedPlatformCost { get; set; }

    public decimal TotalOperatingCost { get; set; }

    public decimal UwaIncome { get; set; }

    public decimal NonUwaIncome { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal ForecastUtilisation { get; set; }

    /// <summary>The unrounded rates the engine returned, kept beside the displayed ones.</summary>
    public decimal? UwaRate { get; set; }

    public decimal? ApfrRate { get; set; }

    public decimal? CommercialRate { get; set; }

    public decimal DisplayUwaRate { get; set; }

    public decimal DisplayApfrRate { get; set; }

    public decimal DisplayCommercialRate { get; set; }

    public decimal ProposedUwaRate { get; set; }

    public decimal ProposedApfrRate { get; set; }

    public decimal ProposedCommercialRate { get; set; }

    public decimal? GrossForecastRevenue { get; set; }

    public decimal? OverheadsRecovered { get; set; }

    public decimal? NetCostToRecover { get; set; }

    public decimal ForecastRevenue { get; set; }

    public decimal ForecastBalance { get; set; }

    public decimal? FullEconomicCostBalance { get; set; }
}

/// <summary>The arithmetic written out with this capability's own numbers in it.</summary>
public sealed class SealedWorkings
{
    /// <summary>Total operating cost, and where it came from.</summary>
    public string? C { get; set; }

    [JsonPropertyName("I_total")]
    public string? TotalIncome { get; set; }

    [JsonPropertyName("I_nonuwa")]
    public string? NonUwaIncome { get; set; }

    /// <summary>Forecast utilisation — the divisor.</summary>
    public string? U { get; set; }

    /// <summary>The indirect cost recovery factor in force.</summary>
    public string? K { get; set; }

    public string? UwaResearcher { get; set; }

    public string? Apfr { get; set; }

    public string? Commercial { get; set; }
}

/// <summary>A snapshot this renderer will not turn into a document, and why.</summary>
public sealed class SealedRecordFormatException : Exception
{
    public SealedRecordFormatException(string message)
        : base(message)
    {
    }

    public SealedRecordFormatException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
