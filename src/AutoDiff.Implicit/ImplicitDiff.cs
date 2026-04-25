using System.Numerics;
using DeepSigma.Mathematics.AutoDiff.Reverse;
using DeepSigma.Mathematics.AutoDiff.Symbolic;

namespace DeepSigma.Mathematics.AutoDiff.Implicit;

/// <summary>
/// Implicit function differentiation.
/// Given a constraint F(x, y) = 0, the implicit function theorem gives dy/dx = -(∂F/∂x) / (∂F/∂y).
/// </summary>
public static class ImplicitDiff
{
    /// <summary>
    /// Compute dy/dx at (x, y) given a constraint F(x, y) = 0.
    /// Uses a single reverse-mode tape evaluation to get both partials.
    /// </summary>
    public static T Derivative<T>(
        Func<Var<T>, Var<T>, Var<T>> constraint,
        T xValue,
        T yValue)
        where T : IFloatingPoint<T>
        => Derivative(constraint, xValue, yValue, T.CreateChecked(1e-12));

    /// <summary>
    /// Compute dy/dx at (x, y) given a constraint F(x, y) = 0 with an explicit singularity tolerance.
    /// </summary>
    /// <exception cref="ImplicitDerivativeException">
    /// Thrown when |∂F/∂y| is smaller than <paramref name="singularityTolerance"/>.
    /// </exception>
    public static T Derivative<T>(
        Func<Var<T>, Var<T>, Var<T>> constraint,
        T xValue,
        T yValue,
        T singularityTolerance)
        where T : IFloatingPoint<T>
    {
        using var tape = TapePool<T>.Rent();
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
    /// Gradient of y w.r.t. each input x_i given F(x⃗, y) = 0.
    /// Returns [∂y/∂x_0, ∂y/∂x_1, ...].
    /// </summary>
    public static T[] Gradient<T>(
        Func<Var<T>[], Var<T>, Var<T>> constraint,
        T[] xValues,
        T yValue)
        where T : IFloatingPoint<T>
        => Gradient(constraint, xValues, yValue, T.CreateChecked(1e-12));

    public static T[] Gradient<T>(
        Func<Var<T>[], Var<T>, Var<T>> constraint,
        T[] xValues,
        T yValue,
        T singularityTolerance)
        where T : IFloatingPoint<T>
    {
        using var tape = TapePool<T>.Rent(xValues.Length * 4);
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
    /// Symbolic version: returns an Expr&lt;T&gt; representing dy/dx for F(x,y)=0.
    /// Output is -∂F/∂x ÷ ∂F/∂y, simplified.
    /// </summary>
    public static Expr<T> DerivativeSymbolic<T>(Expr<T> constraint, string xVar, string yVar)
        where T : IFloatingPoint<T>
    {
        var dFdx = SymbolicDiff.Differentiate(constraint, xVar);
        var dFdy = SymbolicDiff.Differentiate(constraint, yVar);
        return Simplifier.Simplify(new NegExpr<T>(new DivExpr<T>(dFdx, dFdy)));
    }
}

public sealed class ImplicitDerivativeException : Exception
{
    public double DenominatorValue { get; }

    public ImplicitDerivativeException(string message, double denominatorValue)
        : base(message)
    {
        DenominatorValue = denominatorValue;
    }
}
