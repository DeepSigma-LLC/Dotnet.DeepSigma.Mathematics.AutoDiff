using DeepSigma.Mathematics.AutoDiff.Symbolic;

namespace DeepSigma.Mathematics.AutoDiff.Tests.UnitSymbolic;

public class ExpressionTests
{
    private static Dictionary<string, double> Env(params (string k, double v)[] pairs)
        => pairs.ToDictionary(p => p.k, p => p.v);

    [Fact]
    public void Evaluate_Polynomial()
    {
        var x = SymbolicFactory.Variable<double>("x");
        var f = x * x + 3.0 * x + SymbolicFactory.Constant(1.0);
        Assert.Equal(11.0, f.Evaluate(Env(("x", 2.0))), 10);
    }

    [Fact]
    public void Evaluate_TranscendentalMix()
    {
        var x = SymbolicFactory.Variable<double>("x");
        var f = SymbolicFactory.Sin(x) + SymbolicFactory.Exp(x);
        var expected = Math.Sin(0.5) + Math.Exp(0.5);
        Assert.Equal(expected, f.Evaluate(Env(("x", 0.5))), 10);
    }

    [Fact]
    public void Evaluate_MultiVariable()
    {
        var x = SymbolicFactory.Variable<double>("x");
        var y = SymbolicFactory.Variable<double>("y");
        var f = x * y + SymbolicFactory.Log(x);
        var expected = 2.0 * 3.0 + Math.Log(2.0);
        Assert.Equal(expected, f.Evaluate(Env(("x", 2.0), ("y", 3.0))), 10);
    }

    [Fact]
    public void Interpreter_MatchesDirect()
    {
        var x = SymbolicFactory.Variable<double>("x");
        var f = SymbolicFactory.Cos(x * x);
        Assert.Equal(
            f.Evaluate(Env(("x", 1.3))),
            ExpressionInterpreter.EvaluateAt(f, "x", 1.3),
            10);
    }
}
