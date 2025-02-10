using Ramstack.Parsing;

using static Ramstack.Parsing.Parser;

namespace Samples;

public static class ExpressionParser
{
    public static readonly Parser<double> Parser = CreateExpressionParser();

    private static Parser<double> CreateExpressionParser()
    {
        // Grammar:
        // ----------------------------------------
        // Start       :  Sum $
        // Sum         :  Product (S [+-] Product)*
        // Product     :  Unary (S [*/] Unary)*
        // Unary       :  S '-'? Primary
        // Primary     :  Parenthesis / Value
        // Parenthesis :  S '(' Sum S ')'
        // Value       :  S Number
        // S           :  Whitespace*

        var sum = Deferred<double>();
        var number = S.Then(Literal.Number<double>("number"));

        var parenthesis = sum.Between(
            Seq(S, L('(')),
            Seq(S, L(')'))
            );

        var primary = parenthesis.Or(number);

        var unary = Seq(
            S,
            L('-').Optional(),
            primary
        ).Do((_, u, d) => u.HasValue ? -d : d);

        var product = unary.Fold(
            S.Then(OneOf("*/")),
            (l, r, o) => o == '*' ? l * r : l / r);

        sum.Parser = product.Fold(
            S.Then(OneOf("+-")),
            (l, r, o) => o == '+' ? l + r : l - r);

        return sum.ThenIgnore(Eof);
    }
}
