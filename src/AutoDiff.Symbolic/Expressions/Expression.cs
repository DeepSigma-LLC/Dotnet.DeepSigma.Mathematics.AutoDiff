using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Symbolic;

/// <summary>
/// Abstract base for symbolic expression trees.
/// Concrete subtypes (<see cref="AddExpression{T}"/>, <see cref="SineExpression{T}"/>, etc.)
/// use C# record semantics to provide structural equality, which drives the
/// <see cref="Simplifier"/>'s fixed-point convergence check.
/// </summary>
/// <typeparam name="T">A floating-point scalar type that implements <see cref="IFloatingPoint{T}"/>.</typeparam>
public abstract record Expression<T> where T : IFloatingPoint<T>
{
    /// <summary>Evaluates the expression by substituting variable values from <paramref name="environment"/>.</summary>
    /// <param name="environment">A mapping from variable names to their current values.</param>
    public abstract T Evaluate(IReadOnlyDictionary<string, T> environment);

    /// <summary>
    /// Returns the symbolic derivative of this expression with respect to <paramref name="variable"/>.
    /// The result is an unsimplified expression tree.
    /// </summary>
    /// <param name="variable">The name of the variable to differentiate with respect to.</param>
    public abstract Expression<T> Differentiate(string variable);

    /// <summary>
    /// Applies one pass of algebraic simplification rules (constant folding, identity elimination).
    /// Call <see cref="Simplifier.Simplify{T}(Expression{T}, int)"/> to run to a fixed point.
    /// </summary>
    public abstract Expression<T> Simplify();

    /// <summary>Addition of two expressions.</summary>
    public static Expression<T> operator +(Expression<T> a, Expression<T> b) => new AddExpression<T>(a, b);

    /// <summary>Subtraction of two expressions.</summary>
    public static Expression<T> operator -(Expression<T> a, Expression<T> b) => new SubtractExpression<T>(a, b);

    /// <summary>Multiplication of two expressions.</summary>
    public static Expression<T> operator *(Expression<T> a, Expression<T> b) => new MultiplyExpression<T>(a, b);

    /// <summary>Division of two expressions.</summary>
    public static Expression<T> operator /(Expression<T> a, Expression<T> b) => new DivideExpression<T>(a, b);

    /// <summary>Negation of an expression.</summary>
    public static Expression<T> operator -(Expression<T> a) => new NegateExpression<T>(a);

    /// <summary>Lifts a scalar constant into an expression tree.</summary>
    public static implicit operator Expression<T>(T value) => new ConstantExpression<T>(value);
}
