using System.Numerics;
using System.Runtime.CompilerServices;
using DeepSigma.Mathematics.AutoDiff.Core;

namespace DeepSigma.Mathematics.AutoDiff.Reverse;

/// <summary>
/// Elementary differentiable functions for <see cref="Var{T}"/>.
/// Each method records both the primal value and the local partial-derivative weight onto
/// the active <see cref="ComputationTape{T}"/> so that a subsequent backward pass can
/// accumulate gradients via the chain rule.
/// </summary>
/// <typeparam name="T">A floating-point scalar type supporting the required numeric interfaces.</typeparam>
public static class ReverseFunctions<T>
    where T : IFloatingPoint<T>,
              ITrigonometricFunctions<T>,
              IExponentialFunctions<T>,
              ILogarithmicFunctions<T>,
              IHyperbolicFunctions<T>,
              IPowerFunctions<T>,
              IRootFunctions<T>
{
    /// <summary>Sine. Records weight cos(x) for the backward pass.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Sin(Var<T> x)
        => x.Tape.RecordUnary(x.NodeId, T.Sin(x.Value), T.Cos(x.Value));

    /// <summary>Cosine. Records weight −sin(x) for the backward pass.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Cos(Var<T> x)
        => x.Tape.RecordUnary(x.NodeId, T.Cos(x.Value), -T.Sin(x.Value));

    /// <summary>Tangent. Records weight 1/cos²(x) for the backward pass.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Tan(Var<T> x)
    {
        var c = T.Cos(x.Value);
        return x.Tape.RecordUnary(x.NodeId, T.Tan(x.Value), T.One / (c * c));
    }

    /// <summary>Natural exponential. Records weight e^x (equal to the primal) for the backward pass.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Exp(Var<T> x)
    {
        var primal = T.Exp(x.Value);
        return x.Tape.RecordUnary(x.NodeId, primal, primal);
    }

    /// <summary>Natural logarithm. Records weight 1/x for the backward pass.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Log(Var<T> x)
        => x.Tape.RecordUnary(x.NodeId, T.Log(x.Value), T.One / x.Value);

    /// <summary>Square root. Records weight 1/(2√x) for the backward pass.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Sqrt(Var<T> x)
    {
        var primal = T.Sqrt(x.Value);
        return x.Tape.RecordUnary(x.NodeId, primal, T.One / (FloatingPointConstants.Two<T>() * primal));
    }

    /// <summary>Hyperbolic tangent. Records weight sech²(x) = 1 − tanh²(x) for the backward pass.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Tanh(Var<T> x)
    {
        var primal = T.Tanh(x.Value);
        return x.Tape.RecordUnary(x.NodeId, primal, T.One - primal * primal);
    }

    /// <summary>Hyperbolic sine. Records weight cosh(x) for the backward pass.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Sinh(Var<T> x)
        => x.Tape.RecordUnary(x.NodeId, T.Sinh(x.Value), T.Cosh(x.Value));

    /// <summary>Hyperbolic cosine. Records weight sinh(x) for the backward pass.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Cosh(Var<T> x)
        => x.Tape.RecordUnary(x.NodeId, T.Cosh(x.Value), T.Sinh(x.Value));

    /// <summary>
    /// Absolute value. Records subgradient +1 for positive inputs and −1 for negative inputs.
    /// The subgradient at zero is treated as +1.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Abs(Var<T> x)
    {
        var weight = x.Value >= T.Zero ? T.One : -T.One;
        return x.Tape.RecordUnary(x.NodeId, T.Abs(x.Value), weight);
    }

    /// <summary>
    /// General power with both base and exponent as tape variables.
    /// Records ∂/∂b[b^e] = e·b^(e−1) and ∂/∂e[b^e] = b^e·ln(b).
    /// </summary>
    /// <param name="b">The base variable.</param>
    /// <param name="e">The exponent variable.</param>
    public static Var<T> Pow(Var<T> b, Var<T> e)
    {
        var primal = T.Pow(b.Value, e.Value);
        var wb = e.Value * T.Pow(b.Value, e.Value - T.One);
        var we = primal * T.Log(b.Value);
        return b.Tape.RecordBinary(b.NodeId, e.NodeId, primal, wb, we);
    }

    /// <summary>
    /// Power with a constant scalar exponent. Records weight n·x^(n−1) for the backward pass.
    /// </summary>
    /// <param name="b">The base variable.</param>
    /// <param name="exponent">The constant scalar exponent.</param>
    public static Var<T> Pow(Var<T> b, T exponent)
    {
        var primal = T.Pow(b.Value, exponent);
        var weight = exponent * T.Pow(b.Value, exponent - T.One);
        return b.Tape.RecordUnary(b.NodeId, primal, weight);
    }
}
