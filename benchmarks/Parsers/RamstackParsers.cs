using static Ramstack.Parsing.Character;
using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing.Benchmarks.Parsers;

public static class RamstackParsers
{
    public static readonly Parser<string> EmailParser = CreateEmailParser();
    public static readonly Parser<Unit> EmailVoidParser = EmailParser.Void();
    public static readonly Parser<double> ExpressionParser = Samples.ExpressionParser.Parser;
    public static readonly Parser<object?> JsonParser = Samples.JsonParser.Parser;

    private static Parser<string> CreateEmailParser()
    {
        var parser = Seq(
            Choice(LetterOrDigit, OneOf("_.+-")).OneOrMore(),
            L('@'),
            Choice(LetterOrDigit, L('-')).OneOrMore(),
            L('.'),
            LetterOrDigit.AtLeast(2)
        ).Text();

        return parser;
    }
}
