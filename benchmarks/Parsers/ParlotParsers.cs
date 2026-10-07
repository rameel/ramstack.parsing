using Parlot;
using Parlot.Fluent;

using static Parlot.Fluent.Parsers;

namespace Ramstack.Parsing.Benchmarks.Parsers;

public static class ParlotParsers
{
    public static readonly Parlot.Fluent.Parser<TextSpan> EmailParser = CreateEmailParser();
    public static readonly Parlot.Fluent.Parser<TextSpan> EmailParserCompiled = EmailParser.Compile();
    public static readonly Parlot.Fluent.Parser<double> ExpressionParser = CreateExpressionParser();
    public static readonly Parlot.Fluent.Parser<double> ExpressionParserCompiled = ExpressionParser.Compile();
    public static readonly Parlot.Fluent.Parser<object?> JsonParser = CreateJsonParser();
    public static readonly Parlot.Fluent.Parser<object?> JsonParserCompiled = JsonParser.Compile();

    private static Parlot.Fluent.Parser<TextSpan> CreateEmailParser()
    {
        return Capture(
            Literals.Pattern(c => IsWordCharacter(c) || c is '.' or '+' or '-', minSize: 1)
                .And(Literals.Char('@'))
                .And(Literals.Pattern(c => IsWordCharacter(c) || c == '-', minSize: 1))
                .And(Literals.Char('.'))
                .And(Literals.Pattern(IsWordCharacter, minSize: 2))
                .Eof());
    }

    private static bool IsWordCharacter(char c) =>
        char.IsLetterOrDigit(c) || char.GetUnicodeCategory(c) is
            System.Globalization.UnicodeCategory.NonSpacingMark or
            System.Globalization.UnicodeCategory.ConnectorPunctuation;

    private static Parlot.Fluent.Parser<double> CreateExpressionParser()
    {
        /*
         * Grammar:
         * The top declaration has a lower priority than the lower one.
         *
         * additive       => multiplicative ( ( "-" | "+" ) multiplicative )* ;
         * multiplicative => unary ( ( "/" | "*" ) unary )* ;
         * unary          => "-"? primary ;
         * primary        => NUMBER
         *                   | "(" expression ")" ;
         */

        // The Deferred helper creates a parser that can be referenced by others before it is defined
        var expression = Deferred<double>();

        var number = Terms.Number<double>(NumberOptions.Float);
        var divided = Terms.Char('/');
        var times = Terms.Char('*');
        var minus = Terms.Char('-');
        var plus = Terms.Char('+');
        var openParen = Terms.Char('(');
        var closeParen = Terms.Char(')');

        // "(" expression ")"
        var groupExpression = Between(openParen, expression, closeParen).Named("group");

        // primary => NUMBER | "(" expression ")";
        var primary = number.Or(groupExpression).Named("primary");

        // unary => "-"? primary;
        var unary = minus.Optional().And(primary)
            .Then(x => x.Item1.HasValue ? -x.Item2 : x.Item2)
            .Named("unary");

        // multiplicative => unary ( ( "/" | "*" ) unary )* ;
        var multiplicative = unary.LeftAssociative(
            (divided, (a, b) => a / b),
            (times, (a, b) => a * b)
        ).Named("multiplicative");

        // additive => multiplicative(("-" | "+") multiplicative) * ;
        var additive = multiplicative.LeftAssociative(
            (plus, (a, b) => a + b),
            (minus, (a, b) => a - b)
        ).Named("additive");

        expression.Parser = additive;
        return expression.Named("expression")
            .AndSkip(Literals.Pattern(char.IsWhiteSpace).Optional()).Eof();
    }

    private static Parlot.Fluent.Parser<object?> CreateJsonParser()
    {
        var value = Deferred<object?>();

        var colon = Terms.Char(':');
        var comma = Terms.Char(',');

        var text = Terms
            .String(StringLiteralQuotes.Double)
            .Then(object? (v) => v.ToString());

        var number = Terms
            .Number<double>(NumberOptions.Float)
            .Then(object? (v) => v);

        var boolTrue = Terms
            .Text("true")
            .Then(object? (_) => true);

        var boolFalse = Terms
            .Text("false")
            .Then(object? (_) => false);

        var nullValue = Terms
            .Text("null")
            .Then(object? (_) => null);

        var emptyArray =
            Terms.Char('[')
                 .And(Terms.Char(']'))
                 .Then(object? (_) => new List<object?>());

        var array =
            Between(
                Terms.Char('['),
                Separated(comma, value),
                Terms.Char(']')
                ).Then(object? (list) => list);

        var member =
            text
                .And(colon)
                .And(value)
                .Then(member => KeyValuePair.Create(
                    member.Item1!.ToString()!,
                    member.Item3));

        var emptyMap =
            Terms.Char('{')
                .And(Terms.Char('}'))
                .Then(object? (_) => new Dictionary<string, object?>());

        var map =
            Between(
                Terms.Char('{'),
                Separated(comma, member),
                Terms.Char('}')
                ).Then(object? (members) => members.ToDictionary());

        value.Parser = text
            .Or(number)
            .Or(boolTrue)
            .Or(boolFalse)
            .Or(nullValue)
            .Or(emptyArray)
            .Or(array)
            .Or(emptyMap)
            .Or(map);

        return value.AndSkip(Literals.Pattern(char.IsWhiteSpace).Optional()).Eof();
    }
}
