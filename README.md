# AutoDiff

A streamlined automatic differentiation library for .NET 10 — forward mode, reverse mode, symbolic differentiation, and a `[Differentiable]` source generator. Native AOT compatible, zero runtime dependencies, generic over `IFloatingPoint<T>`.

> **Status:** active development. APIs are stabilizing toward a 1.0.

---

## Why

Gradient-based optimization, neural-net experimentation, and scientific computing in .NET have historically meant pulling in a Python interop layer or a heavyweight framework. AutoDiff is intentionally small: a flat-array tape, a dual-number struct, and a Roslyn generator that wires them up. No reflection, no dynamic codegen on the hot path, no external packages.

## Features

- **Forward mode** — `DualNumber<T>` for cheap directional derivatives and Jacobians of low-input/high-output functions.
- **Reverse mode** — flat-array `Tape<T>` with cache-coherent backward sweep; ideal for many-input scalar functions (loss gradients, MLPs).
- **Symbolic** — `Expr<T>` tree with algebraic simplifier, `SymbolicDiff.Differentiate`, and an AOT-safe interpreter.
- **JVP / VJP primitives** — Jacobian-vector and vector-Jacobian products as first-class operations.
- **Implicit differentiation** — `dy/dx` from a constraint `F(x, y) = 0` via the implicit function theorem.
- **Higher-order** — `HyperDual<T>` for second derivatives in a single forward pass.
- **NaN / Inf guards** — opt-in detection during the backward sweep, with optional diagnostic trees pinpointing the originating node.
- **Source generator** — `[Differentiable]` emits a `Grad_*` companion taking plain `T` parameters, with full diagnostic feedback (8 analyzer rules).
- **Native AOT compatible** — `IsAotCompatible=true` across all libraries; analyzers run clean.
- **Generic math** — works with any `IFloatingPoint<T>` (`double`, `float`, `Half`, ...).

## Performance

Measured on an 11th Gen Core i7-11800H, .NET 10 RyuJIT (BenchmarkDotNet, short job).

| Benchmark | Result |
|---|---|
| Reverse vs Forward, Σxᵢ², N=1000 | **105×** faster reverse |
| Reverse vs Forward, Σxᵢ², N=100 | **17.6×** faster reverse |
| `TapePool` rent/return vs fresh allocation | **52×** faster, ~15× less allocation |
| MLP forward+backward, 16-64-64-1 (~4400 params) | 111 µs / 85 KB / op |

Run them yourself: `dotnet run -c Release --project tests/AutoDiff.Tests.Benchmarks -- --filter '*'`.

---

## Installation

The library is split into focused packages so embedded / real-time consumers can pull only what they need.

| Package | Contents |
|---|---|
| `DeepSigma.Mathematics.AutoDiff.Core` | Interfaces, generic-math helpers, NaN/Inf guards, diagnostics |
| `DeepSigma.Mathematics.AutoDiff.Forward` | `DualNumber<T>`, `DualMath<T>`, `ForwardDiff<T>`, `HyperDual<T>` |
| `DeepSigma.Mathematics.AutoDiff.Reverse` | `Tape<T>`, `Var<T>`, `ReverseMath<T>`, `TapePool<T>` |
| `DeepSigma.Mathematics.AutoDiff.Symbolic` | `Expr<T>` tree, `Simplifier`, `SymbolicDiff` |
| `DeepSigma.Mathematics.AutoDiff.JVP` | `JVP.Compute`, `VJP.Compute`, `VJP.Jacobian` |
| `DeepSigma.Mathematics.AutoDiff.Implicit` | `ImplicitDiff.Derivative`, `Gradient`, `DerivativeSymbolic` |
| `DeepSigma.Mathematics.AutoDiff.Generator` | `[Differentiable]` source generator (analyzer; dev-time only) |

> **Note:** NuGet packages are not yet published — see [Roadmap](#roadmap). To use locally, clone the repo and add project references:
> ```bash
> dotnet add reference src/AutoDiff.Reverse/DeepSigma.Mathematics.AutoDiff.Reverse.csproj
> ```

---

## Getting started

### Forward mode — single-input derivative

```csharp
using DeepSigma.Mathematics.AutoDiff.Forward;

// d/dx [sin(x²) + x³] at x = 1.5
var d = ForwardDiff<double>.Derivative(
    x => DualMath<double>.Sin(x * x) + x * x * x,
    1.5);
```

### Forward mode — gradient

```csharp
// f(x, y) = x²·y + exp(y)
var grad = ForwardDiff<double>.Gradient(
    v => v[0] * v[0] * v[1] + DualMath<double>.Exp(v[1]),
    new[] { 1.5, 0.3 });
// grad[0] = 2xy, grad[1] = x² + exp(y)
```

### Reverse mode — many-input scalar function

For loss functions and any case where input dim ≫ output dim, reverse mode wins decisively (see the benchmark above).

```csharp
using DeepSigma.Mathematics.AutoDiff.Reverse;

using var tape = TapePool<double>.Rent();
var x = tape.Variable(2.0, "x");
var y = tape.Variable(3.0, "y");

var z = x * x + ReverseMath<double>.Sin(x * y);
tape.Backward(z);

// x.Gradient = 2x + y·cos(x·y)
// y.Gradient = x·cos(x·y)
```

### Reverse mode — convenience wrapper

```csharp
var grad = ReverseDiff<double>.Gradient(
    v => v[0] * v[0] + v[1] * v[1] * v[1],   // x² + y³
    new[] { 1.0, 2.0 });
// grad = [2, 12]
```

### Source generator — `[Differentiable]`

Annotate a method that takes `Var<T>` (reverse) or `DualNumber<T>` (forward) and the generator emits a sibling `Grad_*` method taking plain `T` parameters. The `Grad_` prefix keeps the original (typed) method and the gradient entrypoint visually distinct at every call site:

```csharp
using DeepSigma.Mathematics.AutoDiff;
using DeepSigma.Mathematics.AutoDiff.Reverse;

public static partial class Demo
{
    [Differentiable]
    public static Var<double> Rosenbrock(Var<double> x, Var<double> y)
    {
        var a = 1.0 - x;
        var b = y - x * x;
        return a * a + 100.0 * b * b;
    }
}

// Usage — Grad_Rosenbrock is generated at compile time.
var (dx, dy) = Demo.Grad_Rosenbrock(-1.2, 1.0);
```

Forward mode works the same way:

```csharp
[Differentiable(Mode = DiffMode.Forward)]
public static DualNumber<double> Cubic(DualNumber<double> x) => x * x * x;

// Usage — single-arg returns the scalar derivative directly.
double dx = Demo.Grad_Cubic(2.0);   // 12
```

The generator reports clear diagnostics for misuse: missing `partial`, non-static methods, unsupported parameter types, mixed `Var`/`DualNumber`, inconsistent element types, `ref`/`out`/`in`, or a `Mode` that contradicts the parameter shape (rules `AD001`–`AD008`).

### Symbolic differentiation

```csharp
using DeepSigma.Mathematics.AutoDiff.Symbolic;

var x = Sym.Var<double>("x");
var f = Sym.Sin(x * x) + Sym.Exp(x);

var df = SymbolicDiff.Differentiate(f, "x");
// df = (cos(x²)·2x + exp(x)), simplified

double value = df.Evaluate(new Dictionary<string, double> { ["x"] = 0.5 });
```

The `Simplifier` runs a fixed-point loop applying constant folding, identity/zero rules, `log(exp(x)) → x`, double-negation collapse, and more.

### Implicit differentiation

```csharp
using DeepSigma.Mathematics.AutoDiff.Implicit;

// Unit circle: x² + y² = 1, so dy/dx = -x/y.
var dyDx = ImplicitDiff.Derivative<double>(
    (x, y) => x * x + y * y - 1.0,
    xValue: 0.6,
    yValue: 0.8);
// dyDx = -0.75
```

Throws `ImplicitDerivativeException` when `|∂F/∂y|` is below the singularity tolerance.

### Jacobian-vector / vector-Jacobian products

```csharp
using DeepSigma.Mathematics.AutoDiff.JVP;

// JVP: J·v in one forward pass
var jv = JVP.Compute<double>(
    d => new[] { d[0] * d[1], d[0] + d[1] },
    x: new[] { 2.0, 3.0 },
    tangent: new[] { 1.0, 0.0 });

// VJP: vᵀ·J in one backward pass
var vj = VJP.Compute<double>(
    vars => new[] { vars[0] * vars[1], vars[0] + vars[1] },
    x: new[] { 2.0, 3.0 },
    cotangent: new[] { 1.0, 1.0 });

// Full Jacobian via N forward passes
var J = VJP.Jacobian<double>(
    d => new[] { d[0] * d[0] + d[1], DualMath<double>.Sin(d[0]) * d[1] },
    new[] { 1.3, 0.7 });
```

### NaN / Inf guards with diagnostics

```csharp
using var tape = new Tape<double>
{
    EnableNaNGuard = true,
    EnableDiagnostics = true
};

var x = tape.Variable(0.0, "x");
var y = ReverseMath<double>.Log(x) * x;

try
{
    tape.Backward(y);
}
catch (GradientNaNException ex)
{
    Console.WriteLine($"NaN at node {ex.NodeId} ({ex.OperationContext})");
    PrintTree(ex.DiagnosticTree);   // walks the originating subtree
}
```

---

## Worked example: Rosenbrock minimization

```csharp
using DeepSigma.Mathematics.AutoDiff;
using DeepSigma.Mathematics.AutoDiff.Reverse;

double x = -1.2, y = 1.0, lr = 1e-3;
for (int step = 0; step <= 5000; step++)
{
    var (dx, dy) = Demo.Grad_Rosenbrock(x, y);
    x -= lr * dx;
    y -= lr * dy;
}
// converges to ≈ (0.94, 0.88) toward the true minimum (1, 1)

public static partial class Demo
{
    [Differentiable]
    public static Var<double> Rosenbrock(Var<double> x, Var<double> y)
    {
        var a = 1.0 - x;
        var b = y - x * x;
        return a * a + 100.0 * b * b;
    }
}
```

Full sample: [`samples/AutoDiff.Samples.Optimization`](samples/AutoDiff.Samples.Optimization/Program.cs).

## Worked example: XOR with a tiny MLP

A 2-4-1 MLP with `tanh` hidden activations and sigmoid output learns XOR in ~4 000 epochs of full-batch gradient descent. See [`samples/AutoDiff.Samples.NeuralNet`](samples/AutoDiff.Samples.NeuralNet/Program.cs).

```text
epoch    0  loss=0.255869
epoch 4000  loss=0.003582

  0 XOR 0 = 0.0415
  0 XOR 1 = 0.9466
  1 XOR 0 = 0.9308
  1 XOR 1 = 0.0705
```

---

## Architecture

### Reference graph

```
DeepSigma.Mathematics.AutoDiff.Core ◀── DeepSigma.Mathematics.AutoDiff.Forward ◀── DeepSigma.Mathematics.AutoDiff.Implicit
                                    ◀── DeepSigma.Mathematics.AutoDiff.Reverse  ◀── DeepSigma.Mathematics.AutoDiff.JVP
                                    ◀── DeepSigma.Mathematics.AutoDiff.Symbolic ◀──┘

DeepSigma.Mathematics.AutoDiff.Generator   (analyzer-only; references Roslyn)
```

### Repository layout

```
src/
  AutoDiff.Core/         DeepSigma.Mathematics.AutoDiff.Core       — Interfaces, generic-math helpers, NaNGuard, diagnostics
  AutoDiff.Forward/      DeepSigma.Mathematics.AutoDiff.Forward     — DualNumber<T>, DualMath<T>, ForwardDiff<T>, HyperDual<T>
  AutoDiff.Reverse/      DeepSigma.Mathematics.AutoDiff.Reverse     — Tape<T>, Var<T>, ReverseMath<T>, TapePool<T>
  AutoDiff.Symbolic/     DeepSigma.Mathematics.AutoDiff.Symbolic    — Expr<T> tree, Simplifier, SymbolicDiff
  AutoDiff.JVP/          DeepSigma.Mathematics.AutoDiff.JVP         — JVP.Compute, VJP.Compute, VJP.Jacobian
  AutoDiff.Implicit/     DeepSigma.Mathematics.AutoDiff.Implicit    — ImplicitDiff
  AutoDiff.Generator/    DeepSigma.Mathematics.AutoDiff.Generator   — [Differentiable] Roslyn incremental generator

tests/
  AutoDiff.Tests.Unit/         xUnit tests (118 passing) with finite-diff cross-checks
  AutoDiff.Tests.Generator/    Generator diagnostic tests (9 passing)
  AutoDiff.Tests.Benchmarks/   BenchmarkDotNet suites

samples/
  AutoDiff.Samples.Optimization/   Rosenbrock gradient descent
  AutoDiff.Samples.NeuralNet/      XOR MLP
```

### Key design choices

| Choice | Why |
|---|---|
| **`struct` for `DualNumber<T>` and `Var<T>`** | No heap allocation in tight loops; tape holds all mutable state. |
| **Flat `TapeNode<T>[]` array** | Cache-coherent backward sweep — measurably faster than linked-list-of-objects layouts. |
| **Thread-local `TapePool<T>`** | 52× cheaper than fresh allocation in the rent/return benchmark. |
| **`record` for `Expr<T>`** | Structural equality drives the `Simplifier` fixed-point loop for free. |
| **Generic over `IFloatingPoint<T>`** | Static abstract members → AOT-safe, no boxing, works with `double`/`float`/`Half`. |
| **Delegation pattern in generator** | Body rewriting is fragile; delegating to a user-provided `Var<T>` overload makes the generator trivially correct. |

---

## Native AOT

All runtime libraries set `IsAotCompatible=true` and pass the AOT/trim analyzers cleanly:

```bash
dotnet build samples/AutoDiff.Samples.NeuralNet \
    -c Release \
    -p:EnableAotAnalyzer=true \
    -p:EnableTrimAnalyzer=true
```

Full native publish (`-p:PublishAot=true`) requires the platform linker (Visual Studio "Desktop development with C++" workload on Windows; `clang` on Linux/macOS).

The symbolic `ExprInterpreter` is the AOT-safe expression evaluator.

---

## Building and testing

```bash
# Restore & build everything
dotnet build

# Run the unit + generator test suites
dotnet test tests/AutoDiff.Tests.Unit
dotnet test tests/AutoDiff.Tests.Generator

# Run the benchmarks
dotnet run -c Release --project tests/AutoDiff.Tests.Benchmarks -- --filter '*'

# Run a sample
dotnet run -c Release --project samples/AutoDiff.Samples.Optimization
```

Test totals: **127 passing** (118 unit + 9 generator). Every non-trivial gradient test cross-checks against central finite differences with tolerance `1e-5`.

---

## Choosing forward vs. reverse mode

| Function shape | Pick |
|---|---|
| R → R (scalar → scalar) | Either; forward is slightly cheaper |
| Rⁿ → R, n large (loss, energy, etc.) | **Reverse** — one backward pass yields all ∂/∂xᵢ |
| R → Rᵐ, m large | **Forward** — one forward pass yields all ∂fⱼ/∂x |
| Rⁿ → Rᵐ, full Jacobian, n ≈ m | Either; pick whichever has the smaller dimension to seed |
| Higher-order at a single point | `HyperDual<T>` |

The benchmark numbers above quantify this: at N=1000 inputs, reverse beats forward by **two orders of magnitude** for a scalar output.

---

## Roadmap

- NuGet packaging + symbol packages (`.snupkg`)
- Property-based tests (FsCheck) — random expression agreement, simplifier semantic preservation
- Real Native AOT publish smoke test in CI
- Sparse Jacobians, GPU-friendly array primitives (long-term)

## License

TBD.

---

*Made for people who want gradients in C# without leaving C#.*
