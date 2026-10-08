using CostingTool.Models;
using CostingTool.Pdf;

namespace CostingTool.Pages.Ric;

/// <summary>
/// The key figures of a sealed record, for a new cycle to be set against (US-01, F13).
///
/// <b>Read from the snapshot, not from the rows.</b> The snapshot is what was approved; the
/// figures shown beside a new cycle have to be the ones that were actually charged, in the
/// same way the PDF is (see <c>Export.cshtml.cs</c>). A snapshot this code cannot read is
/// reported rather than filled in from the rows.
/// </summary>
public sealed class PreviousRecord
{
    public int Id { get; private init; }

    public string PlatformName { get; private init; } = string.Empty;

    public int StartYear { get; private init; }

    public int EndYear { get; private init; }

    public string BillableUnit { get; private init; } = string.Empty;

    public DateTime? SealedAtUtc { get; private init; }

    public DateTime? EffectiveDateUtc { get; private init; }

    public decimal? TotalOperatingCost { get; private init; }

    public decimal? TotalIncome { get; private init; }

    public decimal? ForecastBalance { get; private init; }

    public IReadOnlyList<PreviousRates> Capabilities { get; private init; } = [];

    /// <summary>Why the figures cannot be shown, when the snapshot could not be read.</summary>
    public string? Problem { get; private init; }

    /// <summary>The rates that were approved for a capability of this name, if the record had one.</summary>
    public PreviousRates? For(string name) =>
        Capabilities.FirstOrDefault(x => string.Equals(x.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));

    public static PreviousRecord From(RicCycle cycle)
    {
        SealedRecord record;
        try
        {
            record = SealedRecord.Parse(cycle.SnapshotJson ?? string.Empty);
        }
        catch (SealedRecordFormatException error)
        {
            return new PreviousRecord
            {
                Id = cycle.Id,
                PlatformName = cycle.PlatformName,
                StartYear = cycle.StartYear,
                EndYear = cycle.EndYear,
                BillableUnit = cycle.BillableUnit,
                SealedAtUtc = cycle.SealedAtUtc,
                Problem = error.Message
            };
        }

        return new PreviousRecord
        {
            Id = cycle.Id,
            PlatformName = record.Cycle?.PlatformName ?? cycle.PlatformName,
            StartYear = record.Cycle?.StartYear ?? cycle.StartYear,
            EndYear = record.Cycle?.EndYear ?? cycle.EndYear,
            BillableUnit = record.Cycle?.BillableUnit ?? cycle.BillableUnit,
            SealedAtUtc = record.SealedAtUtc,
            EffectiveDateUtc = record.Cycle?.EffectiveDateUtc,
            TotalOperatingCost = record.Platform?.TotalOperatingCost,
            TotalIncome = record.Platform?.TotalIncome,
            ForecastBalance = record.Platform?.ForecastBalance,
            Capabilities = record.Capabilities
                .Select(x => new PreviousRates(
                    x.Name ?? x.Result?.CapabilityName ?? string.Empty,
                    x.ForecastUwaUse + x.ForecastApfrUse + x.ForecastCommercialUse,
                    x.Result?.ProposedUwaRate,
                    x.Result?.ProposedApfrRate,
                    x.Result?.ProposedCommercialRate))
                .ToList()
        };
    }
}

/// <summary>One capability's approved rates in a sealed record.</summary>
public sealed record PreviousRates(string Name, decimal ForecastUtilisation, decimal? Uwa, decimal? Apfr, decimal? Commercial);
