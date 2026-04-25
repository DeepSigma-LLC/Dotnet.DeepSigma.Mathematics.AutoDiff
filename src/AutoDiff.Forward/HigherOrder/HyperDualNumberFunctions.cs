using System.Numerics;
using System.Runtime.CompilerServices;
using DeepSigma.Mathematics.AutoDiff.Core;

namespace DeepSigma.Mathematics.AutoDiff.Forward;

/// <summary>
/// Elementary differentiable functions for <see cref="HyperDualNumber{T}"/>.
/// Each propagates both first- and second-order derivative information through the four
/// hyper-dual components (Real, Eps1, Eps2, Eps12) in a single evaluation pass.
/// </summary>
/// <typeparam name="T">A floating-point scalar type supporting the required numeric interfaces.</typeparam>
public static class HyperDualNumberFunctions<T>
    where T : IFloatingPoint<T>,
              ITrigonometricFunctions<T>,
              IExponentialFunctions<T>,
              ILogarithmicFunctions<T>,
              IHyperbolicFunctions<T>,
              IPowerFunctions<T>,
              IRootFunctions<T>
{
    /// <summary>Sine of a hyper-dual number, including mixed second-order term.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDualNumber<T> Sin(HyperDualNumber<T> x)
    {
        var s = T.Sin(x.Real);
        var c = T.Cos(x.Real);
        return new(s,
            x.Eps1 * c,
            x.Eps2 * c,
            x.Eps12 * c - x.Eps1 * x.Eps2 * s);
    }

    /// <summary>Cosine of a hyper-dual number, including mixed second-order term.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDualNumber<T> Cos(HyperDualNumber<T> x)
    {
        var s = T.Sin(x.Real);
        var c = T.Cos(x.Real);
        return new(c,
            -x.Eps1 * s,
            -x.Eps2 * s,
            -x.Eps12 * s - x.Eps1 * x.Eps2 * c);
    }

    /// <summary>Natural exponential of a hyper-dual number.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDualNumber<T> Exp(HyperDualNumber<T> x)
    {
        var e = T.Exp(x.Real);
        return new(e,
            x.Eps1 * e,
            x.Eps2 * e,
            (x.Eps12 + x.Eps1 * x.Eps2) * e);
    }

    /// <summary>Natural logarithm of a hyper-dual number.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDualNumber<T> Log(HyperDualNumber<T> x)
    {
        var invX = T.One / x.Real;
        var invX2 = invX * invX;
        return new(T.Log(x.Real),
            x.Eps1 * invX,
            x.Eps2 * invX,
            x.Eps12 * invX - x.Eps1 * x.Eps2 * invX2);
    }

    /// <summary>Square root of a hyper-dual number.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDualNumber<T> Sqrt(HyperDualNumber<T> x)
    {
        var s = T.Sqrt(x.Real);
        var two = FloatingPointConstants.Two<T>();
        var inv2s = T.One / (two * s);
        var inv2s3 = T.One / (two * two * x.Real * s); // 1/(4x√x)
        return new(s,
            x.Eps1 * inv2s,
            x.Eps2 * inv2s,
            x.Eps12 * inv2s - x.Eps1 * x.Eps2 * inv2s3);
    }

    /// <summary>Hyperbolic tangent of a hyper-dual number.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDualNumber<T> Tanh(HyperDualNumber<T> x)
    {
        var t = T.Tanh(x.Real);
        var sech2 = T.One - t * t;
        var two = FloatingPointConstants.Two<T>();
        return new(t,
            x.Eps1 * sech2,
            x.Eps2 * sech2,
            x.Eps12 * sech2 - two * t * x.Eps1 * x.Eps2 * sech2);
    }
}
