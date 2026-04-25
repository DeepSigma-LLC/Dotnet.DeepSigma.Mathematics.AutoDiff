using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Symbolic;

public sealed record ConstExpr<T>(T Value) : Expr<T> where T : IFloatingPoint<T>
{
    public override T Evaluate(IReadOnlyDictionary<string, T> env) => Value;
    public override Expr<T> Differentiate(string variable) => new ConstExpr<T>(T.Zero);
    public override Expr<T> Simplify() => this;
    public override string ToString() => Value.ToString() ?? "0";
}

public sealed record VarExpr<T>(string Name) : Expr<T> where T : IFloatingPoint<T>
{
    public override T Evaluate(IReadOnlyDictionary<string, T> env) => env[Name];
    public override Expr<T> Differentiate(string variable)
        => Name == variable ? new ConstExpr<T>(T.One) : new ConstExpr<T>(T.Zero);
    public override Expr<T> Simplify() => this;
    public override string ToString() => Name;
}
