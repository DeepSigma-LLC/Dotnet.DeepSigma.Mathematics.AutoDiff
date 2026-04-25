using DeepSigma.Mathematics.AutoDiff.Tests.Unit;
using DeepSigma.Mathematics.AutoDiff.Symbolic;

namespace DeepSigma.Mathematics.AutoDiff.Tests.UnitSymbolic;

public class SymbolicDiffTests
{
    private static Dictionary<string, double> Env(params (string k, double v)[] pairs)
        => pairs.ToDictionary(p => p.k, p => p.v);

    [Fact]
    public void Derivative_OfXSquared_Is2X()
    {
        var x = SymbolicFactory.Variable<double>("x");
        var d = SymbolicDiff.Differentiate(x * x, "x");
        for (double v = -2.0; v <= 2.0; v += 0.5)
            Assert.Equal(2 * v, d.Evaluate(Env(("x", v))), 10);
    }

    [Fact]
    public void Derivative_OfSin_IsCos()
    {
        var x = SymbolicFactory.Variable<double>("x");
        var d = SymbolicDiff.Differentiate(SymbolicFactory.Sin(x), "x");
        Assert.Equal(Math.Cos(0.7), d.Evaluate(Env(("x", 0.7))), 10);
    }

    [Fact]
    public void ChainRule_SinOfXSquared()
    {
        var x = SymbolicFactory.Variable<double>("x");
        var d = SymbolicDiff.Differentiate(SymbolicFactory.Sin(x * x), "x");
        // d/dx sin(x²) = 2x cos(x²)
        var expected = 2 * 1.3 * Math.Cos(1.3 * 1.3);
        Assert.Equal(expected, d.Evaluate(Env(("x", 1.3))), 10);
    }

    [Fact]
    public void QuotientRule()
    {
        var x = SymbolicFactory.Variable<double>("x");
        var d = SymbolicDiff.Differentiate(SymbolicFactory.Sin(x) / x, "x");
        // d/dx [sin(x)/x] = (cos(x)*x - sin(x))/x²
        var v = 1.5;
        var expected = (Math.Cos(v) * v - Math.Sin(v)) / (v * v);
        Assert.Equal(expected, d.Evaluate(Env(("x", v))), 8);
    }

    [Fact]
    public void PartialDerivatives_Gradient()
    {
        var x = SymbolicFactory.Variable<double>("x");
        var y = SymbolicFactory.Variable<double>("y");
        var f = x * x * y + SymbolicFactory.Exp(y);
        var grad = SymbolicDiff.Gradient(f, "x", "y");

        var env = Env(("x", 1.5), ("y", 0.3));
        // ∂f/∂x = 2xy
        Assert.Equal(2 * 1.5 * 0.3, grad[0].Evaluate(env), 10);
        // ∂f/∂y = x² + exp(y)
        Assert.Equal(1.5 * 1.5 + Math.Exp(0.3), grad[1].Evaluate(env), 10);
    }

    [Fact]
    public void MatchesFiniteDifference()
    {
        var x = SymbolicFactory.Variable<double>("x");
        var f = SymbolicFactory.Exp(SymbolicFactory.Sin(x)) + x * x * x;
        var d = SymbolicDiff.Differentiate(f, "x");

        for (double v = -1.0; v <= 1.0; v += 0.5)
        {
            var actual = d.Evaluate(new Dictionary<string, double> { ["x"] = v });
            var expected = FiniteDiff.Derivative(
                xv => Math.Exp(Math.Sin(xv)) + xv * xv * xv, v);
            Assert.Equal(expected, actual, 5);
        }
    }
}
