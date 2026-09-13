using System.Globalization;
using System.Text;
using CostingTool.Data;
using CostingTool.Pdf;
using Microsoft.AspNetCore.Mvc;

namespace CostingTool.Pages.Ric;

/// <summary>
/// Download the sealed record as a PDF — US-16.
///
/// <b>The document is built from the snapshot, not from the cycle.</b> The rows are still
/// in the database and can be superseded later; the snapshot is what the delegated
/// authority approved. Rendering from the rows would mean the exported document could
/// quietly disagree with the record it claims to be, which is the whole failure this
/// tool exists to remove (architecture.md §4).
///
/// So this handler does three things and nothing else: check the cycle belongs to the
/// signed-in custodian, check it is sealed, and hand the stored snapshot to the renderer.
/// </summary>
public class ExportModel(CostingDbContext db) : RicPageModel(db)
{
    public async Task<IActionResult> OnGetAsync(int cycleId)
    {
        if (!await LoadCycleAsync(cycleId))
        {
            return NotFound();
        }

        if (Cycle.Status != "Sealed" || string.IsNullOrWhiteSpace(Cycle.SnapshotJson))
        {
            TempData["Error"] =
                "This cycle has not been sealed yet, so there is no record to export. " +
                "A record can be exported once the delegated authority has approved it.";
            return RedirectToPage("/Ric/Review", new { cycleId });
        }

        byte[] pdf;
        try
        {
            pdf = SealedRecordPdf.Render(Cycle.SnapshotJson, Cycle.SnapshotHash);
        }
        catch (SealedRecordFormatException error)
        {
            // The renderer refuses rather than producing a document with holes in it. The
            // message is written for a custodian, so it is shown rather than swallowed.
            TempData["Error"] = error.Message;
            return RedirectToPage("/Ric/Review", new { cycleId });
        }

        return File(pdf, "application/pdf", FileName());
    }

    /// <summary>
    /// A filename someone can find again in six months, and file in Content Manager
    /// without renaming: platform, pricing period, and the date it was sealed.
    /// </summary>
    private string FileName()
    {
        var sealedOn = (Cycle.SealedAtUtc ?? DateTime.UtcNow).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        return $"costing-record-{Slug(Cycle.PlatformName)}-{Cycle.StartYear}-{Cycle.EndYear}-sealed-{sealedOn}.pdf";
    }

    private static string Slug(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "platform";
        }

        var slug = new StringBuilder(value.Length);
        var lastWasDash = false;

        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                slug.Append(char.ToLowerInvariant(character));
                lastWasDash = false;
            }
            else if (!lastWasDash && slug.Length > 0)
            {
                slug.Append('-');
                lastWasDash = true;
            }
        }

        return slug.ToString().Trim('-') is { Length: > 0 } cleaned ? cleaned : "platform";
    }
}
