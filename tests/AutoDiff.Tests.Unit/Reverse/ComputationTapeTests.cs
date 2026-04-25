using DeepSigma.Mathematics.AutoDiff.Reverse;

namespace DeepSigma.Mathematics.AutoDiff.Tests.UnitReverse;

public class ComputationTapeTests
{
    [Fact]
    public void Variable_HasCorrectPrimal()
    {
        using var tape = ComputationTapePool<double>.Rent();
        var x = tape.Variable(3.14);
        Assert.Equal(3.14, x.Value);
    }

    [Fact]
    public void Backward_Univariate_xSquared()
    {
        // f(x) = x^2, df/dx = 2x
        using var tape = ComputationTapePool<double>.Rent();
        var x = tape.Variable(3.0);
        var y = x * x;
        tape.Backward(y);
        Assert.Equal(6.0, x.Gradient, precision: 12);
    }

    [Fact]
    public void Backward_Bivariate_xPlusY()
    {
        // f(x,y) = x+y, df/dx=1, df/dy=1
        using var tape = ComputationTapePool<double>.Rent();
        var x = tape.Variable(2.0);
        var y = tape.Variable(5.0);
        var z = x + y;
        tape.Backward(z);
        Assert.Equal(1.0, x.Gradient, precision: 12);
        Assert.Equal(1.0, y.Gradient, precision: 12);
    }

    [Fact]
    public void Backward_Bivariate_xTimesY()
    {
        // f(x,y) = x*y, df/dx=y, df/dy=x
        using var tape = ComputationTapePool<double>.Rent();
        var x = tape.Variable(3.0);
        var y = tape.Variable(4.0);
        var z = x * y;
        tape.Backward(z);
        Assert.Equal(4.0, x.Gradient, precision: 12);
        Assert.Equal(3.0, y.Gradient, precision: 12);
    }

    [Fact]
    public void Backward_Division_QuotientRule()
    {
        // f(x,y) = x/y, df/dx = 1/y, df/dy = -x/y^2
        using var tape = ComputationTapePool<double>.Rent();
        var x = tape.Variable(6.0);
        var y = tape.Variable(3.0);
        var z = x / y;
        tape.Backward(z);
        Assert.Equal(1.0 / 3.0, x.Gradient, precision: 12);
        Assert.Equal(-6.0 / 9.0, y.Gradient, precision: 12);
    }

    [Fact]
    public void Backward_SharedSubexpression_AccumulatesGradient()
    {
        // f(x) = x * x + x  =>  df/dx = 2x + 1 = 5 at x=2
        using var tape = ComputationTapePool<double>.Rent();
        var x = tape.Variable(2.0);
        var z = x * x + x;
        tape.Backward(z);
        Assert.Equal(5.0, x.Gradient, precision: 12);
    }

    [Fact]
    public void ZeroGradients_ClearsGradients()
    {
        using var tape = ComputationTapePool<double>.Rent();
        var x = tape.Variable(3.0);
        var y = x * x;
        tape.Backward(y);
        Assert.Equal(6.0, x.Gradient, precision: 12);

        tape.ZeroGradients();
        Assert.Equal(0.0, x.Gradient);
    }

    [Fact]
    public void Reset_AllowsReuse()
    {
        using var tape = ComputationTapePool<double>.Rent();
        var x = tape.Variable(3.0);
        var y = x * x;
        tape.Backward(y);

        tape.Reset();
        Assert.Equal(0, tape.NodeCount);

        var x2 = tape.Variable(5.0);
        var y2 = x2 * x2;
        tape.Backward(y2);
        Assert.Equal(10.0, x2.Gradient, precision: 12);
    }

    [Fact]
    public void TapePool_RentReturn_Cycle()
    {
        var tape1 = ComputationTapePool<double>.Rent();
        tape1.Variable(1.0);
        tape1.Dispose();

        var tape2 = ComputationTapePool<double>.Rent();
        Assert.Equal(0, tape2.NodeCount); // reset on return
        tape2.Dispose();
    }

    [Fact]
    public void Negation_GradientIsNegativeOne()
    {
        using var tape = ComputationTapePool<double>.Rent();
        var x = tape.Variable(5.0);
        var y = -x;
        tape.Backward(y);
        Assert.Equal(-1.0, x.Gradient, precision: 12);
    }

    [Fact]
    public void ScalarOperators_Work()
    {
        // f(x) = 2*x + 3  =>  df/dx = 2
        using var tape = ComputationTapePool<double>.Rent();
        var x = tape.Variable(4.0);
        var y = 2.0 * x + 3.0;
        tape.Backward(y);
        Assert.Equal(2.0, x.Gradient, precision: 12);
    }
}
