using System.Globalization;

using Pidgin;
using Pidgin.Expression;

using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace Ramstack.Parsing.Benchmarks.Parsers;

public static class PidginParsers
{
    public static readonly Parser<char, double> ExpressionParser =
        ExprParserImpl.Instance.Between(SkipWhitespaces).Before(End);

    public static readonly Parser<char, Pidgin.Unit> EmailParser =
        Token(c => IsWordCharacter(c) || c is '.' or '+' or '-').SkipAtLeastOnce()
            .Before(Char('@'))
            .Before(Token(c => IsWordCharacter(c) || c == '-').SkipAtLeastOnce())
            .Before(Char('.'))
            .Before(Token(IsWordCharacter))
            .Before(Token(IsWordCharacter))
            .Before(Token(IsWordCharacter).SkipMany())
            .Before(End);

    private static bool IsWordCharacter(char c) =>
        char.IsLetterOrDigit(c) || char.GetUnicodeCategory(c) is
            UnicodeCategory.NonSpacingMark or UnicodeCategory.ConnectorPunctuation;

    private static class ExprParserImpl
    {
        private static Parser<char, T> Tok<T>(Parser<char, T> token) => Try(token).Before(SkipWhitespaces);
        private static Parser<char, string> Tok(string token) => Tok(String(token));
        private static Parser<char, T> Parenthesized<T>(Parser<char, T> parser) => parser.Between(Tok("("), Tok(")"));
        private static Parser<char, Func<double, double, double>> Binary(Parser<char, char> op) =>
            op.Select<Func<double, double, double>>(type => (l, r) =>
            {
                return type switch
                {
                    '+' => l + r,
                    '-' => l - r,
                    '*' => l * r,
                    _   => l / r
                };
            }
        );

        private static readonly Parser<char, Func<double, double, double>> Add = Binary(Tok("+").ThenReturn('+'));
        private static readonly Parser<char, Func<double, double, double>> Sub = Binary(Tok("-").ThenReturn('-'));
        private static readonly Parser<char, Func<double, double, double>> Mul = Binary(Tok("*").ThenReturn('*'));
        private static readonly Parser<char, Func<double, double, double>> Div = Binary(Tok("/").ThenReturn('/'));
        private static readonly Parser<char, double> Literal = Tok(Real).Labelled("decimal literal");

        public static readonly Parser<char, double> Instance = Pidgin.Expression.ExpressionParser.Build<char, double>(
            expr => (
                // unary => "-"? primary
                Tok("-").Optional().Then(
                    OneOf(
                        Literal,
                        Parenthesized(expr).Labelled("parenthesized expression")
                    ),
                    (minus, x) => minus.HasValue ? -x : x),
                [
                    Operator.InfixL(Mul).And(Operator.InfixL(Div)),
                    Operator.InfixL(Add).And(Operator.InfixL(Sub))
                ]
            )
        ).Labelled("expression");
    }

    public static class JsonParser
    {
        private static readonly Parser<char, char> LBrace = Char('{');
        private static readonly Parser<char, char> RBrace = Char('}');
        private static readonly Parser<char, char> LBracket = Char('[');
        private static readonly Parser<char, char> RBracket = Char(']');
        private static readonly Parser<char, char> Quote = Char('"');
        private static readonly Parser<char, char> Colon = Char(':');
        private static readonly Parser<char, char> ColonWhitespace =
            Colon.Between(SkipWhitespaces);

        private static readonly Parser<char, char> Comma = Char(',');
        private static readonly Parser<char, char> HexDigit =
            Token(c => c
                is >= '0' and <= '9'
                or >= 'A' and <= 'F'
                or >= 'a' and <= 'f');

        private static readonly Parser<char, string> String =
            Rec(() =>
                OneOf(
                    AnyCharExcept('"', '\\'),
                    Char('\\')
                        .Then(
                            OneOf(
                                Char('"').Select (_ => '"'),
                                Char('\\').Select(_ => '\\'),
                                Char('/').Select (_ => '/'),
                                Char('b').Select (_ => '\b'),
                                Char('f').Select (_ => '\f'),
                                Char('n').Select (_ => '\n'),
                                Char('r').Select (_ => '\r'),
                                Char('t').Select (_ => '\t'),
                                Char('u').Then(HexDigit.Repeat(4)
                                    .Select(h => (char)int.Parse(new string(h.ToArray()), NumberStyles.HexNumber, CultureInfo.InvariantCulture)))
                            )
                        )
                ).ManyString().Between(Quote)
            );
        private static readonly Parser<char, object?> JsonString =
            String.Select<object?>(s => s);

        private static readonly Parser<char, object?> JsonNumber =
            Real.Select<object?>(n => n);

        private static readonly Parser<char, object?> JsonPrimitive =
            String("true")
                .Or(String("false"))
                .Or(String("null"))
                .Select<object?>(s =>
                    {
                        object? r = null;
                        if (s.Length >= 4 && s[0] != 'n')
                            r = s[0] == 't';

                        return r;
                    });

        private static readonly Parser<char, object?> Json =
            OneOf(
                JsonString,
                JsonNumber,
                JsonPrimitive,
                Rec(() => JsonArray!),
                Rec(() => JsonObject!)
            );

        private static readonly Parser<char, object?> JsonArray =
            Json.Between(SkipWhitespaces)
                .Separated(Comma)
                .Between(LBracket, RBracket)
                .Select<object?>(v => v as List<object?> ?? v.ToList());

        private static readonly Parser<char, KeyValuePair<string, object?>> JsonMember =
            String
                .Before(ColonWhitespace)
                .Then(Json, (name, val) => new KeyValuePair<string, object?>(name, val));

        private static readonly Parser<char, object?> JsonObject =
            JsonMember.Between(SkipWhitespaces)
                .Separated(Comma)
                .Between(LBrace, RBrace)
                .Select<object?>(members => members.ToDictionary());

        private static readonly Parser<char, object?> Document =
            Json.Between(SkipWhitespaces).Before(End);

        public static object? Parse(string input) => Document.ParseOrThrow(input);

        public static bool TryParse(string input, out object? value)
        {
            var result = Document.Parse(input);
            value = result.Success ? result.Value : null;
            return result.Success;
        }
    }
}
