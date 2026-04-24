```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.8246)
11th Gen Intel Core i7-11800H 2.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.626.17701), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-VYDNZW : .NET 10.0.6 (10.0.626.17701), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

IterationCount=3  LaunchCount=1  WarmupCount=1  

```
| Method              | Mean     | Error    | StdDev  | Gen0   | Gen1   | Allocated |
|-------------------- |---------:|---------:|--------:|-------:|-------:|----------:|
| Mlp_ForwardBackward | 111.1 μs | 15.38 μs | 0.84 μs | 2.5635 | 0.1221 |  85.48 KB |
