using DeepSigma.Mathematics.AutoDiff.Reverse;
using BenchmarkDotNet.Attributes;

namespace DeepSigma.Mathematics.AutoDiff.Tests.Benchmarks;

/// <summary>
/// 3-layer MLP forward+backward at the input/hidden sizes called out in the plan.
/// </summary>
[MemoryDiagnoser]
public class MlpBenchmarks
{
    private const int Input = 16;
    private const int Hidden = 64;
    private const int Output = 1;

    private double[] _W1 = Array.Empty<double>();
    private double[] _b1 = Array.Empty<double>();
    private double[] _W2 = Array.Empty<double>();
    private double[] _b2 = Array.Empty<double>();
    private double[] _W3 = Array.Empty<double>();
    private double[] _b3 = Array.Empty<double>();
    private double[] _x = Array.Empty<double>();

    [GlobalSetup]
    public void Setup()
    {
        var rng = new Random(42);
        _W1 = NewRandom(rng, Input * Hidden);
        _b1 = NewRandom(rng, Hidden);
        _W2 = NewRandom(rng, Hidden * Hidden);
        _b2 = NewRandom(rng, Hidden);
        _W3 = NewRandom(rng, Hidden * Output);
        _b3 = NewRandom(rng, Output);
        _x = NewRandom(rng, Input);
    }

    private static double[] NewRandom(Random rng, int n)
    {
        var arr = new double[n];
        for (int i = 0; i < n; i++) arr[i] = (rng.NextDouble() - 0.5) * 0.1;
        return arr;
    }

    [Benchmark]
    public double Mlp_ForwardBackward()
    {
        using var tape = TapePool<double>.Rent(8192);
        var W1 = Wrap(tape, _W1);
        var b1 = Wrap(tape, _b1);
        var W2 = Wrap(tape, _W2);
        var b2 = Wrap(tape, _b2);
        var W3 = Wrap(tape, _W3);
        var b3 = Wrap(tape, _b3);
        var x = Wrap(tape, _x);

        var h1 = new Var<double>[Hidden];
        for (int j = 0; j < Hidden; j++)
        {
            var z = b1[j];
            for (int i = 0; i < Input; i++) z = z + W1[j * Input + i] * x[i];
            h1[j] = ReverseMath<double>.Tanh(z);
        }

        var h2 = new Var<double>[Hidden];
        for (int j = 0; j < Hidden; j++)
        {
            var z = b2[j];
            for (int i = 0; i < Hidden; i++) z = z + W2[j * Hidden + i] * h1[i];
            h2[j] = ReverseMath<double>.Tanh(z);
        }

        var o = b3[0];
        for (int i = 0; i < Hidden; i++) o = o + W3[i] * h2[i];

        tape.Backward(o);
        return o.Value;
    }

    private static Var<double>[] Wrap(Tape<double> tape, double[] vals)
    {
        var arr = new Var<double>[vals.Length];
        for (int i = 0; i < vals.Length; i++) arr[i] = tape.Variable(vals[i]);
        return arr;
    }
}

[MemoryDiagnoser]
public class TapePoolBenchmarks
{
    [Benchmark(Baseline = true)]
    public int RentReturn_Cycle()
    {
        int total = 0;
        for (int k = 0; k < 1000; k++)
        {
            using var tape = TapePool<double>.Rent();
            var x = tape.Variable(1.0);
            var y = x * x + x;
            tape.Backward(y);
            total += tape.NodeCount;
        }
        return total;
    }

    [Benchmark]
    public int FreshAlloc_Cycle()
    {
        int total = 0;
        for (int k = 0; k < 1000; k++)
        {
            var tape = new Tape<double>();
            var x = tape.Variable(1.0);
            var y = x * x + x;
            tape.Backward(y);
            total += tape.NodeCount;
        }
        return total;
    }
}
