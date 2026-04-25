using Microsoft.CodeAnalysis;

namespace DeepSigma.Mathematics.AutoDiff.Generator;

/// <summary>
/// Diagnostic descriptors for the <c>[Differentiable]</c> source generator.
/// Each descriptor corresponds to a numbered rule (<c>AD001</c>–<c>AD008</c>) that is
/// reported as a compile-time error when the annotated method violates a generator constraint.
/// </summary>
internal static class Diagnostics
{
    private const string Category = "AutoDiff";

    /// <summary>
    /// AD001 — The annotated method must be <c>static</c>.
    /// Generated gradient wrappers call the original method as a static method and cannot
    /// capture an instance.
    /// </summary>
    public static readonly DiagnosticDescriptor MustBeStatic = new(
        id: "AD001",
        title: "[Differentiable] method must be static",
        messageFormat: "Method '{0}' must be static — generated wrappers call it as a static method",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// AD002 — The type containing the annotated method must be declared <c>partial</c>.
    /// The generator emits the <c>Grad_Foo</c> wrapper into a second partial declaration of
    /// the same type and requires the <c>partial</c> keyword to be present on the original.
    /// </summary>
    public static readonly DiagnosticDescriptor ContainerMustBePartial = new(
        id: "AD002",
        title: "[Differentiable] containing type must be partial",
        messageFormat: "Type '{0}' must be declared 'partial' so the generator can emit Grad_{1} alongside it",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// AD003 — The annotated method must have at least one parameter.
    /// A parameterless method has no inputs to differentiate against.
    /// </summary>
    public static readonly DiagnosticDescriptor MustHaveParameters = new(
        id: "AD003",
        title: "[Differentiable] method requires at least one parameter",
        messageFormat: "Method '{0}' has no parameters — there is nothing to differentiate against",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// AD004 — Each parameter must be <c>Var&lt;T&gt;</c> (reverse mode) or
    /// <c>DualNumber&lt;T&gt;</c> (forward mode).
    /// Other types cannot be differentiated by the generator.
    /// </summary>
    public static readonly DiagnosticDescriptor UnsupportedParameterType = new(
        id: "AD004",
        title: "[Differentiable] parameter type is not supported",
        messageFormat: "Parameter '{0}' on '{1}' must be 'Var<T>' (reverse mode) or 'DualNumber<T>' (forward mode); got '{2}'",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// AD005 — All parameters must use the same differentiation mode.
    /// Mixing <c>Var&lt;T&gt;</c> and <c>DualNumber&lt;T&gt;</c> in one method is not supported.
    /// </summary>
    public static readonly DiagnosticDescriptor MixedParameterModes = new(
        id: "AD005",
        title: "[Differentiable] parameters mix Var<T> and DualNumber<T>",
        messageFormat: "Method '{0}' mixes Var<T> and DualNumber<T> parameters; all parameters must use the same form",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// AD006 — All parameters must share the same element type <c>T</c>.
    /// Mixing e.g. <c>Var&lt;double&gt;</c> and <c>Var&lt;float&gt;</c> is not supported.
    /// </summary>
    public static readonly DiagnosticDescriptor InconsistentElementType = new(
        id: "AD006",
        title: "[Differentiable] parameters use different element types",
        messageFormat: "Method '{0}' uses inconsistent element types ('{1}' and '{2}'); all parameters must share the same T",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// AD007 — Parameters must be passed by value.
    /// <c>ref</c>, <c>out</c>, and <c>in</c> modifiers are not supported.
    /// </summary>
    public static readonly DiagnosticDescriptor RefOrOutNotSupported = new(
        id: "AD007",
        title: "[Differentiable] does not support ref/out/in parameters",
        messageFormat: "Parameter '{0}' on '{1}' uses 'ref', 'out', or 'in'; only by-value parameters are supported",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// AD008 — The <c>Mode</c> property on the <c>[Differentiable]</c> attribute contradicts
    /// the parameter types inferred from the method signature.
    /// Either change <c>Mode</c> to match the parameter types or update the parameters.
    /// </summary>
    public static readonly DiagnosticDescriptor ModeMismatch = new(
        id: "AD008",
        title: "[Differentiable] Mode does not match parameter type",
        messageFormat: "Method '{0}' is annotated as '{1}' but parameters are '{2}'; either change Mode or change the parameter type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
