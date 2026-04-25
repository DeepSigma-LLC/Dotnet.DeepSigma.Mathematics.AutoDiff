using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Symbolic;

/// <summary>
/// Factory methods for constructing symbolic expression trees with a concise syntax.
/// </summary>
/// <example>
/// <code>
/// var x = SymbolicFactory.Variable&lt;double&gt;("x");
/// var f = SymbolicFactory.Sin(x * x) + SymbolicFactory.Exp(x);
/// var df = SymbolicDiff.Differentiate(f, "x");
/// </code>
/// </example>
public static class SymbolicFactory
{
    /// <summary>Creates a named variable leaf node.</summary>
    /// <typeparam name="T">The scalar type for the expression tree.</typeparam>
    /// <param name="name">The variable's identifier, used to look it up in the evaluation environment.</param>
    public static VariableExpression<T> Variable<T>(string name) where T : IFloatingPoint<T> => new(name);

    /// <summary>Creates a constant leaf node with the given value.</summary>
    /// <typeparam name="T">The scalar type for the expression tree.</typeparam>
    /// <param name="value">The constant scalar value.</param>
    public static ConstantExpression<T> Constant<T>(T value) where T : IFloatingPoint<T> => new(value);

    /// <summary>Creates a sine expression: sin(<paramref name="x"/>).</summary>
    public static Expression<T> Sin<T>(Expression<T> x)
        where T : IFloatingPoint<T>, ITrigonometricFunctions<T> => new SineExpression<T>(x);

    /// <summary>Creates a cosine expression: cos(<paramref name="x"/>).</summary>
    public static Expression<T> Cos<T>(Expression<T> x)
        where T : IFloatingPoint<T>, ITrigonometricFunctions<T> => new CosineExpression<T>(x);

    /// <summary>Creates a natural exponential expression: exp(<paramref name="x"/>).</summary>
    public static Expression<T> Exp<T>(Expression<T> x)
        where T : IFloatingPoint<T>, IExponentialFunctions<T>, ILogarithmicFunctions<T>
        => new ExponentialExpression<T>(x);

    /// <summary>Creates a natural logarithm expression: log(<paramref name="x"/>).</summary>
    public static Expression<T> Log<T>(Expression<T> x)
        where T : IFloatingPoint<T>, ILogarithmicFunctions<T>, IExponentialFunctions<T>
        => new LogarithmExpression<T>(x);

    /// <summary>Creates a power expression: <paramref name="b"/>^<paramref name="e"/>.</summary>
    public static Expression<T> Pow<T>(Expression<T> b, Expression<T> e)
        where T : IFloatingPoint<T>, IPowerFunctions<T>, ILogarithmicFunctions<T>, IExponentialFunctions<T>
        => new PowerExpression<T>(b, e);
}
