using System.Numerics;
using DeepSigma.Mathematics.AutoDiff.Reverse;
using DeepSigma.Mathematics.AutoDiff.Symbolic;

namespace DeepSigma.Mathematics.AutoDiff.Implicit;

/// <summary>
/// Implicit function differentiation via the implicit function theorem.
/// Given a constraint F(x, y) = 0, the theorem states dy/dx = −(∂F/∂x) / (∂F/∂y).
/// </summary>
public static class ImplicitDiff
{
    /// <summary>
    /// Computes dy/dx at (<paramref name="xValue"/>, <paramref name="yValue"/>) given a
    /// constraint F(x, y) = 0 using a single reverse-mode tape evaluation for both partials.
    /// Uses a default singularity tolerance of 1e−12.
    /// </summary>
    /// <typeparam name="T">A floating-point scalar type.</typeparam>
    /// <param name="constraint">The constraint function F(x, y); must equal zero at the given point.</param>
    /// <param name="xValue">The x-coordinate at which to evaluate the derivative.</param>
    /// <param name="yValue">The y-coordinate at which to evaluate the derivative.</param>
    /// <returns>dy/dx = −∂F/∂x ÷ ∂F/∂y at the given point.</returns>
    public static T Derivative<T>(
        Func<Var<T>, Var<T>, Var<T>> constraint,
        T xValue,
        T yValue)
        where T : IFloatingPoint<T>
        => Derivative(constraint, xValue, yValue, T.CreateChecked(1e-12));

    /// <summary>
    /// Computes dy/dx at (<paramref name="xValue"/>, <paramref name="yValue"/>) given a
    /// constraint F(x, y) = 0 with an explicit singularity tolerance.
    /// </summary>
    /// <typeparam name="T">A floating-point scalar type.</typeparam>
    /// <param name="constraint">The constraint function F(x, y).</param>
    /// <param name="xValue">The x-coordinate at which to evaluate.</param>
    /// <param name="yValue">The y-coordinate at which to evaluate.</param>
    /// <param name="singularityTolerance">
    /// Minimum acceptable |∂F/∂y|; throws <see cref="ImplicitDerivativeException"/> if below this threshold.
    /// </param>
    /// <exception cref="ImplicitDerivativeException">
    /// Thrown when |∂F/∂y| is smaller than <paramref name="singularityTolerance"/>,
    /// indicating the implicit derivative is undefined at this point.
    /// </exception>
    public static T Derivative<T>(
        Func<Var<T>, Var<T>, Var<T>> constraint,
        T xValue,
        T yValue,
        T singularityTolerance)
        where T : IFloatingPoint<T>
    {
        using var tape = ComputationTapePool<T>.Rent();
        var x = tape.Variable(xValue, "x");
        var y = tape.Variable(yValue, "y");

        var F = constraint(x, y);
        tape.Backward(F);

        var dFdx = x.Gradient;
        var dFdy = y.Gradient;

        if (T.Abs(dFdy) < singularityTolerance)
            throw new ImplicitDerivativeException(
                $"∂F/∂y is near-singular (|{dFdy}| < {singularityTolerance}). Implicit derivative is undefined.",
                double.CreateChecked(dFdy));

        return -dFdx / dFdy;
    }

    /// <summary>
    /// Computes the gradient [∂y/∂x₀, ∂y/∂x₁, …] given a constraint F(x⃗, y) = 0.
    /// Uses a default singularity tolerance of 1e−12.
    /// </summary>
    /// <typeparam name="T">A floating-point scalar type.</typeparam>
    /// <param name="constraint">The constraint function F(x⃗, y).</param>
    /// <param name="xValues">The input coordinates x⃗.</param>
    /// <param name="yValue">The y-coordinate.</param>
    /// <returns>An array where element i is −(∂F/∂xᵢ) / (∂F/∂y).</returns>
    public static T[] Gradient<T>(
        Func<Var<T>[], Var<T>, Var<T>> constraint,
        T[] xValues,
        T yValue)
        where T : IFloatingPoint<T>
        => Gradient(constraint, xValues, yValue, T.CreateChecked(1e-12));

    /// <summary>
    /// Computes the gradient [∂y/∂x₀, ∂y/∂x₁, …] given a constraint F(x⃗, y) = 0
    /// with an explicit singularity tolerance.
    /// </summary>
    /// <exception cref="ImplicitDerivativeException">
    /// Thrown when |∂F/∂y| is smaller than <paramref name="singularityTolerance"/>.
    /// </exception>
    public static T[] Gradient<T>(
        Func<Var<T>[], Var<T>, Var<T>> constraint,
        T[] xValues,
        T yValue,
        T singularityTolerance)
        where T : IFloatingPoint<T>
    {
        using var tape = ComputationTapePool<T>.Rent(xValues.Length * 4);
        var xVars = new Var<T>[xValues.Length];
        for (int i = 0; i < xValues.Length; i++)
            xVars[i] = tape.Variable(xValues[i]);
        var y = tape.Variable(yValue, "y");

        var F = constraint(xVars, y);
        tape.Backward(F);

        var dFdy = y.Gradient;
        if (T.Abs(dFdy) < singularityTolerance)
            throw new ImplicitDerivativeException(
                $"∂F/∂y is near-singular (|{dFdy}| < {singularityTolerance}).",
                double.CreateChecked(dFdy));

        var result = new T[xValues.Length];
        for (int i = 0; i < xValues.Length; i++)
            result[i] = -xVars[i].Gradient / dFdy;
        return result;
    }

    /// <summary>
    /// Symbolic version: returns an <see cref="Expression{T}"/> representing dy/dx for F(x, y) = 0.
    /// The result is −∂F/∂x ÷ ∂F/∂y, simplified.
    /// </summary>
    /// <typeparam name="T">The scalar type of the expression.</typeparam>
    /// <param name="constraint">The symbolic expression for F.</param>
    /// <param name="xVar">The name of the x variable.</param>
    /// <param name="yVar">The name of the y variable.</param>
    public static Expression<T> DerivativeSymbolic<T>(Expression<T> constraint, string xVar, string yVar)
        where T : IFloatingPoint<T>
    {
        var dFdx = SymbolicDiff.Differentiate(constraint, xVar);
        var dFdy = SymbolicDiff.Differentiate(constraint, yVar);
        return Simplifier.Simplify(new NegateExpression<T>(new DivideExpression<T>(dFdx, dFdy)));
    }
}

/// <summary>
/// Thrown when the denominator ∂F/∂y is near-singular during implicit differentiation,
/// meaning the implicit derivative is undefined at the evaluated point.
/// </summary>
public sealed class ImplicitDerivativeException : Exception
{
    /// <summary>The value of ∂F/∂y that triggered the singularity check.</summary>
    public double DenominatorValue { get; }

    /// <summary>Initializes the exception with a message and the near-zero denominator value.</summary>
    public ImplicitDerivativeException(string message, double denominatorValue)
        : base(message)
    {
        DenominatorValue = denominatorValue;
    }
}
