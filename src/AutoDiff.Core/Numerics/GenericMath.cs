using System.Numerics;
using System.Runtime.CompilerServices;

namespace AutoDiff.Core;

/// <summary>
/// Helpers for generic numeric constants that can't be expressed as literals.
/// Math functions (Sin, Cos, Exp, …) are accessed directly via T.Sin etc. — no wrappers needed.
/// </summary>
public static class GenericMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Zero<T>() where T : IFloatingPoint<T> => T.Zero;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T One<T>() where T : IFloatingPoint<T> => T.One;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Two<T>() where T : IFloatingPoint<T> => T.One + T.One;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NegOne<T>() where T : IFloatingPoint<T> => -T.One;
}
