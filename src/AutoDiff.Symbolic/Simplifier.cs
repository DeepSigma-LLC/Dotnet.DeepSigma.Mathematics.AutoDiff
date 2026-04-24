using System.Numerics;

namespace AutoDiff.Symbolic;

/// <summary>
/// Runs <see cref="Expr{T}.Simplify"/> in a fixed-point loop until the structure stops changing.
/// Record structural equality drives the termination check.
/// </summary>
public static class Simplifier
{
    public const int DefaultMaxPasses = 32;

    public static Expr<T> Simplify<T>(Expr<T> expr, int maxPasses = DefaultMaxPasses)
        where T : IFloatingPoint<T>
    {
        Expr<T> prev;
        int passes = 0;
        do
        {
            prev = expr;
            expr = expr.Simplify();
            passes++;
        } while (!expr.Equals(prev) && passes < maxPasses);

        return expr;
    }
}
