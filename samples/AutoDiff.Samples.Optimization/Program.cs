using DeepSigma.Mathematics.AutoDiff;
using DeepSigma.Mathematics.AutoDiff.Reverse;

// Gradient descent on Rosenbrock: f(x,y) = (1-x)² + 100(y-x²)²
// Minimum at (1, 1), f = 0.

var x = -1.2;
var y = 1.0;
var lr = 1e-3;

for (int step = 0; step <= 5000; step++)
{
    var (dx, dy) = Demo.Grad_Rosenbrock(x, y);
    if (step % 1000 == 0)
        Console.WriteLine($"step={step,5}  x={x:F6}  y={y:F6}  f={Demo.Eval(x, y):F6}");
    x -= lr * dx;
    y -= lr * dy;
}

Console.WriteLine($"\nfinal: x={x:F6}, y={y:F6}");

public static partial class Demo
{
    public static double Eval(double x, double y)
    {
        var a = 1.0 - x;
        var b = y - x * x;
        return a * a + 100.0 * b * b;
    }

    [Differentiable]
    public static Var<double> Rosenbrock(Var<double> x, Var<double> y)
    {
        var a = 1.0 - x;
        var b = y - x * x;
        return a * a + 100.0 * b * b;
    }
}
