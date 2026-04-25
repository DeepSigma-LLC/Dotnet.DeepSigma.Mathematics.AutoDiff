using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Symbolic;

/// <summary>
/// AOT-safe interpreter for <see cref="Expression{T}"/> trees.
/// Evaluates by walking the tree via virtual dispatch on each node's
/// <see cref="Expression{T}.Evaluate"/> method — no code generation and no reflection.
/// This is the default evaluator for Native AOT deployments.
/// </summary>
public static class ExpressionInterpreter
{
    /// <summary>
    /// Evaluates <paramref name="expression"/> by substituting variable values from
    /// <paramref name="environment"/>.
    /// </summary>
    /// <typeparam name="T">The scalar type of the expression.</typeparam>
    /// <param name="expression">The expression tree to evaluate.</param>
    /// <param name="environment">A mapping from variable names to their current scalar values.</param>
    public static T Evaluate<T>(Expression<T> expression, IReadOnlyDictionary<string, T> environment)
        where T : IFloatingPoint<T>
        => expression.Evaluate(environment);

    /// <summary>
    /// Convenience overload for single-variable expressions.
    /// Wraps <paramref name="value"/> into a single-entry environment and evaluates.
    /// </summary>
    /// <typeparam name="T">The scalar type of the expression.</typeparam>
    /// <param name="expression">The expression tree to evaluate.</param>
    /// <param name="variable">The name of the single variable.</param>
    /// <param name="value">The value to substitute for <paramref name="variable"/>.</param>
    public static T EvaluateAt<T>(Expression<T> expression, string variable, T value)
        where T : IFloatingPoint<T>
        => expression.Evaluate(new Dictionary<string, T> { [variable] = value });
}
