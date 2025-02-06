using System.Text.RegularExpressions;

using BenchmarkDotNet.Attributes;

namespace Ramstack.Parsing.Benchmarks;

[MemoryDiagnoser]
[OperationsPerSecond]
public partial class LiteralsBenchmark
{
    private const string RegexExpr = "^(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while)$";

    [GeneratedRegex(RegexExpr)]
    private static partial Regex LiteralsRegexGenerated();
    private static readonly Regex LiteralsRegexCompiled = new Regex(RegexExpr, RegexOptions.Compiled);
    private static readonly Regex LiteralsRegex = new Regex(RegexExpr);

    private static readonly Parser<Unit> RamstackLiteralParser = Parser.OneOf(RegexExpr[1..^1].Split('|')).Void();

    [Params("sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch")]
    // [Params(
    //     "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default",
    //     "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto",
    //     "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out",
    //     "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc",
    //     "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
    //     "virtual", "void", "volatile", "while"
    // )]
    public string Input { get; set; } = null!;

    [Benchmark(Description = "Regex")]
    public bool RegexEmail() =>
        LiteralsRegex.IsMatch(Input);

    [Benchmark(Description = "RegexCompiled")]
    public bool RegexEmailCompiled() =>
        LiteralsRegexCompiled.IsMatch(Input);

    [Benchmark(Description = "RegexGenerated")]
    public bool RegexEmailGenerated() =>
        LiteralsRegexGenerated().IsMatch(Input);

    [Benchmark(Description = "Ramstack.Parsing")]
    public bool RamstackParsing() =>
        RamstackLiteralParser.TryParse(Input, out _);
}
