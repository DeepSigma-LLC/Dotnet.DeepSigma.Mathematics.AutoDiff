using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DeepSigma.Mathematics.AutoDiff.Forward;

/// <summary>
/// Dual number a + bε where ε²=0. The dual part b carries the directional derivative.
/// </summary>
[DebuggerDisplay("{Real} + {Dual}ε")]
public readonly struct DualNumber<T>
    : IAdditionOperators<DualNumber<T>, DualNumber<T>, DualNumber<T>>,
      ISubtractionOperators<DualNumber<T>, DualNumber<T>, DualNumber<T>>,
      IMultiplyOperators<DualNumber<T>, DualNumber<T>, DualNumber<T>>,
      IDivisionOperators<DualNumber<T>, DualNumber<T>, DualNumber<T>>,
      IUnaryNegationOperators<DualNumber<T>, DualNumber<T>>,
      IEquatable<DualNumber<T>>
    where T : IFloatingPoint<T>
{
    public T Real { get; }
    public T Dual { get; }

    public DualNumber(T real, T dual)
    {
        Real = real;
        Dual = dual;
    }

    /// <summary>Lift a constant — dual part is zero.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Const(T value) => new(value, T.Zero);

    /// <summary>Lift a variable — dual part is one (seeds the derivative direction).</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Variable(T value) => new(value, T.One);

    /// <summary>Lift a variable with an explicit seed for directional derivative.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Variable(T value, T seed) => new(value, seed);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator +(DualNumber<T> a, DualNumber<T> b)
        => new(a.Real + b.Real, a.Dual + b.Dual);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator -(DualNumber<T> a, DualNumber<T> b)
        => new(a.Real - b.Real, a.Dual - b.Dual);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator *(DualNumber<T> a, DualNumber<T> b)
        => new(a.Real * b.Real, a.Real * b.Dual + a.Dual * b.Real);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator /(DualNumber<T> a, DualNumber<T> b)
        => new(a.Real / b.Real,
               (a.Dual * b.Real - a.Real * b.Dual) / (b.Real * b.Real));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator -(DualNumber<T> a)
        => new(-a.Real, -a.Dual);

    // Scalar convenience operators
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator +(DualNumber<T> a, T b)
        => new(a.Real + b, a.Dual);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator +(T a, DualNumber<T> b)
        => new(a + b.Real, b.Dual);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator -(DualNumber<T> a, T b)
        => new(a.Real - b, a.Dual);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator *(DualNumber<T> a, T b)
        => new(a.Real * b, a.Dual * b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator *(T a, DualNumber<T> b)
        => new(a * b.Real, a * b.Dual);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator /(DualNumber<T> a, T b)
        => new(a.Real / b, a.Dual / b);

    public bool Equals(DualNumber<T> other) => Real == other.Real && Dual == other.Dual;
    public override bool Equals(object? obj) => obj is DualNumber<T> d && Equals(d);
    public override int GetHashCode() => HashCode.Combine(Real, Dual);
    public override string ToString() => $"{Real} + {Dual}ε";

    public static bool operator ==(DualNumber<T> left, DualNumber<T> right) => left.Equals(right);
    public static bool operator !=(DualNumber<T> left, DualNumber<T> right) => !left.Equals(right);
}
