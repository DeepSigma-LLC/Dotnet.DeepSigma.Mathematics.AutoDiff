using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Symbolic;

/// <summary>
/// A leaf expression node representing a compile-time constant scalar value.
/// Its derivative with respect to any variable is zero.
/// </summary>
/// <typeparam name="T">A floating-point scalar type that implements <see cref="IFloatingPoint{T}"/>.</typeparam>
public sealed record ConstantExpression<T>(T Value) : Expression<T> where T : IFloatingPoint<T>
{
    /// <inheritdoc/>
    public override T Evaluate(IReadOnlyDictionary<string, T> environment) => Value;

    /// <inheritdoc/>
    public override Expression<T> Differentiate(string variable) => new ConstantExpression<T>(T.Zero);

    /// <inheritdoc/>
    public override Expression<T> Simplify() => this;

    /// <inheritdoc/>
    public override string ToString() => Value.ToString() ?? "0";
}

/// <summary>
/// A leaf expression node representing a named variable.
/// Its derivative with respect to itself is 1; with respect to any other variable it is 0.
/// </summary>
/// <typeparam name="T">A floating-point scalar type that implements <see cref="IFloatingPoint{T}"/>.</typeparam>
public sealed record VariableExpression<T>(string Name) : Expression<T> where T : IFloatingPoint<T>
{
    /// <inheritdoc/>
    public override T Evaluate(IReadOnlyDictionary<string, T> environment) => environment[Name];

    /// <inheritdoc/>
    public override Expression<T> Differentiate(string variable)
        => Name == variable ? new ConstantExpression<T>(T.One) : new ConstantExpression<T>(T.Zero);

    /// <inheritdoc/>
    public override Expression<T> Simplify() => this;

    /// <inheritdoc/>
    public override string ToString() => Name;
}
