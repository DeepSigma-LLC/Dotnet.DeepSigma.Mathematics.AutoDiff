using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Reverse;

/// <summary>
/// High-level entry points for computing gradients and derivatives using reverse-mode
/// automatic differentiation. Prefer reverse mode when the number of inputs is large
/// relative to the number of outputs (e.g. training neural networks).
/// </summary>
/// <typeparam name="T">A floating-point scalar type that implements <see cref="IFloatingPoint{T}"/>.</typeparam>
public static class ReverseDiff<T>
    where T : IFloatingPoint<T>
{
    /// <summary>
    /// Computes the gradient of a scalar function f: ℝⁿ → ℝ at the given point
    /// using a single forward pass followed by one backward pass.
    /// </summary>
    /// <param name="f">The scalar function to differentiate.</param>
    /// <param name="point">The evaluation point in ℝⁿ.</param>
    /// <returns>An array of length n containing ∂f/∂xᵢ for each input.</returns>
    public static T[] Gradient(Func<Var<T>[], Var<T>> f, T[] point)
    {
        using var tape = ComputationTapePool<T>.Rent(point.Length * 4);
        var vars = new Var<T>[point.Length];
        for (int i = 0; i < point.Length; i++)
            vars[i] = tape.Variable(point[i]);

        var output = f(vars);
        tape.Backward(output.NodeId);

        var grad = new T[point.Length];
        for (int i = 0; i < point.Length; i++)
            grad[i] = vars[i].Gradient;
        return grad;
    }

    /// <summary>
    /// Computes the derivative of a univariate function at the given point using a single backward pass.
    /// </summary>
    /// <param name="f">The univariate function to differentiate.</param>
    /// <param name="point">The evaluation point.</param>
    /// <returns>f′(<paramref name="point"/>).</returns>
    public static T Derivative(Func<Var<T>, Var<T>> f, T point)
    {
        using var tape = ComputationTapePool<T>.Rent(16);
        var x = tape.Variable(point);
        var output = f(x);
        tape.Backward(output.NodeId);
        return x.Gradient;
    }
}
