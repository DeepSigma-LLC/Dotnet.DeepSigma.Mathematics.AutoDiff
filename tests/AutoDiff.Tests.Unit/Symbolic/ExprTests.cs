using AutoDiff.Symbolic;

namespace AutoDiff.Tests.Unit.Symbolic;

public class ExprTests
{
    private static Dictionary<string, double> Env(params (string k, double v)[] pairs)
        => pairs.ToDictionary(p => p.k, p => p.v);

    [Fact]
    public void Evaluate_Polynomial()
    {
        var x = Sym.Var<double>("x");
        var f = x * x + 3.0 * x + Sym.Const(1.0);
        Assert.Equal(11.0, f.Evaluate(Env(("x", 2.0))), 10);
    }

    [Fact]
    public void Evaluate_TranscendentalMix()
    {
        var x = Sym.Var<double>("x");
        var f = Sym.Sin(x) + Sym.Exp(x);
        var expected = Math.Sin(0.5) + Math.Exp(0.5);
        Assert.Equal(expected, f.Evaluate(Env(("x", 0.5))), 10);
    }

    [Fact]
    public void Evaluate_MultiVariable()
    {
        var x = Sym.Var<double>("x");
        var y = Sym.Var<double>("y");
        var f = x * y + Sym.Log(x);
        var expected = 2.0 * 3.0 + Math.Log(2.0);
        Assert.Equal(expected, f.Evaluate(Env(("x", 2.0), ("y", 3.0))), 10);
    }

    [Fact]
    public void Interpreter_MatchesDirect()
    {
        var x = Sym.Var<double>("x");
        var f = Sym.Cos(x * x);
        Assert.Equal(
            f.Evaluate(Env(("x", 1.3))),
            ExprInterpreter.EvaluateAt(f, "x", 1.3),
            10);
    }
}
