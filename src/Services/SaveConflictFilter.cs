using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace CostingTool.Data;

/// <summary>
/// A save refused because the record changed underneath the request becomes a 409 page that
/// says so, rather than an unhandled exception (C3).
///
/// Two refusals end up here. <see cref="DbUpdateConcurrencyException"/>: another request saved
/// the same cycle after this one read it, so its concurrency stamp no longer matched.
/// <see cref="SealedRecordException"/>: the cycle is sealed. The approval handlers catch the
/// first themselves, because an approver racing another approver deserves a sentence on the
/// same page; everywhere else this filter is the backstop, and nothing was written.
/// </summary>
public sealed class SaveConflictFilter : IAsyncPageFilter
{
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var executed = await next();

        if (executed.Exception is DbUpdateConcurrencyException or SealedRecordException && !executed.ExceptionHandled)
        {
            executed.ExceptionHandled = true;
            executed.Result = new StatusCodeResult(StatusCodes.Status409Conflict);
        }
    }
}
