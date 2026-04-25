using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Forward;

/// <summary>
/// Convenience API for computing derivatives and gradients using forward-mode AD.
/// </summary>
public static class ForwardDiff<T>
    where T : IFloatingPoint<T>,
              ITrigonometricFunctions<T>,
              IExponentialFunctions<T>,
              ILogarithmicFunctions<T>,
              IHyperbolicFunctions<T>,
              IPowerFunctions<T>,
              IRootFunctions<T>
{
    /// <summary>Derivative of a univariate function at <paramref name="point"/>.</summary>
    public static T Derivative(Func<DualNumber<T>, DualNumber<T>> f, T point)
        => f(DualNumber<T>.Variable(point)).Dual;

    /// <summary>
    /// Gradient of f: R^n → R via n sequential forward passes (one per input variable).
    /// For many-input functions prefer reverse mode.
    /// </summary>
    public static T[] Gradient(Func<DualNumber<T>[], DualNumber<T>> f, T[] point)
    {
        var grad = new T[point.Length];
        var inputs = new DualNumber<T>[point.Length];

        for (int i = 0; i < point.Length; i++)
        {
            for (int j = 0; j < point.Length; j++)
                inputs[j] = j == i ? DualNumber<T>.Variable(point[j]) : DualNumber<T>.Const(point[j]);

            grad[i] = f(inputs).Dual;
        }

        return grad;
    }

    /// <summary>Directional derivative df/dv at <paramref name="point"/> along <paramref name="direction"/>.</summary>
    public static T DirectionalDerivative(
        Func<DualNumber<T>[], DualNumber<T>> f, T[] point, T[] direction)
    {
        var inputs = new DualNumber<T>[point.Length];
        for (int i = 0; i < point.Length; i++)
            inputs[i] = DualNumber<T>.Variable(point[i], direction[i]);
        return f(inputs).Dual;
    }

    /// <summary>
    /// Jacobian of f: R^n → R^m. Returns an m×n matrix where row i is the gradient of output i.
    /// Computed via n forward passes.
    /// </summary>
    public static T[,] Jacobian(Func<DualNumber<T>[], DualNumber<T>[]> f, T[] point)
    {
        var inputs = new DualNumber<T>[point.Length];
        T[,]? jacobian = null;

        for (int i = 0; i < point.Length; i++)
        {
            for (int j = 0; j < point.Length; j++)
                inputs[j] = j == i ? DualNumber<T>.Variable(point[j]) : DualNumber<T>.Const(point[j]);

            var outputs = f(inputs);
            jacobian ??= new T[outputs.Length, point.Length];

            for (int row = 0; row < outputs.Length; row++)
                jacobian[row, i] = outputs[row].Dual;
        }

        return jacobian ?? new T[0, point.Length];
    }
}
