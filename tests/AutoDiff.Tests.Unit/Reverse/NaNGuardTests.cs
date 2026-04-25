using DeepSigma.Mathematics.AutoDiff.Core;
using DeepSigma.Mathematics.AutoDiff.Reverse;

namespace DeepSigma.Mathematics.AutoDiff.Tests.UnitReverse;

public class NaNGuardTests
{
    [Fact]
    public void Check_CleanValue_PassesThrough()
    {
        var result = NaNGuard.Check(3.14, "test");
        Assert.Equal(3.14, result);
    }

    [Fact]
    public void Check_NaN_Throws()
    {
        Assert.Throws<GradientNaNException>(() =>
            NaNGuard.Check(double.NaN, "test_op"));
    }

    [Fact]
    public void Check_Infinity_Throws()
    {
        Assert.Throws<GradientNaNException>(() =>
            NaNGuard.Check(double.PositiveInfinity, "test_op"));
    }

    [Fact]
    public void GradientNaNException_ContainsContext()
    {
        var ex = Assert.Throws<GradientNaNException>(() =>
            NaNGuard.Check(double.NaN, "my_operation"));
        Assert.Equal("my_operation", ex.OperationContext);
    }

    [Fact]
    public void Tape_NaNGuard_LogOfZero_ThrowsWithContext()
    {
        using var tape = ComputationTapePool<double>.Rent();
        tape.EnableNaNGuard = true;

        var x = tape.Variable(0.0);          // log(0) = -Inf
        var y = ReverseFunctions<double>.Log(x);  // primal = -Inf
        // weight = 1/0 = Inf; backward: 1.0 * Inf → NaN guard fires
        Assert.Throws<GradientNaNException>(() => tape.Backward(y));
    }

    [Fact]
    public void IsPathological_DetectsNaNAndInfinity()
    {
        Assert.True(NaNGuard.IsPathological(double.NaN));
        Assert.True(NaNGuard.IsPathological(double.PositiveInfinity));
        Assert.True(NaNGuard.IsPathological(double.NegativeInfinity));
        Assert.False(NaNGuard.IsPathological(3.14));
        Assert.False(NaNGuard.IsPathological(0.0));
    }
}
