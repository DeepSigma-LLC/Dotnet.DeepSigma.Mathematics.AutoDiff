using System.Numerics;
using System.Runtime.CompilerServices;
using DeepSigma.Mathematics.AutoDiff.Core;

namespace DeepSigma.Mathematics.AutoDiff.Reverse;

/// <summary>
/// Records a forward computation graph and runs the reverse (backward) pass to accumulate gradients.
/// Nodes are stored in a flat array for cache-coherent backward sweep.
/// </summary>
/// <remarks>
/// Typical usage:
/// <code>
/// using var tape = ComputationTapePool&lt;double&gt;.Rent();
/// var x = tape.Variable(3.0, "x");
/// var y = tape.Variable(4.0, "y");
/// var z = ReverseFunctions&lt;double&gt;.Sin(x) * y;
/// tape.Backward(z);
/// double dz_dx = x.Gradient;
/// </code>
/// <see cref="ComputationTape{T}"/> implements <see cref="IDisposable"/>; disposing it returns the instance
/// to the <see cref="ComputationTapePool{T}"/> for reuse.
/// </remarks>
/// <typeparam name="T">A floating-point scalar type that implements <see cref="IFloatingPoint{T}"/>.</typeparam>
public sealed class ComputationTape<T> : IComputationTape<T>, IDisposable
    where T : IFloatingPoint<T>
{
    private const int DefaultDiagnosticDepth = 8;

    private ComputationTapeNode<T>[] _nodes;
    private int _count;
    private bool _disposed;

    /// <summary>
    /// When <see langword="true"/>, the backward pass throws <see cref="GradientNaNException"/>
    /// if a gradient contribution is NaN or infinity.
    /// </summary>
    public bool EnableNaNGuard { get; set; }

    /// <summary>
    /// When <see langword="true"/>, a full <see cref="GradientDiagnostics"/> tree is attached to any
    /// <see cref="GradientNaNException"/> thrown during the backward pass. Has no effect when
    /// <see cref="EnableNaNGuard"/> is <see langword="false"/>.
    /// </summary>
    public bool EnableDiagnostics { get; set; }

    /// <summary>The number of nodes currently recorded on the tape.</summary>
    public int NodeCount => _count;

    /// <summary>Initializes a new tape with the specified initial node-array capacity.</summary>
    /// <param name="initialCapacity">Starting capacity for the internal node array. Doubles on overflow.</param>
    public ComputationTape(int initialCapacity = 256)
    {
        _nodes = new ComputationTapeNode<T>[initialCapacity];
    }

    // ── Variable registration ────────────────────────────────────────────────

    /// <summary>
    /// Registers a new leaf variable on the tape and returns the corresponding <see cref="Var{T}"/>.
    /// </summary>
    /// <param name="value">The primal value of the variable.</param>
    /// <param name="name">Optional debug name shown in diagnostic trees and the debugger display.</param>
    public Var<T> Variable(T value, string? name = null)
    {
        var id = Append(ComputationTapeNode<T>.Leaf(_count, value, name));
        return new Var<T>(this, id);
    }

    // ── Recording ops (called by Var<T> operators and ReverseFunctions) ──────

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Var<T> RecordUnary(int inputId, T primal, T weight)
    {
        var id = Append(ComputationTapeNode<T>.Unary(_count, inputId, primal, weight));
        return new Var<T>(this, id);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Var<T> RecordBinary(int in0, int in1, T primal, T w0, T w1)
    {
        var id = Append(ComputationTapeNode<T>.Binary(_count, in0, in1, primal, w0, w1));
        return new Var<T>(this, id);
    }

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

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetGradient(int nodeId) => _nodes[nodeId].Gradient;

    // ── Backward pass ────────────────────────────────────────────────────────

    /// <summary>
    /// Runs the reverse sweep from a scalar output variable, seeding its gradient to 1.
    /// Gradients for all ancestor nodes are accumulated via the chain rule.
    /// </summary>
    /// <param name="output">The scalar output variable to differentiate.</param>
    public void Backward(Var<T> output) => Backward(output.NodeId);

    /// <summary>
    /// Runs the reverse sweep from a scalar output node, seeding its gradient to 1.
    /// </summary>
    /// <param name="outputNodeId">Index of the output node in the tape's node array.</param>
    public void Backward(int outputNodeId)
    {
        ZeroGradients();
        _nodes[outputNodeId].Gradient = T.One;
        RunBackward(outputNodeId);
    }

    /// <summary>
    /// Runs the reverse sweep seeding multiple output variables with explicit cotangent values.
    /// Computes v^T · J in a single backward pass; used internally by VJP.
    /// </summary>
    /// <param name="outputs">The output variables to seed.</param>
    /// <param name="seeds">Cotangent values — one per output variable.</param>
    public void BackwardWithSeed(Var<T>[] outputs, T[] seeds)
    {
        var ids = new int[outputs.Length];
        for (int i = 0; i < outputs.Length; i++)
            ids[i] = outputs[i].NodeId;
        BackwardWithSeed(ids, seeds);
    }

    /// <summary>
    /// Runs the reverse sweep seeding multiple output nodes with explicit cotangent values.
    /// </summary>
    /// <param name="outputNodeIds">Indices of the output nodes to seed.</param>
    /// <param name="seeds">Cotangent values — one per output node.</param>
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
    /// Builds a <see cref="GradientDiagnostics"/> tree rooted at the given node by walking
    /// the forward-graph inputs recursively. Depth is capped to prevent runaway trees on
    /// large computations.
    /// </summary>
    /// <param name="nodeId">The root node to start from.</param>
    /// <param name="maxDepth">Maximum recursion depth (default 8).</param>
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

    private static string DescribeNode(ref ComputationTapeNode<T> node)
    {
        if (node.Input0 < 0) return "leaf";
        if (node.Input1 < 0) return "unary";
        return "binary";
    }

    // ── Memory management ────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void ZeroGradients()
    {
        for (int i = 0; i < _count; i++)
            _nodes[i].Gradient = T.Zero;
    }

    /// <inheritdoc/>
    public void Reset()
    {
        _count = 0;
    }

    /// <summary>
    /// Resets the tape and returns it to the <see cref="ComputationTapePool{T}"/> for reuse.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            Reset();
            ComputationTapePool<T>.Return(this);
        }
    }

    // ── Internal helpers ─────────────────────────────────────────────────────

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int Append(ComputationTapeNode<T> node)
    {
        if (_count == _nodes.Length)
            Array.Resize(ref _nodes, _nodes.Length * 2);
        var id = _count++;
        _nodes[id] = node;
        return id;
    }
}
