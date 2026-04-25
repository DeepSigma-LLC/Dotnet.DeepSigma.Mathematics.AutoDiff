using System.Text;

namespace DeepSigma.Mathematics.AutoDiff.Core;

public sealed record GradientDiagnostics
{
    public int NodeId { get; init; }
    public string OperationName { get; init; } = string.Empty;
    public double GradientValue { get; init; }
    public bool IsNaN { get; init; }
    public bool IsInfinity { get; init; }
    public IReadOnlyList<GradientDiagnostics> Inputs { get; init; } = [];

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
