using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DeepSigma.Mathematics.AutoDiff.Core;

/// <summary>
/// Provides inline checks that detect NaN or infinite values during forward
/// evaluation and the backward gradient sweep, throwing
/// <see cref="GradientNaNException"/> with diagnostic context on failure.
/// All methods are aggressively inlined so they disappear entirely from the
/// hot path when values are well-behaved.
/// </summary>
public static class NaNGuard
{
    /// <summary>
    /// Checks <paramref name="value"/> for NaN or infinity and throws
    /// <see cref="GradientNaNException"/> if either is detected. Intended for
    /// forward-pass validation where a tape node identity is not yet known.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="context">
    /// A human-readable description of the operation being validated
    /// (e.g. <c>"Log"</c>, <c>"Sqrt"</c>), included in the exception message.
    /// </param>
    /// <returns>
    /// <paramref name="value"/> unchanged when it is finite and not NaN.
    /// </returns>
    /// <exception cref="GradientNaNException">
    /// Thrown when <paramref name="value"/> is NaN or infinite.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Check<T>(T value, string context) where T : IFloatingPoint<T>
    {
        if (T.IsNaN(value) || T.IsInfinity(value))
            ThrowPathological(value, context, -1, null);
        return value;
    }

    /// <summary>
    /// Checks a gradient <paramref name="value"/> for NaN or infinity during the
    /// backward sweep and throws <see cref="GradientNaNException"/> enriched with
    /// the tape node's identity and an optional diagnostic tree if either is detected.
    /// </summary>
    /// <param name="value">The gradient value to validate.</param>
    /// <param name="nodeId">
    /// The zero-based index of the tape node whose gradient is being checked.
    /// </param>
    /// <param name="opName">
    /// The name of the operation at this node (e.g. <c>"Sin"</c>, <c>"Mul"</c>).
    /// </param>
    /// <param name="diagnostics">
    /// An optional pre-built <see cref="GradientDiagnostics"/> subtree for the node,
    /// attached to the thrown exception when provided.
    /// </param>
    /// <returns>
    /// <paramref name="value"/> unchanged when it is finite and not NaN.
    /// </returns>
    /// <exception cref="GradientNaNException">
    /// Thrown when <paramref name="value"/> is NaN or infinite.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CheckGradient<T>(T value, int nodeId, string opName,
        GradientDiagnostics? diagnostics = null) where T : IFloatingPoint<T>
    {
        if (T.IsNaN(value) || T.IsInfinity(value))
            ThrowPathological(value, opName, nodeId, diagnostics);
        return value;
    }

    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="value"/> is NaN or
    /// infinite; otherwise <see langword="false"/>.
    /// </summary>
    /// <param name="value">The value to test.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPathological<T>(T value) where T : IFloatingPoint<T>
        => T.IsNaN(value) || T.IsInfinity(value);

    [DoesNotReturn]
    private static void ThrowPathological<T>(T value, string context, int nodeId,
        GradientDiagnostics? diagnostics) where T : IFloatingPoint<T>
    {
        var dbl = double.CreateChecked(value);
        throw new GradientNaNException(context, nodeId, dbl, diagnostics);
    }
}

/// <summary>
/// Thrown when a NaN or infinite gradient is detected during the backward sweep.
/// Carries the tape node identity, the pathological value, and an optional
/// diagnostic subtree for root-cause analysis.
/// </summary>
public sealed class GradientNaNException : Exception
{
    /// <summary>
    /// The name of the operation (e.g. <c>"Log"</c>, <c>"Sqrt"</c>) in whose
    /// backward kernel the pathological gradient was produced.
    /// </summary>
    public string OperationContext { get; }

    /// <summary>
    /// The zero-based index of the tape node where the pathological gradient was
    /// first detected, or <c>-1</c> when the failure occurred outside a tape sweep.
    /// </summary>
    public int NodeId { get; }

    /// <summary>
    /// The raw gradient value captured as <see cref="double"/> for diagnostic
    /// display, regardless of the computation's element type <c>T</c>.
    /// </summary>
    public double GradientValue { get; }

    /// <summary>
    /// An optional subtree of <see cref="GradientDiagnostics"/> nodes tracing the
    /// origin of the pathological gradient back through the computation graph, or
    /// <see langword="null"/> if diagnostics were not enabled on the tape.
    /// </summary>
    public GradientDiagnostics? DiagnosticTree { get; }

    /// <summary>
    /// Initialises a new <see cref="GradientNaNException"/> with full context.
    /// </summary>
    /// <param name="context">The operation name where the failure occurred.</param>
    /// <param name="nodeId">The tape node index, or <c>-1</c> if not applicable.</param>
    /// <param name="value">The pathological gradient value as <see cref="double"/>.</param>
    /// <param name="diagnostics">Optional diagnostic subtree, or <see langword="null"/>.</param>
    public GradientNaNException(string context, int nodeId, double value,
        GradientDiagnostics? diagnostics)
        : base($"Pathological gradient ({value}) at node {nodeId} in '{context}'")
    {
        OperationContext = context;
        NodeId = nodeId;
        GradientValue = value;
        DiagnosticTree = diagnostics;
    }
}
