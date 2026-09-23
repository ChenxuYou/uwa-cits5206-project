namespace CostingTool.Pages.Ric;

/// <param name="Current">The step being shown, 1 to 6.</param>
/// <param name="CycleId">Null on the first step, before a cycle exists.</param>
/// <param name="CanNavigate">
/// Whether the steps already completed are links (US-10, "go back and change an input").
/// False for a cycle that is submitted or sealed, where the earlier screens are read-only
/// and a link back would lead nowhere useful.
/// </param>
public record RicStepViewModel(int Current, int? CycleId, bool CanNavigate = true);

/// <summary>
/// The six steps of the guided workflow, in order.
///
/// One list, read by the step bar and by the overview's "resume" link (US-02), so the number
/// stored in <see cref="Models.RicCycle.LastStep"/> cannot come to mean a different page in
/// one place than in the other.
/// </summary>
public static class RicSteps
{
    public static readonly (int Number, string Name, string Page)[] All =
    [
        (1, "Platform", "/Ric/Start"),
        (2, "Costs", "/Ric/Costs"),
        (3, "Funding", "/Ric/Funding"),
        (4, "Capacity", "/Ric/Capacity"),
        (5, "Rates", "/Ric/Rates"),
        (6, "Review", "/Ric/Review")
    ];

    public const int Review = 6;

    /// <summary>The step's number, clamped into range, so an out-of-range value still resumes somewhere real.</summary>
    public static (int Number, string Name, string Page) Get(int number) =>
        All[Math.Clamp(number, 1, All.Length) - 1];
}
