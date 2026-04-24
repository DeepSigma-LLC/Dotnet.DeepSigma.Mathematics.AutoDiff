using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AutoDiff.Forward;

/// <summary>
/// Hyper-dual number a + b·ε₁ + c·ε₂ + d·ε₁ε₂ where ε₁²=ε₂²=0.
/// Enables computing mixed second-order partial derivatives in a single pass.
/// For the diagonal Hessian of f(x), seed x with HyperDual(x, 1, 1, 0).
/// The ε₁ε₂ component of f(x) gives f''(x).
/// </summary>
[DebuggerDisplay("{Real} + {Eps1}ε₁ + {Eps2}ε₂ + {Eps12}ε₁ε₂")]
public readonly struct HyperDual<T>
    : IAdditionOperators<HyperDual<T>, HyperDual<T>, HyperDual<T>>,
      ISubtractionOperators<HyperDual<T>, HyperDual<T>, HyperDual<T>>,
      IMultiplyOperators<HyperDual<T>, HyperDual<T>, HyperDual<T>>,
      IDivisionOperators<HyperDual<T>, HyperDual<T>, HyperDual<T>>,
      IUnaryNegationOperators<HyperDual<T>, HyperDual<T>>
    where T : IFloatingPoint<T>
{
    public T Real { get; }
    public T Eps1 { get; }
    public T Eps2 { get; }
    public T Eps12 { get; }

    public HyperDual(T real, T eps1, T eps2, T eps12)
    {
        Real = real;
        Eps1 = eps1;
        Eps2 = eps2;
        Eps12 = eps12;
    }

    public static HyperDual<T> Const(T value) => new(value, T.Zero, T.Zero, T.Zero);

    /// <summary>
    /// Seed for computing both first and second derivatives.
    /// Eps12 of the result equals f''(x).
    /// </summary>
    public static HyperDual<T> Variable(T value) => new(value, T.One, T.One, T.Zero);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDual<T> operator +(HyperDual<T> a, HyperDual<T> b)
        => new(a.Real + b.Real, a.Eps1 + b.Eps1, a.Eps2 + b.Eps2, a.Eps12 + b.Eps12);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDual<T> operator -(HyperDual<T> a, HyperDual<T> b)
        => new(a.Real - b.Real, a.Eps1 - b.Eps1, a.Eps2 - b.Eps2, a.Eps12 - b.Eps12);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDual<T> operator *(HyperDual<T> a, HyperDual<T> b)
        => new(
            a.Real * b.Real,
            a.Real * b.Eps1 + a.Eps1 * b.Real,
            a.Real * b.Eps2 + a.Eps2 * b.Real,
            a.Real * b.Eps12 + a.Eps1 * b.Eps2 + a.Eps2 * b.Eps1 + a.Eps12 * b.Real);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDual<T> operator /(HyperDual<T> a, HyperDual<T> b)
    {
        var r = a.Real / b.Real;
        var invB = T.One / b.Real;
        var invB2 = invB * invB;
        var e1 = (a.Eps1 - r * b.Eps1) * invB;
        var e2 = (a.Eps2 - r * b.Eps2) * invB;
        var e12 = (a.Eps12 - e1 * b.Eps2 - e2 * b.Eps1 - r * b.Eps12) * invB;
        return new(r, e1, e2, e12);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDual<T> operator -(HyperDual<T> a)
        => new(-a.Real, -a.Eps1, -a.Eps2, -a.Eps12);

    public override string ToString() => $"{Real} + {Eps1}ε₁ + {Eps2}ε₂ + {Eps12}ε₁ε₂";
}
