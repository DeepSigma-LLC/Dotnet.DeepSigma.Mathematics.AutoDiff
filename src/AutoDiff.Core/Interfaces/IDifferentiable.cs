using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Core;

public interface IDifferentiable<TSelf, T>
    where TSelf : IDifferentiable<TSelf, T>
    where T : IFloatingPoint<T>
{
    T Value { get; }
    TSelf Add(TSelf other);
    TSelf Multiply(TSelf other);
    TSelf Negate();
}
