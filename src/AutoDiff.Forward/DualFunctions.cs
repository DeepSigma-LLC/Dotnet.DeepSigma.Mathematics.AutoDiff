using System.Numerics;
using System.Runtime.CompilerServices;
using DeepSigma.Mathematics.AutoDiff.Core;

namespace DeepSigma.Mathematics.AutoDiff.Forward;

/// <summary>
/// Elementary differentiable functions for <see cref="DualNumber{T}"/>.
/// Each applies the rule f(a + bε) = f(a) + b·f′(a)·ε, propagating the derivative
/// through the dual component automatically.
/// </summary>
/// <typeparam name="T">A floating-point scalar type supporting the required numeric interfaces.</typeparam>
public static class DualFunctions<T>
    where T : IFloatingPoint<T>,
              ITrigonometricFunctions<T>,
              IExponentialFunctions<T>,
              ILogarithmicFunctions<T>,
              IHyperbolicFunctions<T>,
              IPowerFunctions<T>,
              IRootFunctions<T>
{
    /// <summary>Sine: sin(a + bε) = sin(a) + b·cos(a)·ε</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Sin(DualNumber<T> x)
        => new(T.Sin(x.Real), x.Dual * T.Cos(x.Real));

    /// <summary>Cosine: cos(a + bε) = cos(a) − b·sin(a)·ε</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Cos(DualNumber<T> x)
        => new(T.Cos(x.Real), -x.Dual * T.Sin(x.Real));

    /// <summary>Tangent: tan(a + bε) = tan(a) + b/cos²(a)·ε</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Tan(DualNumber<T> x)
    {
        var c = T.Cos(x.Real);
        return new(T.Tan(x.Real), x.Dual / (c * c));
    }

    /// <summary>Natural exponential: exp(a + bε) = exp(a) + b·exp(a)·ε</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Exp(DualNumber<T> x)
    {
        var e = T.Exp(x.Real);
        return new(e, x.Dual * e);
    }

    /// <summary>Natural logarithm: log(a + bε) = log(a) + b/a·ε</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Log(DualNumber<T> x)
        => new(T.Log(x.Real), x.Dual / x.Real);

    /// <summary>
    /// Logarithm with an arbitrary dual-number base.
    /// Applies the change-of-base identity log_b(x) = log(x)/log(b) and differentiates both.
    /// </summary>
    /// <param name="x">The dual-number argument.</param>
    /// <param name="newBase">The dual-number base.</param>
    public static DualNumber<T> Log(DualNumber<T> x, DualNumber<T> newBase)
    {
        var logX = T.Log(x.Real);
        var logB = T.Log(newBase.Real);
        var primal = logX / logB;
        var dual = (x.Dual / (x.Real * logB)) - (logX * newBase.Dual / (newBase.Real * logB * logB));
        return new(primal, dual);
    }

    /// <summary>
    /// General power with dual-number base and exponent: d/dx[b^e] = b^e·(e·b′/b + e′·ln b)·ε
    /// </summary>
    /// <param name="b">Base as a dual number.</param>
    /// <param name="e">Exponent as a dual number.</param>
    public static DualNumber<T> Pow(DualNumber<T> b, DualNumber<T> e)
    {
        var p = T.Pow(b.Real, e.Real);
        var dual = p * (e.Real * b.Dual / b.Real + e.Dual * T.Log(b.Real));
        return new(p, dual);
    }

    /// <summary>
    /// Power with a constant scalar exponent: d/dx[x^n] = n·x^(n−1)·x′·ε
    /// </summary>
    /// <param name="b">Base as a dual number.</param>
    /// <param name="exponent">Constant scalar exponent.</param>
    public static DualNumber<T> Pow(DualNumber<T> b, T exponent)
    {
        var p = T.Pow(b.Real, exponent);
        var dual = exponent * T.Pow(b.Real, exponent - T.One) * b.Dual;
        return new(p, dual);
    }

    /// <summary>Square root: √(a + bε) = √a + b/(2√a)·ε</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Sqrt(DualNumber<T> x)
    {
        var s = T.Sqrt(x.Real);
        return new(s, x.Dual / (FloatingPointConstants.Two<T>() * s));
    }

    /// <summary>Hyperbolic tangent: tanh(a + bε) = tanh(a) + b·sech²(a)·ε</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Tanh(DualNumber<T> x)
    {
        var t = T.Tanh(x.Real);
        return new(t, x.Dual * (T.One - t * t));
    }

    /// <summary>Hyperbolic sine: sinh(a + bε) = sinh(a) + b·cosh(a)·ε</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Sinh(DualNumber<T> x)
        => new(T.Sinh(x.Real), x.Dual * T.Cosh(x.Real));

    /// <summary>Hyperbolic cosine: cosh(a + bε) = cosh(a) + b·sinh(a)·ε</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Cosh(DualNumber<T> x)
        => new(T.Cosh(x.Real), x.Dual * T.Sinh(x.Real));

    /// <summary>
    /// Absolute value. The subgradient is used at zero (treated as positive).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Abs(DualNumber<T> x)
        => new(T.Abs(x.Real), x.Real >= T.Zero ? x.Dual : -x.Dual);
}
