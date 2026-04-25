using DeepSigma.Mathematics.AutoDiff.Forward;
using JvpFn = DeepSigma.Mathematics.AutoDiff.JVP.JVP;
using VjpFn = DeepSigma.Mathematics.AutoDiff.JVP.VJP;

namespace DeepSigma.Mathematics.AutoDiff.Tests.UnitJVPTests;

public class JVPTests
{
    [Fact]
    public void JVP_Scalar_DirectionalDerivative()
    {
        // f(x, y) = x² + y², at (1, 2), direction (1, 0) → ∂f/∂x = 2
        var result = JvpFn.Compute<double>(
            d => d[0] * d[0] + d[1] * d[1],
            new[] { 1.0, 2.0 },
            new[] { 1.0, 0.0 });
        Assert.Equal(2.0, result, 10);
    }

    [Fact]
    public void JVP_Vector_MatchesAnalytical()
    {
        // f(x, y) = (x*y, x+y); J = [[y, x], [1, 1]]
        // J · (1, 0) = (y, 1) = (3, 1) at (2, 3)
        var result = JvpFn.Compute<double>(
            d => new[] { d[0] * d[1], d[0] + d[1] },
            new[] { 2.0, 3.0 },
            new[] { 1.0, 0.0 });
        Assert.Equal(3.0, result[0], 10);
        Assert.Equal(1.0, result[1], 10);
    }

    [Fact]
    public void VJP_MatchesAnalytical()
    {
        // f(x, y) = (x*y, x+y); v^T J with v=(1,1) → (y+1, x+1) = (4, 3) at (2, 3)
        var result = VjpFn.Compute<double>(
            vars => new[] { vars[0] * vars[1], vars[0] + vars[1] },
            new[] { 2.0, 3.0 },
            new[] { 1.0, 1.0 });
        Assert.Equal(4.0, result[0], 10);
        Assert.Equal(3.0, result[1], 10);
    }

    [Fact]
    public void VJP_Jacobian_MatchesFiniteDiff()
    {
        // f(x, y) = (x² + y, sin(x) * y)
        DualNumber<double>[] F(DualNumber<double>[] d)
            => new[] { d[0] * d[0] + d[1], DualMath<double>.Sin(d[0]) * d[1] };

        var x = new[] { 1.3, 0.7 };
        var J = VjpFn.Jacobian<double>(F, x);

        // Analytical: J[0,0]=2x, J[0,1]=1, J[1,0]=cos(x)*y, J[1,1]=sin(x)
        Assert.Equal(2 * 1.3, J[0, 0], 8);
        Assert.Equal(1.0, J[0, 1], 8);
        Assert.Equal(Math.Cos(1.3) * 0.7, J[1, 0], 8);
        Assert.Equal(Math.Sin(1.3), J[1, 1], 8);
    }
}
