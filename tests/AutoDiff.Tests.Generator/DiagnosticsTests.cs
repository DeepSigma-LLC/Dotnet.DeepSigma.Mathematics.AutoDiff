namespace DeepSigma.Mathematics.AutoDiff.Tests.Generator;

public class DiagnosticsTests
{
    [Fact]
    public void AD001_NonStaticMethod()
    {
        var src = """
            using AutoDiff;
            using AutoDiff.Reverse;
            public partial class C
            {
                [Differentiable]
                public Var<double> F(Var<double> x) => x;
            }
            """;
        var d = GeneratorHarness.RunAndGetDiagnostics(src);
        Assert.Contains(d, x => x.Id == "AD001");
    }

    [Fact]
    public void AD002_ContainerNotPartial()
    {
        var src = """
            using AutoDiff;
            using AutoDiff.Reverse;
            public class C
            {
                [Differentiable]
                public static Var<double> F(Var<double> x) => x;
            }
            """;
        var d = GeneratorHarness.RunAndGetDiagnostics(src);
        Assert.Contains(d, x => x.Id == "AD002");
    }

    [Fact]
    public void AD003_NoParameters()
    {
        var src = """
            using AutoDiff;
            using AutoDiff.Reverse;
            public partial class C
            {
                [Differentiable]
                public static double F() => 0;
            }
            """;
        var d = GeneratorHarness.RunAndGetDiagnostics(src);
        Assert.Contains(d, x => x.Id == "AD003");
    }

    [Fact]
    public void AD004_UnsupportedParameterType()
    {
        var src = """
            using AutoDiff;
            public partial class C
            {
                [Differentiable]
                public static double F(double x) => x;
            }
            """;
        var d = GeneratorHarness.RunAndGetDiagnostics(src);
        Assert.Contains(d, x => x.Id == "AD004");
    }

    [Fact]
    public void AD005_MixedModes()
    {
        var src = """
            using AutoDiff;
            using AutoDiff.Reverse;
            using AutoDiff.Forward;
            public partial class C
            {
                [Differentiable]
                public static Var<double> F(Var<double> x, DualNumber<double> y) => x;
            }
            """;
        var d = GeneratorHarness.RunAndGetDiagnostics(src);
        Assert.Contains(d, x => x.Id == "AD005");
    }

    [Fact]
    public void AD006_InconsistentElementTypes()
    {
        var src = """
            using AutoDiff;
            using AutoDiff.Reverse;
            public partial class C
            {
                [Differentiable]
                public static Var<double> F(Var<double> x, Var<float> y) => x;
            }
            """;
        var d = GeneratorHarness.RunAndGetDiagnostics(src);
        Assert.Contains(d, x => x.Id == "AD006");
    }

    [Fact]
    public void AD007_RefParameter()
    {
        var src = """
            using AutoDiff;
            using AutoDiff.Reverse;
            public partial class C
            {
                [Differentiable]
                public static Var<double> F(ref Var<double> x) => x;
            }
            """;
        var d = GeneratorHarness.RunAndGetDiagnostics(src);
        Assert.Contains(d, x => x.Id == "AD007");
    }

    [Fact]
    public void AD008_ModeMismatch()
    {
        var src = """
            using DeepSigma.Mathematics.AutoDiff;
            using DeepSigma.Mathematics.AutoDiff.Reverse;
            public partial class C
            {
                [Differentiable(Mode = DiffMode.Forward)]
                public static Var<double> F(Var<double> x) => x;
            }
            """;
        var d = GeneratorHarness.RunAndGetDiagnostics(src);
        Assert.Contains(d, x => x.Id == "AD008");
    }

    [Fact]
    public void Valid_NoDiagnostics()
    {
        var src = """
            using DeepSigma.Mathematics.AutoDiff;
            using DeepSigma.Mathematics.AutoDiff.Reverse;
            public partial class C
            {
                [Differentiable]
                public static Var<double> F(Var<double> x, Var<double> y) => x * y;
            }
            """;
        var d = GeneratorHarness.RunAndGetDiagnostics(src);
        Assert.Empty(d.Where(x => x.Id.StartsWith("AD")));
    }
}
