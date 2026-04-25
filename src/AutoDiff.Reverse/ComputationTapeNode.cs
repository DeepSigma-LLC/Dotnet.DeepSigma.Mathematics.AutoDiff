using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Reverse;

/// <summary>
/// A single node in the <see cref="ComputationTape{T}"/> graph.
/// Stores the primal value, accumulated gradient, up to two parent-node indices, and
/// the corresponding local partial-derivative weights needed for the backward sweep.
/// </summary>
/// <remarks>
/// Nodes are stored as value types in a flat array for cache-coherent access during the
/// backward pass. Leaf nodes have <c>Input0 = Input1 = -1</c>. Unary nodes have
/// <c>Input1 = -1</c>. Binary nodes use both input slots.
/// </remarks>
internal struct ComputationTapeNode<T> where T : IFloatingPoint<T>
{
    /// <summary>The index of this node within the tape's node array.</summary>
    internal int Id;

    /// <summary>Index of the first parent node, or −1 for leaf nodes.</summary>
    internal int Input0;

    /// <summary>Index of the second parent node, or −1 for unary ops and leaf nodes.</summary>
    internal int Input1;

    /// <summary>Local partial derivative ∂(this)/∂(Input0).</summary>
    internal T Weight0;

    /// <summary>Local partial derivative ∂(this)/∂(Input1).</summary>
    internal T Weight1;

    /// <summary>The primal (forward-pass) value computed at this node.</summary>
    internal T Primal;

    /// <summary>The accumulated gradient ∂output/∂(this), populated during the backward pass.</summary>
    internal T Gradient;

    /// <summary>Optional human-readable name; used in diagnostic trees and debugger display.</summary>
    internal string? DebugName;

    private ComputationTapeNode(int id, int input0, int input1, T primal, T weight0, T weight1, string? debugName = null)
    {
        Id = id; Input0 = input0; Input1 = input1;
        Primal = primal; Gradient = T.Zero;
        Weight0 = weight0; Weight1 = weight1;
        DebugName = debugName;
    }

    internal static ComputationTapeNode<T> Leaf(int id, T primal, string? name)
        => new(id, -1, -1, primal, T.Zero, T.Zero, name);

    internal static ComputationTapeNode<T> Unary(int id, int input0, T primal, T weight0)
        => new(id, input0, -1, primal, weight0, T.Zero);

    internal static ComputationTapeNode<T> Binary(int id, int input0, int input1, T primal, T weight0, T weight1)
        => new(id, input0, input1, primal, weight0, weight1);
}
