using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Symbolic;

/// <summary>Represents the sum of two sub-expressions: Left + Right.</summary>
public sealed record AddExpression<T>(Expression<T> Left, Expression<T> Right) : Expression<T> where T : IFloatingPoint<T>
{
    /// <inheritdoc/>
    public override T Evaluate(IReadOnlyDictionary<string, T> environment)
        => Left.Evaluate(environment) + Right.Evaluate(environment);

    /// <inheritdoc/>
    public override Expression<T> Differentiate(string v)
        => new AddExpression<T>(Left.Differentiate(v), Right.Differentiate(v));

    /// <inheritdoc/>
    public override Expression<T> Simplify()
    {
        var l = Left.Simplify();
        var r = Right.Simplify();

        if (l is ConstantExpression<T> { Value: var lv } && lv == T.Zero) return r;
        if (r is ConstantExpression<T> { Value: var rv } && rv == T.Zero) return l;
        if (l is ConstantExpression<T> lc && r is ConstantExpression<T> rc)
            return new ConstantExpression<T>(lc.Value + rc.Value);

        return new AddExpression<T>(l, r);
    }

    /// <inheritdoc/>
    public override string ToString() => $"({Left} + {Right})";
}

/// <summary>Represents the difference of two sub-expressions: Left − Right.</summary>
public sealed record SubtractExpression<T>(Expression<T> Left, Expression<T> Right) : Expression<T> where T : IFloatingPoint<T>
{
    /// <inheritdoc/>
    public override T Evaluate(IReadOnlyDictionary<string, T> environment)
        => Left.Evaluate(environment) - Right.Evaluate(environment);

    /// <inheritdoc/>
    public override Expression<T> Differentiate(string v)
        => new SubtractExpression<T>(Left.Differentiate(v), Right.Differentiate(v));

    /// <inheritdoc/>
    public override Expression<T> Simplify()
    {
        var l = Left.Simplify();
        var r = Right.Simplify();

        if (r is ConstantExpression<T> { Value: var rv } && rv == T.Zero) return l;
        if (l is ConstantExpression<T> { Value: var lv } && lv == T.Zero)
            return new NegateExpression<T>(r).Simplify();
        if (l is ConstantExpression<T> lc && r is ConstantExpression<T> rc)
            return new ConstantExpression<T>(lc.Value - rc.Value);
        if (l.Equals(r)) return new ConstantExpression<T>(T.Zero);

        return new SubtractExpression<T>(l, r);
    }

    /// <inheritdoc/>
    public override string ToString() => $"({Left} - {Right})";
}

/// <summary>Represents the product of two sub-expressions: Left × Right.</summary>
public sealed record MultiplyExpression<T>(Expression<T> Left, Expression<T> Right) : Expression<T> where T : IFloatingPoint<T>
{
    /// <inheritdoc/>
    public override T Evaluate(IReadOnlyDictionary<string, T> environment)
        => Left.Evaluate(environment) * Right.Evaluate(environment);

    /// <inheritdoc/>
    public override Expression<T> Differentiate(string v)
        => new AddExpression<T>(
            new MultiplyExpression<T>(Left.Differentiate(v), Right),
            new MultiplyExpression<T>(Left, Right.Differentiate(v)));

    /// <inheritdoc/>
    public override Expression<T> Simplify()
    {
        var l = Left.Simplify();
        var r = Right.Simplify();

        if (l is ConstantExpression<T> { Value: var lv })
        {
            if (lv == T.Zero) return new ConstantExpression<T>(T.Zero);
            if (lv == T.One) return r;
            if (lv == -T.One) return new NegateExpression<T>(r).Simplify();
        }
        if (r is ConstantExpression<T> { Value: var rv })
        {
            if (rv == T.Zero) return new ConstantExpression<T>(T.Zero);
            if (rv == T.One) return l;
            if (rv == -T.One) return new NegateExpression<T>(l).Simplify();
        }
        if (l is ConstantExpression<T> lc && r is ConstantExpression<T> rc)
            return new ConstantExpression<T>(lc.Value * rc.Value);

        return new MultiplyExpression<T>(l, r);
    }

    /// <inheritdoc/>
    public override string ToString() => $"({Left} * {Right})";
}

/// <summary>Represents the quotient of two sub-expressions: Numerator ÷ Denominator.</summary>
public sealed record DivideExpression<T>(Expression<T> Numerator, Expression<T> Denominator) : Expression<T> where T : IFloatingPoint<T>
{
    /// <inheritdoc/>
    public override T Evaluate(IReadOnlyDictionary<string, T> environment)
        => Numerator.Evaluate(environment) / Denominator.Evaluate(environment);

    /// <inheritdoc/>
    public override Expression<T> Differentiate(string v)
    {
        // d/dv[n/d] = (n'd - nd') / d²
        var dn = Numerator.Differentiate(v);
        var dd = Denominator.Differentiate(v);
        var numerator = new SubtractExpression<T>(
            new MultiplyExpression<T>(dn, Denominator),
            new MultiplyExpression<T>(Numerator, dd));
        var denominator = new MultiplyExpression<T>(Denominator, Denominator);
        return new DivideExpression<T>(numerator, denominator);
    }

    /// <inheritdoc/>
    public override Expression<T> Simplify()
    {
        var n = Numerator.Simplify();
        var d = Denominator.Simplify();

        if (n is ConstantExpression<T> { Value: var nv } && nv == T.Zero)
            return new ConstantExpression<T>(T.Zero);
        if (d is ConstantExpression<T> { Value: var dv } && dv == T.One) return n;
        if (n is ConstantExpression<T> nc && d is ConstantExpression<T> dc)
            return new ConstantExpression<T>(nc.Value / dc.Value);
        if (n.Equals(d)) return new ConstantExpression<T>(T.One);

        return new DivideExpression<T>(n, d);
    }

    /// <inheritdoc/>
    public override string ToString() => $"({Numerator} / {Denominator})";
}
