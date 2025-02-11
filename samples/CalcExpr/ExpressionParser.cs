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
            Literal.Number<double>("number").ThenIgnore(S);

        var parenthesis_expr =
            sum_expr.Between(
                Seq(L('('), S),
                Seq(L(')'), S));

        var primary_expr =
            parenthesis_expr.Or(number_expr);

        var unary_expr =
            Seq(
                L('-').Optional(),
                S,
                primary_expr
            ).Do((u, _, d) => u.HasValue ? -d : d);

        var mul_expr =
            unary_expr.Fold(
                OneOf("*/").ThenIgnore(S),
                (l, r, o) => o == '*' ? l * r : l / r);

        sum_expr.Parser =
            mul_expr.Fold(
                OneOf("+-").ThenIgnore(S),
                (l, r, o) => o == '+' ? l + r : l - r);

        return sum_expr.Between(S, Eof);
    }
}
