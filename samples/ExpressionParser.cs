using Ramstack.Parsing;
using Ramstack.Parsing.Collections;

using static Ramstack.Parsing.Parser;

namespace Samples;

public static class ExpressionParser
{
    public static readonly Parser<double> Parser = CreateExpressionParser();

    private static Parser<double> CreateExpressionParser()
    {
        // Expr        :  Sum
        // Sum         :  Product (S [+-] Product)*
        // Product     :  Unary (S [*/] Unary)*
        // Unary       :  S -? Primary
        // Primary     :  Parenthesis / Value
        // Parenthesis :  S '(' Sum S ')'
        // Value       :  S Number

        var sum = Deferred<double>();
        var value = S.Then(Literal.Number<double>("number"));

        var parenthesis = sum.Between(
            Seq(S, L('(')),
            Seq(S, L(')'))
            );

        var primary = parenthesis.Or(value);

        var unary = Seq(
            S,
            L('-').Optional(),
            primary
            ).Do((_, u, d) => u.HasValue ? -d : d);

        var product = Seq(
            unary,
            Seq(S, OneOf("*/"), unary).Many()
            ).Do(Multiply);

        sum.Parser = Seq(
            product,
            Seq(S, OneOf("+-"), product).Many()
            ).Do(Add);

        return sum.Before(Eof);

        static double Multiply(double v, ArrayList<(Unit, char, double)> results)
        {
            foreach (var (_, op, d) in results)
                v = op == '*' ? v * d : v / d;

            return v;
        }

        static double Add(double v, ArrayList<(Unit, char, double)> results)
        {
            foreach (var (_, op, d) in results)
                v = op == '+' ? v + d : v - d;

            return v;
        }
    }
}
