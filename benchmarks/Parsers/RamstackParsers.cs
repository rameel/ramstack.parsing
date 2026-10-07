using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing.Benchmarks.Parsers;

public static class RamstackParsers
{
    public static readonly Parser<Unit> EmailParser = CreateEmailParser();
    public static readonly Parser<double> ExpressionParser = Samples.CalcExpr.ExpressionParser.Parser;
    public static readonly Parser<object?> JsonParser = Samples.Json.JsonParser.Parser;

    private static Parser<Unit> CreateEmailParser()
    {
        var parser = Seq(
            Set("\\w.+-").OneOrMore(),
            L('@'),
            Set("\\w-").OneOrMore(),
            L('.'),
            Set("\\w").AtLeast(2)
        ).Void().ThenIgnore(Eof);

        return parser;
    }
}
