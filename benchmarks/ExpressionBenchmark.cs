using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;

using Pidgin;

using Ramstack.Parsing.Benchmarks.Parsers;

namespace Ramstack.Parsing.Benchmarks;

[MemoryDiagnoser]
[OperationsPerSecond]
[HideColumns(Column.Error, Column.StdDev)]
public class ExpressionBenchmark
{
    [SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
    public const double LargeResult = ((3.14159 * 2.3 * 4) / (1.5 - 0.3) + (2.71828 * 2.5)) * ((8.1 * 3.2 + 4.7) / 2.1 - ((15.7 + 2.3 * 4) / (1.5 - 0.3))) - (((3.5 * 1.41421) * (2.71828 + 3.14159)) / 2);
    public const double SmallResult = 2.5 * 4.7 - 8.1 / 3;

    private readonly string _largeExpression = "((3.14159 * 2.3 * 4) / (1.5 - 0.3) + (2.71828 * 2.5)) * ((8.1 * 3.2 + 4.7) / 2.1 - ((15.7 + 2.3 * 4) / (1.5 - 0.3))) - (((3.5 * 1.41421) * (2.71828 + 3.14159)) / 2)";
    private readonly string _smallExpression = "2.5 * 4.7 - 8.1 / 3";

    public IEnumerable<ExpressionInput> Source
    {
        get
        {
            yield return new ExpressionInput(_smallExpression, SmallResult, "small");
            yield return new ExpressionInput(_largeExpression, LargeResult, "large");
        }
    }

    [ParamsSource(nameof(Source))]
    public ExpressionInput Input { get; set; }

    [GlobalSetup]
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public void Setup()
    {
        (string Name, double Value)[] results =
        [
            (nameof(RamstackExpression), RamstackExpression()),
            (nameof(RamstackExpressionDiagnostics), RamstackExpressionDiagnostics()),
            (nameof(ParlotExpression), ParlotExpression()),
            (nameof(ParlotExpressionCompiled), ParlotExpressionCompiled()),
            (nameof(PidginExpression), PidginExpression())
        ];

        foreach (var (name, value) in results)
            if (value != Input.Expected)
                throw new InvalidOperationException($"{name}: {value} != {Input.Expected} for {Input}");

        (string Name, Func<string, double?> Parse)[] parsers =
        [
            ("Ramstack", input => RamstackParsers.ExpressionParser.TryParse(input, out var value) ? value : null),
            ("Ramstack:Diagnostics", input => RamstackParsers.ExpressionParser.Parse(input) is { Success: true } result ? result.Value : null),
            ("Parlot", input => ParlotParsers.ExpressionParser.TryParse(input, out var value) ? value : null),
            ("Parlot:Compiled", input => ParlotParsers.ExpressionParserCompiled.TryParse(input, out var value) ? value : null),
            ("Pidgin", input => PidginParsers.ExpressionParser.Parse(input) is { Success: true } result ? result.Value : null)
        ];

        foreach (var (name, parse) in parsers)
        {
            if (parse($" \t{_smallExpression}\r\n") != SmallResult)
                throw new InvalidOperationException($"{name}: surrounding whitespace");

            foreach (var suffix in new[] { " garbage", "+", ")" })
                if (parse(_smallExpression + suffix) is not null)
                    throw new InvalidOperationException($"{name}: accepted trailing input {suffix}");

            // Every grammar allows a single unary minus before a primary expression.
            foreach (var (input, expected) in new (string, double?)[] { ("-2", -2), ("- 2 * 3", -6), ("-(1 + 2)", -3), ("--2", 2), ("---2", null) })
                if (parse(input) != expected)
                    throw new InvalidOperationException($"{name}: unexpected result for unary minus '{input}'");
        }
    }

    [Benchmark(Baseline = true, Description = "Ramstack")]
    public double RamstackExpression()
    {
        RamstackParsers.ExpressionParser.TryParse(Input.Value, out var result);
        return result;
    }

    [Benchmark(Description = "Ramstack:Diagnostics")]
    public double RamstackExpressionDiagnostics() =>
        RamstackParsers.ExpressionParser.Parse(Input.Value).Value;

    [Benchmark(Description = "Parlot")]
    public double ParlotExpression() =>
        ParlotParsers.ExpressionParser.Parse(Input.Value);

    [Benchmark(Description = "Parlot:Compiled")]
    public double ParlotExpressionCompiled() =>
        ParlotParsers.ExpressionParserCompiled.Parse(Input.Value);

    [Benchmark(Description = "Pidgin")]
    public double PidginExpression()
    {
        var result = PidginParsers.ExpressionParser.Parse(Input.Value);
        return result.Success ? result.Value : 0;
    }

    public readonly struct ExpressionInput(string value, double expected, string name)
    {
        public string Value => value;
        public double Expected => expected;
        public override string ToString() => name;
    }
}
