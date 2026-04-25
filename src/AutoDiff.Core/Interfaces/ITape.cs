using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Core;

public interface ITape<T> where T : IFloatingPoint<T>
{
    T GetGradient(int nodeId);
    void ZeroGradients();
    void Reset();
}
