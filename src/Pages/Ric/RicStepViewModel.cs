namespace CostingTool.Pages.Ric;

/// <param name="Current">The step being shown, 1 to 6.</param>
/// <param name="CycleId">Null on the first step, before a cycle exists.</param>
/// <param name="CanNavigate">
/// Whether the steps already completed are links (US-10, "go back and change an input").
/// False for a cycle that is submitted or sealed, where the earlier screens are read-only
/// and a link back would lead nowhere useful.
/// </param>
public record RicStepViewModel(int Current, int? CycleId, bool CanNavigate = true);
