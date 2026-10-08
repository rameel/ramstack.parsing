using System.Text.RegularExpressions;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;

using Pidgin;

using Ramstack.Parsing.Benchmarks.Parsers;

namespace Ramstack.Parsing.Benchmarks;

[MemoryDiagnoser]
[OperationsPerSecond]
[HideColumns(Column.Error, Column.StdDev)]
public partial class EmailBenchmark
{
    public const string RegexPattern = @"\A[\w.+-]+@[\w-]+\.\w{2,}\z";
    private readonly string _emailValue = "development.team-2021@example.com";

    public static readonly Regex EmailRegex = new Regex(RegexPattern);
    public static readonly Regex EmailRegexCompiled = new Regex(RegexPattern, RegexOptions.Compiled);

    [GeneratedRegex(RegexPattern)]
    private static partial Regex EmailRegexGenerated();

    [GlobalSetup]
    public void Setup()
    {
        (string Name, Func<string, bool> Match)[] matchers =
        [
            ("Ramstack", input => RamstackParsers.EmailParser.TryParse(input, out _)),
            ("Regex", EmailRegex.IsMatch),
            ("Regex:Compiled", EmailRegexCompiled.IsMatch),
            ("Regex:Generated", EmailRegexGenerated().IsMatch),
            ("Parlot", input => ParlotParsers.EmailParser.TryParse(input, out _)),
            ("Parlot:Compiled", input => ParlotParsers.EmailParserCompiled.TryParse(input, out _)),
            ("Pidgin", input => PidginParsers.EmailParser.Parse(input).Success)
        ];
        (string Input, bool Expected)[] cases =
        [
            (_emailValue, true),
            ("a_b+c.d-e@host-name.c0", true),
            ("e\u0301@\u203Fhost.\u203F\u0301", true),
            ("x@host.12", true),
            ("", false),
            ("@host.com", false),
            ("x@.com", false),
            ("x@host.c", false),
            ("x@host.co.uk", false),
            ("x@host.c-", false),
            (_emailValue + "!", false),
            (_emailValue + "\n", false),
            (_emailValue + "\r\n", false),
            (" " + _emailValue, false),
            ("x\u200D@host.com", false)
        ];

        foreach (var (name, match) in matchers)
        foreach (var (input, expected) in cases)
            if (match(input) != expected)
                throw new InvalidOperationException($"{name}: unexpected email match for {input}");
    }

    [Benchmark(Baseline = true, Description = "Ramstack")]
    public bool RamstackEmail() =>
        RamstackParsers.EmailParser.TryParse(_emailValue, out _);

    [Benchmark(Description = "Regex")]
    public bool RegexEmail() =>
        EmailRegex.IsMatch(_emailValue);

    [Benchmark(Description = "Regex:Compiled")]
    public bool RegexEmailCompiled() =>
        EmailRegexCompiled.IsMatch(_emailValue);

    [Benchmark(Description = "Regex:Generated")]
    public bool RegexEmailGenerated() =>
        EmailRegexGenerated().IsMatch(_emailValue);

    [Benchmark(Description = "Parlot")]
    public bool ParlotEmail() =>
        ParlotParsers.EmailParser.TryParse(_emailValue, out _);

    [Benchmark(Description = "Parlot:Compiled")]
    public bool ParlotEmailCompiled() =>
        ParlotParsers.EmailParserCompiled.TryParse(_emailValue, out _);

    [Benchmark(Description = "Pidgin")]
    public bool PidginEmail() =>
        PidginParsers.EmailParser.Parse(_emailValue).Success;
}
