using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using Pidgin;

using Ramstack.Parsing.Benchmarks.Parsers;

namespace Ramstack.Parsing.Benchmarks;

[MemoryDiagnoser]
[OperationsPerSecond]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class ExpressionBenchmark
{
    [SuppressMessage("ReSharper", "ArrangeRedundantParentheses")]
    private const double Large = ((3.14159 * 2.3 * 4) / (1.5 - 0.3) + (2.71828 * 2.5)) * ((8.1 * 3.2 + 4.7) / 2.1 - ((15.7 + 2.3 * 4) / (1.5 - 0.3))) - (((3.5 * 1.41421) * (2.71828 + 3.14159)) / 2);
    private const double Small = 2.5 * 4.7 - 8.1 / 3;

    private const string SmallExpression = "2.5 * 4.7 - 8.1 / 3";
    private const string LargeExpression = "(( 3.14159 * 2.3 * 4) / (1.5 - 0.3) + (2.71828 * 2.5) ) * ((8.1 * 3.2 + 4.7) / 2.1 - ((15.7 + 2.3 * 4) / (1.5 - 0.3))) - (((3.5 * 1.41421) * (2.71828 + 3.14159)) / 2)";

    [GlobalSetup]
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public void Setup()
    {
        if (RamstackParsers.ExpressionParser.Parse(SmallExpression).Value != Small) throw new Exception("Ramstack: Small");
        if (RamstackParsers.ExpressionParser.Parse(LargeExpression).Value != Large) throw new Exception("Ramstack: Large");

        if (ParlotParsers.ExpressionParser.Parse(SmallExpression) != Small) throw new Exception("Parlot: Small");
        if (ParlotParsers.ExpressionParser.Parse(LargeExpression) != Large) throw new Exception("Parlot: Large");

        if (PidginParsers.ExpressionParser.Parse(SmallExpression).Value != Small) throw new Exception("Pidgin: Small");
        if (PidginParsers.ExpressionParser.Parse(LargeExpression).Value != Large) throw new Exception("Pidgin: Large");
    }

    [BenchmarkCategory("Small")]
    [Benchmark(Description = "Ramstack:Small")]
    public double RamstackSmall()
    {
        RamstackParsers.ExpressionParser.TryParse(SmallExpression, out var result);
        return result;
    }

    [BenchmarkCategory("Small")]
    [Benchmark(Description = "Ramstack:Diag:Small")]
    public double RamstackSmall_Diagnostics() =>
        RamstackParsers.ExpressionParser.Parse(SmallExpression).Value;

    [BenchmarkCategory("Large")]
    [Benchmark(Description = "Ramstack:Large")]
    public double RamstackLarge()
    {
        RamstackParsers.ExpressionParser.TryParse(LargeExpression, out var result);
        return result;
    }

    [BenchmarkCategory("Large")]
    [Benchmark(Description = "Ramstack:Diag:Large")]
    public double RamstackLarge_Diagnostics() =>
        RamstackParsers.ExpressionParser.Parse(LargeExpression).Value;

    [BenchmarkCategory("Small")]
    [Benchmark(Description = "Parlot:Small")]
    public double ParlotSmall() =>
        ParlotParsers.ExpressionParser.Parse(SmallExpression);

    [BenchmarkCategory("Small")]
    [Benchmark(Description = "Parlot:Compiled:Small")]
    public double ParlotSmallCompiled() =>
        ParlotParsers.ExpressionParserCompiled.Parse(SmallExpression);

    [BenchmarkCategory("Large")]
    [Benchmark(Description = "Parlot:Large")]
    public double ParlotLarge() =>
        ParlotParsers.ExpressionParser.Parse(LargeExpression);

    [BenchmarkCategory("Large")]
    [Benchmark(Description = "Parlot:Compiled:Large")]
    public double ParlotLargeCompiled() =>
        ParlotParsers.ExpressionParserCompiled.Parse(LargeExpression);

    [BenchmarkCategory("Small")]
    [Benchmark(Description = "Pidgin:Small")]
    public double PidginSmall()
    {
        var result = PidginParsers.ExpressionParser.Parse(SmallExpression);
        return result.Success ? result.Value : 0;
    }

    [BenchmarkCategory("Large")]
    [Benchmark(Description = "Pidgin:Large")]
    public double PidginLarge()
    {
        var result = PidginParsers.ExpressionParser.Parse(LargeExpression);
        return result.Success ? result.Value : 0;
    }
}
