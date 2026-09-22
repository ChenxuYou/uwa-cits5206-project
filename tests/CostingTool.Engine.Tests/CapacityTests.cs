using CostingTool.Engine;
using Xunit;

namespace CostingTool.Engine.Tests;

/// <summary>
/// US-07: usable capacity from a stated baseline, less deductions, capped by staff where a
/// person has to be present. The figures are the client's workbook's [W, sheet 2].
/// </summary>
public class CapacityTests
{
    private static readonly MethodConfig Method = MethodConfig.Fallback;

    [Fact]
    public void TheTwoBaselinesInHoursAreTheWorkbooksFigures()
    {
        var baselines = CapacityEngine.BaselinesFor(Method, "Hours");

        Assert.Equal(1_882.5m, baselines.Single(x => x.Key == CapacityBaseline.Machine).Amount);
        Assert.Equal(1_725m, baselines.Single(x => x.Key == CapacityBaseline.Staff).Amount);
    }

    [Fact]
    public void InDaysTheBaselinesAreTheWorkingDaysThemselves()
    {
        var baselines = CapacityEngine.BaselinesFor(Method, "Days");

        Assert.Equal(251m, baselines.Single(x => x.Key == CapacityBaseline.Machine).Amount);
        Assert.Equal(230m, baselines.Single(x => x.Key == CapacityBaseline.Staff).Amount);
    }

    [Fact]
    public void SamplesHaveNoBaselineBecauseTimeDoesNotConvert()
    {
        Assert.Empty(CapacityEngine.BaselinesFor(Method, "Samples"));
        Assert.Null(CapacityEngine.StaffAvailabilityPerFte(Method, "Samples"));
    }

    [Fact]
    public void TheBaselinesFollowTheConfigurationRatherThanAConstant()
    {
        // N7: a later method version with a different working year moves the baseline.
        var method = MethodConfig.Fallback;
        method.MachineAvailableDays = 250m;
        method.HoursPerDay = 8m;

        Assert.Equal(2_000m, CapacityEngine.BaselinesFor(method, "Hours").Single(x => x.Key == CapacityBaseline.Machine).Amount);
    }

    [Fact]
    public void DeductionsComeOffTheBaselineAndAreKeptForItemising()
    {
        var result = CapacityEngine.Calculate(new CapacityInputs
        {
            Baseline = 1_882.5m,
            Deductions = [new("Maintenance", 112.5m), new("Planned outages", 75m)]
        });

        Assert.Equal(187.5m, result.TotalDeductions);
        Assert.Equal(1_695m, result.Usable);
        Assert.Equal(2, result.Deductions.Count);
        Assert.False(result.IsStaffCapped);
    }

    [Fact]
    public void AStaffReliantCapabilityIsCappedAtItsAllocatedFte()
    {
        // Capability 7 and the Analysis/Consulting line in the workbook: 0.05 FTE × 1,725 h.
        var result = CapacityEngine.Calculate(new CapacityInputs
        {
            Baseline = 1_882.5m,
            StaffFte = 0.05m,
            StaffBaseline = CapacityEngine.StaffAvailabilityPerFte(Method, "Hours")
        });

        Assert.Equal(86.25m, result.StaffCap);
        Assert.Equal(86.25m, result.Usable);
        Assert.True(result.IsStaffCapped);
    }

    [Fact]
    public void AStaffCapAboveWhatTheMachineCanGiveDoesNotRaiseCapacity()
    {
        var result = CapacityEngine.Calculate(new CapacityInputs
        {
            Baseline = 1_882.5m,
            Deductions = [new("Downtime", 400m)],
            StaffFte = 1m,
            StaffBaseline = 1_725m
        });

        Assert.Equal(1_482.5m, result.Usable);
        Assert.False(result.IsStaffCapped);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1_000, -1)]
    public void ANonsensicalInputIsRefusedRatherThanPriced(decimal baseline, decimal deduction)
    {
        Assert.Throws<CapacityCalculationException>(() => CapacityEngine.Calculate(new CapacityInputs
        {
            Baseline = baseline,
            Deductions = [new("Maintenance", deduction)]
        }));
    }
}
