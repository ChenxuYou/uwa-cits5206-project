namespace CostingTool.Data;

/// <summary>
/// What a password must be, defined once.
///
/// The rules used to live inside <c>ChangePassword.cshtml.cs</c>, private to the one page
/// that happened to need them first. Any second place that sets a password — an
/// administrator creating an account, an administrator resetting one — would have had to
/// restate them, and a restated rule is a rule that drifts. Everything that accepts a new
/// password calls <see cref="Problems"/>; the screens that describe the rule print
/// <see cref="Description"/> rather than their own wording of it.
/// </summary>
public static class PasswordPolicy
{
    public const int MinimumLength = 12;

    /// <summary>The rule in words, for the hint beneath a password field.</summary>
    public const string Description =
        "At least 12 characters, with an uppercase letter, a lowercase letter, a number and a symbol.";

    /// <summary>
    /// Every rule the candidate breaks. An empty list means it is acceptable.
    /// </summary>
    public static IReadOnlyList<string> Problems(string? password)
    {
        var candidate = password ?? string.Empty;
        var problems = new List<string>();

        if (candidate.Length < MinimumLength)
        {
            problems.Add($"The password must contain at least {MinimumLength} characters.");
        }

        if (!candidate.Any(char.IsUpper)
            || !candidate.Any(char.IsLower)
            || !candidate.Any(char.IsDigit)
            || candidate.All(char.IsLetterOrDigit))
        {
            problems.Add("Use at least one uppercase letter, lowercase letter, number and symbol.");
        }

        return problems;
    }

    /// <summary>True when the candidate breaks no rule.</summary>
    public static bool IsAcceptable(string? password) => Problems(password).Count == 0;
}
