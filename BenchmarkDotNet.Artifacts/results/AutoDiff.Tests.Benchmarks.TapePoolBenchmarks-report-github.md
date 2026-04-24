```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.8246)
11th Gen Intel Core i7-11800H 2.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 10.0.202
  [Host]     : .NET 10.0.6 (10.0.626.17701), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  Job-VYDNZW : .NET 10.0.6 (10.0.626.17701), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

IterationCount=3  LaunchCount=1  WarmupCount=1  

```
| Method           | Mean        | Error       | StdDev    | Ratio | RatioSD | Gen0     | Gen1    | Gen2   | Allocated | Alloc Ratio |
|----------------- |------------:|------------:|----------:|------:|--------:|---------:|--------:|-------:|----------:|------------:|
| RentReturn_Cycle |    27.28 μs |    14.66 μs |  0.803 μs |  1.00 |    0.04 |        - |       - |      - |         - |          NA |
| FreshAlloc_Cycle | 1,418.68 μs | 1,271.22 μs | 69.680 μs | 52.04 |    2.57 | 619.1406 | 17.5781 | 1.9531 |         - |          NA |
