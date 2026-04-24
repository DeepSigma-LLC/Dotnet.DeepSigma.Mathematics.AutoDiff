using System.Numerics;
using System.Runtime.CompilerServices;
using AutoDiff.Core;

namespace AutoDiff.Reverse;

/// <summary>
/// Records a forward computation and runs the reverse (backward) pass to accumulate gradients.
/// Nodes are stored in a flat array for cache-coherent backward sweep.
/// </summary>
public sealed class Tape<T> : ITape<T>, IDisposable
    where T : IFloatingPoint<T>
{
    private const int DefaultDiagnosticDepth = 8;

    private TapeNode<T>[] _nodes;
    private int _count;
    private bool _disposed;

    public bool EnableNaNGuard { get; set; }
    public bool EnableDiagnostics { get; set; }

    public int NodeCount => _count;

    public Tape(int initialCapacity = 256)
    {
        _nodes = new TapeNode<T>[initialCapacity];
    }

    // ── Variable registration ────────────────────────────────────────────────

    public Var<T> Variable(T value, string? name = null)
    {
        var id = Append(TapeNode<T>.Leaf(_count, value, name));
        return new Var<T>(this, id);
    }

    // ── Recording ops (called by Var<T> operators and ReverseMath) ───────────

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Var<T> RecordUnary(int inputId, T primal, T weight)
    {
        var id = Append(TapeNode<T>.Unary(_count, inputId, primal, weight));
        return new Var<T>(this, id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Var<T> RecordBinary(int in0, int in1, T primal, T w0, T w1)
    {
        var id = Append(TapeNode<T>.Binary(_count, in0, in1, primal, w0, w1));
        return new Var<T>(this, id);
    }

    // Specific ops (called by Var<T> operator overloads)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Var<T> RecordAdd(Var<T> a, Var<T> b)
        => RecordBinary(a.NodeId, b.NodeId, a.Value + b.Value, T.One, T.One);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Var<T> RecordSub(Var<T> a, Var<T> b)
        => RecordBinary(a.NodeId, b.NodeId, a.Value - b.Value, T.One, -T.One);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Var<T> RecordMul(Var<T> a, Var<T> b)
        => RecordBinary(a.NodeId, b.NodeId, a.Value * b.Value, b.Value, a.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Var<T> RecordDiv(Var<T> a, Var<T> b)
    {
        var primal = a.Value / b.Value;
        var w0 = T.One / b.Value;
        var w1 = -a.Value / (b.Value * b.Value);
        return RecordBinary(a.NodeId, b.NodeId, primal, w0, w1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Var<T> RecordNeg(Var<T> a)
        => RecordUnary(a.NodeId, -a.Value, -T.One);

    // Scalar arithmetic — avoids creating a constant leaf node for the scalar operand.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Var<T> RecordAddScalar(Var<T> a, T b) => RecordUnary(a.NodeId, a.Value + b, T.One);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Var<T> RecordSubScalar(Var<T> a, T b) => RecordUnary(a.NodeId, a.Value - b, T.One);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Var<T> RecordSubScalarLeft(T a, Var<T> b) => RecordUnary(b.NodeId, a - b.Value, -T.One);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Var<T> RecordMulScalar(Var<T> a, T b) => RecordUnary(a.NodeId, a.Value * b, b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Var<T> RecordDivScalar(Var<T> a, T b) => RecordUnary(a.NodeId, a.Value / b, T.One / b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Var<T> RecordDivScalarLeft(T a, Var<T> b) => RecordUnary(b.NodeId, a / b.Value, -a / (b.Value * b.Value));

    // ── Primal / gradient accessors ──────────────────────────────────────────

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal T GetPrimal(int nodeId) => _nodes[nodeId].Primal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetGradient(int nodeId) => _nodes[nodeId].Gradient;

    // ── Backward pass ────────────────────────────────────────────────────────

    /// <summary>Run the reverse sweep from a scalar output variable.</summary>
    public void Backward(Var<T> output) => Backward(output.NodeId);

    /// <summary>Run the reverse sweep from a scalar output node.</summary>
    public void Backward(int outputNodeId)
    {
        ZeroGradients();
        _nodes[outputNodeId].Gradient = T.One;
        RunBackward(outputNodeId);
    }

    /// <summary>
    /// Run the reverse sweep seeding multiple output variables with the given cotangent values.
    /// Used by VJP to compute v^T · J(f) in a single pass.
    /// </summary>
    public void BackwardWithSeed(Var<T>[] outputs, T[] seeds)
    {
        var ids = new int[outputs.Length];
        for (int i = 0; i < outputs.Length; i++)
            ids[i] = outputs[i].NodeId;
        BackwardWithSeed(ids, seeds);
    }

    /// <summary>
    /// Run the reverse sweep seeding multiple output nodes with the given cotangent values.
    /// </summary>
    public void BackwardWithSeed(int[] outputNodeIds, T[] seeds)
    {
        ZeroGradients();
        for (int i = 0; i < outputNodeIds.Length; i++)
            _nodes[outputNodeIds[i]].Gradient += seeds[i];

        int maxId = outputNodeIds.Length > 0 ? outputNodeIds[0] : 0;
        foreach (var id in outputNodeIds)
            if (id > maxId) maxId = id;

        RunBackward(maxId);
    }

    private void RunBackward(int fromId)
    {
        for (int i = fromId; i >= 0; i--)
        {
            if (_nodes[i].Gradient == T.Zero) continue;
            if (_nodes[i].Input0 >= 0) AccumulateGradient(i, _nodes[i].Input0, _nodes[i].Weight0);
            if (_nodes[i].Input1 >= 0) AccumulateGradient(i, _nodes[i].Input1, _nodes[i].Weight1);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AccumulateGradient(int fromNode, int inputId, T weight)
    {
        var contrib = _nodes[fromNode].Gradient * weight;
        if (EnableNaNGuard && NaNGuard.IsPathological(contrib))
        {
            var diag = EnableDiagnostics ? BuildDiagnostic(fromNode) : null;
            NaNGuard.CheckGradient(contrib, fromNode, _nodes[fromNode].DebugName ?? $"node_{fromNode}", diag);
        }
        _nodes[inputId].Gradient += contrib;
    }

    /// <summary>
    /// Build a diagnostic tree rooted at the given node, walking its forward-graph inputs recursively.
    /// Depth is capped to prevent runaway trees on large computations.
    /// </summary>
    public GradientDiagnostics BuildDiagnostic(int nodeId, int maxDepth = DefaultDiagnosticDepth)
        => BuildDiagnosticRecursive(nodeId, maxDepth);

    private GradientDiagnostics BuildDiagnosticRecursive(int nodeId, int depth)
    {
        ref var node = ref _nodes[nodeId];
        var gradDouble = double.CreateChecked(node.Gradient);

        var inputs = new List<GradientDiagnostics>();
        if (depth > 0)
        {
            if (node.Input0 >= 0)
                inputs.Add(BuildDiagnosticRecursive(node.Input0, depth - 1));
            if (node.Input1 >= 0)
                inputs.Add(BuildDiagnosticRecursive(node.Input1, depth - 1));
        }

        return new GradientDiagnostics
        {
            NodeId = nodeId,
            OperationName = node.DebugName ?? DescribeNode(ref node),
            GradientValue = gradDouble,
            IsNaN = double.IsNaN(gradDouble),
            IsInfinity = double.IsInfinity(gradDouble),
            Inputs = inputs
        };
    }

    private static string DescribeNode(ref TapeNode<T> node)
    {
        if (node.Input0 < 0) return "leaf";
        if (node.Input1 < 0) return "unary";
        return "binary";
    }

    // ── Memory management ────────────────────────────────────────────────────

    /// <summary>Zero all gradient slots while preserving primal values and structure.</summary>
    public void ZeroGradients()
    {
        for (int i = 0; i < _count; i++)
            _nodes[i].Gradient = T.Zero;
    }

    /// <summary>Reset the tape to empty, reusing the allocated array.</summary>
    public void Reset()
    {
        _count = 0;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            Reset();
            TapePool<T>.Return(this);
        }
    }

    // ── Internal helpers ─────────────────────────────────────────────────────

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Append(TapeNode<T> node)
    {
        if (_count == _nodes.Length)
            Array.Resize(ref _nodes, _nodes.Length * 2);
        var id = _count++;
        _nodes[id] = node;
        return id;
    }
}
