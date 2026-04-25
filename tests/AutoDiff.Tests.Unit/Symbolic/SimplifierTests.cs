using DeepSigma.Mathematics.AutoDiff.Symbolic;

namespace DeepSigma.Mathematics.AutoDiff.Tests.UnitSymbolic;

public class SimplifierTests
{
    [Fact]
    public void Additive_Identity_LeftAndRight()
    {
        var x = Sym.Var<double>("x");
        Assert.Equal(x, Simplifier.Simplify(x + Sym.Const(0.0)));
        Assert.Equal(x, Simplifier.Simplify(Sym.Const(0.0) + x));
    }

    [Fact]
    public void Multiplicative_Identity_And_Zero()
    {
        var x = Sym.Var<double>("x");
        Assert.Equal(x, Simplifier.Simplify(x * Sym.Const(1.0)));
        Assert.Equal(new ConstExpr<double>(0.0), Simplifier.Simplify(x * Sym.Const(0.0)));
    }

    [Fact]
    public void ConstantFolding()
    {
        var e = Sym.Const(2.0) + Sym.Const(3.0);
        Assert.Equal(new ConstExpr<double>(5.0), Simplifier.Simplify(e));
    }

    [Fact]
    public void DoubleNegation()
    {
        var x = Sym.Var<double>("x");
        var e = new NegExpr<double>(new NegExpr<double>(x));
        Assert.Equal(x, Simplifier.Simplify(e));
    }

    [Fact]
    public void XMinusX_Zero()
    {
        var x = Sym.Var<double>("x");
        Assert.Equal(new ConstExpr<double>(0.0), Simplifier.Simplify(x - x));
    }

    [Fact]
    public void XOverX_One()
    {
        var x = Sym.Var<double>("x");
        Assert.Equal(new ConstExpr<double>(1.0), Simplifier.Simplify(x / x));
    }

    [Fact]
    public void Power_Identity_And_Zero()
    {
        var x = Sym.Var<double>("x");
        Assert.Equal(x, Simplifier.Simplify(Sym.Pow(x, Sym.Const(1.0))));
        Assert.Equal(new ConstExpr<double>(1.0), Simplifier.Simplify(Sym.Pow(x, Sym.Const(0.0))));
    }

    [Fact]
    public void LogExp_Cancellation()
    {
        var x = Sym.Var<double>("x");
        Assert.Equal(x, Simplifier.Simplify(Sym.Log(Sym.Exp(x))));
        Assert.Equal(x, Simplifier.Simplify(Sym.Exp(Sym.Log(x))));
    }

    [Fact]
    public void Simplify_PreservesSemantics()
    {
        var x = Sym.Var<double>("x");
        var y = Sym.Var<double>("y");
        var f = (x + Sym.Const(0.0)) * (y * Sym.Const(1.0)) + Sym.Log(Sym.Exp(x * y));
        var simplified = Simplifier.Simplify(f);
        var env = new Dictionary<string, double> { ["x"] = 1.3, ["y"] = 0.7 };
        Assert.Equal(f.Evaluate(env), simplified.Evaluate(env), 10);
    }
}
