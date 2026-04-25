using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Forward;

/// <summary>
/// High-level entry points for computing derivatives, gradients, directional derivatives,
/// and Jacobians using forward-mode automatic differentiation via <see cref="DualNumber{T}"/>.
/// </summary>
/// <typeparam name="T">
/// A floating-point scalar type that satisfies the required generic math interfaces.
/// Typically <see langword="double"/> or <see langword="float"/>.
/// </typeparam>
public static class ForwardDiff<T>
    where T : IFloatingPoint<T>,
              ITrigonometricFunctions<T>,
              IExponentialFunctions<T>,
              ILogarithmicFunctions<T>,
              IHyperbolicFunctions<T>,
              IPowerFunctions<T>,
              IRootFunctions<T>
{
    /// <summary>
    /// Computes the derivative of a univariate function at the given point using a single forward pass.
    /// </summary>
    /// <param name="f">The scalar function to differentiate.</param>
    /// <param name="point">The point at which to evaluate the derivative.</param>
    /// <returns>f′(<paramref name="point"/>).</returns>
    public static T Derivative(Func<DualNumber<T>, DualNumber<T>> f, T point)
        => f(DualNumber<T>.Variable(point)).Dual;

    /// <summary>
    /// Computes the gradient of a scalar multivariate function via <c>n</c> sequential forward passes,
    /// one per input variable. For functions with many inputs, reverse-mode AD is generally more efficient.
    /// </summary>
    /// <param name="f">The scalar function f: ℝⁿ → ℝ to differentiate.</param>
    /// <param name="point">The point at which to evaluate the gradient.</param>
    /// <returns>An array of length <c>n</c> containing ∂f/∂xᵢ for each input.</returns>
    public static T[] Gradient(Func<DualNumber<T>[], DualNumber<T>> f, T[] point)
    {
        var grad = new T[point.Length];
        var inputs = new DualNumber<T>[point.Length];

        for (int i = 0; i < point.Length; i++)
        {
            for (int j = 0; j < point.Length; j++)
                inputs[j] = j == i ? DualNumber<T>.Variable(point[j]) : DualNumber<T>.Const(point[j]);

            grad[i] = f(inputs).Dual;
        }

        return grad;
    }

    /// <summary>
    /// Computes the directional derivative df/dv at <paramref name="point"/> along
    /// <paramref name="direction"/> using a single forward pass with the direction as the dual seed.
    /// </summary>
    /// <param name="f">The scalar function f: ℝⁿ → ℝ.</param>
    /// <param name="point">The evaluation point.</param>
    /// <param name="direction">The direction vector v (need not be unit-length).</param>
    /// <returns>∇f(<paramref name="point"/>) · <paramref name="direction"/>.</returns>
    public static T DirectionalDerivative(
        Func<DualNumber<T>[], DualNumber<T>> f, T[] point, T[] direction)
    {
        var inputs = new DualNumber<T>[point.Length];
        for (int i = 0; i < point.Length; i++)
            inputs[i] = DualNumber<T>.Variable(point[i], direction[i]);
        return f(inputs).Dual;
    }

    /// <summary>
    /// Computes the full Jacobian matrix of a vector-valued function f: ℝⁿ → ℝᵐ
    /// via <c>n</c> forward passes (one per input column).
    /// </summary>
    /// <param name="f">The vector function f: ℝⁿ → ℝᵐ.</param>
    /// <param name="point">The evaluation point in ℝⁿ.</param>
    /// <returns>
    /// An m×n matrix where element [i, j] is ∂fᵢ/∂xⱼ evaluated at <paramref name="point"/>.
    /// </returns>
    public static T[,] Jacobian(Func<DualNumber<T>[], DualNumber<T>[]> f, T[] point)
    {
        var inputs = new DualNumber<T>[point.Length];
        T[,]? jacobian = null;

        for (int i = 0; i < point.Length; i++)
        {
            for (int j = 0; j < point.Length; j++)
                inputs[j] = j == i ? DualNumber<T>.Variable(point[j]) : DualNumber<T>.Const(point[j]);

            var outputs = f(inputs);
            jacobian ??= new T[outputs.Length, point.Length];

            for (int row = 0; row < outputs.Length; row++)
                jacobian[row, i] = outputs[row].Dual;
        }

        return jacobian ?? new T[0, point.Length];
    }
}
