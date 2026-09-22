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
/// schema 1.2. The two must move together: adding a field to the snapshot without adding
/// it here means the PDF silently stops showing it, so <see cref="SupportedSchemaVersions"/>
/// is checked on the way in rather than trusted.
///
/// <b>Older snapshots keep rendering.</b> Schema 1.2 added the balance lines US-12 asks for;
/// a record sealed under 1.1 does not carry them, so they are nullable here and the document
/// leaves them out rather than printing a zero the approver never saw. Schema 1.3 added the
/// costing assumptions and how each capability's capacity was built, with the notes behind
/// it (US-07, US-13); an older record simply has none to show. The cost and funding lines have
/// been in every snapshot since 1.1 and are read from all of them. A snapshot is never
/// rewritten — the renderer is what learns the older shape.
/// </summary>
public sealed class SealedRecord
{
    /// <summary>The schema new snapshots are written in.</summary>
    public const string CurrentSchemaVersion = "1.3";

    /// <summary>Every schema this renderer can read, oldest first.</summary>
    public static readonly string[] SupportedSchemaVersions = ["1.1", "1.2", CurrentSchemaVersion];

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

    /// <summary>Every cost and funding line, with the note or justification entered against it.</summary>
    public List<SealedCostLine> Costs { get; set; } = [];

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

    // ---- Capacity baselines: schema 1.3 and later -----------------------------------------

    public decimal? MachineAvailableDays { get; set; }

    public string? MachineAvailabilityBasis { get; set; }

    public decimal? StaffAvailableDays { get; set; }

    public string? StaffAvailabilityBasis { get; set; }

    public decimal? HoursPerDay { get; set; }

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

    /// <summary>Schema 1.3 and later.</summary>
    public string? CostingAssumptions { get; set; }

    public string? UtilisationAssumptions { get; set; }

    public string? BenchmarkNotes { get; set; }

    public string? PricingJustification { get; set; }

    public string? SubmittedBy { get; set; }

    public DateTime? SubmittedAtUtc { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAtUtc { get; set; }

    public string? ApprovalComment { get; set; }

    public DateTime? EffectiveDateUtc { get; set; }
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

    /// <summary>Usable capacity, as built on the capacity step.</summary>
    public decimal MaximumCapacity { get; set; }

    // ---- How the capacity was built: schema 1.3 and later, empty before -----------------

    /// <summary>"Machine", "Staff" or "Stated"; null in a record sealed before schema 1.3.</summary>
    public string? CapacityBaseline { get; set; }

    /// <summary>The baseline in the billable unit, e.g. 1,882.5 hours for machine availability.</summary>
    public decimal? CapacityBaselineAmount { get; set; }

    /// <summary>FTE × staff availability, when the capability is staff-reliant.</summary>
    public decimal? StaffCapacity { get; set; }

    public decimal? StatedBaseline { get; set; }

    public string? StatedBaselineNote { get; set; }

    public bool? IsStaffReliant { get; set; }

    public decimal? StaffFte { get; set; }

    public List<SealedDeduction> CapacityDeductions { get; set; } = [];

    public string? AboveCapacityReason { get; set; }

    public decimal ForecastUwaUse { get; set; }

    public decimal ForecastApfrUse { get; set; }

    public decimal ForecastCommercialUse { get; set; }

    public SealedResult? Result { get; set; }

    public SealedWorkings? Workings { get; set; }
}

/// <summary>
/// One capability's figures, exactly as the engine returned them when the record was
/// sealed. <c>Display*</c> are the rounded rates: the same values the custodian saw on
/// screen, so the PDF cannot round differently from the page that was approved.
/// </summary>
/// <summary>One deduction from a capability's capacity baseline, with its note.</summary>
public sealed class SealedDeduction
{
    public string? Kind { get; set; }

    public decimal Amount { get; set; }

    public string? Note { get; set; }
}

/// <summary>
/// One cost or funding line as sealed. For a funding line, <see cref="Notes"/> holds the
/// justification the funding step requires.
/// </summary>
public sealed class SealedCostLine
{
    public int Id { get; set; }

    /// <summary>Null for a platform-level line.</summary>
    public int? RicCapabilityId { get; set; }

    public string? Scope { get; set; }

    public string? CostType { get; set; }

    public string? Category { get; set; }

    /// <summary>The annual figure: the mean of the per-year amounts.</summary>
    public decimal Amount { get; set; }

    public string? Notes { get; set; }

    public string? Description { get; set; }

    public string? PersonnelName { get; set; }

    /// <summary>Schema 1.3 and later.</summary>
    public string? Position { get; set; }

    /// <summary>Schema 1.3 and later: m², for a floor-area line.</summary>
    public decimal? FloorArea { get; set; }

    /// <summary>Schema 1.3 and later: dollars per m² per year.</summary>
    public decimal? FloorAreaRate { get; set; }

    /// <summary>Income rather than cost. Matches the web project's wording.</summary>
    public bool IsIncome => CostType == "Non-variable income";
}

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
