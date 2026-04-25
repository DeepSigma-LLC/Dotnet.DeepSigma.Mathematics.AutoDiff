using System.Numerics;
using DeepSigma.Mathematics.AutoDiff.Forward;

namespace DeepSigma.Mathematics.AutoDiff.JVP;

/// <summary>
/// Jacobian-vector product (JVP) via forward-mode automatic differentiation.
/// Given f: ℝⁿ → ℝᵐ and a tangent vector v ∈ ℝⁿ, computes J(f)(x)·v ∈ ℝᵐ
/// in a single forward pass by seeding each <see cref="DualNumber{T}"/> with the
/// corresponding component of <paramref name="tangent"/>.
/// </summary>
public static class ForwardJacobian
{
    /// <summary>
    /// Computes J(f)(x)·v for a vector-valued function f: ℝⁿ → ℝᵐ.
    /// </summary>
    /// <typeparam name="T">A floating-point scalar type.</typeparam>
    /// <param name="f">The vector function to differentiate.</param>
    /// <param name="x">The evaluation point in ℝⁿ.</param>
    /// <param name="tangent">The tangent vector v ∈ ℝⁿ to multiply the Jacobian by.</param>
    /// <returns>The JVP result J(f)(x)·v ∈ ℝᵐ.</returns>
    public static T[] Compute<T>(
        Func<DualNumber<T>[], DualNumber<T>[]> f,
        T[] x,
        T[] tangent)
        where T : IFloatingPoint<T>
    {
        JacobianHelpers.ValidateLengths(nameof(x), x.Length, nameof(tangent), tangent.Length);

        var duals = new DualNumber<T>[x.Length];
        for (int i = 0; i < x.Length; i++)
            duals[i] = new DualNumber<T>(x[i], tangent[i]);

        var result = f(duals);
        var jvp = new T[result.Length];
        for (int i = 0; i < result.Length; i++)
            jvp[i] = result[i].Dual;
        return jvp;
    }

    /// <summary>
    /// Computes J(f)(x)·v for a scalar-output function f: ℝⁿ → ℝ.
    /// Returns the directional derivative ∇f(x)·v as a scalar.
    /// </summary>
    /// <typeparam name="T">A floating-point scalar type.</typeparam>
    /// <param name="f">The scalar function to differentiate.</param>
    /// <param name="x">The evaluation point in ℝⁿ.</param>
    /// <param name="tangent">The tangent vector v ∈ ℝⁿ.</param>
    /// <returns>The directional derivative ∇f(x)·v.</returns>
    public static T Compute<T>(
        Func<DualNumber<T>[], DualNumber<T>> f,
        T[] x,
        T[] tangent)
        where T : IFloatingPoint<T>
    {
        JacobianHelpers.ValidateLengths(nameof(x), x.Length, nameof(tangent), tangent.Length);

        var duals = new DualNumber<T>[x.Length];
        for (int i = 0; i < x.Length; i++)
            duals[i] = new DualNumber<T>(x[i], tangent[i]);

        return f(duals).Dual;
    }
}
