using System.Text;

namespace DeepSigma.Mathematics.AutoDiff.Core;

/// <summary>
/// An immutable diagnostic snapshot of a single tape node's gradient state at the
/// time of a backward-sweep failure. Nodes reference their upstream inputs, forming
/// a tree that traces the origin of a pathological gradient back through the
/// computation graph.
/// </summary>
/// <remarks>
/// Diagnostic trees are only built when <c>Tape&lt;T&gt;.EnableDiagnostics</c> is
/// <see langword="true"/>. Building the tree has a small but non-zero cost, so it
/// should be left disabled in production hot paths.
/// </remarks>
public sealed record GradientDiagnostics
{
    /// <summary>The zero-based index of this node in the computation tape.</summary>
    public int NodeId { get; init; }

    /// <summary>
    /// The name of the operation recorded at this node
    /// (e.g. <c>"Sin"</c>, <c>"Mul"</c>, <c>"Variable"</c>).
    /// </summary>
    public string OperationName { get; init; } = string.Empty;

    /// <summary>
    /// The gradient value accumulated at this node at the time the diagnostic
    /// snapshot was taken.
    /// </summary>
    public double GradientValue { get; init; }

    /// <summary>
    /// Indicates whether <see cref="GradientValue"/> is NaN.
    /// </summary>
    public bool IsNaN { get; init; }

    /// <summary>
    /// Indicates whether <see cref="GradientValue"/> is positive or negative
    /// infinity.
    /// </summary>
    public bool IsInfinity { get; init; }

    /// <summary>
    /// The diagnostic snapshots for the upstream nodes that feed into this node.
    /// Empty for leaf nodes (input variables that have no recorded inputs).
    /// </summary>
    public IReadOnlyList<GradientDiagnostics> Inputs { get; init; } = [];

    /// <summary>
    /// Returns a human-readable, indented representation of this node and all its
    /// upstream inputs, suitable for logging or debug output.
    /// </summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        AppendTo(sb, 0);
        return sb.ToString();
    }

    private void AppendTo(StringBuilder sb, int depth)
    {
        sb.Append(' ', depth * 2);
        sb.Append($"Node {NodeId} ({OperationName}): grad={GradientValue:G6}");
        if (IsNaN) sb.Append(" [NaN]");
        if (IsInfinity) sb.Append(" [Inf]");
        sb.AppendLine();
        foreach (var input in Inputs)
            input.AppendTo(sb, depth + 1);
    }
}
