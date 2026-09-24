using CostingTool.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Records;

/// <summary>
/// The register of sealed records, by platform and pricing period (US-17, F13).
///
/// Only the columns a row shows are read: a snapshot can run to tens of kilobytes, and the
/// register never needs one. Whether a record is still current is worked out from the
/// reference a later sealed record holds to it (F22), the same way the overview does,
/// because the older record is never written to.
/// </summary>
public class IndexModel(CostingDbContext db) : PageModel
{
    public List<PlatformRecords> Platforms { get; private set; } = [];

    /// <summary>Every platform with a sealed record this user can see, for the filter.</summary>
    public List<string> PlatformNames { get; private set; } = [];

    public string? Platform { get; private set; }

    public bool SeesEveryRecord { get; private set; }

    public int Count => Platforms.Sum(x => x.Records.Count);

    public async Task OnGetAsync(string? platform)
    {
        Platform = string.IsNullOrWhiteSpace(platform) ? null : platform.Trim();
        SeesEveryRecord = RecordAccess.SeesEveryRecord(User);

        var visible = db.RicCycles.AsNoTracking().SealedVisibleTo(User);

        var rows = await visible
            .Select(x => new RecordRow(
                x.Id,
                x.PlatformName,
                x.StartYear,
                x.EndYear,
                x.SealedAtUtc,
                x.ApprovedBy,
                x.MethodVersion,
                x.CreatedByDisplay))
            .ToListAsync();

        var superseded = (await visible
                .Where(x => x.SupersedesCycleId != null)
                .Select(x => x.SupersedesCycleId!.Value)
                .ToListAsync())
            .ToHashSet();

        foreach (var row in rows)
        {
            row.IsSuperseded = superseded.Contains(row.Id);
        }

        // One platform is one heading however its name was typed from one cycle to the next.
        var groups = rows
            .GroupBy(x => x.PlatformName.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => new PlatformRecords(
                group.First().PlatformName.Trim(),
                [.. group
                    .OrderByDescending(x => x.StartYear)
                    .ThenByDescending(x => x.EndYear)
                    .ThenByDescending(x => x.SealedAtUtc)]))
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        PlatformNames = [.. groups.Select(x => x.Name)];

        Platforms = Platform is null
            ? groups
            : [.. groups.Where(x => string.Equals(x.Name, Platform, StringComparison.OrdinalIgnoreCase))];
    }
}

public sealed record PlatformRecords(string Name, List<RecordRow> Records);

public sealed class RecordRow(
    int id,
    string platformName,
    int startYear,
    int endYear,
    DateTime? sealedAtUtc,
    string? approvedBy,
    string methodVersion,
    string preparedBy)
{
    public int Id { get; } = id;
    public string PlatformName { get; } = platformName;
    public int StartYear { get; } = startYear;
    public int EndYear { get; } = endYear;
    public DateTime? SealedAtUtc { get; } = sealedAtUtc;
    public string? ApprovedBy { get; } = approvedBy;
    public string MethodVersion { get; } = methodVersion;
    public string PreparedBy { get; } = preparedBy;

    /// <summary>A later sealed record replaces this one (F22). Otherwise it holds the current rates.</summary>
    public bool IsSuperseded { get; set; }
}
