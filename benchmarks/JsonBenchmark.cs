using BenchmarkDotNet.Attributes;

using Ramstack.Parsing.Benchmarks.Parsers;

namespace Ramstack.Parsing.Benchmarks;

[MemoryDiagnoser]
[OperationsPerSecond]
public class JsonBenchmark
{
    public static readonly string JsonBig = File.ReadAllText("Data/twitter.json");
    public static readonly string JsonMedium = File.ReadAllText("Data/medium.json");
    public static readonly string JsonSmall = File.ReadAllText("Data/small.json");

    public IEnumerable<JsonString> Source
    {
        get
        {
            yield return new JsonString(JsonSmall.TrimEnd(), "small");
            yield return new JsonString(JsonMedium.TrimEnd(), "medium");
            yield return new JsonString(JsonBig.TrimEnd(), "big");
        }
    }

    [ParamsSource(nameof(Source))]
    public JsonString Input { get; set; }

    [Benchmark(Description = "Ramstack")]
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
        System.Text.Json.JsonSerializer.Deserialize<object?>(Input.Value);

    public readonly struct JsonString(string json, string name)
    {
        public string Value => json;
        public override string ToString() => name;
    }
}
