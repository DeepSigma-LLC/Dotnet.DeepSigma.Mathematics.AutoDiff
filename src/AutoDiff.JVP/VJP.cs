using System.Numerics;
using DeepSigma.Mathematics.AutoDiff.Forward;
using DeepSigma.Mathematics.AutoDiff.Reverse;

namespace DeepSigma.Mathematics.AutoDiff.JVP;

/// <summary>
/// Vector-Jacobian product (pullback): v^T·J(f) via reverse mode with a custom cotangent seed.
/// Given f: R^n → R^m, computes v^T·J(f)(x) ∈ R^n in a single backward pass.
/// </summary>
public static class VJP
{
    public static T[] Compute<T>(
        Func<Var<T>[], Var<T>[]> f,
        T[] x,
        T[] cotangent)
        where T : IFloatingPoint<T>
    {
        using var tape = TapePool<T>.Rent(x.Length * 4);
        var vars = new Var<T>[x.Length];
        for (int i = 0; i < x.Length; i++)
            vars[i] = tape.Variable(x[i]);

        var outputs = f(vars);
        JvpHelpers.ValidateLengths(nameof(outputs), outputs.Length, nameof(cotangent), cotangent.Length);

        tape.BackwardWithSeed(outputs, cotangent);

        var result = new T[x.Length];
        for (int i = 0; i < x.Length; i++)
            result[i] = vars[i].Gradient;
        return result;
    }

    /// <summary>
    /// Full Jacobian of f: R^n → R^m computed via n forward JVP passes with standard basis vectors.
    /// Rows are outputs, columns are inputs: J[i,j] = ∂f_i/∂x_j.
    /// </summary>
    public static T[,] Jacobian<T>(
        Func<DualNumber<T>[], DualNumber<T>[]> f,
        T[] x)
        where T : IFloatingPoint<T>
    {
        var tangent = new T[x.Length];
        T[,]? jacobian = null;

        for (int j = 0; j < x.Length; j++)
        {
            for (int k = 0; k < x.Length; k++)
                tangent[k] = k == j ? T.One : T.Zero;

            var col = JVP.Compute(f, x, tangent);
            jacobian ??= new T[col.Length, x.Length];
            for (int i = 0; i < col.Length; i++)
                jacobian[i, j] = col[i];
        }

        return jacobian ?? new T[0, x.Length];
    }
}
