# Benchmarks

Full BenchmarkDotNet results for email matching, expression evaluation, and JSON parsing:
`EmailBenchmark`, `ExpressionBenchmark`, and `JsonBenchmark`.

Results depend on the input, implementation, runtime, and hardware. They are not a general ranking of the libraries.

## Setup

Measured on 2026-10-08 with the default BenchmarkDotNet job and concurrent workstation GC.
Each benchmark class ran separately, after a one-minute pause:

```text
BenchmarkDotNet v0.16.0-preview.2, Linux CachyOS
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 11.0.100-rc.1.26425.128
  [Host]    : .NET 10.0.12, X64 RyuJIT x86-64-v3
  .NET 11.0 : .NET 11.0.0-rc.1.26425.128, X64 RyuJIT x86-64-v3
```

Library versions:

- Parlot 1.5.9
- Pidgin 3.5.1
- Newtonsoft.Json 13.0.4
- System.Text.Json from the .NET 11 runtime

`Ratio` is relative to `Ramstack` for the same input; lower is faster.
`-` means no measured allocation or GC collection in that column.
Parser construction and regex compilation are not timed.
`Parlot:Compiled` uses Parlot's `Compile()`. On these grammars it is about as fast as the interpreted parser.

## Email matching

The benchmark matches `development.team-2021@example.com` against this pattern:

```csharp
[GeneratedRegex(@"\A[\w.+-]+@[\w-]+\.\w{2,}\z")]
private static partial Regex EmailRegexGenerated();
```

The Ramstack parser:

```csharp
var parser = Seq(
    Set("\\w.+-").OneOrMore(),
    L('@'),
    Set("\\w-").OneOrMore(),
    L('.'),
    Set("\\w").AtLeast(2)
).Void().ThenIgnore(Eof);
```

All implementations use the same Unicode `\w` character set and must match the whole input.
The regex uses `\z` because `$` also matches before a final newline.
This is a simplified pattern, not an RFC-compliant email validator.

The benchmark only checks whether the input matches. It doesn't create a string containing the match.
Parlot captures a `TextSpan`; Pidgin skips the matched characters.

| Method          |      Mean |         Op/s | Ratio |     Gen0 | Allocated |
|---------------- |----------:|-------------:|------:|---------:|----------:|
| Ramstack        |  34.81 ns | 28,723,442.7 |  1.00 |        - |         - |
| Regex           | 106.58 ns |  9,382,454.8 |  3.06 |        - |         - |
| Regex:Compiled  |  38.02 ns | 26,300,462.7 |  1.09 |        - |         - |
| Regex:Generated |  38.42 ns | 26,029,963.8 |  1.10 |        - |         - |
| Parlot          | 160.11 ns |  6,245,594.6 |  4.60 |   0.0110 |     184 B |
| Parlot:Compiled | 150.57 ns |  6,641,431.5 |  4.32 |   0.0110 |     184 B |
| Pidgin          | 202.13 ns |  4,947,403.8 |  5.81 | 0–0.0024 |    0–40 B |

Pidgin's `Gen0` and `Allocated` ranges come from repeated runs. The timing statistics are from the latest run, in which Pidgin showed no allocation.
Its `Parse(...).Success` call creates a 40 B result object, which the .NET 11 JIT sometimes places on the stack.
Disassembly showed heap allocation in some benchmark processes and stack allocation in others.

## Expressions

Each parser evaluates the whole expression and rejects any leftover input.

- **Small**: `2.5 * 4.7 - 8.1 / 3`
- **Large**: `((3.14159 * 2.3 * 4) / (1.5 - 0.3) + (2.71828 * 2.5)) * ((8.1 * 3.2 + 4.7) / 2.1 - ((15.7 + 2.3 * 4) / (1.5 - 0.3))) - (((3.5 * 1.41421) * (2.71828 + 3.14159)) / 2)`

`Ramstack` uses `TryParse`, which doesn't collect diagnostics.
`Ramstack:Diagnostics` uses `Parse`, which collects diagnostics and returns a `ParseResult`.

All three grammars allow at most one unary minus before a primary expression, so `-2` and `-(1 + 2)` are valid, but `---2` isn't.

| Method               | Input |        Mean |        Op/s | Ratio |   Gen0 | Allocated |
|--------------------- |------ |------------:|------------:|------:|-------:|----------:|
| Ramstack             | large |  1,542.2 ns |   648,418.1 |  1.00 |      - |         - |
| Ramstack:Diagnostics | large |  1,760.1 ns |   568,148.6 |  1.14 | 0.0057 |     104 B |
| Parlot               | large |  2,805.9 ns |   356,389.2 |  1.82 | 0.0191 |     328 B |
| Parlot:Compiled      | large |  2,569.3 ns |   389,213.2 |  1.67 | 0.0191 |     328 B |
| Pidgin               | large | 34,104.7 ns |    29,321.5 | 22.11 | 0.2441 |    4112 B |
|                      |       |             |             |       |        |           |
| Ramstack             | small |    210.9 ns | 4,740,834.8 |  1.00 |      - |         - |
| Ramstack:Diagnostics | small |    248.6 ns | 4,022,355.2 |  1.18 | 0.0062 |     104 B |
| Parlot               | small |    371.2 ns | 2,694,200.0 |  1.76 | 0.0196 |     328 B |
| Parlot:Compiled      | small |    349.1 ns | 2,864,173.4 |  1.66 | 0.0110 |     184 B |
| Pidgin               | small |  3,770.9 ns |   265,190.4 | 17.88 | 0.0381 |     648 B |

## JSON

The benchmark parses JSON and builds an object model for each input:

- **Small**: `small.json`, 1,191 bytes (1.16 KiB)
- **Medium**: `medium.json`, 11,130 bytes (10.87 KiB)
- **Large**: `twitter.json`, 631,514 bytes (616.71 KiB), from [serde-rs/json-benchmark](https://github.com/serde-rs/json-benchmark)

Sizes are for the UTF-8 files. Each parser reads the same .NET string, loaded before the benchmark.
File reading isn't timed.

Before timing, the setup code compares each parser's output with the output from `System.Text.Json`
and checks that extra input after the JSON is rejected.

The object models differ:

- Combinator parsers build dictionary/list trees with boxed values.
- Newtonsoft.Json builds a `JToken` tree.
- System.Text.Json returns a compact `JsonElement` representation.

Because the parsers build different objects, the memory allocation figures aren't directly comparable.
The parser combinator examples use `double` for numbers and aren't full JSON validators.
These benchmarks don't test whether the parsers follow every JSON rule or preserve numbers exactly.

| Method           | Input  |          Mean |       Op/s | Ratio |     Gen0 |     Gen1 |     Gen2 |  Allocated |
|----------------- |------- |--------------:|-----------:|------:|---------:|---------:|---------:|-----------:|
| Ramstack         | big    |  1,303.976 μs |     766.89 |  1.00 | 152.3438 | 130.8594 |        - | 2498.88 KB |
| Parlot           | big    |  2,456.401 μs |     407.10 |  1.88 | 152.3438 | 148.4375 |        - | 2514.81 KB |
| Parlot:Compiled  | big    |  2,469.364 μs |     404.96 |  1.89 | 152.3438 | 148.4375 |        - | 2514.81 KB |
| Pidgin           | big    | 16,176.153 μs |      61.82 | 12.41 | 156.2500 |  93.7500 |        - |  2570.3 KB |
| Newtonsoft.Json  | big    |  3,756.486 μs |     266.21 |  2.88 | 343.7500 | 320.3125 |        - | 5638.63 KB |
| System.Text.Json | big    |  1,312.258 μs |     762.05 |  1.01 | 158.2031 | 158.2031 | 158.2031 |  964.79 KB |
|                  |        |               |            |       |          |          |          |            |
| Ramstack         | medium |     22.734 μs |  43,986.34 |  1.00 |   2.6245 |   0.2747 |        - |   42.94 KB |
| Parlot           | medium |     42.917 μs |  23,300.54 |  1.89 |   2.6245 |   0.2441 |        - |    43.7 KB |
| Parlot:Compiled  | medium |     43.321 μs |  23,083.65 |  1.91 |   2.6245 |   0.2441 |        - |    43.7 KB |
| Pidgin           | medium |    260.133 μs |   3,844.18 | 11.44 |   2.4414 |        - |        - |   44.45 KB |
| Newtonsoft.Json  | medium |     51.799 μs |  19,305.49 |  2.28 |   6.0425 |   1.6479 |        - |   99.44 KB |
| System.Text.Json | medium |     19.981 μs |  50,047.99 |  0.88 |   1.0376 |        - |        - |   17.03 KB |
|                  |        |               |            |       |          |          |          |            |
| Ramstack         | small  |      3.416 μs | 292,712.86 |  1.00 |   0.3815 |   0.0038 |        - |    6.24 KB |
| Parlot           | small  |      6.237 μs | 160,341.11 |  1.83 |   0.4044 |   0.0076 |        - |    6.67 KB |
| Parlot:Compiled  | small  |      6.131 μs | 163,114.90 |  1.79 |   0.4044 |   0.0076 |        - |    6.67 KB |
| Pidgin           | small  |     35.843 μs |  27,899.82 | 10.49 |   0.3662 |        - |        - |    6.49 KB |
| Newtonsoft.Json  | small  |      8.151 μs | 122,680.16 |  2.39 |   1.0529 |   0.0458 |        - |   17.26 KB |
| System.Text.Json | small  |      3.097 μs | 322,903.49 |  0.91 |   0.1373 |        - |        - |    2.25 KB |

## Run the benchmarks

Use the .NET 10 SDK or later to build the project. Package versions are pinned in
[Directory.Packages.props](../Directory.Packages.props).

To reproduce the published results, also install the .NET 11 SDK listed above.
The benchmark project targets `net10.0`; the commands below use `--runtimes net11.0` to run the
measured workloads on .NET 11. The library and sample projects keep their current target frameworks.

From the repository root:

```shell
dotnet build benchmarks/Ramstack.Parsing.Benchmarks.csproj -c Release

# Check that the benchmarks build and run correctly.
# Don't use Dry timings to compare performance.
# --------------------------------------------------
dotnet run --project benchmarks/Ramstack.Parsing.Benchmarks.csproj -c Release --no-build -- \
  --runtimes net11.0 --filter '*EmailBenchmark*' --job Dry --noOverwrite --artifacts ./BenchmarkDotNet.Artifacts/dry-email

dotnet run --project benchmarks/Ramstack.Parsing.Benchmarks.csproj -c Release --no-build -- \
  --runtimes net11.0 --filter '*ExpressionBenchmark*' --job Dry --noOverwrite --artifacts ./BenchmarkDotNet.Artifacts/dry-expressions

dotnet run --project benchmarks/Ramstack.Parsing.Benchmarks.csproj -c Release --no-build -- \
  --runtimes net11.0 --filter '*JsonBenchmark*' --job Dry --noOverwrite --artifacts ./BenchmarkDotNet.Artifacts/dry-json

# Run each benchmark class separately, one at a time, with the default job.
# -------------------------------------------------------------------------
dotnet run --project benchmarks/Ramstack.Parsing.Benchmarks.csproj -c Release --no-build -- \
  --runtimes net11.0 --filter '*EmailBenchmark*' --noOverwrite --artifacts ./BenchmarkDotNet.Artifacts/email

dotnet run --project benchmarks/Ramstack.Parsing.Benchmarks.csproj -c Release --no-build -- \
  --runtimes net11.0 --filter '*ExpressionBenchmark*' --noOverwrite --artifacts ./BenchmarkDotNet.Artifacts/expressions

dotnet run --project benchmarks/Ramstack.Parsing.Benchmarks.csproj -c Release --no-build -- \
  --runtimes net11.0 --filter '*JsonBenchmark*' --noOverwrite --artifacts ./BenchmarkDotNet.Artifacts/json
```
