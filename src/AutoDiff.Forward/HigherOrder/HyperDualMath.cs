using System.Numerics;
using System.Runtime.CompilerServices;
using AutoDiff.Core;

namespace AutoDiff.Forward;

/// <summary>Elementary functions for HyperDual&lt;T&gt;.</summary>
public static class HyperDualMath<T>
    where T : IFloatingPoint<T>,
              ITrigonometricFunctions<T>,
              IExponentialFunctions<T>,
              ILogarithmicFunctions<T>,
              IHyperbolicFunctions<T>,
              IPowerFunctions<T>,
              IRootFunctions<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDual<T> Sin(HyperDual<T> x)
    {
        var s = T.Sin(x.Real);
        var c = T.Cos(x.Real);
        return new(s,
            x.Eps1 * c,
            x.Eps2 * c,
            x.Eps12 * c - x.Eps1 * x.Eps2 * s);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDual<T> Cos(HyperDual<T> x)
    {
        var s = T.Sin(x.Real);
        var c = T.Cos(x.Real);
        return new(c,
            -x.Eps1 * s,
            -x.Eps2 * s,
            -x.Eps12 * s - x.Eps1 * x.Eps2 * c);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDual<T> Exp(HyperDual<T> x)
    {
        var e = T.Exp(x.Real);
        return new(e,
            x.Eps1 * e,
            x.Eps2 * e,
            (x.Eps12 + x.Eps1 * x.Eps2) * e);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDual<T> Log(HyperDual<T> x)
    {
        var invX = T.One / x.Real;
        var invX2 = invX * invX;
        return new(T.Log(x.Real),
            x.Eps1 * invX,
            x.Eps2 * invX,
            x.Eps12 * invX - x.Eps1 * x.Eps2 * invX2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDual<T> Sqrt(HyperDual<T> x)
    {
        var s = T.Sqrt(x.Real);
        var two = GenericMath.Two<T>();
        var inv2s = T.One / (two * s);
        var inv2s3 = T.One / (two * two * x.Real * s); // 1/(4x√x)
        return new(s,
            x.Eps1 * inv2s,
            x.Eps2 * inv2s,
            x.Eps12 * inv2s - x.Eps1 * x.Eps2 * inv2s3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDual<T> Tanh(HyperDual<T> x)
    {
        var t = T.Tanh(x.Real);
        var sech2 = T.One - t * t;
        var two = GenericMath.Two<T>();
        return new(t,
            x.Eps1 * sech2,
            x.Eps2 * sech2,
            x.Eps12 * sech2 - two * t * x.Eps1 * x.Eps2 * sech2);
    }
}
