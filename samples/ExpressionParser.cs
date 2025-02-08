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
            Seq(S, OneOf("*/"), unary),
            (v, _, op, d) => op == '*' ? v * d : v / d);

        sum.Parser = product.Fold(
            Seq(S, OneOf("+-"), product),
            (v, _, op, d) => op == '+' ? v + d : v - d);

        return sum.Before(Eof);
    }
}
