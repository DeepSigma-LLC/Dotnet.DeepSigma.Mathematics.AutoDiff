using System.Numerics;

namespace AutoDiff.Symbolic;

/// <summary>
/// Convenience wrappers for symbolic differentiation. The real work lives on each
/// <see cref="Expr{T}"/> via <c>Differentiate(variable)</c>.
/// </summary>
public static class SymbolicDiff
{
    /// <summary>Differentiate an expression w.r.t. a named variable and simplify the result.</summary>
    public static Expr<T> Differentiate<T>(Expr<T> expr, string variable)
        where T : IFloatingPoint<T>
        => Simplifier.Simplify(expr.Differentiate(variable));

    /// <summary>Differentiate without simplification (useful for debugging the raw derivative tree).</summary>
    public static Expr<T> DifferentiateRaw<T>(Expr<T> expr, string variable)
        where T : IFloatingPoint<T>
        => expr.Differentiate(variable);

    /// <summary>Gradient w.r.t. a list of variables, each simplified.</summary>
    public static Expr<T>[] Gradient<T>(Expr<T> expr, params string[] variables)
        where T : IFloatingPoint<T>
    {
        var result = new Expr<T>[variables.Length];
        for (int i = 0; i < variables.Length; i++)
            result[i] = Differentiate(expr, variables[i]);
        return result;
    }
}
