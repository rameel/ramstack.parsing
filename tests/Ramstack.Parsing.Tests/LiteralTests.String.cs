namespace Ramstack.Parsing;

partial class LiteralTests
{
    [TestCase("'11111111111111111'", "11111111111111111")]
    [TestCase("'1'", "1")]
    [TestCase("''", "")]
    [TestCase("'1\\u00452\\u00453\\u0045'", "1\u00452\u00453\u0045")]
    [TestCase("'1\\r2\\n3\\t4'", "1\r2\n3\t4")]
    [TestCase("'\\t'", "\t")]
    [TestCase("'\\u1234'", "\u1234")]
    [TestCase("'a\\u1234a'", "a\u1234a")]
    [TestCase("'ab\\u1234ab'", "ab\u1234ab")]
    [TestCase("'abc\\u1234abc'", "abc\u1234abc")]
    public void SingleQuotedStringTest(string input, string expected)
    {
        var parser = Literal.SingleQuotedString;

        Assert.That(parser.Parse(input).Value, Is.EqualTo(expected));
        Assert.That(parser.Map(m => m.ToString()).Parse(input).Value, Is.EqualTo(input));
    }

    [TestCase("\"11111111111111111\"", "11111111111111111")]
    [TestCase("\"1\"", "1")]
    [TestCase("\"\"", "")]
    [TestCase("\"1\\u00452\"", "1\u00452")]
    [TestCase("\"1\\u00452\\u00453\\u0045\"", "1\u00452\u00453\u0045")]
    [TestCase("\"1\\r2\\n3\\t4\"", "1\r2\n3\t4")]
    [TestCase("\"\\t\"", "\t")]
    [TestCase("\"\\u1234\"", "\u1234")]
    [TestCase("\"a\\u1234a\"", "a\u1234a")]
    [TestCase("\"ab\\u1234ab\"", "ab\u1234ab")]
    [TestCase("\"abc\\u1234abc\"", "abc\u1234abc")]
    public void DoubleQuotedStringTest(string input, string expected)
    {
        var parser = Literal.DoubleQuotedString;

        var result = parser.Parse(input);
        Assert.That(result.Value, Is.EqualTo(expected));
        Assert.That(parser.Map(m => m.ToString()).Parse(input).Value, Is.EqualTo(input));
    }

    [TestCase("'124")]
    [TestCase("'\\uabcw'")]
    [TestCase("'\\u123q'")]
    [TestCase("'\\z'")]
    public void SingleQuotedString_Error(string input)
    {
        var parser = Literal.SingleQuotedString;
        Assert.That(parser.Parse(input).Success, Is.False);
    }

    [TestCase("\"124")]
    [TestCase("\"\\uabcw\"")]
    [TestCase("\"\\u123q\"")]
    [TestCase("\"\\z\"")]
    public void DoubleQuotedString_Error(string input)
    {
        var parser = Literal.DoubleQuotedString;
        Assert.That(parser.Parse(input).Success, Is.False);
    }
}
