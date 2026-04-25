using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DeepSigma.Mathematics.AutoDiff.Forward;

/// <summary>
/// An immutable hyper-dual number a + b·ε₁ + c·ε₂ + d·ε₁ε₂ where ε₁² = ε₂² = 0.
/// The four components propagate both first-order and mixed second-order partial derivatives
/// through a single evaluation pass.
/// </summary>
/// <remarks>
/// To compute f″(x) (second derivative of a univariate function), seed x as
/// <c>HyperDual(x, 1, 1, 0)</c>; after evaluation the <see cref="Eps12"/> component equals f″(x).
/// To compute ∂²f/∂x∂y, seed x as <c>HyperDual(x, 1, 0, 0)</c> and y as
/// <c>HyperDual(y, 0, 1, 0)</c>; the <see cref="Eps12"/> component of the result gives the mixed partial.
/// </remarks>
/// <typeparam name="T">A floating-point scalar type that implements <see cref="IFloatingPoint{T}"/>.</typeparam>
[DebuggerDisplay("{Real} + {Eps1}ε₁ + {Eps2}ε₂ + {Eps12}ε₁ε₂")]
public readonly struct HyperDualNumber<T>
    : IAdditionOperators<HyperDualNumber<T>, HyperDualNumber<T>, HyperDualNumber<T>>,
      ISubtractionOperators<HyperDualNumber<T>, HyperDualNumber<T>, HyperDualNumber<T>>,
      IMultiplyOperators<HyperDualNumber<T>, HyperDualNumber<T>, HyperDualNumber<T>>,
      IDivisionOperators<HyperDualNumber<T>, HyperDualNumber<T>, HyperDualNumber<T>>,
      IUnaryNegationOperators<HyperDualNumber<T>, HyperDualNumber<T>>
    where T : IFloatingPoint<T>
{
    /// <summary>The primal (real) value.</summary>
    public T Real { get; }

    /// <summary>The first infinitesimal component; carries ∂f/∂x when ε₁ seeds x.</summary>
    public T Eps1 { get; }

    /// <summary>The second infinitesimal component; carries ∂f/∂y when ε₂ seeds y.</summary>
    public T Eps2 { get; }

    /// <summary>The mixed infinitesimal component; carries ∂²f/∂x∂y after evaluation.</summary>
    public T Eps12 { get; }

    /// <summary>Initializes all four hyper-dual components explicitly.</summary>
    public HyperDualNumber(T real, T eps1, T eps2, T eps12)
    {
        Real = real;
        Eps1 = eps1;
        Eps2 = eps2;
        Eps12 = eps12;
    }

    /// <summary>Creates a constant hyper-dual number with all infinitesimal components set to zero.</summary>
    /// <param name="value">The constant primal value.</param>
    public static HyperDualNumber<T> Const(T value) => new(value, T.Zero, T.Zero, T.Zero);

    /// <summary>
    /// Creates a variable seeded to compute both first and second univariate derivatives.
    /// Seeds ε₁ = ε₂ = 1, ε₁ε₂ = 0; after evaluation <see cref="Eps12"/> equals f″(x).
    /// </summary>
    /// <param name="value">The primal value of the variable.</param>
    public static HyperDualNumber<T> Variable(T value) => new(value, T.One, T.One, T.Zero);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDualNumber<T> operator +(HyperDualNumber<T> a, HyperDualNumber<T> b)
        => new(a.Real + b.Real, a.Eps1 + b.Eps1, a.Eps2 + b.Eps2, a.Eps12 + b.Eps12);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDualNumber<T> operator -(HyperDualNumber<T> a, HyperDualNumber<T> b)
        => new(a.Real - b.Real, a.Eps1 - b.Eps1, a.Eps2 - b.Eps2, a.Eps12 - b.Eps12);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDualNumber<T> operator *(HyperDualNumber<T> a, HyperDualNumber<T> b)
        => new(
            a.Real * b.Real,
            a.Real * b.Eps1 + a.Eps1 * b.Real,
            a.Real * b.Eps2 + a.Eps2 * b.Real,
            a.Real * b.Eps12 + a.Eps1 * b.Eps2 + a.Eps2 * b.Eps1 + a.Eps12 * b.Real);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDualNumber<T> operator /(HyperDualNumber<T> a, HyperDualNumber<T> b)
    {
        var r = a.Real / b.Real;
        var invB = T.One / b.Real;
        var e1 = (a.Eps1 - r * b.Eps1) * invB;
        var e2 = (a.Eps2 - r * b.Eps2) * invB;
        var e12 = (a.Eps12 - e1 * b.Eps2 - e2 * b.Eps1 - r * b.Eps12) * invB;
        return new(r, e1, e2, e12);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HyperDualNumber<T> operator -(HyperDualNumber<T> a)
        => new(-a.Real, -a.Eps1, -a.Eps2, -a.Eps12);

    /// <inheritdoc/>
    public override string ToString() => $"{Real} + {Eps1}ε₁ + {Eps2}ε₂ + {Eps12}ε₁ε₂";
}
