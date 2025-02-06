using System.Text.RegularExpressions;

using BenchmarkDotNet.Attributes;

using Pidgin;

using Ramstack.Parsing.Benchmarks.Parsers;

namespace Ramstack.Parsing.Benchmarks;

[MemoryDiagnoser]
[OperationsPerSecond]
public partial class EmailBenchmark
{
    private static readonly string Email = "development.team-2021@example.com";

    private static readonly Regex EmailRegex = new Regex(@"^[\w._+-]+@[\w-]+\.\w{2,}$");
    private static readonly Regex EmailRegexCompiled = new Regex((@"^[\w._+-]+@[\w-]+\.\w{2,}$"), RegexOptions.Compiled);

    [GeneratedRegex(@"^[\w._+-]+@[\w-]+\.\w{2,}$")]
    private static partial Regex EmailRegexGenerated();

    [GlobalSetup]
    public void Setup()
    {
        if (!EmailRegex.IsMatch(Email)) throw new Exception(nameof(EmailRegex));
        if (!EmailRegexCompiled.IsMatch(Email)) throw new Exception(nameof(EmailRegexCompiled));
        if (!EmailRegexGenerated().IsMatch(Email)) throw new Exception(nameof(EmailRegexGenerated));
        if (!RamstackParsers.EmailParser.TryParse(Email, out _)) throw new Exception("RamstackParsers.EmailParser");
        if (!ParlotParsers.EmailParser.TryParse(Email, out _)) throw new Exception("ParlotParsers.EmailParser");
        if (!ParlotParsers.EmailParserCompiled.TryParse(Email, out _)) throw new Exception("ParlotParsers.EmailParserCompiled");
        PidginParsers.EmailParser.ParseOrThrow(Email);
    }

    [Benchmark(Description = "Ramstack")]
    public bool RamstackEmail() =>
        RamstackParsers.EmailVoidParser.TryParse(Email, out _);

    [Benchmark(Description = "Regex")]
    public bool RegexEmail() =>
        EmailRegex.IsMatch(Email);

    [Benchmark(Description = "Regex:Compiled")]
    public bool RegexEmailCompiled() =>
        EmailRegexCompiled.IsMatch(Email);

    [Benchmark(Description = "Regex:Generated")]
    public bool RegexEmailGenerated() =>
        EmailRegexGenerated().IsMatch(Email);

    [Benchmark(Description = "Parlot")]
    public bool ParlotEmail() =>
        ParlotParsers.EmailParser.TryParse(Email, out _);

    [Benchmark(Description = "Parlot:Compiled")]
    public bool ParlotEmailCompiled() =>
        ParlotParsers.EmailParserCompiled.TryParse(Email, out _);

    [Benchmark(Description = "Pidgin")]
    public bool PidginEmail() =>
        PidginParsers.EmailParser.Parse(Email).Success;
}
