using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CostingTool.Pages;

/// <summary>
/// The page behind <c>UseExceptionHandler</c> and <c>UseStatusCodePagesWithReExecute</c>.
///
/// It says what went wrong in ordinary words and never echoes an exception: a custodian
/// meets this page perhaps once, and a stack trace helps nobody who is not us.
/// </summary>
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    /// <summary>
    /// A failed POST is re-executed here as a POST — the exception handler and the status-code
    /// pages keep the original method — so without this the page rendered its generic text
    /// for every refused form, whatever the code. It changes nothing, so it needs no token.
    /// </summary>
    public void OnPost(int? code) => OnGet(code);

    public string Heading { get; private set; } = "Something went wrong";

    public string Detail { get; private set; } =
        "The page could not be shown. Try again, and tell the project team if it keeps happening.";

    /// <summary>The trace identifier, so a report can be matched to a server log entry.</summary>
    public string? RequestId { get; private set; }

    public void OnGet(int? code)
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        (Heading, Detail) = code switch
        {
            404 => ("Page not found",
                "That address does not match anything in this application. It may have been mistyped, " +
                "or it may point at a costing cycle that is not yours."),

            409 => ("Someone else changed this record first",
                "Another request saved this costing cycle while you were working on it — an approver's " +
                "decision, or the same page open in a second tab. Nothing you just sent was saved. Go back, " +
                "reload the page to see the cycle as it now stands, and make the change again if it still applies."),

                        403 => ("You do not have access to that",
                "Your account does not carry the role that page needs. Custodians and delegated " +
                "approvers see different parts of the tool."),

            _ => (Heading, Detail)
        };
    }
}
