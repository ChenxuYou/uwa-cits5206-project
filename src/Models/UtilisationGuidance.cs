namespace CostingTool.Models;

/// <summary>
/// The words that go beside capacity and forecast utilisation, in one place (US-07, US-08).
///
/// Forecast utilisation — not capacity — is what every rate is divided by, and requirements
/// §4 Step 2 calls it "the single most misunderstandable number in the system". The client's
/// own wording is used, and kept here rather than in the view, as <see cref="RateCategories"/>
/// does for the rates, so that the screen and any later record say the same thing.
/// </summary>
public static class UtilisationGuidance
{
    /// <summary>The guide's warning about the weight of this figure [G, Step 2].</summary>
    public const string SignificantAssumption =
        "This figure will be used in the calculation of the capability's minimum sustainable "
        + "charge-out rates and is one of the most significant assumptions underpinning the pricing model.";

    /// <summary>The client's own illustration of the difference [K §2].</summary>
    public const string NotCapacity =
        "While the capacity might be a thousand hours per year, in practice it might only be used 500 hours a year.";

    /// <summary>What the guide asks a forecast to take into account [G, Step 2].</summary>
    public static readonly string[] Prompts =
    [
        "Historical usage trends",
        "Growth or decline in demand",
        "New research centres or major grants",
        "Strategic appointments",
        "Industry demand",
        "Competing facilities",
        "Regulatory change"
    ];

    /// <summary>What the guide says reduces capacity [G, Step 2].</summary>
    public const string Deductions =
        "Take off whatever stops the capability being used: maintenance, downtime, compliance "
        + "requirements, setup and pack-down, and planned outages. Each deduction needs a note.";
}
