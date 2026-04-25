using System.Collections.Immutable;
using System.Reflection;
using DeepSigma.Mathematics.AutoDiff.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DeepSigma.Mathematics.AutoDiff.Tests.Generator;

internal static class GeneratorHarness
{
    public static ImmutableArray<Diagnostic> RunAndGetDiagnostics(string source)
    {
        // Touch the assemblies we depend on so they're guaranteed to be loaded.
        _ = typeof(DeepSigma.Mathematics.AutoDiff.Reverse.ComputationTape<double>).FullName;
        _ = typeof(DeepSigma.Mathematics.AutoDiff.Forward.DualNumber<double>).FullName;

        var refs = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            new[] { CSharpSyntaxTree.ParseText(source) },
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var driver = CSharpGeneratorDriver.Create(new DifferentiableGenerator())
            .RunGeneratorsAndUpdateCompilation(compilation, out _, out var generatorDiagnostics);

        return generatorDiagnostics;
    }
}
