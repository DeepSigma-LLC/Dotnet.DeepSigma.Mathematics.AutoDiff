using AutoDiff.Reverse;

namespace AutoDiff.Tests.Unit.Reverse;

public class ReverseMathTests
{
    private static double ReverseDerivative(
        Func<Var<double>, Var<double>> f, double point)
    {
        using var tape = TapePool<double>.Rent();
        var x = tape.Variable(point);
        var y = f(x);
        tape.Backward(y);
        return x.Gradient;
    }

    private static void AssertMatchesFD(
        Func<Var<double>, Var<double>> adF,
        Func<double, double> f,
        double point,
        int precision = 6)
    {
        var ad = ReverseDerivative(adF, point);
        var fd = FiniteDiff.Derivative(f, point);
        Assert.Equal(fd, ad, precision);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(Math.PI / 4)]
    public void Sin_Derivative(double x) =>
        AssertMatchesFD(ReverseMath<double>.Sin, Math.Sin, x);

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(Math.PI / 4)]
    public void Cos_Derivative(double x) =>
        AssertMatchesFD(ReverseMath<double>.Cos, Math.Cos, x);

    [Theory]
    [InlineData(0.1)]
    [InlineData(1.0)]
    [InlineData(2.5)]
    public void Tan_Derivative(double x) =>
        AssertMatchesFD(ReverseMath<double>.Tan, Math.Tan, x);

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    public void Exp_Derivative(double x) =>
        AssertMatchesFD(ReverseMath<double>.Exp, Math.Exp, x);

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(3.0)]
    public void Log_Derivative(double x) =>
        AssertMatchesFD(ReverseMath<double>.Log, Math.Log, x);

    [Theory]
    [InlineData(0.1)]
    [InlineData(1.0)]
    [InlineData(4.0)]
    public void Sqrt_Derivative(double x) =>
        AssertMatchesFD(ReverseMath<double>.Sqrt, Math.Sqrt, x);

    [Theory]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(-1.0)]
    public void Tanh_Derivative(double x) =>
        AssertMatchesFD(ReverseMath<double>.Tanh, Math.Tanh, x);

    [Fact]
    public void Abs_PositiveInput()
    {
        var g = ReverseDerivative(ReverseMath<double>.Abs, 2.0);
        Assert.Equal(1.0, g, precision: 12);
    }

    [Fact]
    public void Abs_NegativeInput()
    {
        var g = ReverseDerivative(ReverseMath<double>.Abs, -3.0);
        Assert.Equal(-1.0, g, precision: 12);
    }

    [Fact]
    public void Gradient_CompoundFunction_MatchesFiniteDiff()
    {
        // f(x,y) = sin(x) * exp(y)
        var point = new[] { 1.0, 0.5 };
        var fdGrad = FiniteDiff.Gradient(
            p => Math.Sin(p[0]) * Math.Exp(p[1]),
            point);

        var adGrad = ReverseDiff<double>.Gradient(
            vars => ReverseMath<double>.Sin(vars[0]) * ReverseMath<double>.Exp(vars[1]),
            point);

        Assert.Equal(fdGrad[0], adGrad[0], precision: 7);
        Assert.Equal(fdGrad[1], adGrad[1], precision: 7);
    }

    [Fact]
    public void ForwardAndReverse_AgreeOnGradient()
    {
        // f(x,y) = tanh(x) + log(y)
        var point = new[] { 0.8, 1.5 };

        var fdGrad = FiniteDiff.Gradient(
            p => Math.Tanh(p[0]) + Math.Log(p[1]),
            point);

        var adGrad = ReverseDiff<double>.Gradient(
            vars => ReverseMath<double>.Tanh(vars[0]) + ReverseMath<double>.Log(vars[1]),
            point);

        Assert.Equal(fdGrad[0], adGrad[0], precision: 6);
        Assert.Equal(fdGrad[1], adGrad[1], precision: 6);
    }
}
