; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
AD001   | AutoDiff | Error    | [Differentiable] method must be static
AD002   | AutoDiff | Error    | Containing type must be partial
AD003   | AutoDiff | Error    | Method must have parameters
AD004   | AutoDiff | Error    | Unsupported parameter type
AD005   | AutoDiff | Error    | Mixed Var/DualNumber parameters
AD006   | AutoDiff | Error    | Inconsistent element types
AD007   | AutoDiff | Error    | ref/out/in not supported
AD008   | AutoDiff | Error    | Mode mismatch
