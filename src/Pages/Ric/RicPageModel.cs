using CostingTool.Data;
using CostingTool.Models;
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

    /// <summary>The guided steps, in order.</summary>
    protected static readonly string[] Steps =
        ["/Ric/Start", "/Ric/Costs", "/Ric/Funding", "/Ric/Capacity", "/Ric/Rates", "/Ric/Review"];

    /// <summary>
    /// Where a "save and leave" from <paramref name="current"/> may go (US-10): an earlier
    /// step, and only an earlier step. Anything else — a later step, a page outside the flow,
    /// a URL from somewhere other than the step bar — goes to the step just before this one.
    /// </summary>
    protected static string EarlierStep(string? to, string current)
    {
        var here = Array.IndexOf(Steps, current);
        var there = to is null ? -1 : Array.IndexOf(Steps, to);
        return there >= 0 && there < here ? Steps[there] : Steps[Math.Max(0, here - 1)];
    }

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
