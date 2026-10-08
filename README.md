# Ramstack.Parsing
[![NuGet](https://img.shields.io/nuget/v/Ramstack.Parsing.svg)](https://nuget.org/packages/Ramstack.Parsing)
[![MIT](https://img.shields.io/github/license/rameel/ramstack.parsing)](https://github.com/rameel/ramstack.parsing/blob/main/LICENSE)

A fast, lightweight parser combinator library for .NET.
Build text parsers in C# with typed results and optional error diagnostics.

## Installation

Requires .NET 6 or later.

```shell
dotnet add package Ramstack.Parsing
```

## Example

This parser evaluates arithmetic expressions with parentheses, unary minus, and operator precedence:

```csharp
using System;
using Ramstack.Parsing;
using static Ramstack.Parsing.Parser;

var calc = CreateParser();

Console.WriteLine(calc.Parse("2 + 3 * (4 - 1)")); // 11
Console.WriteLine(calc.Parse("1 + ("));           // (1:6) Expected '-', '(', or number

static Parser<double> CreateParser()
{
    var sum = Deferred<double>();

    var value = Literal
        .Number<double>("number")
        .ThenIgnore(S);

    var parenthesis = sum.Between(
        Seq(L('('), S),
        Seq(L(')'), S)
        );

    var primary = parenthesis.Or(value);

    var unary = Seq(
        L('-').Optional(),
        S,
        primary
        ).Do((u, _, d) => u.HasValue ? -d : d);

    var product = unary.FoldL(
        OneOf("*/").ThenIgnore(S),
        (l, r, op) => op == '*' ? l * r : l / r);

    sum.Parser = product.FoldL(
        OneOf("+-").ThenIgnore(S),
        (l, r, op) => op == '+' ? l + r : l - r);

    return sum.Between(S, Eof);
}
```

Full grammar: [calculator](https://github.com/rameel/ramstack.parsing/blob/main/samples/CalcExpr/README.md).

### Parsing results

`Parse` returns a result with `Success`, `Value`, and `ErrorMessage` properties.
Parse errors include the line, column, and expected input:

```csharp
var result = calc.Parse("2 + 3 * (4 - 1)");
if (result.Success)
{
    Console.WriteLine(result.Value);
}
else
{
    Console.WriteLine(result.ErrorMessage);
}
```

Use `TryParse` when you only need the value and success status.
It skips diagnostics, so it allocates less and runs faster:

```csharp
if (calc.TryParse("2 + 3 * (4 - 1)", out var value))
{
    Console.WriteLine(value);
}
```

### Parsing notes

- `Parse` and `TryParse` accept strings and `ReadOnlySpan<char>`.
- `L` matches a literal; `S` matches zero or more whitespace characters. Whitespace is handled explicitly.
- Add `Eof` to require the entire input. Here, `Between(S, Eof)` allows leading whitespace and rejects trailing input.
- Build each parser once and reuse it.

## Performance

In this email-matching benchmark, `Ramstack.Parsing` performs about as fast as compiled and source-generated regex,
with no measured managed allocations.

The benchmark matches `development.team-2021@example.com` against `\A[\w.+-]+@[\w-]+\.\w{2,}\z`.
All implementations use the same Unicode character classes and require the end of input.
This is a simplified pattern, not a full email validator.

Measured with BenchmarkDotNet on .NET 11 RC1 (Linux, AMD Ryzen 9 5900X).

| Method          |      Mean | Ratio | Allocated |
|-----------------|----------:|------:|----------:|
| Ramstack        |  34.81 ns |  1.00 |         - |
| Regex           | 106.58 ns |  3.06 |         - |
| Regex:Compiled  |  38.02 ns |  1.09 |         - |
| Regex:Generated |  38.42 ns |  1.10 |         - |
| Parlot          | 160.11 ns |  4.60 |     184 B |
| Parlot:Compiled | 150.57 ns |  4.32 |     184 B |
| Pidgin          | 202.13 ns |  5.81 |    0–40 B |

Pidgin measured 0–40 B across runs.

Among parser combinator libraries, `Ramstack.Parsing` is the fastest in this comparison - about 1.7–1.9x faster
than the next-fastest implementation on the expression and JSON benchmarks.

See the [full benchmark results](https://github.com/rameel/ramstack.parsing/blob/main/benchmarks/README.md)
for expression evaluation, JSON parsing, library versions, and reproduction commands.
Results depend on the grammar, input, runtime, and hardware.

## Examples

- [Calculator](https://github.com/rameel/ramstack.parsing/tree/main/samples/CalcExpr) — recursive expressions and operator precedence.
- [JSON](https://github.com/rameel/ramstack.parsing/tree/main/samples/Json) — nested objects and arrays.
- [TinyC](https://github.com/rameel/ramstack.parsing/tree/main/samples/TinyC) — statements, expressions, and comments.

## Contributing

Bug reports and pull requests are welcome. See [Contributing](https://github.com/rameel/ramstack.parsing/blob/main/CONTRIBUTING.md).

## License

[MIT](https://github.com/rameel/ramstack.parsing/blob/main/LICENSE).
