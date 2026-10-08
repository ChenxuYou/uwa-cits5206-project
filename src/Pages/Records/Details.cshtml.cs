using System.Security.Cryptography;
using System.Text;
using CostingTool.Data;
using CostingTool.Models;
using CostingTool.Pages.Ric;
using CostingTool.Pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Records;

/// <summary>
/// One sealed record, the whole of it, read-only — US-17, "why does it cost $50 an hour?".
///
/// <b>Everything on this page comes from the snapshot, never from the live rows or the
/// engine.</b> The Review page recalculates a sealed cycle under the method version it was
/// sealed with, which is right as long as that version is still in the table and its rows
/// are untouched. This page does not depend on either: it shows what the approver sealed,
/// in the numbers, workings, method and <c>k</c> the snapshot carries, so a later method
/// version, a new <c>k</c> or an edited row cannot change the answer [N6, N7]. It is the
/// same source the PDF is rendered from, so the page and the document cannot disagree.
///
/// There is no handler here that writes.
/// </summary>
public class DetailsModel(CostingDbContext db) : PageModel
{
    public RicCycle Cycle { get; private set; } = null!;

    /// <summary>The snapshot, read back; null when it could not be read — see <see cref="ReadError"/>.</summary>
    public SealedRecord? Record { get; private set; }

    public string? ReadError { get; private set; }

    /// <summary>
    /// True when the stored hash still matches the stored snapshot, so what is shown is what
    /// was sealed. False is shown, never hidden: it is evidence, not a rendering problem.
    /// </summary>
    public bool HashMatches { get; private set; }

    /// <summary>The sealed record that replaced this one, if one has (F22).</summary>
    public RicCycle? ReplacedBy { get; private set; }

    /// <summary>The record this one replaced, when the user may open it too.</summary>
    public int? ReplacesId { get; private set; }

    public bool IsSuperseded => ReplacedBy is not null;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!await LoadAsync(id))
        {
            return NotFound();
        }

        HashMatches = Cycle.SnapshotHash is { Length: > 0 } stored
            && string.Equals(stored, Hash(Cycle.SnapshotJson!), StringComparison.OrdinalIgnoreCase);

        try
        {
            Record = SealedRecord.Parse(Cycle.SnapshotJson!);
        }
        catch (SealedRecordFormatException error)
        {
            ReadError = error.Message;
            return Page();
        }

        var visible = db.RicCycles.AsNoTracking().SealedVisibleTo(User);

        ReplacedBy = await visible
            .Where(x => x.SupersedesCycleId == Cycle.Id)
            .OrderBy(x => x.SealedAtUtc)
            .FirstOrDefaultAsync();

        if (Record.Cycle?.Supersedes is { } replaced && await visible.AnyAsync(x => x.Id == replaced.Id))
        {
            ReplacesId = replaced.Id;
        }

        return Page();
    }

    /// <summary>
    /// The sealed PDF, for whoever may read the record — the custodian's own export is
    /// limited to the custodian, and an administrator answering a researcher needs the
    /// document too. Rendered from the same snapshot, under the same file name.
    /// </summary>
    public async Task<IActionResult> OnGetPdfAsync(int id)
    {
        if (!await LoadAsync(id))
        {
            return NotFound();
        }

        try
        {
            return File(SealedRecordPdf.Render(Cycle.SnapshotJson!, Cycle.SnapshotHash), "application/pdf", ExportModel.FileName(Cycle));
        }
        catch (SealedRecordFormatException)
        {
            // The page itself explains why; send the reader back to it.
            return RedirectToPage(new { id });
        }
    }

    private async Task<bool> LoadAsync(int id)
    {
        var cycle = await db.RicCycles.AsNoTracking()
            .SealedVisibleTo(User)
            .FirstOrDefaultAsync(x => x.Id == id);

        // Sealed without a snapshot should not exist; if it does, there is no record to show.
        if (cycle is null || string.IsNullOrWhiteSpace(cycle.SnapshotJson))
        {
            return false;
        }

        Cycle = cycle;
        return true;
    }

    /// <summary>The hash the approver's page stores beside the snapshot — see <c>Approvals/Details</c>.</summary>
    private static string Hash(string snapshotJson) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(snapshotJson)));

    /// <summary>"AwayFromZero" → "rounding half away from zero", as the PDF words it.</summary>
    public static string Rounding(SealedMethod method) =>
        $"{method.RateDecimals} decimal places, " + (method.MidpointRule switch
        {
            "AwayFromZero" => "rounding half away from zero",
            "ToEven" => "rounding half to even",
            null or "" => "rounding rule not recorded",
            var other => other
        });
}
