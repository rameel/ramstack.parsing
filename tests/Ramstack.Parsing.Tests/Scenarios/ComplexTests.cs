using System.Globalization;

namespace Ramstack.Parsing.Scenarios;

using static Parser;

[TestFixture]
public class ComplexTests
{
    [TestCase("-2147483648")]
    [TestCase("2147483647")]
    [TestCase("+2147483647")]
    [TestCase("10")]
    [TestCase("10")]
    [TestCase("13")]
    [TestCase("1024")]
    [TestCase("-10")]
    [TestCase("-0")]
    [TestCase("+0")]
    [TestCase("+1075")]
    [TestCase("",           "(1:1) Expected [+-] or [0-9]")]
    [TestCase("text",       "(1:1) Expected [+-] or [0-9]")]
    [TestCase("+",          "(1:2) Expected [0-9]")]
    [TestCase("-",          "(1:2) Expected [0-9]")]
    [TestCase("~10",        "(1:1) Expected [+-] or [0-9]")]
    [TestCase("+text",      "(1:2) Expected [0-9]")]
    [TestCase("-text",      "(1:2) Expected [0-9]")]
    [TestCase("10 ",        "(1:3) Expected [0-9] or end of input")]
    [TestCase(" 10",        "(1:1) Expected [+-] or [0-9]")]
    [TestCase("12345678+0", "(1:9) Expected [0-9] or end of input")]
    public void ParseDecimalNumberTest(string expression, string error = "")
    {
        // [+-]? ['0'..'9']+ EOF

        var parser = Seq(
            Set("+-").Optional(),
            Set("0-9").OneOrMore(),
            Eof).Map(s => int.Parse(s));

        if (int.TryParse(expression, out var value) && !string.IsNullOrEmpty(error))
            value = 0;

        ParserTest(parser, expression, string.IsNullOrEmpty(error), value, error);
    }

    [Test]
    public void ParseFixedFloatTest()
    {
        // [0-9]+ ('.' [0-9]+)? EOL

        var digit = Set("0-9");
        var value = Seq(
            digit.OneOrMore(),
            Seq(L('.'), digit.OneOrMore()).Optional(),
            Eof).Map(s => double.Parse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture));

        Assert.That(value.Parse("1").Value, Is.EqualTo( 1.00));
        Assert.That(value.Parse("10.72").Value, Is.EqualTo(10.72));
        Assert.That(value.Parse("1.75").Value, Is.EqualTo( 1.75));
        Assert.That(value.Parse("1.00").Value, Is.EqualTo( 1.00));

        Assert.That(value.Parse("1.").Success, Is.False);
        Assert.That(value.Parse("1 d").Success, Is.False);
        Assert.That(value.Parse("1.0d").Success, Is.False);
        Assert.That(value.Parse("1.0 d").Success, Is.False);
        Assert.That(value.Parse("1. d").Success, Is.False);
        Assert.That(value.Parse("1.d").Success, Is.False);
        Assert.That(value.Parse(".1").Success, Is.False);
        Assert.That(value.Parse("").Success, Is.False);
        Assert.That(value.Parse(" ").Success, Is.False);
        Assert.That(value.Parse(".").Success, Is.False);
        Assert.That(value.Parse("c").Success, Is.False);
    }

    private static void ParserTest<T>(Parser<T> parser, string expression, bool success, T value, string error)
    {
        var result = parser.Parse(expression);

        Assert.That(result.Success,    Is.EqualTo(success),                             $"#1: {expression}");
        Assert.That(result.Value,      Is.EqualTo(success ? value : default),           $"#2: {expression}");
        Assert.That(result.ToString(), Is.EqualTo(success ? value!.ToString() : error), $"#3: {expression}");
    }
}
