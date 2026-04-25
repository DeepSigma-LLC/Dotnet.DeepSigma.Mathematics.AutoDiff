using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Symbolic;

/// <summary>
/// Runs <see cref="Expression{T}.Simplify"/> in a fixed-point loop until the expression tree
/// stops changing between passes. Record structural equality drives the termination check.
/// </summary>
/// <remarks>
/// Each pass applies one round of algebraic rules (constant folding, identity elimination,
/// double-negation, log-exp cancellation, etc.). Most expressions converge within a few passes;
/// the loop is capped at <see cref="DefaultMaxPasses"/> to guard against pathological trees.
/// </remarks>
public static class Simplifier
{
    /// <summary>Maximum number of simplification passes before the loop terminates unconditionally.</summary>
    public const int DefaultMaxPasses = 32;

    /// <summary>
    /// Simplifies <paramref name="expression"/> to a fixed point, running at most
    /// <paramref name="maxPasses"/> rounds of rewriting.
    /// </summary>
    /// <typeparam name="T">The scalar type of the expression.</typeparam>
    /// <param name="expression">The expression tree to simplify.</param>
    /// <param name="maxPasses">Maximum number of simplification passes (default 32).</param>
    /// <returns>The simplified expression, structurally equal to or simpler than the input.</returns>
    public static Expression<T> Simplify<T>(Expression<T> expression, int maxPasses = DefaultMaxPasses)
        where T : IFloatingPoint<T>
    {
        Expression<T> previous;
        int passes = 0;
        do
        {
            previous = expression;
            expression = expression.Simplify();
            passes++;
        } while (!expression.Equals(previous) && passes < maxPasses);

        return expression;
    }
}
