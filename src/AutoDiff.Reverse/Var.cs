using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DeepSigma.Mathematics.AutoDiff.Reverse;

/// <summary>
/// A variable on a tape. Immutable handle pairing a Tape with a node ID.
/// Operator overloads record operations onto the tape.
/// </summary>
[DebuggerDisplay("Value={Value}, Gradient={Gradient}")]
public readonly struct Var<T>
    where T : IFloatingPoint<T>
{
    internal readonly Tape<T> Tape;
    internal readonly int NodeId;

    internal Var(Tape<T> tape, int nodeId)
    {
        Tape = tape;
        NodeId = nodeId;
    }

    public T Value => Tape.GetPrimal(NodeId);
    public T Gradient => Tape.GetGradient(NodeId);

    // ── Arithmetic operators ─────────────────────────────────────────────────

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator +(Var<T> a, Var<T> b) => a.Tape.RecordAdd(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator -(Var<T> a, Var<T> b) => a.Tape.RecordSub(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator *(Var<T> a, Var<T> b) => a.Tape.RecordMul(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator /(Var<T> a, Var<T> b) => a.Tape.RecordDiv(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator -(Var<T> a) => a.Tape.RecordNeg(a);

    // Scalar operators — record directly without a constant leaf node for the scalar.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator +(Var<T> a, T b) => a.Tape.RecordAddScalar(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator +(T a, Var<T> b) => b.Tape.RecordAddScalar(b, a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator -(Var<T> a, T b) => a.Tape.RecordSubScalar(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator -(T a, Var<T> b) => b.Tape.RecordSubScalarLeft(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator *(Var<T> a, T b) => a.Tape.RecordMulScalar(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator *(T a, Var<T> b) => b.Tape.RecordMulScalar(b, a);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator /(Var<T> a, T b) => a.Tape.RecordDivScalar(a, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator /(T a, Var<T> b) => b.Tape.RecordDivScalarLeft(a, b);

    public override string ToString() => $"Var(value={Value}, grad={Gradient})";


}
