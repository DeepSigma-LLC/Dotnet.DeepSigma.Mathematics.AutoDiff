using System.Numerics;

namespace AutoDiff.Reverse;

/// <summary>
/// Convenience API for computing gradients using reverse-mode AD.
/// Prefer reverse mode when the number of inputs is large relative to outputs.
/// </summary>
public static class ReverseDiff<T>
    where T : IFloatingPoint<T>
{
    /// <summary>Gradient of f: R^n → R in a single backward pass.</summary>
    public static T[] Gradient(Func<Var<T>[], Var<T>> f, T[] point)
    {
        using var tape = TapePool<T>.Rent(point.Length * 4);
        var vars = new Var<T>[point.Length];
        for (int i = 0; i < point.Length; i++)
            vars[i] = tape.Variable(point[i]);

        var output = f(vars);
        tape.Backward(output.NodeId);

        var grad = new T[point.Length];
        for (int i = 0; i < point.Length; i++)
            grad[i] = vars[i].Gradient;
        return grad;
    }

    /// <summary>Gradient of a univariate function.</summary>
    public static T Derivative(Func<Var<T>, Var<T>> f, T point)
    {
        using var tape = TapePool<T>.Rent(16);
        var x = tape.Variable(point);
        var output = f(x);
        tape.Backward(output.NodeId);
        return x.Gradient;
    }
}
