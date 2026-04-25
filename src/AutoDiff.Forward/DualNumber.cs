using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DeepSigma.Mathematics.AutoDiff.Forward;

/// <summary>
/// An immutable dual number a + bε where ε²=0.
/// The real component <see cref="Real"/> carries the primal value and the dual component
/// <see cref="Dual"/> carries the directional derivative. Arithmetic operators propagate
/// both components according to standard dual-number algebra so that evaluating any
/// differentiable function f on a seeded <see cref="DualNumber{T}"/> automatically
/// computes f′ in a single forward pass.
/// </summary>
/// <typeparam name="T">A floating-point scalar type that implements <see cref="IFloatingPoint{T}"/>.</typeparam>
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
    /// <summary>The primal (real) part of the dual number.</summary>
    public T Real { get; }

    /// <summary>The infinitesimal (dual) part; equals f′(x) after evaluation.</summary>
    public T Dual { get; }

    /// <summary>Initializes a dual number with explicit real and dual components.</summary>
    /// <param name="real">The primal value.</param>
    /// <param name="dual">The derivative seed or accumulated derivative.</param>
    public DualNumber(T real, T dual)
    {
        Real = real;
        Dual = dual;
    }

    /// <summary>
    /// Creates a constant dual number with <see cref="Dual"/> = 0.
    /// Constants do not contribute to the derivative.
    /// </summary>
    /// <param name="value">The constant primal value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Const(T value) => new(value, T.Zero);

    /// <summary>
    /// Creates a variable dual number seeded with <see cref="Dual"/> = 1,
    /// indicating that derivative computation is with respect to this input.
    /// </summary>
    /// <param name="value">The primal value of the variable.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Variable(T value) => new(value, T.One);

    /// <summary>
    /// Creates a variable dual number with an explicit derivative seed.
    /// Use this to compute directional derivatives along a chosen vector component.
    /// </summary>
    /// <param name="value">The primal value of the variable.</param>
    /// <param name="seed">The directional derivative seed for this variable.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> Variable(T value, T seed) => new(value, seed);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator +(DualNumber<T> a, DualNumber<T> b)
        => new(a.Real + b.Real, a.Dual + b.Dual);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator -(DualNumber<T> a, DualNumber<T> b)
        => new(a.Real - b.Real, a.Dual - b.Dual);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator *(DualNumber<T> a, DualNumber<T> b)
        => new(a.Real * b.Real, a.Real * b.Dual + a.Dual * b.Real);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator /(DualNumber<T> a, DualNumber<T> b)
        => new(a.Real / b.Real,
               (a.Dual * b.Real - a.Real * b.Dual) / (b.Real * b.Real));

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator -(DualNumber<T> a)
        => new(-a.Real, -a.Dual);

    /// <summary>Adds a scalar constant to the real part; dual part is unchanged.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator +(DualNumber<T> a, T b)
        => new(a.Real + b, a.Dual);

    /// <summary>Adds a scalar constant to the real part; dual part is unchanged.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator +(T a, DualNumber<T> b)
        => new(a + b.Real, b.Dual);

    /// <summary>Subtracts a scalar constant from the real part; dual part is unchanged.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator -(DualNumber<T> a, T b)
        => new(a.Real - b, a.Dual);

    /// <summary>Scales both components by a scalar constant.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator *(DualNumber<T> a, T b)
        => new(a.Real * b, a.Dual * b);

    /// <summary>Scales both components by a scalar constant.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator *(T a, DualNumber<T> b)
        => new(a * b.Real, a * b.Dual);

    /// <summary>Divides both components by a scalar constant.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DualNumber<T> operator /(DualNumber<T> a, T b)
        => new(a.Real / b, a.Dual / b);

    /// <inheritdoc/>
    public bool Equals(DualNumber<T> other) => Real == other.Real && Dual == other.Dual;
    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is DualNumber<T> d && Equals(d);
    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Real, Dual);
    /// <inheritdoc/>
    public override string ToString() => $"{Real} + {Dual}ε";

    /// <inheritdoc/>
    public static bool operator ==(DualNumber<T> left, DualNumber<T> right) => left.Equals(right);
    /// <inheritdoc/>
    public static bool operator !=(DualNumber<T> left, DualNumber<T> right) => !left.Equals(right);
}
