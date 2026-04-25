using DeepSigma.Mathematics.AutoDiff.Symbolic;

namespace DeepSigma.Mathematics.AutoDiff.Tests.UnitSymbolic;

public class SimplifierTests
{
    [Fact]
    public void Additive_Identity_LeftAndRight()
    {
        var x = SymbolicFactory.Variable<double>("x");
        Assert.Equal(x, Simplifier.Simplify(x + SymbolicFactory.Constant(0.0)));
        Assert.Equal(x, Simplifier.Simplify(SymbolicFactory.Constant(0.0) + x));
    }

    [Fact]
    public void Multiplicative_Identity_And_Zero()
    {
        var x = SymbolicFactory.Variable<double>("x");
        Assert.Equal(x, Simplifier.Simplify(x * SymbolicFactory.Constant(1.0)));
        Assert.Equal(new ConstantExpression<double>(0.0), Simplifier.Simplify(x * SymbolicFactory.Constant(0.0)));
    }

    [Fact]
    public void ConstantFolding()
    {
        var e = SymbolicFactory.Constant(2.0) + SymbolicFactory.Constant(3.0);
        Assert.Equal(new ConstantExpression<double>(5.0), Simplifier.Simplify(e));
    }

    [Fact]
    public void DoubleNegation()
    {
        var x = SymbolicFactory.Variable<double>("x");
        var e = new NegateExpression<double>(new NegateExpression<double>(x));
        Assert.Equal(x, Simplifier.Simplify(e));
    }

    [Fact]
    public void XMinusX_Zero()
    {
        var x = SymbolicFactory.Variable<double>("x");
        Assert.Equal(new ConstantExpression<double>(0.0), Simplifier.Simplify(x - x));
    }

    [Fact]
    public void XOverX_One()
    {
        var x = SymbolicFactory.Variable<double>("x");
        Assert.Equal(new ConstantExpression<double>(1.0), Simplifier.Simplify(x / x));
    }

    [Fact]
    public void Power_Identity_And_Zero()
    {
        var x = SymbolicFactory.Variable<double>("x");
        Assert.Equal(x, Simplifier.Simplify(SymbolicFactory.Pow(x, SymbolicFactory.Constant(1.0))));
        Assert.Equal(new ConstantExpression<double>(1.0), Simplifier.Simplify(SymbolicFactory.Pow(x, SymbolicFactory.Constant(0.0))));
    }

    [Fact]
    public void LogExp_Cancellation()
    {
        var x = SymbolicFactory.Variable<double>("x");
        Assert.Equal(x, Simplifier.Simplify(SymbolicFactory.Log(SymbolicFactory.Exp(x))));
        Assert.Equal(x, Simplifier.Simplify(SymbolicFactory.Exp(SymbolicFactory.Log(x))));
    }

    [Fact]
    public void Simplify_PreservesSemantics()
    {
        var x = SymbolicFactory.Variable<double>("x");
        var y = SymbolicFactory.Variable<double>("y");
        var f = (x + SymbolicFactory.Constant(0.0)) * (y * SymbolicFactory.Constant(1.0)) + SymbolicFactory.Log(SymbolicFactory.Exp(x * y));
        var simplified = Simplifier.Simplify(f);
        var env = new Dictionary<string, double> { ["x"] = 1.3, ["y"] = 0.7 };
        Assert.Equal(f.Evaluate(env), simplified.Evaluate(env), 10);
    }
}
