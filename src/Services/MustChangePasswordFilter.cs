using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CostingTool.Data;

/// <summary>
/// Holds every page closed to a session whose password was chosen by someone else — an
/// administrator creating or resetting the account, or the bootstrap value in the server's
/// environment — until the person chooses their own (M4 in the audit).
///
/// A page filter, so it sees the page being asked for and not only the URL: the pages that
/// stay open are named here once, and anything added later is closed by default.
/// </summary>
public sealed class MustChangePasswordFilter : IAsyncPageFilter
{
    /// <summary>The pages such a session may still reach: change the password, sign out, read an error.</summary>
    public static readonly IReadOnlySet<string> Open = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "/Account/ChangePassword",
        "/Account/Logout",
        "/Account/Login",
        "/Account/AccessDenied",
        "/Error"
    };

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (context.HttpContext.User.MustChangePassword()
            && !Open.Contains(context.ActionDescriptor.ViewEnginePath))
        {
            context.Result = new RedirectToPageResult("/Account/ChangePassword");
            return;
        }

        await next();
    }
}
