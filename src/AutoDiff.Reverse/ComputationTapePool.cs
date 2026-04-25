using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Reverse;

/// <summary>
/// Thread-local object pool of reusable <see cref="ComputationTape{T}"/> instances.
/// Renting and returning tapes eliminates repeated heap allocation in tight gradient loops.
/// </summary>
/// <remarks>
/// The pool is thread-local, so no locking is required. Each thread maintains its own stack
/// of idle tapes. Disposing a rented tape automatically returns it to the pool.
/// <code>
/// using var tape = ComputationTapePool&lt;double&gt;.Rent();
/// // ... build graph and call tape.Backward(output) ...
/// // Dispose returns the tape to the pool.
/// </code>
/// </remarks>
/// <typeparam name="T">A floating-point scalar type that implements <see cref="IFloatingPoint{T}"/>.</typeparam>
public static class ComputationTapePool<T> where T : IFloatingPoint<T>
{
    [ThreadStatic]
    private static Stack<ComputationTape<T>>? _pool;

    /// <summary>
    /// Rents a <see cref="ComputationTape{T}"/> from the pool, resetting it before returning.
    /// If the pool is empty a new tape is allocated with the specified capacity.
    /// </summary>
    /// <param name="capacity">Initial node-array capacity for newly allocated tapes.</param>
    /// <returns>A clean, ready-to-use <see cref="ComputationTape{T}"/>.</returns>
    public static ComputationTape<T> Rent(int capacity = 256)
    {
        _pool ??= new Stack<ComputationTape<T>>();
        if (_pool.TryPop(out var tape))
        {
            tape.Reset();
            return tape;
        }
        return new ComputationTape<T>(capacity);
    }

    internal static void Return(ComputationTape<T> tape)
    {
        _pool ??= new Stack<ComputationTape<T>>();
        _pool.Push(tape);
    }
}
