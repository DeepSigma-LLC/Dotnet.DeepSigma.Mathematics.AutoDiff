```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.8246)
11th Gen Intel Core i7-11800H 2.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.626.17701), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-VYDNZW : .NET 10.0.6 (10.0.626.17701), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

IterationCount=3  LaunchCount=1  WarmupCount=1  

```
| Method                    | N    | Mean           | Error         | StdDev       | Ratio  | RatioSD | Gen0      | Gen1    | Gen2    | Allocated  | Alloc Ratio |
|-------------------------- |----- |---------------:|--------------:|-------------:|-------:|--------:|----------:|--------:|--------:|-----------:|------------:|
| **Reverse_QuadraticGradient** | **10**   |       **230.3 ns** |      **99.10 ns** |      **5.43 ns** |   **1.00** |    **0.03** |    **0.0076** |       **-** |       **-** |      **288 B** |        **1.00** |
| Reverse_NoPool            | 10   |       680.4 ns |     492.78 ns |     27.01 ns |   2.96 |    0.12 |    0.1001 |       - |       - |     3728 B |       12.94 |
| Forward_QuadraticGradient | 10   |       414.3 ns |     500.79 ns |     27.45 ns |   1.80 |    0.11 |    0.0520 |       - |       - |     1944 B |        6.75 |
|                           |      |                |               |              |        |         |           |         |         |            |             |
| **Reverse_QuadraticGradient** | **100**  |     **2,094.4 ns** |     **160.64 ns** |      **8.81 ns** |   **1.00** |    **0.01** |    **0.0648** |       **-** |       **-** |     **2448 B** |        **1.00** |
| Reverse_NoPool            | 100  |     5,861.8 ns |   1,169.08 ns |     64.08 ns |   2.80 |    0.03 |    1.0071 |  0.0534 |       - |    36128 B |       14.76 |
| Forward_QuadraticGradient | 100  |    36,794.0 ns |   1,530.71 ns |     83.90 ns |  17.57 |    0.07 |    6.7139 |  0.0610 |  0.0610 |          - |        0.00 |
|                           |      |                |               |              |        |         |           |         |         |            |             |
| **Reverse_QuadraticGradient** | **1000** |    **22,679.8 ns** |   **1,071.85 ns** |     **58.75 ns** |   **1.00** |    **0.00** |    **0.6409** |       **-** |       **-** |    **24048 B** |        **1.00** |
| Reverse_NoPool            | 1000 |   116,153.8 ns |  22,434.41 ns |  1,229.71 ns |   5.12 |    0.05 |   99.9756 | 99.9756 | 99.9756 |   360162 B |       14.98 |
| Forward_QuadraticGradient | 1000 | 2,385,395.8 ns | 209,048.74 ns | 11,458.67 ns | 105.18 |    0.50 | 6101.5625 |       - |       - | 16032026 B |      666.67 |
