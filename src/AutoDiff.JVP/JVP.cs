using System.Numerics;
using DeepSigma.Mathematics.AutoDiff.Forward;

namespace DeepSigma.Mathematics.AutoDiff.JVP;

/// <summary>
/// Jacobian-vector product: J(f)·v via forward mode with a custom tangent seed.
/// Given f: R^n → R^m, computes J(f)(x)·v ∈ R^m in a single forward pass.
/// </summary>
public static class JVP
{
    /// <summary>JVP for f: R^n → R^m.</summary>
    public static T[] Compute<T>(
        Func<DualNumber<T>[], DualNumber<T>[]> f,
        T[] x,
        T[] tangent)
        where T : IFloatingPoint<T>
    {
        JvpHelpers.ValidateLengths(nameof(x), x.Length, nameof(tangent), tangent.Length);

        var duals = new DualNumber<T>[x.Length];
        for (int i = 0; i < x.Length; i++)
            duals[i] = new DualNumber<T>(x[i], tangent[i]);

        var result = f(duals);
        var jvp = new T[result.Length];
        for (int i = 0; i < result.Length; i++)
            jvp[i] = result[i].Dual;
        return jvp;
    }

    /// <summary>JVP for scalar-output f: R^n → R.</summary>
    public static T Compute<T>(
        Func<DualNumber<T>[], DualNumber<T>> f,
        T[] x,
        T[] tangent)
        where T : IFloatingPoint<T>
    {
        JvpHelpers.ValidateLengths(nameof(x), x.Length, nameof(tangent), tangent.Length);

        var duals = new DualNumber<T>[x.Length];
        for (int i = 0; i < x.Length; i++)
            duals[i] = new DualNumber<T>(x[i], tangent[i]);

        return f(duals).Dual;
    }
}
