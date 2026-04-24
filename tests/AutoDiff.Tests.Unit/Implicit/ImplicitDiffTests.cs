using AutoDiff.Implicit;
using AutoDiff.Reverse;
using AutoDiff.Symbolic;

namespace AutoDiff.Tests.Unit.ImplicitTests;

public class ImplicitDiffTests
{
    [Fact]
    public void UnitCircle_DyDx_EqualsMinusXOverY()
    {
        // F(x, y) = x² + y² - 1 = 0 → dy/dx = -x/y
        var x = 0.6;
        var y = Math.Sqrt(1 - x * x);  // 0.8

        var dyDx = ImplicitDiff.Derivative<double>(
            (vx, vy) => vx * vx + vy * vy - 1.0,
            x, y);

        Assert.Equal(-x / y, dyDx, 10);
    }

    [Fact]
    public void Hyperbola_DyDx()
    {
        // F(x, y) = x*y - 2 = 0 → y = 2/x, dy/dx = -2/x²
        var x = 1.5;
        var y = 2.0 / x;

        var dyDx = ImplicitDiff.Derivative<double>(
            (vx, vy) => vx * vy - 2.0,
            x, y);

        Assert.Equal(-2.0 / (x * x), dyDx, 10);
    }

    [Fact]
    public void Singular_Throws()
    {
        // F(x, y) = x² + y² - 1 at (1, 0) → ∂F/∂y = 2y = 0
        Assert.Throws<ImplicitDerivativeException>(() =>
            ImplicitDiff.Derivative<double>(
                (vx, vy) => vx * vx + vy * vy - 1.0,
                1.0, 0.0));
    }

    [Fact]
    public void MultiInput_Gradient()
    {
        // F(x1, x2, y) = x1 + x2*y - 5 = 0 → y = (5 - x1)/x2
        // ∂y/∂x1 = -1/x2, ∂y/∂x2 = -y/x2
        var x = new[] { 1.0, 2.0 };
        var y = (5.0 - x[0]) / x[1];  // 2.0

        var grad = ImplicitDiff.Gradient<double>(
            (vxs, vy) => vxs[0] + vxs[1] * vy - 5.0,
            x, y);

        Assert.Equal(-1.0 / x[1], grad[0], 10);
        Assert.Equal(-y / x[1], grad[1], 10);
    }

    [Fact]
    public void Symbolic_DyDx_ForUnitCircle()
    {
        // Build F = x² + y² - 1 symbolically
        var x = Sym.Var<double>("x");
        var y = Sym.Var<double>("y");
        var F = x * x + y * y - Sym.Const(1.0);

        var dyDx = ImplicitDiff.DerivativeSymbolic<double>(F, "x", "y");
        // Evaluate at (0.6, 0.8)
        var env = new Dictionary<string, double> { ["x"] = 0.6, ["y"] = 0.8 };
        Assert.Equal(-0.6 / 0.8, dyDx.Evaluate(env), 10);
    }
}
