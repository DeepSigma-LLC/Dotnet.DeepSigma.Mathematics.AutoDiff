using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DeepSigma.Mathematics.AutoDiff.Reverse;

/// <summary>
/// An immutable handle to a node on a <see cref="ComputationTape{T}"/>.
/// Pairs a tape reference with a node index so that arithmetic operators can record
/// operations onto the tape during the forward pass.
/// </summary>
/// <remarks>
/// After calling <see cref="ComputationTape{T}.Backward(Var{T})"/>, the
/// <see cref="Gradient"/> property returns ∂output/∂this variable.
/// </remarks>
/// <typeparam name="T">A floating-point scalar type that implements <see cref="IFloatingPoint{T}"/>.</typeparam>
[DebuggerDisplay("Value={Value}, Gradient={Gradient}")]
public readonly struct Var<T>
    where T : IFloatingPoint<T>
{
    internal readonly ComputationTape<T> Tape;
    internal readonly int NodeId;

    internal Var(ComputationTape<T> tape, int nodeId)
    {
        Tape = tape;
        NodeId = nodeId;
    }

    /// <summary>The primal (forward-pass) value of this variable.</summary>
    public T Value => Tape.GetPrimal(NodeId);

    /// <summary>
    /// The accumulated gradient ∂output/∂this, populated after a call to
    /// <see cref="ComputationTape{T}.Backward(Var{T})"/>.
    /// </summary>
    public T Gradient => Tape.GetGradient(NodeId);

    // ── Arithmetic operators ─────────────────────────────────────────────────

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator +(Var<T> a, Var<T> b) => a.Tape.RecordAdd(a, b);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator -(Var<T> a, Var<T> b) => a.Tape.RecordSub(a, b);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator *(Var<T> a, Var<T> b) => a.Tape.RecordMul(a, b);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator /(Var<T> a, Var<T> b) => a.Tape.RecordDiv(a, b);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator -(Var<T> a) => a.Tape.RecordNeg(a);

    /// <summary>Adds a scalar constant to this variable without creating a constant leaf node.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator +(Var<T> a, T b) => a.Tape.RecordAddScalar(a, b);

    /// <summary>Adds a scalar constant to this variable without creating a constant leaf node.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator +(T a, Var<T> b) => b.Tape.RecordAddScalar(b, a);

    /// <summary>Subtracts a scalar constant from this variable without creating a constant leaf node.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator -(Var<T> a, T b) => a.Tape.RecordSubScalar(a, b);

    /// <summary>Subtracts this variable from a scalar constant without creating a constant leaf node.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator -(T a, Var<T> b) => b.Tape.RecordSubScalarLeft(a, b);

    /// <summary>Scales this variable by a scalar constant without creating a constant leaf node.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator *(Var<T> a, T b) => a.Tape.RecordMulScalar(a, b);

    /// <summary>Scales this variable by a scalar constant without creating a constant leaf node.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator *(T a, Var<T> b) => b.Tape.RecordMulScalar(b, a);

    /// <summary>Divides this variable by a scalar constant without creating a constant leaf node.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator /(Var<T> a, T b) => a.Tape.RecordDivScalar(a, b);

    /// <summary>Divides a scalar constant by this variable without creating a constant leaf node.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> operator /(T a, Var<T> b) => b.Tape.RecordDivScalarLeft(a, b);

    /// <inheritdoc/>
    public override string ToString() => $"Var(value={Value}, grad={Gradient})";
}
