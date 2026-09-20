using System.ComponentModel.DataAnnotations.Schema;

namespace CostingTool.Models;

/// <summary>
/// The vocabulary of a cost entry, in one place.
///
/// These used to be string literals compared with <c>==</c> in the engine, the page models
/// and the views. A category renamed in one of those places and not the others would have
/// moved a rate without failing anything — the silent-error class this project exists to
/// remove. Now a typo does not compile.
/// </summary>
public static class CostEntry
{
    public static class Types
    {
        public const string Cost = "Detailed cost";
        public const string Income = "Non-variable income";
    }

    public static class Scopes
    {
        public const string Capability = "Capability";
        public const string Platform = "Platform";
    }

    /// <summary>Operating cost categories, from the client's guide, Step 1.</summary>
    public static class CostCategories
    {
        public const string Personnel = "Personnel";

        public static readonly string[] All =
            [Personnel, "Equipment", "Maintenance", "Travel", "Animal Cost", "Other"];
    }

    /// <summary>
    /// The four non-variable income lines. The UWA / non-UWA split is <b>derived</b> from
    /// this list rather than stored, because the three formulas deduct different subsets
    /// and deriving them from one source means they cannot drift apart
    /// (architecture.md §4).
    /// </summary>
    public static class IncomeCategories
    {
        public const string UwaGpInKind = "UWA GP / in-kind";
        public const string State = "State";
        public const string Federal = "Federal (incl. NCRIS)";

        /// <summary>
        /// Worded as the client's workbook and requirements §4 word it. This used to read
        /// "Other recurrent support"; a row saved under the old label is still non-UWA
        /// income, because only <see cref="UwaGpInKind"/> is treated as UWA money.
        /// </summary>
        public const string Other = "Other (e.g. philanthropic)";

        public static readonly string[] All = [UwaGpInKind, State, Federal, Other];

        /// <summary>
        /// True for the one line that is UWA money. Everything else is non-UWA. The engine
        /// split and the on-screen explanation both come from here, so they cannot disagree.
        /// </summary>
        public static bool IsUwa(string? category) => category == UwaGpInKind;

        /// <summary>
        /// Which calculated rates a line lowers, in words a custodian can read beside the
        /// field (US-06, requirements §4). The commercial rate deducts no income at all.
        /// </summary>
        public static string RatesReduced(string? category) =>
            IsUwa(category)
                ? "Lowers the UWA Researcher rate only"
                : "Lowers the UWA Researcher and APFR rates";
    }
}

public class RicCycle
{
    public int Id { get; set; }
    public string PlatformName { get; set; } = string.Empty;
    public int StartYear { get; set; }
    public int EndYear { get; set; }
    public string BillableUnit { get; set; } = "Hours";
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// The <see cref="Engine.MethodConfig"/> version this cycle's rates were calculated
    /// under. Stamped when the record is sealed, so the record reproduces its own figures
    /// rather than today's — architecture.md §3, rule R6.
    /// </summary>
    public string MethodVersion { get; set; } = string.Empty;

    /// <summary>
    /// The owner, as a <b>username</b>.
    ///
    /// This is an access-control key, so it has to be stable and unique. It used to hold
    /// the display name, which is neither: two people called J. Smith would have seen each
    /// other's cycles, and anyone who changed their display name would have lost their
    /// own. <see cref="CreatedByDisplay"/> is what a human reads; this is what a query
    /// filters on. Never compare a display name to decide who may see a record.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>The owner's name as it should appear on screen and in the sealed record.</summary>
    public string CreatedByDisplay { get; set; } = string.Empty;

    public string? BenchmarkNotes { get; set; }
    public string? PricingJustification { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public string? ReturnedBy { get; set; }
    public DateTime? ReturnedAtUtc { get; set; }
    public string? ReturnReason { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public string? ApprovalComment { get; set; }
    public string? SealedBy { get; set; }
    public DateTime? SealedAtUtc { get; set; }
    public string? SnapshotJson { get; set; }
    public string? SnapshotHash { get; set; }
    public DateTime? EffectiveDateUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public List<RicCapability> Capabilities { get; set; } = [];
    public List<RicCostEntry> Costs { get; set; } = [];

    public bool IsEditable => Status is "Draft" or "Returned";
}

public class AppUser
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = Roles.DataEntry;
    public bool IsActive { get; set; } = true;
    public int AccessFailedCount { get; set; }
    public DateTime? LockoutEndUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
    public DateTime PasswordChangedAtUtc { get; set; } = DateTime.UtcNow;
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");

    public static class Roles
    {
        public const string DataEntry = "DataEntry";
        public const string Approver = "Approver";

        /// <summary>
        /// Sees every cycle, whoever created it and whatever state it is in, and
        /// administers the accounts themselves (US-19).
        ///
        /// <b>Deliberately not a super-custodian.</b> An administrator cannot edit, submit
        /// or seal another person's cycle: those actions write a name into the record, and
        /// US-02, US-15 and US-16 rest on that name being the person who did the work.
        /// The role set beyond this is [Q4], still open with the client.
        /// </summary>
        public const string Administrator = "Administrator";

        /// <summary>Every role the application recognises, for account administration.</summary>
        public static readonly string[] All = [DataEntry, Approver, Administrator];

        /// <summary>The role in words, for a screen.</summary>
        public static string Describe(string? role) => role switch
        {
            Approver => "Delegated approver",
            Administrator => "Administrator",
            _ => "Platform custodian"
        };
    }
}

public class AppNotification
{
    public int Id { get; set; }

    /// <summary>The recipient's <b>username</b> — see the note on <see cref="RicCycle.CreatedBy"/>.</summary>
    public string RecipientUserName { get; set; } = string.Empty;

    public int? RicCycleId { get; set; }
    public string Type { get; set; } = "Info";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class RicCapability
{
    public int Id { get; set; }
    public int RicCycleId { get; set; }
    public RicCycle RicCycle { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public decimal MaximumCapacity { get; set; }
    public decimal ForecastUwaUse { get; set; }
    public decimal ForecastApfrUse { get; set; }
    public decimal ForecastCommercialUse { get; set; }
    public decimal ProposedUwaRate { get; set; }
    public decimal ProposedApfrRate { get; set; }
    public decimal ProposedCommercialRate { get; set; }

    /// <summary>
    /// <c>U</c> — forecast annual utilisation, the divisor behind all three rates. The
    /// per-category split drives only the revenue projection (requirements §9, Q2).
    /// </summary>
    public decimal ForecastUtilisation => ForecastUwaUse + ForecastApfrUse + ForecastCommercialUse;
}

public class RicCostEntry
{
    public int Id { get; set; }
    public int RicCycleId { get; set; }
    public RicCycle RicCycle { get; set; } = null!;

    /// <summary>Null for a platform-level line; set for a line booked to one capability.</summary>
    public int? RicCapabilityId { get; set; }

    public RicCapability? Capability { get; set; }
    public string Scope { get; set; } = CostEntry.Scopes.Capability;
    public string CostType { get; set; } = CostEntry.Types.Cost;
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// The annual figure the engine uses: the mean of the per-year amounts in
    /// <see cref="YearAmounts"/>.
    ///
    /// <b>Averaging across the years of a cycle is our decision, not the client's.</b>
    /// Their guide's worked example is a single year, and requirements assumption A11 says
    /// figures are budgeted annual amounts without saying what a multi-year profile
    /// collapses to. Recorded here so the choice is visible; it is on the list to confirm.
    /// </summary>
    public decimal Amount { get; set; }

    public string? Notes { get; set; }
    public string? PersonnelName { get; set; }
    public string? FundingType { get; set; }
    public string? FellowshipType { get; set; }
    public string? StepOption { get; set; }
    public int? WorkYears { get; set; }
    public string? EmploymentType { get; set; }
    public decimal? PercentWorked { get; set; }
    public decimal? SuperannuationPercent { get; set; }
    public string? StaffType { get; set; }
    public string? SalaryScale { get; set; }
    public string? SalaryStep { get; set; }
    public string? SchoolType { get; set; }
    public decimal? BaseSalary { get; set; }
    public string? Description { get; set; }
    public string? Supplier { get; set; }
    public List<RicCostYearAmount> YearAmounts { get; set; } = [];

    [NotMapped]
    public bool IsIncome => CostType == CostEntry.Types.Income;

    /// <summary>
    /// True for the one income line that is deducted from the UWA researcher rate but not
    /// from the APFR rate. Derived from the category, never stored separately.
    /// </summary>
    [NotMapped]
    public bool IsUwaIncome => IsIncome && CostEntry.IncomeCategories.IsUwa(Category);
}

public class RicCostYearAmount
{
    public int Id { get; set; }
    public int RicCostEntryId { get; set; }
    public RicCostEntry RicCostEntry { get; set; } = null!;
    public int ProjectYear { get; set; }
    public decimal Amount { get; set; }
}
