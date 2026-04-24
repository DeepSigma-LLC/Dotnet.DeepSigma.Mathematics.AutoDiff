using Microsoft.CodeAnalysis;

namespace AutoDiff.Generator;

internal static class Diagnostics
{
    private const string Category = "AutoDiff";

    public static readonly DiagnosticDescriptor MustBeStatic = new(
        id: "AD001",
        title: "[Differentiable] method must be static",
        messageFormat: "Method '{0}' must be static — generated wrappers call it as a static method",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ContainerMustBePartial = new(
        id: "AD002",
        title: "[Differentiable] containing type must be partial",
        messageFormat: "Type '{0}' must be declared 'partial' so the generator can emit Grad_{1} alongside it",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MustHaveParameters = new(
        id: "AD003",
        title: "[Differentiable] method requires at least one parameter",
        messageFormat: "Method '{0}' has no parameters — there is nothing to differentiate against",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnsupportedParameterType = new(
        id: "AD004",
        title: "[Differentiable] parameter type is not supported",
        messageFormat: "Parameter '{0}' on '{1}' must be 'Var<T>' (reverse mode) or 'DualNumber<T>' (forward mode); got '{2}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MixedParameterModes = new(
        id: "AD005",
        title: "[Differentiable] parameters mix Var<T> and DualNumber<T>",
        messageFormat: "Method '{0}' mixes Var<T> and DualNumber<T> parameters; all parameters must use the same form",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InconsistentElementType = new(
        id: "AD006",
        title: "[Differentiable] parameters use different element types",
        messageFormat: "Method '{0}' uses inconsistent element types ('{1}' and '{2}'); all parameters must share the same T",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor RefOrOutNotSupported = new(
        id: "AD007",
        title: "[Differentiable] does not support ref/out/in parameters",
        messageFormat: "Parameter '{0}' on '{1}' uses 'ref', 'out', or 'in'; only by-value parameters are supported",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ModeMismatch = new(
        id: "AD008",
        title: "[Differentiable] Mode does not match parameter type",
        messageFormat: "Method '{0}' is annotated as '{1}' but parameters are '{2}'; either change Mode or change the parameter type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
