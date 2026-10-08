using System.Text.Json;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;

using Ramstack.Parsing.Benchmarks.Parsers;

namespace Ramstack.Parsing.Benchmarks;

[MemoryDiagnoser]
[OperationsPerSecond]
[HideColumns(Column.Error, Column.StdDev)]
public class JsonBenchmark
{
    private readonly string _jsonBig = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Data/twitter.json"));
    private readonly string _jsonMedium = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Data/medium.json"));
    private readonly string _jsonSmall = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Data/small.json"));

    public IEnumerable<JsonString> Source
    {
        get
        {
            yield return new JsonString(_jsonSmall, "small");
            yield return new JsonString(_jsonMedium, "medium");
            yield return new JsonString(_jsonBig, "big");
        }
    }

    [ParamsSource(nameof(Source))]
    public JsonString Input { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var expected = JsonSerializer.Deserialize<JsonElement>(Input.Value);
        (string Name, object? Value)[] results =
        [
            (nameof(RamstackJson), RamstackJson()),
            (nameof(ParlotJson), ParlotJson()),
            (nameof(ParlotJsonCompiled), ParlotJsonCompiled()),
            (nameof(PidginJson), PidginJson()),
            (nameof(NewtonsoftJson), NewtonsoftJson()),
            (nameof(SystemTextJson), SystemTextJson())
        ];

        foreach (var (name, value) in results)
        {
            var actual = value is Newtonsoft.Json.Linq.JToken token
                ? JsonSerializer.Deserialize<JsonElement>(token.ToString(Newtonsoft.Json.Formatting.None))
                : JsonSerializer.SerializeToElement(value);

            if (!Equivalent(actual, expected))
                throw new InvalidOperationException($"{name}: incorrect JSON model for {Input}");
        }

        (string Name, Func<string, bool> Parse)[] parsers =
        [
            ("Ramstack", input => RamstackParsers.JsonParser.TryParse(input, out _)),
            ("Parlot", input => ParlotParsers.JsonParser.TryParse(input, out _)),
            ("Parlot:Compiled", input => ParlotParsers.JsonParserCompiled.TryParse(input, out _)),
            ("Pidgin", input => PidginParsers.JsonParser.TryParse(input, out _))
        ];

        foreach (var (name, parse) in parsers)
        {
            if (!parse($" \t{Input.Value}\r\n"))
                throw new InvalidOperationException($"{name}: surrounding whitespace");

            if (parse($"{Input.Value} garbage"))
                throw new InvalidOperationException($"{name}: accepted trailing input");
        }
    }

    // Combinator parsers store numbers as double; compare numeric values, not JSON formatting.
    private static bool Equivalent(JsonElement actual, JsonElement expected)
    {
        if (actual.ValueKind != expected.ValueKind)
            return false;

        switch (expected.ValueKind)
        {
            case JsonValueKind.Object:
                if (actual.EnumerateObject().Count() != expected.EnumerateObject().Count())
                    return false;

                foreach (var property in expected.EnumerateObject())
                    if (!actual.TryGetProperty(property.Name, out var value) || !Equivalent(value, property.Value))
                        return false;

                return true;

            case JsonValueKind.Array:
                return actual.GetArrayLength() == expected.GetArrayLength()
                    && actual.EnumerateArray().Zip(expected.EnumerateArray()).All(pair => Equivalent(pair.First, pair.Second));

            case JsonValueKind.Number:
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                return actual.GetDouble() == expected.GetDouble();

            case JsonValueKind.String:
                return actual.GetString() == expected.GetString();

            default:
                return true;
        }
    }

    [Benchmark(Baseline = true, Description = "Ramstack")]
    public object? RamstackJson()
    {
        RamstackParsers.JsonParser.TryParse(Input.Value, out var result);
        return result;
    }

    [Benchmark(Description = "Parlot")]
    public object? ParlotJson() =>
        ParlotParsers.JsonParser.Parse(Input.Value);

    [Benchmark(Description = "Parlot:Compiled")]
    public object? ParlotJsonCompiled() =>
        ParlotParsers.JsonParserCompiled.Parse(Input.Value);

    [Benchmark(Description = "Pidgin")]
    public object? PidginJson() =>
        PidginParsers.JsonParser.Parse(Input.Value);

    [Benchmark(Description = "Newtonsoft.Json")]
    public object? NewtonsoftJson() =>
        Newtonsoft.Json.JsonConvert.DeserializeObject(Input.Value);

    [Benchmark(Description = "System.Text.Json")]
    public object? SystemTextJson() =>
        JsonSerializer.Deserialize<object?>(Input.Value);

    public readonly struct JsonString(string json, string name)
    {
        public string Value => json;
        public override string ToString() => name;
    }
}
