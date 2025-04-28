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
        // Expr        :  S Sum EOF
        // Sum         :  Product ([+-] S Product)*
        // Product     :  Primary ([*/] S Primary)*
        // Primary     :  Parenthesis / Value
        // Parenthesis :  '(' S Sum ')' S
        // Value       :  Number S

        var sum = Deferred<double>();

        var digit =
            Set("0-9");

        var value =
            Seq(
                digit.OneOrMore(),
                Seq(L('.'), digit.OneOrMore()).Optional()
            ).ThenIgnore(S).Map(m =>
                double.Parse(m, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture));

        var parenthesis =
            sum.Between(
                Seq(L('('), S),
                Seq(L(')'), S));

        var primary =
            Choice(value, parenthesis);

        var product =
            Seq(
                primary,
                Seq(
                    OneOf("*/"),
                    S,
                    primary
                ).Many()
            ).Do(Multiply);

        sum.Parser =
            Seq(
                product,
                Seq(
                    OneOf("+-"),
                    S,
                    product
                ).Many()
            ).Do(Add);

        var expression =
            sum.Between(S, Eof);

        static double Multiply(double v, List<(char, Unit, double)> results)
        {
            foreach (var (op, _, d) in results)
                v = op == '*' ? v * d : v / d;

            return v;
        }

        static double Add(double v, List<(char, Unit, double)> results)
        {
            foreach (var (op, _, d) in results)
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
