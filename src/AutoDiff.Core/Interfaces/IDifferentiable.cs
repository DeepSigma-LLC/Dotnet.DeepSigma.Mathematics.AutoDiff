using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Core;

/// <summary>
/// Defines the algebraic operations required by the automatic differentiation
/// engine for a differentiable type. Implementations record each operation in a
/// computation graph (tape or dual-number chain) so that derivatives can be
/// propagated later.
/// </summary>
/// <typeparam name="TSelf">
/// The concrete implementing type, enabling fluent return types via the
/// curiously-recurring template pattern (CRTP).
/// </typeparam>
/// <typeparam name="T">
/// The underlying floating-point scalar type (e.g. <see cref="double"/>,
/// <see cref="float"/>).
/// </typeparam>
public interface IDifferentiable<TSelf, T>
    where TSelf : IDifferentiable<TSelf, T>
    where T : IFloatingPoint<T>
{
    /// <summary>
    /// The primal (forward) scalar value held by this differentiable object.
    /// </summary>
    T Value { get; }

    /// <summary>
    /// Returns the sum of this instance and <paramref name="other"/>, producing a
    /// new differentiable value that records the addition in the computation graph.
    /// </summary>
    /// <param name="other">The right-hand operand.</param>
    TSelf Add(TSelf other);

    /// <summary>
    /// Returns the product of this instance and <paramref name="other"/>, producing
    /// a new differentiable value that records the multiplication in the computation
    /// graph.
    /// </summary>
    /// <param name="other">The right-hand operand.</param>
    TSelf Multiply(TSelf other);

    /// <summary>
    /// Returns the additive inverse of this instance (i.e. <c>−this</c>), producing
    /// a new differentiable value that records the negation in the computation graph.
    /// </summary>
    TSelf Negate();
}
