using System.Globalization;

namespace Ramstack.Parsing.Scenarios;

using static Parser;

[TestFixture]
public class SimpleCalcTests
{
    [TestCase("1",                     1.0)]
    [TestCase("(1.00)",                1.0)]
    [TestCase("1 ",                    1.0)]
    [TestCase(" 1 ",                   1.0)]
    [TestCase("1.0 ",                  1.0)]
    [TestCase("1 + 9",                 10d)]
    [TestCase("(1 + 9)",               10d)]
    [TestCase("2*(1 + 9)",             20d)]
    [TestCase("(1 + 9)*2",             20d)]
    [TestCase("(1 + 9) * 3",           30d)]
    [TestCase("1 + 9*2",               19d)]
    [TestCase("1 + 9*2 + 1",           20d)]
    [TestCase("1 + 1 - 1 + 2 - 2",     1.0)]
    [TestCase("3*5/5*5/5",             3.0)]
    [TestCase("3* 5/5 *5/ 3",          5.0)]
    [TestCase("3 * 5 / 5 * 5 / 3",     5.0)]
    [TestCase("(1.2+2.8)*(4+2)/(2*4)", 3.0)]

    [TestCase("1.2+",  0, "(1:5) Expected [0-9] or '('")]
    [TestCase("(1",    0, "(1:3) Expected [0-9], '.', [*/], [+-], or ')'")]
    [TestCase("1+",    0, "(1:3) Expected [0-9] or '('")]
    [TestCase("1)",    0, "(1:2) Expected [0-9], '.', [*/], [+-], or end of input")]
    [TestCase("(1+)",  0, "(1:4) Expected [0-9] or '('")]
    [TestCase("1+)",   0, "(1:3) Expected [0-9] or '('")]
    [TestCase("1+1,2", 0, "(1:4) Expected [0-9], '.', [*/], [+-], or end of input")]
    [TestCase("( )",   0, "(1:3) Expected [0-9] or '('")]
    [TestCase("()",    0, "(1:2) Expected [0-9] or '('")]
    public void SimpleCalcTest(string text, double number, string error = "")
    {
        // Expr        :  Sum S EOF
        // Sum         :  Product (S [+-] Product)*
        // Product     :  Primary (S [*/] Primary)*
        // Primary     :  Parenthesis / Value
        // Parenthesis :  S '(' Sum S ')'
        // Value       :  S Number

        var sum = Deferred<double>();

        // S ['0'..'9']+ ('.' ['0'..'9']+)?
        var digit = Set("0-9");
        var value = S.Then(
            Seq(
                digit.OneOrMore(),
                Seq(L('.'), digit.OneOrMore()).Optional())
            ).Map(m => double.Parse(m, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture));

        // S '(' Sum S ')'
        var parenthesis = sum.Between(Seq(S, L('(')), Seq(S, L(')')));

        // value / parenthesis
        var primary = Choice(value, parenthesis);

        // Primary (S [*/] Primary)*
        var product = Seq(primary, Seq(S, OneOf("*/"), primary).Many()).Do(Multiply);

        // Product (S [+-] Product)*
        sum.Parser = Seq(product, Seq(S, OneOf("+-"), product).Many()).Do(Add);

        // Sum S EOF
        var expression = sum.Before(Seq(S, Eof));

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

        ParserTest(expression, text, string.IsNullOrEmpty(error), number, error);
    }

    private static void ParserTest<T>(Parser<T> parser, string expression, bool success, T value, string error)
    {
        var result = parser.Parse(expression);

        Assert.That(result.Success,    Is.EqualTo(success),                             $"#1: {expression}");
        Assert.That(result.Value,      Is.EqualTo(success ? value : default),           $"#2: {expression}");
        Assert.That(result.ToString(), Is.EqualTo(success ? value!.ToString() : error), $"#3: {expression}");
    }
}
