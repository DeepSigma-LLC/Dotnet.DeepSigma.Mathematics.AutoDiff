using DeepSigma.Mathematics.AutoDiff.Forward;
using DeepSigma.Mathematics.AutoDiff.Tests.Unit;

namespace DeepSigma.Mathematics.AutoDiff.Tests.Unit.Forward;

public class DualMathTests
{
    private static void AssertMatchesFD(
        Func<DualNumber<double>, DualNumber<double>> adF,
        Func<double, double> f,
        double point,
        int precision = 6)
    {
        var ad = adF(DualNumber<double>.Variable(point)).Dual;
        var fd = FiniteDiff.Derivative(f, point);
        Assert.Equal(fd, ad, precision);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(Math.PI / 4)]
    public void Sin_Derivative(double x) =>
        AssertMatchesFD(DualMath<double>.Sin, Math.Sin, x);

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(Math.PI / 4)]
    public void Cos_Derivative(double x) =>
        AssertMatchesFD(DualMath<double>.Cos, Math.Cos, x);

    [Theory]
    [InlineData(0.1)]
    [InlineData(1.0)]
    [InlineData(2.5)]
    public void Tan_Derivative(double x) =>
        AssertMatchesFD(DualMath<double>.Tan, Math.Tan, x);

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    public void Exp_Derivative(double x) =>
        AssertMatchesFD(DualMath<double>.Exp, Math.Exp, x);

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(3.0)]
    public void Log_Derivative(double x) =>
        AssertMatchesFD(DualMath<double>.Log, Math.Log, x);

    [Theory]
    [InlineData(0.1)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public void Sqrt_Derivative(double x) =>
        AssertMatchesFD(DualMath<double>.Sqrt, Math.Sqrt, x);

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    public void Tanh_Derivative(double x) =>
        AssertMatchesFD(DualMath<double>.Tanh, Math.Tanh, x);

    [Theory]
    [InlineData(1.5)]
    [InlineData(2.0)]
    [InlineData(4.0)]
    public void Pow_WithScalarExponent_Derivative(double x)
    {
        double n = 3.0;
        var ad = DualMath<double>.Pow(DualNumber<double>.Variable(x), n).Dual;
        var fd = FiniteDiff.Derivative(v => Math.Pow(v, n), x);
        Assert.Equal(fd, ad, precision: 7);
    }

    [Fact]
    public void Abs_PositiveInput_HasPositiveDerivative()
    {
        var r = DualMath<double>.Abs(DualNumber<double>.Variable(2.0));
        Assert.Equal(2.0, r.Real);
        Assert.Equal(1.0, r.Dual);
    }

    [Fact]
    public void Abs_NegativeInput_HasNegativeDerivative()
    {
        var r = DualMath<double>.Abs(DualNumber<double>.Variable(-3.0));
        Assert.Equal(3.0, r.Real);
        Assert.Equal(-1.0, r.Dual);
    }
}
