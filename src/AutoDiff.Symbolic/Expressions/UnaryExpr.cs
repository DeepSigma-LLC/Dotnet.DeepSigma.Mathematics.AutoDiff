using System.Numerics;

namespace AutoDiff.Symbolic;

public sealed record NegExpr<T>(Expr<T> Operand) : Expr<T> where T : IFloatingPoint<T>
{
    public override T Evaluate(IReadOnlyDictionary<string, T> env) => -Operand.Evaluate(env);
    public override Expr<T> Differentiate(string v) => new NegExpr<T>(Operand.Differentiate(v));

    public override Expr<T> Simplify()
    {
        var o = Operand.Simplify();
        if (o is ConstExpr<T> c) return new ConstExpr<T>(-c.Value);
        if (o is NegExpr<T> n) return n.Operand;                  // --x → x
        return new NegExpr<T>(o);
    }

    public override string ToString() => $"(-{Operand})";
}

public sealed record SinExpr<T>(Expr<T> Arg) : Expr<T>
    where T : IFloatingPoint<T>, ITrigonometricFunctions<T>
{
    public override T Evaluate(IReadOnlyDictionary<string, T> env) => T.Sin(Arg.Evaluate(env));
    public override Expr<T> Differentiate(string v)
        => new MulExpr<T>(new CosExpr<T>(Arg), Arg.Differentiate(v));

    public override Expr<T> Simplify()
    {
        var a = Arg.Simplify();
        if (a is ConstExpr<T> c) return new ConstExpr<T>(T.Sin(c.Value));
        return new SinExpr<T>(a);
    }

    public override string ToString() => $"sin({Arg})";
}

public sealed record CosExpr<T>(Expr<T> Arg) : Expr<T>
    where T : IFloatingPoint<T>, ITrigonometricFunctions<T>
{
    public override T Evaluate(IReadOnlyDictionary<string, T> env) => T.Cos(Arg.Evaluate(env));
    public override Expr<T> Differentiate(string v)
        => new MulExpr<T>(new NegExpr<T>(new SinExpr<T>(Arg)), Arg.Differentiate(v));

    public override Expr<T> Simplify()
    {
        var a = Arg.Simplify();
        if (a is ConstExpr<T> c) return new ConstExpr<T>(T.Cos(c.Value));
        return new CosExpr<T>(a);
    }

    public override string ToString() => $"cos({Arg})";
}

public sealed record ExpExpr<T>(Expr<T> Arg) : Expr<T>
    where T : IFloatingPoint<T>, IExponentialFunctions<T>, ILogarithmicFunctions<T>
{
    public override T Evaluate(IReadOnlyDictionary<string, T> env) => T.Exp(Arg.Evaluate(env));
    public override Expr<T> Differentiate(string v)
        => new MulExpr<T>(new ExpExpr<T>(Arg), Arg.Differentiate(v));

    public override Expr<T> Simplify()
    {
        var a = Arg.Simplify();
        if (a is ConstExpr<T> c) return new ConstExpr<T>(T.Exp(c.Value));
        if (a is LogExpr<T> log) return log.Arg;   // exp(log(x)) → x
        return new ExpExpr<T>(a);
    }

    public override string ToString() => $"exp({Arg})";
}

public sealed record LogExpr<T>(Expr<T> Arg) : Expr<T>
    where T : IFloatingPoint<T>, ILogarithmicFunctions<T>, IExponentialFunctions<T>
{
    public override T Evaluate(IReadOnlyDictionary<string, T> env) => T.Log(Arg.Evaluate(env));
    public override Expr<T> Differentiate(string v)
        => new DivExpr<T>(Arg.Differentiate(v), Arg);

    public override Expr<T> Simplify()
    {
        var a = Arg.Simplify();
        if (a is ConstExpr<T> c) return new ConstExpr<T>(T.Log(c.Value));
        if (a is ExpExpr<T> e) return e.Arg;   // log(exp(x)) → x
        return new LogExpr<T>(a);
    }

    public override string ToString() => $"log({Arg})";
}

public sealed record PowExpr<T>(Expr<T> Base, Expr<T> Exponent) : Expr<T>
    where T : IFloatingPoint<T>, IPowerFunctions<T>, ILogarithmicFunctions<T>, IExponentialFunctions<T>
{
    public override T Evaluate(IReadOnlyDictionary<string, T> env)
        => T.Pow(Base.Evaluate(env), Exponent.Evaluate(env));

    public override Expr<T> Differentiate(string v)
    {
        // d/dv[b^e] = b^e * (e' * log(b) + e * b'/b)
        var db = Base.Differentiate(v);
        var de = Exponent.Differentiate(v);
        var term1 = new MulExpr<T>(de, new LogExpr<T>(Base));
        var term2 = new MulExpr<T>(Exponent, new DivExpr<T>(db, Base));
        return new MulExpr<T>(this, new AddExpr<T>(term1, term2));
    }

    public override Expr<T> Simplify()
    {
        var b = Base.Simplify();
        var e = Exponent.Simplify();

        if (e is ConstExpr<T> ec)
        {
            if (ec.Value == T.Zero) return new ConstExpr<T>(T.One);    // x^0 → 1
            if (ec.Value == T.One) return b;                            // x^1 → x
        }
        if (b is ConstExpr<T> bc)
        {
            if (bc.Value == T.Zero) return new ConstExpr<T>(T.Zero);    // 0^x → 0
            if (bc.Value == T.One) return new ConstExpr<T>(T.One);      // 1^x → 1
            if (e is ConstExpr<T> ec2)
                return new ConstExpr<T>(T.Pow(bc.Value, ec2.Value));    // fold
        }
        return new PowExpr<T>(b, e);
    }

    public override string ToString() => $"({Base}^{Exponent})";
}
