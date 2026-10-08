using CostingTool.Data;
using CostingTool.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Pages.Ric;

/// <summary>
/// The one way a page in the guided workflow loads a cycle.
///
/// <b>Two things it makes impossible.</b> Every step used to write its own query, so each
/// carried its own <c>.Include()</c> list and its own ownership filter. A step that omitted
/// an <c>Include</c> would sum an unloaded collection to zero and quietly show different
/// figures from the step before it, and a step that omitted the filter would show one
/// custodian another's record. Loading through here means neither can happen by forgetting.
/// </summary>
public abstract class RicPageModel(CostingDbContext db) : PageModel
{
    protected CostingDbContext Db { get; } = db;

    public RicCycle Cycle { get; private set; } = null!;

    /// <summary>
    /// Load the cycle if it exists and belongs to the signed-in custodian.
    /// </summary>
    /// <returns>False when it does not exist or is not theirs — the page returns NotFound
    /// for both, so the URL cannot be used to find out which cycles exist.</returns>
    protected async Task<bool> LoadCycleAsync(int cycleId)
    {
        var owner = User.UserName();

        var cycle = await Db.RicCycles
            .Include(x => x.Capabilities).ThenInclude(x => x.CapacityDeductions)
            .Include(x => x.Costs).ThenInclude(x => x.Capability)
            .Include(x => x.Costs).ThenInclude(x => x.YearAmounts)
            .FirstOrDefaultAsync(x => x.Id == cycleId && x.CreatedBy == owner);

        if (cycle is null)
        {
            return false;
        }

        Cycle = cycle;
        return true;
    }

    /// <summary>
    /// The sealed record the loaded cycle replaces, if it replaces one (US-01), for its key
    /// figures to be shown beside this cycle's.
    /// </summary>
    protected async Task<PreviousRecord?> LoadReplacedRecordAsync()
    {
        if (Cycle.SupersedesCycleId is not { } id)
        {
            return null;
        }

        var owner = User.UserName();
        var replaced = await Db.RicCycles.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.CreatedBy == owner && x.Status == "Sealed");

        return replaced is null ? null : PreviousRecord.From(replaced);
    }

    /// <summary>
    /// Stamp the loaded cycle as changed, now, by the signed-in custodian (US-02). Every
    /// handler that saves a figure or an answer calls this before <c>SaveChangesAsync</c>.
    /// </summary>
    protected void RecordEdit()
    {
        var now = DateTime.UtcNow;
        Cycle.UpdatedAtUtc = now;
        Cycle.LastEditedAtUtc = now;
        Cycle.LastEditedBy = User.UserName();
        Cycle.LastEditedByDisplay = User.DisplayName();
    }

    /// <summary>
    /// Note which step of a draft is open, so reopening it from the overview comes back here
    /// (US-02). Opening a step is not an edit: nothing else about the cycle changes.
    /// </summary>
    protected async Task RememberStepAsync(int step)
    {
        if (!Cycle.IsEditable || Cycle.LastStep == step)
        {
            return;
        }

        Cycle.LastStep = step;
        await Db.SaveChangesAsync();
    }

    /// <summary>
    /// Where a save that was asked for on the way out of a page should land: the address of
    /// the link that was followed (see <c>data-save-on-leave</c> in the layout), or
    /// <paramref name="fallback"/>. Only a path on this site is accepted, so the field cannot
    /// be used to send anyone elsewhere.
    /// </summary>
    protected IActionResult Continue(string? leavingFor, string fallback)
    {
        if (IsLocalPath(leavingFor))
        {
            return LocalRedirect(leavingFor!);
        }

        return RedirectToPage(fallback, new { cycleId = Cycle.Id });
    }

    internal static bool IsLocalPath(string? path) =>
        !string.IsNullOrEmpty(path)
        && path[0] == '/'
        && (path.Length == 1 || (path[1] != '/' && path[1] != '\\'));

    /// <summary>
    /// "Inputs[1].UwaUse" → "Cryo-EM: UWA forecast use", for a page that binds one row per
    /// capability. Null for any key it does not recognise.
    /// </summary>
    protected string? IndexedFieldLabel(string key, IReadOnlyList<int> capabilityIds, IReadOnlyDictionary<string, string> fields)
    {
        const string prefix = "Inputs[";
        var close = key.IndexOf("].", StringComparison.Ordinal);

        if (!key.StartsWith(prefix, StringComparison.Ordinal)
            || close < 0
            || !int.TryParse(key.AsSpan(prefix.Length, close - prefix.Length), out var index)
            || !fields.TryGetValue(key[(close + 2)..], out var field))
        {
            return null;
        }

        var name = index < capabilityIds.Count
            ? Cycle.Capabilities.FirstOrDefault(x => x.Id == capabilityIds[index])?.Name
            : null;

        return name is null ? field : $"{name}: {field}";
    }
}
