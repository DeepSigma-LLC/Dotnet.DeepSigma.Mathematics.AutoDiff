namespace AutoDiff.Tests.Unit;

/// <summary>Numerical gradient checker via central finite differences.</summary>
internal static class FiniteDiff
{
    public static double[] Gradient(Func<double[], double> f, double[] x, double h = 1e-5)
    {
        var grad = new double[x.Length];
        var xp = (double[])x.Clone();
        var xm = (double[])x.Clone();

        for (int i = 0; i < x.Length; i++)
        {
            xp[i] = x[i] + h;
            xm[i] = x[i] - h;
            grad[i] = (f(xp) - f(xm)) / (2 * h);
            xp[i] = x[i];
            xm[i] = x[i];
        }

        return grad;
    }

    public static double Derivative(Func<double, double> f, double x, double h = 1e-5)
        => (f(x + h) - f(x - h)) / (2 * h);
}
