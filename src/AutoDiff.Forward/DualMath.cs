using System.Numerics;
using System.Runtime.CompilerServices;
using DeepSigma.Mathematics.AutoDiff.Core;

namespace DeepSigma.Mathematics.AutoDiff.Forward;

/// <summary>
/// Elementary functions for DualNumber&lt;T&gt;. Each follows f(a+bε) = f(a) + b·f'(a)ε.
/// </summary>
public static class DualMath<T>
    where T : IFloatingPoint<T>,
              ITrigonometricFunctions<T>,
              IExponentialFunctions<T>,
              ILogarithmicFunctions<T>,
              IHyperbolicFunctions<T>,
              IPowerFunctions<T>,
              IRootFunctions<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Sin(DualNumber<T> x)
        => new(T.Sin(x.Real), x.Dual * T.Cos(x.Real));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Cos(DualNumber<T> x)
        => new(T.Cos(x.Real), -x.Dual * T.Sin(x.Real));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Tan(DualNumber<T> x)
    {
        var c = T.Cos(x.Real);
        return new(T.Tan(x.Real), x.Dual / (c * c));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Exp(DualNumber<T> x)
    {
        var e = T.Exp(x.Real);
        return new(e, x.Dual * e);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Log(DualNumber<T> x)
        => new(T.Log(x.Real), x.Dual / x.Real);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Log(DualNumber<T> x, DualNumber<T> newBase)
    {
        // log_b(x) = log(x)/log(b)
        var logX = T.Log(x.Real);
        var logB = T.Log(newBase.Real);
        var primal = logX / logB;
        var dual = (x.Dual / (x.Real * logB)) - (logX * newBase.Dual / (newBase.Real * logB * logB));
        return new(primal, dual);
    }

    public static DualNumber<T> Pow(DualNumber<T> b, DualNumber<T> e)
    {
        var p = T.Pow(b.Real, e.Real);
        // d/dx[b^e] = b^e * (e·b'/b + e'·ln(b))
        var dual = p * (e.Real * b.Dual / b.Real + e.Dual * T.Log(b.Real));
        return new(p, dual);
    }

    public static DualNumber<T> Pow(DualNumber<T> b, T exponent)
    {
        var p = T.Pow(b.Real, exponent);
        // d/dx[x^n] = n·x^(n-1)
        var dual = exponent * T.Pow(b.Real, exponent - T.One) * b.Dual;
        return new(p, dual);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Sqrt(DualNumber<T> x)
    {
        var s = T.Sqrt(x.Real);
        return new(s, x.Dual / (GenericMath.Two<T>() * s));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Tanh(DualNumber<T> x)
    {
        var t = T.Tanh(x.Real);
        return new(t, x.Dual * (T.One - t * t));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Sinh(DualNumber<T> x)
        => new(T.Sinh(x.Real), x.Dual * T.Cosh(x.Real));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Cosh(DualNumber<T> x)
        => new(T.Cosh(x.Real), x.Dual * T.Sinh(x.Real));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Abs(DualNumber<T> x)
        => new(T.Abs(x.Real), x.Real >= T.Zero ? x.Dual : -x.Dual);
}
