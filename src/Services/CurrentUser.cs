using System.Security.Claims;

namespace CostingTool.Data;

/// <summary>
/// Who is signed in, split into the two things a page actually needs.
///
/// The distinction matters enough to have its own file. <see cref="UserName"/> is the
/// identity a query filters on: unique, stable, never shown. <see cref="DisplayName"/> is
/// what a person reads: not unique, and free to change. Reaching for
/// <c>User.Identity.Name</c> gives you the second one, which is why every ownership check
/// in this application once compared display names — and why none of them do now.
/// </summary>
public static class CurrentUser
{
    /// <summary>The claim type carrying the username. Set at sign-in.</summary>
    public const string UserNameClaim = "username";

    /// <summary>
    /// Rotated whenever credentials change. A cookie carrying an old value is rejected,
    /// which signs out other sessions after a password change or account reset.
    /// </summary>
    public const string SecurityStampClaim = "security_stamp";

    /// <summary>
    /// Present, as "true", on a session whose password was set by someone else. Until it is
    /// changed the session can reach only the change-password page (M4).
    /// </summary>
    public const string MustChangePasswordClaim = "must_change_password";

    /// <summary>Whether this session has to choose its own password before anything else.</summary>
    public static bool MustChangePassword(this ClaimsPrincipal principal) =>
        principal.HasClaim(MustChangePasswordClaim, "true");

    /// <summary>The signed-in user's username, or empty for an anonymous request.</summary>
    public static string UserName(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(UserNameClaim) ?? string.Empty;

    /// <summary>The signed-in user's name as it should appear on screen.</summary>
    public static string DisplayName(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Name) ?? principal.Identity?.Name ?? "Unknown";
}
