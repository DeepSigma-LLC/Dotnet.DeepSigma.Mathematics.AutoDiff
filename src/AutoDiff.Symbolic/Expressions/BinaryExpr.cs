using System.Numerics;

namespace DeepSigma.Mathematics.AutoDiff.Symbolic;

public sealed record AddExpr<T>(Expr<T> Left, Expr<T> Right) : Expr<T> where T : IFloatingPoint<T>
{
    public override T Evaluate(IReadOnlyDictionary<string, T> env)
        => Left.Evaluate(env) + Right.Evaluate(env);

    public override Expr<T> Differentiate(string v)
        => new AddExpr<T>(Left.Differentiate(v), Right.Differentiate(v));

    public override Expr<T> Simplify()
    {
        var l = Left.Simplify();
        var r = Right.Simplify();

        if (l is ConstExpr<T> { Value: var lv } && lv == T.Zero) return r;       // 0+x → x
        if (r is ConstExpr<T> { Value: var rv } && rv == T.Zero) return l;       // x+0 → x
        if (l is ConstExpr<T> lc && r is ConstExpr<T> rc)
            return new ConstExpr<T>(lc.Value + rc.Value);                         // fold

        return new AddExpr<T>(l, r);
    }

    public override string ToString() => $"({Left} + {Right})";
}

public sealed record SubExpr<T>(Expr<T> Left, Expr<T> Right) : Expr<T> where T : IFloatingPoint<T>
{
    public override T Evaluate(IReadOnlyDictionary<string, T> env)
        => Left.Evaluate(env) - Right.Evaluate(env);

    public override Expr<T> Differentiate(string v)
        => new SubExpr<T>(Left.Differentiate(v), Right.Differentiate(v));

    public override Expr<T> Simplify()
    {
        var l = Left.Simplify();
        var r = Right.Simplify();

        if (r is ConstExpr<T> { Value: var rv } && rv == T.Zero) return l;       // x-0 → x
        if (l is ConstExpr<T> { Value: var lv } && lv == T.Zero)
            return new NegExpr<T>(r).Simplify();                                  // 0-x → -x
        if (l is ConstExpr<T> lc && r is ConstExpr<T> rc)
            return new ConstExpr<T>(lc.Value - rc.Value);                         // fold
        if (l.Equals(r)) return new ConstExpr<T>(T.Zero);                         // x-x → 0

        return new SubExpr<T>(l, r);
    }

    public override string ToString() => $"({Left} - {Right})";
}

public sealed record MulExpr<T>(Expr<T> Left, Expr<T> Right) : Expr<T> where T : IFloatingPoint<T>
{
    public override T Evaluate(IReadOnlyDictionary<string, T> env)
        => Left.Evaluate(env) * Right.Evaluate(env);

    public override Expr<T> Differentiate(string v)
        => new AddExpr<T>(
            new MulExpr<T>(Left.Differentiate(v), Right),
            new MulExpr<T>(Left, Right.Differentiate(v)));

    public override Expr<T> Simplify()
    {
        var l = Left.Simplify();
        var r = Right.Simplify();

        if (l is ConstExpr<T> { Value: var lv })
        {
            if (lv == T.Zero) return new ConstExpr<T>(T.Zero);                    // 0*x → 0
            if (lv == T.One) return r;                                            // 1*x → x
            if (lv == -T.One) return new NegExpr<T>(r).Simplify();                // -1*x → -x
        }
        if (r is ConstExpr<T> { Value: var rv })
        {
            if (rv == T.Zero) return new ConstExpr<T>(T.Zero);                    // x*0 → 0
            if (rv == T.One) return l;                                            // x*1 → x
            if (rv == -T.One) return new NegExpr<T>(l).Simplify();                // x*-1 → -x
        }
        if (l is ConstExpr<T> lc && r is ConstExpr<T> rc)
            return new ConstExpr<T>(lc.Value * rc.Value);                         // fold

        return new MulExpr<T>(l, r);
    }

    public override string ToString() => $"({Left} * {Right})";
}

public sealed record DivExpr<T>(Expr<T> Numerator, Expr<T> Denominator) : Expr<T> where T : IFloatingPoint<T>
{
    public override T Evaluate(IReadOnlyDictionary<string, T> env)
        => Numerator.Evaluate(env) / Denominator.Evaluate(env);

    public override Expr<T> Differentiate(string v)
    {
        // d/dv[n/d] = (n'd - nd') / d²
        var dn = Numerator.Differentiate(v);
        var dd = Denominator.Differentiate(v);
        var num = new SubExpr<T>(
            new MulExpr<T>(dn, Denominator),
            new MulExpr<T>(Numerator, dd));
        var den = new MulExpr<T>(Denominator, Denominator);
        return new DivExpr<T>(num, den);
    }

    public override Expr<T> Simplify()
    {
        var n = Numerator.Simplify();
        var d = Denominator.Simplify();

        if (n is ConstExpr<T> { Value: var nv } && nv == T.Zero)
            return new ConstExpr<T>(T.Zero);                                      // 0/x → 0
        if (d is ConstExpr<T> { Value: var dv } && dv == T.One) return n;         // x/1 → x
        if (n is ConstExpr<T> nc && d is ConstExpr<T> dc)
            return new ConstExpr<T>(nc.Value / dc.Value);                         // fold
        if (n.Equals(d)) return new ConstExpr<T>(T.One);                          // x/x → 1

        return new DivExpr<T>(n, d);
    }

    public override string ToString() => $"({Numerator} / {Denominator})";
}
