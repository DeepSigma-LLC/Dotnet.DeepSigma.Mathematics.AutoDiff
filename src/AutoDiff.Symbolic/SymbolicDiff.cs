using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Symbolic;

/// <summary>
/// High-level entry points for symbolic differentiation.
/// The actual differentiation logic lives on each <see cref="Expression{T}"/> node via
/// <see cref="Expression{T}.Differentiate(string)"/>; this class provides convenience wrappers
/// that also run the simplifier on the result.
/// </summary>
public static class SymbolicDiff
{
    /// <summary>
    /// Differentiates <paramref name="expression"/> with respect to <paramref name="variable"/>
    /// and simplifies the result to a fixed point.
    /// </summary>
    /// <typeparam name="T">The scalar type of the expression.</typeparam>
    /// <param name="expression">The expression to differentiate.</param>
    /// <param name="variable">The variable name to differentiate with respect to.</param>
    /// <returns>The simplified symbolic derivative.</returns>
    public static Expression<T> Differentiate<T>(Expression<T> expression, string variable)
        where T : IFloatingPoint<T>
        => Simplifier.Simplify(expression.Differentiate(variable));

    /// <summary>
    /// Differentiates <paramref name="expression"/> without applying the simplifier.
    /// Useful for inspecting the raw derivative tree before simplification.
    /// </summary>
    public static Expression<T> DifferentiateRaw<T>(Expression<T> expression, string variable)
        where T : IFloatingPoint<T>
        => expression.Differentiate(variable);

    /// <summary>
    /// Computes the gradient of <paramref name="expression"/> with respect to each variable
    /// in <paramref name="variables"/>. Each partial derivative is simplified independently.
    /// </summary>
    /// <typeparam name="T">The scalar type of the expression.</typeparam>
    /// <param name="expression">The scalar expression to differentiate.</param>
    /// <param name="variables">The variable names to differentiate with respect to.</param>
    /// <returns>An array where element i is ∂expression/∂variables[i], simplified.</returns>
    public static Expression<T>[] Gradient<T>(Expression<T> expression, params string[] variables)
        where T : IFloatingPoint<T>
    {
        var result = new Expression<T>[variables.Length];
        for (int i = 0; i < variables.Length; i++)
            result[i] = Differentiate(expression, variables[i]);
        return result;
    }
}
