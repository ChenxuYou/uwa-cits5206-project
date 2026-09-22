namespace CostingTool.Engine;

/// <summary>
/// Usable annual capacity (US-07, requirements §4 Step 2).
///
/// Capacity is built, not typed: a stated baseline, less whatever stops the capability being
/// used, and — where a person has to be present — capped at the time the allocated staff can
/// give it. The workbook does this on sheet 2; here it is one pure function, so the capacity
/// screen, its tests and the figure a record carries cannot work it out three different ways.
///
/// <b>Capacity is not the divisor.</b> The rates divide by forecast utilisation (US-08), and
/// nothing in <see cref="RateEngine"/> reads the figure produced here. It is the ceiling the
/// forecast is judged against, and the "of capacity" in "45% of capacity".
/// </summary>
public static class CapacityEngine
{
    /// <summary>Work out one capability's usable capacity, in its billable unit.</summary>
    /// <exception cref="CapacityCalculationException">
    /// When the baseline is not positive, or a deduction or the staff allocation is negative.
    /// Rule R4: refuse, do not return a plausible number.
    /// </exception>
    public static UsableCapacity Calculate(CapacityInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        Guard(inputs);

        var deducted = inputs.Deductions.Sum(x => x.Amount);
        var afterDeductions = inputs.Baseline - deducted;

        // "If reliant on staff, row 17 = row 12" [W, sheet 2 row 18]. Read as a cap: a
        // capability cannot be used for more hours than the staff allocated to it can give,
        // nor for more than the machine itself is available once its deductions are taken.
        decimal? staffCap = inputs.StaffFte is { } fte && inputs.StaffBaseline is { } perFte
            ? fte * perFte
            : null;

        var usable = staffCap is { } cap ? Math.Min(afterDeductions, cap) : afterDeductions;

        return new UsableCapacity
        {
            Baseline = inputs.Baseline,
            Deductions = inputs.Deductions,
            TotalDeductions = deducted,
            AfterDeductions = afterDeductions,
            StaffFte = inputs.StaffFte,
            StaffBaseline = inputs.StaffBaseline,
            StaffCap = staffCap,
            Usable = usable
        };
    }

    /// <summary>
    /// The baselines the method offers in a billable unit, largest first.
    ///
    /// Hours and days convert from the configured working year. Samples do not: nothing in
    /// the client's documents says how many samples an hour holds, so a custodian costing
    /// per sample states their own baseline and says where it came from.
    /// </summary>
    public static IReadOnlyList<CapacityBaseline> BaselinesFor(MethodConfig method, string? billableUnit)
    {
        ArgumentNullException.ThrowIfNull(method);

        var perDay = UnitsPerDay(method, billableUnit);
        if (perDay is null)
        {
            return [];
        }

        return
        [
            new CapacityBaseline(CapacityBaseline.Machine, method.MachineAvailableDays, method.MachineAvailableDays * perDay.Value),
            new CapacityBaseline(CapacityBaseline.Staff, method.StaffAvailableDays, method.StaffAvailableDays * perDay.Value)
        ];
    }

    /// <summary>
    /// What one full-time member of staff gives a capability in a year, in its billable unit:
    /// 1,725 hours or 230 days as at 2026. Null for samples, where time does not convert.
    /// </summary>
    public static decimal? StaffAvailabilityPerFte(MethodConfig method, string? billableUnit) =>
        UnitsPerDay(method, billableUnit) is { } perDay ? method.StaffAvailableDays * perDay : null;

    private static decimal? UnitsPerDay(MethodConfig method, string? billableUnit) => billableUnit switch
    {
        "Hours" => method.HoursPerDay,
        "Days" => 1m,
        _ => null
    };

    private static void Guard(CapacityInputs inputs)
    {
        if (inputs.Baseline <= 0)
        {
            throw new CapacityCalculationException("A capacity baseline must be greater than zero.");
        }

        if (inputs.Deductions.Any(x => x.Amount < 0))
        {
            throw new CapacityCalculationException("A capacity deduction cannot be negative.");
        }

        if (inputs.StaffFte is < 0 || inputs.StaffBaseline is < 0)
        {
            throw new CapacityCalculationException("A staff allocation cannot be negative.");
        }
    }
}

/// <summary>One of the method's capacity baselines, in a billable unit.</summary>
/// <param name="Key"><see cref="Machine"/> or <see cref="Staff"/>.</param>
/// <param name="Days">The working days it rests on.</param>
/// <param name="Amount">The baseline in the billable unit, e.g. 1,882.5 hours.</param>
public sealed record CapacityBaseline(string Key, decimal Days, decimal Amount)
{
    /// <summary>Machine availability: 251 days × 7.5 h = 1,882.5 h as at 2026 [W, sheet 2 row 3].</summary>
    public const string Machine = "Machine";

    /// <summary>Staff availability: 230 days × 7.5 h = 1,725 h as at 2026 [W, sheet 2 row 4].</summary>
    public const string Staff = "Staff";

    /// <summary>A figure the custodian states and explains, for a unit the method cannot convert.</summary>
    public const string Stated = "Stated";
}

/// <summary>Everything the capacity calculation needs, and nothing else.</summary>
public sealed record CapacityInputs
{
    /// <summary>The starting figure, in the billable unit.</summary>
    public required decimal Baseline { get; init; }

    /// <summary>Maintenance, downtime and the rest, each in the billable unit.</summary>
    public IReadOnlyList<CapacityDeduction> Deductions { get; init; } = [];

    /// <summary>FTE allocated where a person must be present; null when the capability is not staff-reliant.</summary>
    public decimal? StaffFte { get; init; }

    /// <summary>What one FTE gives in a year, in the billable unit; null when there is no staff cap.</summary>
    public decimal? StaffBaseline { get; init; }
}

/// <summary>One reason a capability is not available for part of its baseline.</summary>
public sealed record CapacityDeduction(string Kind, decimal Amount);

/// <summary>Usable capacity and every figure behind it, so a screen can itemise it.</summary>
public sealed record UsableCapacity
{
    public required decimal Baseline { get; init; }

    public required IReadOnlyList<CapacityDeduction> Deductions { get; init; }

    public required decimal TotalDeductions { get; init; }

    /// <summary>Baseline less every deduction.</summary>
    public required decimal AfterDeductions { get; init; }

    public decimal? StaffFte { get; init; }

    public decimal? StaffBaseline { get; init; }

    /// <summary>FTE × staff availability; null when the capability is not staff-reliant.</summary>
    public decimal? StaffCap { get; init; }

    /// <summary>What the capability can actually be used for in a year.</summary>
    public required decimal Usable { get; init; }

    /// <summary>True when the staff allocation, not the machine, is what limits capacity.</summary>
    public bool IsStaffCapped => StaffCap is { } cap && cap < AfterDeductions;
}

public sealed class CapacityCalculationException(string message) : Exception(message);
