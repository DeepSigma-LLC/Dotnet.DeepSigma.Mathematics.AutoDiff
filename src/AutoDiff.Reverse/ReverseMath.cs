using System.Numerics;
using System.Runtime.CompilerServices;
using DeepSigma.Mathematics.AutoDiff.Core;

namespace DeepSigma.Mathematics.AutoDiff.Reverse;

/// <summary>
/// Elementary functions for Var&lt;T&gt;. Each records the primal value and local derivative weight
/// onto the tape so the backward pass can accumulate gradients.
/// </summary>
public static class ReverseMath<T>
    where T : IFloatingPoint<T>,
              ITrigonometricFunctions<T>,
              IExponentialFunctions<T>,
              ILogarithmicFunctions<T>,
              IHyperbolicFunctions<T>,
              IPowerFunctions<T>,
              IRootFunctions<T>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Sin(Var<T> x)
        => x.Tape.RecordUnary(x.NodeId, T.Sin(x.Value), T.Cos(x.Value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Cos(Var<T> x)
        => x.Tape.RecordUnary(x.NodeId, T.Cos(x.Value), -T.Sin(x.Value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Tan(Var<T> x)
    {
        var c = T.Cos(x.Value);
        return x.Tape.RecordUnary(x.NodeId, T.Tan(x.Value), T.One / (c * c));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Exp(Var<T> x)
    {
        var primal = T.Exp(x.Value);
        return x.Tape.RecordUnary(x.NodeId, primal, primal); // d/dx e^x = e^x
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Log(Var<T> x)
        => x.Tape.RecordUnary(x.NodeId, T.Log(x.Value), T.One / x.Value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Sqrt(Var<T> x)
    {
        var primal = T.Sqrt(x.Value);
        return x.Tape.RecordUnary(x.NodeId, primal, T.One / (GenericMath.Two<T>() * primal));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Tanh(Var<T> x)
    {
        var primal = T.Tanh(x.Value);
        return x.Tape.RecordUnary(x.NodeId, primal, T.One - primal * primal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Sinh(Var<T> x)
        => x.Tape.RecordUnary(x.NodeId, T.Sinh(x.Value), T.Cosh(x.Value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Cosh(Var<T> x)
        => x.Tape.RecordUnary(x.NodeId, T.Cosh(x.Value), T.Sinh(x.Value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Var<T> Abs(Var<T> x)
    {
        var weight = x.Value >= T.Zero ? T.One : -T.One;
        return x.Tape.RecordUnary(x.NodeId, T.Abs(x.Value), weight);
    }

    public static Var<T> Pow(Var<T> b, Var<T> e)
    {
        var primal = T.Pow(b.Value, e.Value);
        // ∂/∂b[b^e] = e·b^(e-1)
        var wb = e.Value * T.Pow(b.Value, e.Value - T.One);
        // ∂/∂e[b^e] = b^e·ln(b)
        var we = primal * T.Log(b.Value);
        return b.Tape.RecordBinary(b.NodeId, e.NodeId, primal, wb, we);
    }

    public static Var<T> Pow(Var<T> b, T exponent)
    {
        var primal = T.Pow(b.Value, exponent);
        var weight = exponent * T.Pow(b.Value, exponent - T.One);
        return b.Tape.RecordUnary(b.NodeId, primal, weight);
    }
}
