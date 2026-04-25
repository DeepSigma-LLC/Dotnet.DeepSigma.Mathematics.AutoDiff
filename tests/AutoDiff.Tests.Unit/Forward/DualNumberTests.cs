using DeepSigma.Mathematics.AutoDiff.Forward;
using DeepSigma.Mathematics.AutoDiff.Tests.Unit;

namespace DeepSigma.Mathematics.AutoDiff.Tests.UnitForward;

public class DualNumberTests
{
    private const double Tol = 1e-10;

    [Fact]
    public void Const_HasZeroDual()
    {
        var d = DualNumber<double>.Const(3.14);
        Assert.Equal(3.14, d.Real);
        Assert.Equal(0.0, d.Dual);
    }

    [Fact]
    public void Variable_HasOneDual()
    {
        var d = DualNumber<double>.Variable(2.0);
        Assert.Equal(2.0, d.Real);
        Assert.Equal(1.0, d.Dual);
    }

    [Fact]
    public void Addition_SumsRealAndDual()
    {
        var a = new DualNumber<double>(3, 1);
        var b = new DualNumber<double>(2, 0);
        var r = a + b;
        Assert.Equal(5, r.Real);
        Assert.Equal(1, r.Dual);
    }

    [Fact]
    public void Subtraction_SubtractsRealAndDual()
    {
        var a = new DualNumber<double>(5, 1);
        var b = new DualNumber<double>(2, 0);
        var r = a - b;
        Assert.Equal(3, r.Real);
        Assert.Equal(1, r.Dual);
    }

    [Fact]
    public void Multiplication_ProductRule()
    {
        // d/dx[x*x] at x=3 => 2*x = 6
        var x = DualNumber<double>.Variable(3);
        var r = x * x;
        Assert.Equal(9, r.Real);
        Assert.Equal(6, r.Dual);
    }

    [Fact]
    public void Division_QuotientRule()
    {
        // d/dx[x / 2] at x=4 => 0.5
        var x = DualNumber<double>.Variable(4);
        var c = DualNumber<double>.Const(2);
        var r = x / c;
        Assert.Equal(2.0, r.Real);
        Assert.Equal(0.5, r.Dual);
    }

    [Fact]
    public void Negation_NegatesRealAndDual()
    {
        var x = DualNumber<double>.Variable(3);
        var r = -x;
        Assert.Equal(-3, r.Real);
        Assert.Equal(-1, r.Dual);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public void DerivativeOf_xSquared_Matches_FiniteDiff(double point)
    {
        var ad = ForwardDiff<double>.Derivative(x => x * x, point);
        var fd = FiniteDiff.Derivative(x => x * x, point);
        Assert.Equal(fd, ad, precision: 8);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public void DerivativeOf_xCubed_Matches_FiniteDiff(double point)
    {
        var ad = ForwardDiff<double>.Derivative(x => x * x * x, point);
        var fd = FiniteDiff.Derivative(x => x * x * x, point);
        Assert.Equal(fd, ad, precision: 7);
    }

    [Fact]
    public void Gradient_QuadraticSurface()
    {
        // f(x,y) = x^2 + y^2  =>  grad = [2x, 2y]
        var point = new[] { 3.0, 4.0 };
        var grad = ForwardDiff<double>.Gradient(
            inputs => inputs[0] * inputs[0] + inputs[1] * inputs[1],
            point);

        Assert.Equal(6.0, grad[0], precision: 10);
        Assert.Equal(8.0, grad[1], precision: 10);
    }

    [Fact]
    public void DirectionalDerivative_AlongDiagonal()
    {
        // f(x,y) = x*y, direction (1,1)/√2
        // d/dv[x*y] at (2,3) = y*1/√2 + x*1/√2 = 5/√2
        var point = new[] { 2.0, 3.0 };
        var dir = new[] { 1.0 / Math.Sqrt(2), 1.0 / Math.Sqrt(2) };
        var dd = ForwardDiff<double>.DirectionalDerivative(
            inputs => inputs[0] * inputs[1], point, dir);
        Assert.Equal(5.0 / Math.Sqrt(2), dd, precision: 10);
    }

    [Fact]
    public void ScalarOperators_Work()
    {
        var x = DualNumber<double>.Variable(3.0);
        var r = x * 2.0 + 1.0;
        Assert.Equal(7.0, r.Real);
        Assert.Equal(2.0, r.Dual);
    }
}
