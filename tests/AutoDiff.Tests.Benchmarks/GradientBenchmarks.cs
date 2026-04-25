using DeepSigma.Mathematics.AutoDiff.Forward;
using DeepSigma.Mathematics.AutoDiff.Reverse;
using BenchmarkDotNet.Attributes;

namespace DeepSigma.Mathematics.AutoDiff.Tests.Benchmarks;

/// <summary>
/// Reverse vs forward mode on f(x⃗) = Σ xᵢ². Reverse should be one pass; forward needs N passes.
/// </summary>
[MemoryDiagnoser]
public class GradientBenchmarks
{
    [Params(10, 100, 1000)]
    public int N;

    private double[] _x = Array.Empty<double>();

    [GlobalSetup]
    public void Setup()
    {
        _x = new double[N];
        for (int i = 0; i < N; i++) _x[i] = i * 0.01;
    }

    [Benchmark(Baseline = true)]
    public double[] Reverse_QuadraticGradient()
    {
        using var tape = TapePool<double>.Rent(N * 2);
        var vars = new Var<double>[N];
        for (int i = 0; i < N; i++) vars[i] = tape.Variable(_x[i]);

        var sum = vars[0] * vars[0];
        for (int i = 1; i < N; i++) sum = sum + vars[i] * vars[i];
        tape.Backward(sum);

        var grad = new double[N];
        for (int i = 0; i < N; i++) grad[i] = vars[i].Gradient;
        return grad;
    }

    [Benchmark]
    public double[] Reverse_NoPool()
    {
        var tape = new Tape<double>(N * 2);
        var vars = new Var<double>[N];
        for (int i = 0; i < N; i++) vars[i] = tape.Variable(_x[i]);

        var sum = vars[0] * vars[0];
        for (int i = 1; i < N; i++) sum = sum + vars[i] * vars[i];
        tape.Backward(sum);

        var grad = new double[N];
        for (int i = 0; i < N; i++) grad[i] = vars[i].Gradient;
        return grad;
    }

    [Benchmark]
    public double[] Forward_QuadraticGradient()
    {
        var grad = new double[N];
        for (int j = 0; j < N; j++)
        {
            var duals = new DualNumber<double>[N];
            for (int i = 0; i < N; i++)
                duals[i] = new DualNumber<double>(_x[i], i == j ? 1.0 : 0.0);

            var sum = duals[0] * duals[0];
            for (int i = 1; i < N; i++) sum = sum + duals[i] * duals[i];
            grad[j] = sum.Dual;
        }
        return grad;
    }
}
