using System.Security.Claims;
using CostingTool.Data;
using CostingTool.Models;

namespace CostingTool.Pages.Records;

/// <summary>
/// Who may read which sealed record (US-17).
///
/// A custodian reads their own, as everywhere else in the application. An approver and an
/// administrator read every one: the approver sealed them, and the administrator is the
/// person US-17 is written for, answering a researcher about a rate set years ago by a
/// custodian who may since have left. Reading is all the register allows, so a wider view
/// here cannot put anyone's name into a record.
/// </summary>
public static class RecordAccess
{
    public static bool SeesEveryRecord(ClaimsPrincipal user) =>
        user.IsInRole(AppUser.Roles.Approver) || user.IsInRole(AppUser.Roles.Administrator);

    /// <summary>The sealed records <paramref name="user"/> may open. Drafts and submissions never appear.</summary>
    public static IQueryable<RicCycle> SealedVisibleTo(this IQueryable<RicCycle> cycles, ClaimsPrincipal user)
    {
        var sealedRecords = cycles.Where(x => x.Status == "Sealed");

        if (SeesEveryRecord(user))
        {
            return sealedRecords;
        }

        var owner = user.UserName();
        return sealedRecords.Where(x => x.CreatedBy == owner);
    }
}
