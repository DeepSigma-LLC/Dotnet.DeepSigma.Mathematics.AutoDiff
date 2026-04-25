using DeepSigma.Mathematics.AutoDiff.Core;
using DeepSigma.Mathematics.AutoDiff.Reverse;

namespace DeepSigma.Mathematics.AutoDiff.Tests.UnitReverse;

public class DiagnosticsTests
{
    [Fact]
    public void NaNGuard_DetectsNaNInGradient()
    {
        using var tape = new Tape<double> { EnableNaNGuard = true };
        var x = tape.Variable(0.0, "x");
        var y = ReverseMath<double>.Log(x) * x;
        Assert.Throws<GradientNaNException>(() => { tape.Backward(y); });
    }

    [Fact]
    public void NaNGuard_Off_DoesNotThrow()
    {
        using var tape = new Tape<double>();
        var x = tape.Variable(0.0, "x");
        var y = ReverseMath<double>.Log(x) * x;
        tape.Backward(y);
    }

    [Fact]
    public void DiagnosticTree_HasRootAndInputs()
    {
        using var tape = new Tape<double>();
        var x = tape.Variable(1.0, "x");
        var y = tape.Variable(2.0, "y");
        var z = x * y + x;
        tape.Backward(z);

        var rootId = tape.NodeCount - 1;
        var diag = tape.BuildDiagnostic(rootId);
        Assert.Equal(rootId, diag.NodeId);
        Assert.NotEmpty(diag.Inputs);
    }

    [Fact]
    public void GradientNaNException_CarriesContext()
    {
        using var tape = new Tape<double> { EnableNaNGuard = true, EnableDiagnostics = true };
        var x = tape.Variable(0.0, "x");
        var y = ReverseMath<double>.Log(x) * x;
        var ex = Assert.Throws<GradientNaNException>(() => { tape.Backward(y); });
        Assert.NotNull(ex.DiagnosticTree);
    }
}
