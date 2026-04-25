using System.Numerics;
using DeepSigma.Mathematics.AutoDiff.Forward;
using DeepSigma.Mathematics.AutoDiff.Reverse;

namespace DeepSigma.Mathematics.AutoDiff.JVP;

/// <summary>
/// Vector-Jacobian product (VJP / pullback) via reverse-mode automatic differentiation.
/// Given f: ℝⁿ → ℝᵐ and a cotangent vector v ∈ ℝᵐ, computes vᵀ·J(f)(x) ∈ ℝⁿ
/// in a single backward pass by seeding the output gradients with <paramref name="cotangent"/>.
/// </summary>
public static class ReverseJacobian
{
    /// <summary>
    /// Computes vᵀ·J(f)(x) for a vector-valued function f: ℝⁿ → ℝᵐ using one backward pass.
    /// </summary>
    /// <typeparam name="T">A floating-point scalar type.</typeparam>
    /// <param name="f">The vector function whose Jacobian transpose-vector product to compute.</param>
    /// <param name="x">The evaluation point in ℝⁿ.</param>
    /// <param name="cotangent">The cotangent vector v ∈ ℝᵐ to left-multiply the Jacobian by.</param>
    /// <returns>The VJP result vᵀ·J(f)(x) ∈ ℝⁿ.</returns>
    public static T[] Compute<T>(
        Func<Var<T>[], Var<T>[]> f,
        T[] x,
        T[] cotangent)
        where T : IFloatingPoint<T>
    {
        using var tape = ComputationTapePool<T>.Rent(x.Length * 4);
        var vars = new Var<T>[x.Length];
        for (int i = 0; i < x.Length; i++)
            vars[i] = tape.Variable(x[i]);

        var outputs = f(vars);
        JacobianHelpers.ValidateLengths(nameof(outputs), outputs.Length, nameof(cotangent), cotangent.Length);

        tape.BackwardWithSeed(outputs, cotangent);

        var result = new T[x.Length];
        for (int i = 0; i < x.Length; i++)
            result[i] = vars[i].Gradient;
        return result;
    }

    /// <summary>
    /// Computes the full Jacobian matrix of f: ℝⁿ → ℝᵐ via n forward JVP passes,
    /// one per standard basis vector. Element [i, j] = ∂fᵢ/∂xⱼ.
    /// </summary>
    /// <typeparam name="T">A floating-point scalar type.</typeparam>
    /// <param name="f">The vector function to differentiate.</param>
    /// <param name="x">The evaluation point in ℝⁿ.</param>
    /// <returns>An m×n matrix where row i is the gradient of output i.</returns>
    public static T[,] Jacobian<T>(
        Func<DualNumber<T>[], DualNumber<T>[]> f,
        T[] x)
        where T : IFloatingPoint<T>
    {
        var tangent = new T[x.Length];
        T[,]? jacobian = null;

        for (int j = 0; j < x.Length; j++)
        {
            for (int k = 0; k < x.Length; k++)
                tangent[k] = k == j ? T.One : T.Zero;

            var col = ForwardJacobian.Compute(f, x, tangent);
            jacobian ??= new T[col.Length, x.Length];
            for (int i = 0; i < col.Length; i++)
                jacobian[i, j] = col[i];
        }

        return jacobian ?? new T[0, x.Length];
    }
}
