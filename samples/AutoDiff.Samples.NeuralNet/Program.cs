using DeepSigma.Mathematics.AutoDiff.Reverse;

// Tiny MLP learning XOR: 2 → 4 → 1 with tanh hidden, sigmoid output, MSE loss.

double[][] X = [[0, 0], [0, 1], [1, 0], [1, 1]];
double[] Y = [0, 1, 1, 0];

var rng = new Random(42);
double[] W1 = new double[8], b1 = new double[4];
double[] W2 = new double[4], b2Arr = new double[1];
for (int i = 0; i < W1.Length; i++) W1[i] = rng.NextDouble() * 2 - 1;
for (int i = 0; i < W2.Length; i++) W2[i] = rng.NextDouble() * 2 - 1;

double lr = 0.1;
int epochs = 4000;

for (int epoch = 0; epoch <= epochs; epoch++)
{
    double totalLoss = 0;
    var dW1 = new double[W1.Length];
    var db1 = new double[b1.Length];
    var dW2 = new double[W2.Length];
    var db2 = new double[b2Arr.Length];

    for (int s = 0; s < X.Length; s++)
    {
        using var tape = TapePool<double>.Rent(64);
        var pW1 = Wrap(tape, W1);
        var pb1 = Wrap(tape, b1);
        var pW2 = Wrap(tape, W2);
        var pb2 = Wrap(tape, b2Arr);

        var x0 = tape.Variable(X[s][0]);
        var x1 = tape.Variable(X[s][1]);

        var h = new Var<double>[4];
        for (int j = 0; j < 4; j++)
        {
            var z = pW1[j * 2] * x0 + pW1[j * 2 + 1] * x1 + pb1[j];
            h[j] = ReverseMath<double>.Tanh(z);
        }

        var o = pb2[0];
        for (int j = 0; j < 4; j++) o = o + pW2[j] * h[j];
        var sig = 1.0 / (1.0 + ReverseMath<double>.Exp(-o));
        var diff = sig - tape.Variable(Y[s]);
        var loss = diff * diff;

        tape.Backward(loss);
        totalLoss += loss.Value;

        for (int i = 0; i < W1.Length; i++) dW1[i] += pW1[i].Gradient;
        for (int i = 0; i < b1.Length; i++) db1[i] += pb1[i].Gradient;
        for (int i = 0; i < W2.Length; i++) dW2[i] += pW2[i].Gradient;
        for (int i = 0; i < b2Arr.Length; i++) db2[i] += pb2[i].Gradient;
    }

    for (int i = 0; i < W1.Length; i++) W1[i] -= lr * dW1[i] / X.Length;
    for (int i = 0; i < b1.Length; i++) b1[i] -= lr * db1[i] / X.Length;
    for (int i = 0; i < W2.Length; i++) W2[i] -= lr * dW2[i] / X.Length;
    for (int i = 0; i < b2Arr.Length; i++) b2Arr[i] -= lr * db2[i] / X.Length;

    if (epoch % 500 == 0)
        Console.WriteLine($"epoch {epoch,4}  loss={totalLoss / X.Length:F6}");
}

Console.WriteLine("\nXOR predictions:");
for (int s = 0; s < X.Length; s++)
    Console.WriteLine($"  {X[s][0]} XOR {X[s][1]} = {Predict(X[s][0], X[s][1]):F4} (target {Y[s]})");

double Predict(double a, double b)
{
    double[] h = new double[4];
    for (int j = 0; j < 4; j++)
        h[j] = Math.Tanh(W1[j * 2] * a + W1[j * 2 + 1] * b + b1[j]);
    double o = b2Arr[0];
    for (int j = 0; j < 4; j++) o += W2[j] * h[j];
    return 1.0 / (1.0 + Math.Exp(-o));
}

static Var<double>[] Wrap(Tape<double> tape, double[] vals)
{
    var arr = new Var<double>[vals.Length];
    for (int i = 0; i < vals.Length; i++) arr[i] = tape.Variable(vals[i]);
    return arr;
}
