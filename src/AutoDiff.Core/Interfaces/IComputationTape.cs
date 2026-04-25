using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Core;

/// <summary>
/// Defines the read and management operations on a computation tape used during
/// the reverse (backward) pass of automatic differentiation.
/// </summary>
/// <typeparam name="T">
/// The floating-point element type of the gradients stored on this tape.
/// </typeparam>
public interface IComputationTape<T> where T : IFloatingPoint<T>
{
    /// <summary>
    /// Returns the accumulated gradient for the tape node identified by
    /// <paramref name="nodeId"/>.
    /// </summary>
    /// <param name="nodeId">The zero-based index of the tape node.</param>
    T GetGradient(int nodeId);

    /// <summary>
    /// Resets all accumulated gradients to zero without discarding the recorded
    /// computation graph, allowing the tape to be reused for another backward pass
    /// over the same graph.
    /// </summary>
    void ZeroGradients();

    /// <summary>
    /// Clears both the computation graph and all accumulated gradients, returning
    /// the tape to its initial empty state ready for a new computation.
    /// </summary>
    void Reset();
}
