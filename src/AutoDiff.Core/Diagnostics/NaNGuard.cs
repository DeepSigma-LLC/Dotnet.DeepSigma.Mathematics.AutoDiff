using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace DeepSigma.Mathematics.AutoDiff.Core;

public static class NaNGuard
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Check<T>(T value, string context) where T : IFloatingPoint<T>
    {
        if (T.IsNaN(value) || T.IsInfinity(value))
            ThrowPathological(value, context, -1, null);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T CheckGradient<T>(T value, int nodeId, string opName,
        GradientDiagnostics? diagnostics = null) where T : IFloatingPoint<T>
    {
        if (T.IsNaN(value) || T.IsInfinity(value))
            ThrowPathological(value, opName, nodeId, diagnostics);
        return value;
    }

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

public sealed class GradientNaNException : Exception
{
    public string OperationContext { get; }
    public int NodeId { get; }
    public double GradientValue { get; }
    public GradientDiagnostics? DiagnosticTree { get; }

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
