using AutoDiff;
using AutoDiff.Reverse;
using AutoDiff.Forward;

namespace AutoDiff.Tests.Unit.Generator;

public static partial class GenSubjects
{
    [Differentiable]
    public static Var<double> Quad(Var<double> x, Var<double> y) => x * x + y * y;

    [Differentiable(Mode = DiffMode.Forward)]
    public static DualNumber<double> CubicForward(DualNumber<double> x)
        => x * x * x;
}

public class GeneratorIntegrationTests
{
    [Fact]
    public void Reverse_GeneratedGradientMatchesAnalytical()
    {
        var (dx, dy) = GenSubjects.Grad_Quad(3.0, 4.0);
        Assert.Equal(6.0, dx, 10);
        Assert.Equal(8.0, dy, 10);
    }

    [Fact]
    public void Forward_GeneratedGradientMatchesAnalytical()
    {
        // d/dx [x³] = 3x²
        var dx = GenSubjects.Grad_CubicForward(2.0);
        Assert.Equal(12.0, dx, 10);
    }
}
