using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Symbolic;

/// <summary>
/// AOT-safe interpreter for Expr&lt;T&gt; trees. Walks the tree recursively via virtual dispatch
/// on the record's <see cref="Expr{T}.Evaluate"/> method — no codegen, no reflection.
/// </summary>
public static class ExprInterpreter
{
    public static T Evaluate<T>(Expr<T> expr, IReadOnlyDictionary<string, T> env)
        where T : IFloatingPoint<T>
        => expr.Evaluate(env);

    /// <summary>Convenience overload for single-variable expressions.</summary>
    public static T EvaluateAt<T>(Expr<T> expr, string variable, T value)
        where T : IFloatingPoint<T>
        => expr.Evaluate(new Dictionary<string, T> { [variable] = value });
}
