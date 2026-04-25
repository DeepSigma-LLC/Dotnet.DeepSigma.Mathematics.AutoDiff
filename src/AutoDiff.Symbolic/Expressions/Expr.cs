using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Symbolic;

/// <summary>
/// Base type for symbolic expression trees. Records provide structural equality
/// which powers the Simplifier's fixed-point loop.
/// </summary>
public abstract record Expr<T> where T : IFloatingPoint<T>
{
    public abstract T Evaluate(IReadOnlyDictionary<string, T> env);
    public abstract Expr<T> Differentiate(string variable);
    public abstract Expr<T> Simplify();

    // Operator overloads for natural math syntax
    public static Expr<T> operator +(Expr<T> a, Expr<T> b) => new AddExpr<T>(a, b);
    public static Expr<T> operator -(Expr<T> a, Expr<T> b) => new SubExpr<T>(a, b);
    public static Expr<T> operator *(Expr<T> a, Expr<T> b) => new MulExpr<T>(a, b);
    public static Expr<T> operator /(Expr<T> a, Expr<T> b) => new DivExpr<T>(a, b);
    public static Expr<T> operator -(Expr<T> a) => new NegExpr<T>(a);

    public static implicit operator Expr<T>(T value) => new ConstExpr<T>(value);
}
