using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Symbolic;

/// <summary>Represents the arithmetic negation of a sub-expression: −Operand.</summary>
public sealed record NegateExpression<T>(Expression<T> Operand) : Expression<T> where T : IFloatingPoint<T>
{
    /// <inheritdoc/>
    public override T Evaluate(IReadOnlyDictionary<string, T> environment) => -Operand.Evaluate(environment);

    /// <inheritdoc/>
    public override Expression<T> Differentiate(string v) => new NegateExpression<T>(Operand.Differentiate(v));

    /// <inheritdoc/>
    public override Expression<T> Simplify()
    {
        var o = Operand.Simplify();
        if (o is ConstantExpression<T> c) return new ConstantExpression<T>(-c.Value);
        if (o is NegateExpression<T> n) return n.Operand; // --x → x
        return new NegateExpression<T>(o);
    }

    /// <inheritdoc/>
    public override string ToString() => $"(-{Operand})";
}

/// <summary>Represents the sine function applied to a sub-expression: sin(Arg).</summary>
public sealed record SineExpression<T>(Expression<T> Arg) : Expression<T>
    where T : IFloatingPoint<T>, ITrigonometricFunctions<T>
{
    /// <inheritdoc/>
    public override T Evaluate(IReadOnlyDictionary<string, T> environment) => T.Sin(Arg.Evaluate(environment));

    /// <inheritdoc/>
    public override Expression<T> Differentiate(string v)
        => new MultiplyExpression<T>(new CosineExpression<T>(Arg), Arg.Differentiate(v));

    /// <inheritdoc/>
    public override Expression<T> Simplify()
    {
        var a = Arg.Simplify();
        if (a is ConstantExpression<T> c) return new ConstantExpression<T>(T.Sin(c.Value));
        return new SineExpression<T>(a);
    }

    /// <inheritdoc/>
    public override string ToString() => $"sin({Arg})";
}

/// <summary>Represents the cosine function applied to a sub-expression: cos(Arg).</summary>
public sealed record CosineExpression<T>(Expression<T> Arg) : Expression<T>
    where T : IFloatingPoint<T>, ITrigonometricFunctions<T>
{
    /// <inheritdoc/>
    public override T Evaluate(IReadOnlyDictionary<string, T> environment) => T.Cos(Arg.Evaluate(environment));

    /// <inheritdoc/>
    public override Expression<T> Differentiate(string v)
        => new MultiplyExpression<T>(new NegateExpression<T>(new SineExpression<T>(Arg)), Arg.Differentiate(v));

    /// <inheritdoc/>
    public override Expression<T> Simplify()
    {
        var a = Arg.Simplify();
        if (a is ConstantExpression<T> c) return new ConstantExpression<T>(T.Cos(c.Value));
        return new CosineExpression<T>(a);
    }

    /// <inheritdoc/>
    public override string ToString() => $"cos({Arg})";
}

/// <summary>Represents the natural exponential function applied to a sub-expression: exp(Arg).</summary>
public sealed record ExponentialExpression<T>(Expression<T> Arg) : Expression<T>
    where T : IFloatingPoint<T>, IExponentialFunctions<T>, ILogarithmicFunctions<T>
{
    /// <inheritdoc/>
    public override T Evaluate(IReadOnlyDictionary<string, T> environment) => T.Exp(Arg.Evaluate(environment));

    /// <inheritdoc/>
    public override Expression<T> Differentiate(string v)
        => new MultiplyExpression<T>(new ExponentialExpression<T>(Arg), Arg.Differentiate(v));

    /// <inheritdoc/>
    public override Expression<T> Simplify()
    {
        var a = Arg.Simplify();
        if (a is ConstantExpression<T> c) return new ConstantExpression<T>(T.Exp(c.Value));
        if (a is LogarithmExpression<T> log) return log.Arg; // exp(log(x)) → x
        return new ExponentialExpression<T>(a);
    }

    /// <inheritdoc/>
    public override string ToString() => $"exp({Arg})";
}

/// <summary>Represents the natural logarithm function applied to a sub-expression: log(Arg).</summary>
public sealed record LogarithmExpression<T>(Expression<T> Arg) : Expression<T>
    where T : IFloatingPoint<T>, ILogarithmicFunctions<T>, IExponentialFunctions<T>
{
    /// <inheritdoc/>
    public override T Evaluate(IReadOnlyDictionary<string, T> environment) => T.Log(Arg.Evaluate(environment));

    /// <inheritdoc/>
    public override Expression<T> Differentiate(string v)
        => new DivideExpression<T>(Arg.Differentiate(v), Arg);

    /// <inheritdoc/>
    public override Expression<T> Simplify()
    {
        var a = Arg.Simplify();
        if (a is ConstantExpression<T> c) return new ConstantExpression<T>(T.Log(c.Value));
        if (a is ExponentialExpression<T> e) return e.Arg; // log(exp(x)) → x
        return new LogarithmExpression<T>(a);
    }

    /// <inheritdoc/>
    public override string ToString() => $"log({Arg})";
}

/// <summary>
/// Represents exponentiation with both base and exponent as sub-expressions: Base ^ Exponent.
/// </summary>
public sealed record PowerExpression<T>(Expression<T> Base, Expression<T> Exponent) : Expression<T>
    where T : IFloatingPoint<T>, IPowerFunctions<T>, ILogarithmicFunctions<T>, IExponentialFunctions<T>
{
    /// <inheritdoc/>
    public override T Evaluate(IReadOnlyDictionary<string, T> environment)
        => T.Pow(Base.Evaluate(environment), Exponent.Evaluate(environment));

    /// <inheritdoc/>
    public override Expression<T> Differentiate(string v)
    {
        // d/dv[b^e] = b^e * (e' * log(b) + e * b'/b)
        var db = Base.Differentiate(v);
        var de = Exponent.Differentiate(v);
        var term1 = new MultiplyExpression<T>(de, new LogarithmExpression<T>(Base));
        var term2 = new MultiplyExpression<T>(Exponent, new DivideExpression<T>(db, Base));
        return new MultiplyExpression<T>(this, new AddExpression<T>(term1, term2));
    }

    /// <inheritdoc/>
    public override Expression<T> Simplify()
    {
        var b = Base.Simplify();
        var e = Exponent.Simplify();

        if (e is ConstantExpression<T> ec)
        {
            if (ec.Value == T.Zero) return new ConstantExpression<T>(T.One);
            if (ec.Value == T.One) return b;
        }
        if (b is ConstantExpression<T> bc)
        {
            if (bc.Value == T.Zero) return new ConstantExpression<T>(T.Zero);
            if (bc.Value == T.One) return new ConstantExpression<T>(T.One);
            if (e is ConstantExpression<T> ec2)
                return new ConstantExpression<T>(T.Pow(bc.Value, ec2.Value));
        }
        return new PowerExpression<T>(b, e);
    }

    /// <inheritdoc/>
    public override string ToString() => $"({Base}^{Exponent})";
}
