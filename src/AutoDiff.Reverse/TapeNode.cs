using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Reverse;

internal struct TapeNode<T> where T : IFloatingPoint<T>
{
    internal int Id;
    internal int Input0;   // -1 = no input (leaf / constant)
    internal int Input1;   // -1 = unary op or leaf
    internal T Weight0;    // ∂(this)/∂(Input0)
    internal T Weight1;    // ∂(this)/∂(Input1)
    internal T Primal;
    internal T Gradient;
    internal string? DebugName;

    private TapeNode(int id, int input0, int input1, T primal, T weight0, T weight1, string? debugName = null)
    {
        Id = id; Input0 = input0; Input1 = input1;
        Primal = primal; Gradient = T.Zero;
        Weight0 = weight0; Weight1 = weight1;
        DebugName = debugName;
    }

    internal static TapeNode<T> Leaf(int id, T primal, string? name)
        => new(id, -1, -1, primal, T.Zero, T.Zero, name);

    internal static TapeNode<T> Unary(int id, int input0, T primal, T weight0)
        => new(id, input0, -1, primal, weight0, T.Zero);

    internal static TapeNode<T> Binary(int id, int input0, int input1, T primal, T weight0, T weight1)
        => new(id, input0, input1, primal, weight0, weight1);
}
