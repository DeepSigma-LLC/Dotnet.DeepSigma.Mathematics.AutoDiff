using System.Numerics;
using System.Runtime.CompilerServices;

namespace DeepSigma.Mathematics.AutoDiff.Core;

/// <summary>
/// Provides commonly-needed numeric constants as inline static methods generic over
/// <see cref="IFloatingPoint{T}"/>, supplementing the constants available directly on
/// <c>T</c> itself. Math functions (Sin, Cos, Exp, …) are accessed directly via
/// <c>T.Sin</c>, <c>T.Cos</c>, etc. — no wrappers are needed for those.
/// </summary>
public static class FloatingPointConstants
{
    /// <summary>Returns the additive identity (0) for <typeparamref name="T"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Zero<T>() where T : IFloatingPoint<T> => T.Zero;

    /// <summary>Returns the multiplicative identity (1) for <typeparamref name="T"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T One<T>() where T : IFloatingPoint<T> => T.One;

    /// <summary>Returns the value 2 for <typeparamref name="T"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Two<T>() where T : IFloatingPoint<T> => T.One + T.One;

    /// <summary>Returns negative one (−1) for <typeparamref name="T"/>.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NegOne<T>() where T : IFloatingPoint<T> => -T.One;
}
