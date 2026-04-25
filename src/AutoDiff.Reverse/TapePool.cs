using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Reverse;

/// <summary>
/// Thread-local pool of reusable Tape&lt;T&gt; instances.
/// Use with <c>using var tape = TapePool&lt;double&gt;.Rent();</c>
/// </summary>
public static class TapePool<T> where T : IFloatingPoint<T>
{
    [ThreadStatic]
    private static Stack<Tape<T>>? _pool;

    public static Tape<T> Rent(int capacity = 256)
    {
        _pool ??= new Stack<Tape<T>>();
        if (_pool.TryPop(out var tape))
        {
            tape.Reset();
            return tape;
        }
        return new Tape<T>(capacity);
    }

    internal static void Return(Tape<T> tape)
    {
        _pool ??= new Stack<Tape<T>>();
        _pool.Push(tape);
    }
}
