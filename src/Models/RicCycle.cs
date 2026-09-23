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

    /// <summary>
    /// Operating cost categories, as the client's workbook lays them out [W, sheet 1] and
    /// requirements §4 Step 1 lists them.
    ///
    /// <b>Which list applies depends on where the cost sits.</b> A capability carries its
    /// <i>directly incurred</i> costs (rows 11–23). The platform carries <i>directly
    /// allocated</i> costs (rows 27–36) and <i>indirect</i> floor-area costs (rows 40–41),
    /// entered once and split across the capabilities. The same words — "materials and
    /// supplies", say — appear in both lists because the workbook has both rows; which one a
    /// line is depends on its scope, not its name.
    ///
    /// These replaced a list of six (Personnel, Equipment, Maintenance, Travel, Animal Cost,
    /// Other) that matched no client document. Travel and animal costs now go under
    /// "Other expenses".
    /// </summary>
    public static class CostCategories
    {
        /// <summary>A capability's staff: base salary and on-costs [W, rows 11–14].</summary>
        public const string EmployeeSalaryAndOnCosts = "Employee salary and on-costs";

        /// <summary>The platform leader's salary, entered once at platform level [W, rows 35–36].</summary>
        public const string PlatformLeaderSalary = "Platform leader salary and on-costs";

        public const string LaboratoryFloorArea = "Laboratory floor area";
        public const string OfficeFloorArea = "Office floor area";

        /// <summary>Booked against one capability [W, sheet 1 rows 11–23].</summary>
        public static readonly string[] DirectlyIncurred =
        [
            EmployeeSalaryAndOnCosts,
            "Materials and supplies",
            "Non-capital equipment purchases",
            "Other expenses",
            "Rental, hiring and leasing fees",
            "Repairs and maintenance",
            "Maintenance contracts",
            "R&M assumption threshold",
            "Decommissioning costs",
            "Utilities and rates"
        ];

        /// <summary>Entered once for the platform and apportioned [W, sheet 1 rows 27–36].</summary>
        public static readonly string[] DirectlyAllocated =
        [
            PlatformLeaderSalary,
            "Materials and supplies",
            "Other expenses",
            "Repairs and maintenance",
            "Cleaning and waste disposal",
            "IT costs",
            "Rental, hiring and leasing fees",
            "Anticipated R&M cost buffer",
            "Administration costs"
        ];

        /// <summary>Floor area at a rate per m² per annum [W, sheet 1 rows 40–41].</summary>
        public static readonly string[] Indirect = [LaboratoryFloorArea, OfficeFloorArea];

        /// <summary>The categories a line in this scope may take.</summary>
        public static IReadOnlyList<string> For(string? scope) =>
            scope == Scopes.Platform ? [.. DirectlyAllocated, .. Indirect] : DirectlyIncurred;

        /// <summary>A line entered as a person, with the personnel details.</summary>
        public static bool IsPersonnel(string? category) =>
            category is EmployeeSalaryAndOnCosts or PlatformLeaderSalary;

        /// <summary>A line entered as floor area × a rate per m², rather than as dollars by year.</summary>
        public static bool IsFloorArea(string? category) =>
            category is LaboratoryFloorArea or OfficeFloorArea;

        /// <summary>The workbook's three kinds of cost, in words for a screen.</summary>
        public static string ClassOf(string? scope, string? category) =>
            scope != Scopes.Platform ? "Directly incurred"
            : IsFloorArea(category) ? "Indirect"
            : "Directly allocated";
    }

    /// <summary>The roles the workbook costs staff under [W, sheet 1 rows 11–14].</summary>
    public static class Positions
    {
        public const string PlatformLeader = "Platform leader";
        public const string ResearchOfficer = "Research officer";

        public static readonly string[] All = [PlatformLeader, ResearchOfficer];
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

    /// <summary>
    /// How the forecast utilisation was arrived at.
    ///
    /// The guide's Step 5 checklist requires utilisation assumptions to be documented before
    /// approval, and US-13 asks for the explanation to sit in the section it explains rather
    /// than all at the end. Required by the capacity step, carried into the sealed record.
    /// </summary>
    public string? UtilisationAssumptions { get; set; }

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

    /// <summary>
    /// The sealed cycle this one replaces, when it replaces one (US-01, F22).
    ///
    /// <b>The reference is the whole of supersession.</b> The older record is never written
    /// to: it stays sealed, readable and byte-for-byte what was approved. Whether it is
    /// superseded is worked out from this column — it is, once a cycle pointing at it has
    /// itself been sealed — so there is no second copy of the fact to fall out of step.
    /// </summary>
    public int? SupersedesCycleId { get; set; }

    public RicCycle? Supersedes { get; set; }

    /// <summary>
    /// When the custodian last changed a figure or an answer (US-02). Kept apart from
    /// <see cref="UpdatedAtUtc"/>, which the approver's decisions also move, so "last edited"
    /// never names an approver who edited nothing.
    /// </summary>
    public DateTime LastEditedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Who made that change, as a <b>username</b> — see the note on <see cref="CreatedBy"/>.</summary>
    public string LastEditedBy { get; set; } = string.Empty;

    /// <summary>Who made that change, as it should appear on screen.</summary>
    public string LastEditedByDisplay { get; set; } = string.Empty;

    /// <summary>
    /// The step the custodian last had open, 1 to 6, so reopening a draft returns them to it
    /// (US-02). See <c>RicSteps</c> for the pages the numbers stand for.
    /// </summary>
    public int LastStep { get; set; } = 1;

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
    /// <summary>
    /// Usable annual capacity, in the billable unit.
    ///
    /// Since US-07 this is <b>worked out</b>, not typed: the capacity step builds it from
    /// <see cref="CapacityBaseline"/>, the <see cref="CapacityDeductions"/> and the staff cap
    /// through <c>CapacityEngine</c>, and stores the result here so that every later screen,
    /// the sealed snapshot and the PDF read one figure. The name is kept from before, when it
    /// was a single typed number, so that sealed records and their schema do not move.
    /// </summary>
    public decimal MaximumCapacity { get; set; }
    public decimal ForecastUwaUse { get; set; }
    public decimal ForecastApfrUse { get; set; }
    public decimal ForecastCommercialUse { get; set; }
    public decimal ProposedUwaRate { get; set; }
    public decimal ProposedApfrRate { get; set; }
    public decimal ProposedCommercialRate { get; set; }

    // ---- How MaximumCapacity was built (US-07) --------------------------------------------

    /// <summary>
    /// <see cref="Engine.CapacityBaseline.Machine"/>, <see cref="Engine.CapacityBaseline.Staff"/>
    /// or <see cref="Engine.CapacityBaseline.Stated"/>; empty until the capacity step is saved.
    /// </summary>
    public string CapacityBaseline { get; set; } = string.Empty;

    /// <summary>The custodian's own baseline, used only when <see cref="CapacityBaseline"/> is Stated.</summary>
    public decimal StatedBaseline { get; set; }

    /// <summary>Where a stated baseline came from. Required with one.</summary>
    public string? StatedBaselineNote { get; set; }

    /// <summary>A person must be present to run it, so capacity is capped at their FTE [W, sheet 2 row 18].</summary>
    public bool IsStaffReliant { get; set; }

    /// <summary>FTE allocated to the capability when it is staff-reliant, e.g. 0.05.</summary>
    public decimal StaffFte { get; set; }

    /// <summary>
    /// Why the forecast is above usable capacity. Allowed, but never unexplained (US-08):
    /// required whenever the forecast exceeds <see cref="MaximumCapacity"/>.
    /// </summary>
    public string? AboveCapacityReason { get; set; }

    /// <summary>What is taken off the baseline, each with its note.</summary>
    public List<RicCapacityDeduction> CapacityDeductions { get; set; } = [];

    /// <summary>
    /// <c>U</c> — forecast annual utilisation, the divisor behind all three rates. The
    /// per-category split drives only the revenue projection (requirements §9, Q2).
    /// </summary>
    public decimal ForecastUtilisation => ForecastUwaUse + ForecastApfrUse + ForecastCommercialUse;
}

/// <summary>
/// One reason a capability is unavailable for part of its baseline (US-07): maintenance,
/// downtime, compliance, setup and pack-down, or planned outages [G, Step 2].
/// </summary>
public class RicCapacityDeduction
{
    /// <summary>The deductions the guide names, in the order it names them [G, Step 2].</summary>
    public static readonly string[] Kinds =
        ["Maintenance", "Downtime", "Compliance requirements", "Setup and pack-down", "Planned outages"];

    public int Id { get; set; }
    public int RicCapabilityId { get; set; }
    public RicCapability RicCapability { get; set; } = null!;
    public string Kind { get; set; } = string.Empty;

    /// <summary>In the capability's billable unit.</summary>
    public decimal Amount { get; set; }

    /// <summary>Why this much is taken off. Required: each deduction takes a note (US-07).</summary>
    public string Note { get; set; } = string.Empty;
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

    /// <summary>Platform leader or research officer, for a staff line [W, sheet 1 rows 11–14].</summary>
    public string? Position { get; set; }

    /// <summary>Square metres, for an indirect floor-area line [W, sheet 1 rows 40–41].</summary>
    public decimal? FloorArea { get; set; }

    /// <summary>Dollars per m² per annum, for an indirect floor-area line.</summary>
    public decimal? FloorAreaRate { get; set; }

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
