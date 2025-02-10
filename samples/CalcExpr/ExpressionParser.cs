using System.Diagnostics.CodeAnalysis;

using Ramstack.Parsing;

using static Ramstack.Parsing.Parser;

namespace Samples.CalcExpr;

public static class ExpressionParser
{
    public static readonly Parser<double> Parser = CreateExpressionParser();

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static Parser<double> CreateExpressionParser()
    {
        var sum_expr =
            Deferred<double>();

        var number_expr =
            S.Then(Literal.Number<double>("number"));

        var parenthesis_expr =
            sum_expr.Between(
                Seq(S, L('(')),
                Seq(S, L(')')));

        var primary_expr =
            parenthesis_expr.Or(number_expr);

        var unary_expr =
            Seq(
                S,
                L('-').Optional(),
                primary_expr
            ).Do((_, u, d) => u.HasValue ? -d : d);

        var mul_expr =
            unary_expr.Fold(
                S.Then(OneOf("*/")),
                (l, r, o) => o == '*' ? l * r : l / r);

        sum_expr.Parser =
            mul_expr.Fold(
                S.Then(OneOf("+-")),
                (l, r, o) => o == '+' ? l + r : l - r);

        return sum_expr.ThenIgnore(Eof);
    }
}
