# Ramstack.Parsing
[![NuGet](https://img.shields.io/nuget/v/Ramstack.Parsing.svg)](https://nuget.org/packages/Ramstack.Parsing)
[![MIT](https://img.shields.io/github/license/rameel/ramstack.parsing)](https://github.com/rameel/ramstack.parsing/blob/main/LICENSE)

A blazing-fast, lightweight, and intuitive parser combinator library for .NET.

## Getting Started

To install the `Ramstack.Parsing` [NuGet package](https://www.nuget.org/packages/Ramstack.Parsing) to your project, run the following command:
```shell
dotnet add package Ramstack.Parsing
```

## Usage

Here's how to define a simple expression parser that parses and evaluates mathematical expressions in one step:

```csharp
public static Parser<double> Calc { get; } = CreateParser();

private static Parser<double> CreateParser()
{
    // Grammar:
    // ----------------------------------------
    // Start       :  Sum $
    // Sum         :  Product (S [+-] Product)*
    // Product     :  Unary (S [*/] Unary)*
    // Unary       :  S '-'? Primary
    // Primary     :  Parenthesis / Value
    // Parenthesis :  S '(' Sum S ')'
    // Value       :  S Number
    // S           :  Whitespace*

    var sum = Deferred<double>();
    var value = S.Then(Literal.Number<double>());

    var parenthesis = sum.Between(
        Seq(S, L('(')),
        Seq(S, L(')'))
        );

    var primary = parenthesis.Or(value);

    var unary = Seq(
        S,
        L('-').Optional(),
        primary
        ).Do((_, u, d) => u.HasValue ? -d : d);

    var product = unary.Fold(
        S.Then(OneOf("*/")),
        (l, r, op) => op == '*' ? l * r : l / r);

    sum.Parser = product.Fold(
        S.Then(OneOf("+-")),
        (l, r, op) => op == '+' ? l + r : l - r);

    return sum.ThenIgnore(Eof);
}
```

As you can see, the parser is highly readable and easy to define. Using it is just as simple:

```csharp
var result = Calc.Parse(expression);
if (result.Success)
{
    var value = result.Value;
    ...
}
else
{
    Console.WriteLine(result.ErrorMessage);

    //
    // result.ToString() prints the parsed value or an error message
    // depending on the parsing status
    //
    // Console.WriteLine(result);
    //
}
```

The parser provides detailed error messages when the input does not conform to the specified grammar.
For example, when parsing the expression `"1 + ("`, the parser expects `'-'`, `'('`, or a `number` after the opening parenthesis.
The error message clearly indicates this:

```
(1:6) Expected '-', '(' or number
```

In addition to the main `Parse` method, there is also a lightweight `bool TryParse(ReadOnlySpan<char> source, out T value)` method.
This method suppresses diagnostic messages, returning only the parsing success status and the parsed value if successful.

If detailed error messages are not required, using `TryParse` is the preferred choice, as it reduces memory allocations
and slightly improves performance by avoiding the overhead associated with collecting diagnostic data.

## Documentation

- Documentation can be found in [docs/documentation.md](docs/documentation.md) (Work in Progress).
- Additional usage samples, see the [samples/Samples](samples) directory.

## Performance

`Ramstack.Parsing` stands out as an exceptionally fast and efficient parsing library, outperforming many alternatives in both speed and memory usage.

### Benchmarks: Regex

Here's a comparison between a regex-based email matcher and an equivalent parser combinator implementation:

**Regular Expression:**
```csharp
[GeneratedRegex(@"^[\w.+-]+@[\w-]+\.\w{2,}$")]
private static partial Regex EmailRegexGenerated();
```

**Parser**:
```csharp
var parser = Seq(
    Choice(LetterOrDigit, OneOf("_-+.")).OneOrMore(),
    L('@'),
    Choice(LetterOrDigit, L('-')).OneOrMore(),
    L('.'),
    LetterOrDigit.AtLeast(2)
).Text();
```
The library supports nearly full character class definitions from regular expressions, allowing grammars to be written in a similar way
as it written for regular expression. Optimizations ensure that both definitions are internally normalized to an equivalent representation:
```csharp
var parser = Seq(
    Set("\\w+.-").OneOrMore(),
    L('@'),
    Set("\\w-").OneOrMore(),
    L('.'),
    Set("\\w").AtLeast(2)
).Text();
```

The benchmark results show that `Ramstack.Parsing` outperforms even highly optimized Regex implementations,
including compiled and source-generated regexes.

```
BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3037)
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 9.0.200-preview.0.25057.12
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2


| Method          | Mean        | Error    | StdDev   | Op/s         | Gen0   | Allocated |
|---------------- |------------:|---------:|---------:|-------------:|-------:|----------:|
| Ramstack        |    34.47 ns | 0.073 ns | 0.065 ns | 29,008,779.8 |      - |         - |
| Regex           |   107.35 ns | 0.125 ns | 0.117 ns |  9,315,658.6 |      - |         - |
| Regex:Compiled  |    44.63 ns | 0.093 ns | 0.083 ns | 22,408,327.6 |      - |         - |
| Regex:Generated |    41.85 ns | 0.282 ns | 0.250 ns | 23,893,269.4 |      - |         - |
| Parlot          |   295.05 ns | 0.601 ns | 0.533 ns |  3,389,302.6 | 0.0229 |     384 B |
| Parlot:Compiled |   228.98 ns | 0.446 ns | 0.417 ns |  4,367,155.0 | 0.0091 |     152 B |
| Pidgin          | 1,602.00 ns | 9.016 ns | 8.434 ns |    624,219.5 | 0.0572 |     976 B |
```

In this specific scenario, the library simply checks for a match against the email pattern, similar to what `Regex.IsMatch` does,
without extracting the match result itself. Like with regular expressions, this approach avoids unnecessary memory allocations.

### Benchmarks: Expressions

This benchmarks tests 2 expressions:
- **Small**: `2.5 * 4.7 - 8.1 / 3`
- **Large**: `((3.14159 * 2.3 * 4) / (1.5 - 0.3) + (2.71828 * 2.5)) * ((8.1 * 3.2 + 4.7) / 2.1 - ((15.7 + 2.3 * 4) / (1.5 - 0.3))) - (((3.5 * 1.41421) * (2.71828 + 3.14159)) / 2)`

```
BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3037)
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 9.0.200-preview.0.25057.12
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2


| Method                | Mean        | Error     | StdDev    | Op/s        | Gen0   | Allocated |
|---------------------- |------------:|----------:|----------:|------------:|-------:|----------:|
| Ramstack:Large        |  1,600.2 ns |   9.54 ns |   7.96 ns |   624,925.9 |      - |         - |
| Ramstack:Diag:Large   |  1,699.6 ns |   3.53 ns |   3.13 ns |   588,379.8 | 0.0057 |     104 B |
| Parlot:Large          |  3,520.1 ns |  11.08 ns |  10.37 ns |   284,081.7 | 0.1984 |    3344 B |
| Parlot:Compiled:Large |  3,356.7 ns |   7.08 ns |   6.27 ns |   297,910.3 | 0.1984 |    3344 B |
| Pidgin:Large          | 39,197.3 ns | 150.13 ns | 140.43 ns |    25,511.9 | 0.2441 |    4464 B |
|                       |             |           |           |             |        |           |
| Ramstack:Small        |    209.5 ns |   0.28 ns |   0.25 ns | 4,772,894.8 |      - |         - |
| Ramstack:Diag:Small   |    260.6 ns |   0.57 ns |   0.50 ns | 3,838,032.3 | 0.0062 |     104 B |
| Parlot:Small          |    410.7 ns |   1.46 ns |   1.36 ns | 2,434,653.7 | 0.0391 |     656 B |
| Parlot:Compiled:Small |    385.1 ns |   1.22 ns |   0.95 ns | 2,596,891.9 | 0.0391 |     656 B |
| Pidgin:Small          |  4,322.2 ns |  18.28 ns |  17.10 ns |   231,363.9 | 0.0381 |     736 B |
```

- `Ramstack` diagnostic messages disabled.
- `Ramstack:Diag` includes diagnostic messages.

Even with diagnostics enabled, `Ramstack.Parsing` outperforms other popular libraries.

### Benchmarks: JSON

This benchmark evaluates JSON parsing performance, including object model construction, across different file sizes.

- **Small**: 1.16KB
- **Medium**: 11KB
- **Large**: `twitter.json` (617KB) from [serde-rs/json-benchmark](https://github.com/serde-rs/json-benchmark)

```
BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3037)
AMD Ryzen 9 5900X, 1 CPU, 24 logical and 12 physical cores
.NET SDK 9.0.200-preview.0.25057.12
  [Host]     : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2
  DefaultJob : .NET 9.0.1 (9.0.124.61010), X64 RyuJIT AVX2


| Method           | Input  | Mean          | Error      | StdDev     | Op/s       | Gen0     | Gen1     | Gen2     | Allocated  |
|----------------- |------- |--------------:|-----------:|-----------:|-----------:|---------:|---------:|---------:|-----------:|
| Ramstack         | big    |  1,307.929 us | 12.6067 us | 11.7923 us |     764.57 | 152.3438 | 130.8594 |        - | 2498.88 KB |
| Parlot           | big    |  3,102.953 us |  9.3441 us |  8.7405 us |     322.27 | 156.2500 | 132.8125 |        - | 2554.06 KB |
| Parlot:Compiled  | big    |  3,078.931 us |  7.4028 us |  5.7796 us |     324.79 | 156.2500 | 132.8125 |        - | 2554.06 KB |
| Pidgin           | big    | 16,675.788 us | 40.7050 us | 36.0839 us |      59.97 | 156.2500 |  93.7500 |        - | 2570.33 KB |
| Newtonsoft.Json  | big    |  3,878.753 us | 18.3372 us | 16.2554 us |     257.81 | 343.7500 | 320.3125 |        - | 5638.63 KB |
| System.Text.Json | big    |  1,492.658 us | 10.2347 us |  9.0728 us |     669.95 | 216.7969 | 216.7969 | 216.7969 |  963.67 KB |

| Ramstack         | medium |     20.163 us |  0.0968 us |  0.0906 us |  49,595.14 |   2.6245 |   0.2747 |        - |   42.94 KB |
| Parlot           | medium |     72.128 us |  0.1193 us |  0.1058 us |  13,864.18 |   2.5635 |   0.2441 |        - |   43.09 KB |
| Parlot:Compiled  | medium |     70.931 us |  0.2758 us |  0.2445 us |  14,098.14 |   2.5635 |   0.2441 |        - |   43.09 KB |
| Pidgin           | medium |    266.283 us |  0.4172 us |  0.3698 us |   3,755.41 |   2.4414 |        - |        - |   44.45 KB |
| Newtonsoft.Json  | medium |     52.036 us |  0.4550 us |  0.4257 us |  19,217.47 |   6.0425 |   1.6479 |        - |   99.44 KB |
| System.Text.Json | medium |     24.471 us |  0.0968 us |  0.0756 us |  40,865.15 |   1.0376 |        - |        - |   17.03 KB |

| Ramstack         | small  |      3.016 us |  0.0064 us |  0.0053 us | 331,565.28 |   0.3815 |   0.0038 |        - |    6.24 KB |
| Parlot           | small  |      6.723 us |  0.0313 us |  0.0292 us | 148,732.16 |   0.3891 |        - |        - |    6.39 KB |
| Parlot:Compiled  | small  |      6.661 us |  0.0210 us |  0.0175 us | 150,120.62 |   0.3891 |        - |        - |    6.39 KB |
| Pidgin           | small  |     33.369 us |  0.3909 us |  0.3656 us |  29,968.03 |   0.3662 |        - |        - |    6.49 KB |
| Newtonsoft.Json  | small  |      7.940 us |  0.0226 us |  0.0201 us | 125,951.32 |   1.0529 |   0.0458 |        - |   17.26 KB |
| System.Text.Json | small  |      3.477 us |  0.0133 us |  0.0111 us | 287,598.87 |   0.1373 |        - |        - |    2.25 KB |
```

`Ramstack.Parsing` demonstrated superior performance, outperforming other parser libraries by at least 2-3 times,
and even exceeded specialized JSON parsers such as `Newtonsoft.Json` and `System.Text.Json` in terms of both speed and memory efficiency.

### Benchmarked Libraries

1. [x] Parlot 1.3.2
2. [x] Pidgin 3.3.0
3. [x] Newtonsoft.Json 13.0.3
4. [x] System.Text.Json 9.0.0

## Supported Versions

|      | Version    |
|------|------------|
| .NET | 6, 7, 8, 9 |

## Contributions

Bug reports and contributions are welcome.

## License

This package is released under the **MIT License**.
See the [LICENSE](https://github.com/rameel/ramstack.parsing/blob/main/LICENSE) file for details.
