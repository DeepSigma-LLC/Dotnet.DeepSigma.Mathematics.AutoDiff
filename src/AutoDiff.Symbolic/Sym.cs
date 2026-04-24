using System.Numerics;

namespace AutoDiff.Symbolic;

/// <summary>
/// Factory helpers for building expression trees with a concise syntax.
/// <code>
/// var x = Sym.Var&lt;double&gt;("x");
/// var f = Sym.Sin(x * x) + Sym.Exp(x);
/// var df = SymbolicDiff.Differentiate(f, "x");
/// </code>
/// </summary>
public static class Sym
{
    public static VarExpr<T> Var<T>(string name) where T : IFloatingPoint<T> => new(name);
    public static ConstExpr<T> Const<T>(T value) where T : IFloatingPoint<T> => new(value);

    public static Expr<T> Sin<T>(Expr<T> x)
        where T : IFloatingPoint<T>, ITrigonometricFunctions<T> => new SinExpr<T>(x);

    public static Expr<T> Cos<T>(Expr<T> x)
        where T : IFloatingPoint<T>, ITrigonometricFunctions<T> => new CosExpr<T>(x);

    public static Expr<T> Exp<T>(Expr<T> x)
        where T : IFloatingPoint<T>, IExponentialFunctions<T>, ILogarithmicFunctions<T>
        => new ExpExpr<T>(x);

    public static Expr<T> Log<T>(Expr<T> x)
        where T : IFloatingPoint<T>, ILogarithmicFunctions<T>, IExponentialFunctions<T>
        => new LogExpr<T>(x);

    public static Expr<T> Pow<T>(Expr<T> b, Expr<T> e)
        where T : IFloatingPoint<T>, IPowerFunctions<T>, ILogarithmicFunctions<T>, IExponentialFunctions<T>
        => new PowExpr<T>(b, e);
}
