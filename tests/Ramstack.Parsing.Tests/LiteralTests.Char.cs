namespace Ramstack.Parsing;

partial class LiteralTests
{
    [TestCase("'1'", '1')]
    [TestCase("'\\u0045'", '\u0045')]
    [TestCase("'\\r'", '\r')]
    [TestCase("'\\n'", '\n')]
    [TestCase("'\\t'", '\t')]
    public void QuotedCharacterTest(string input, char expected)
    {
        Assert.That(
            Literal.QuotedCharacter.Parse(input).Value,
            Is.EqualTo(expected));
    }

    [TestCase("'1")]
    [TestCase("'\\uabcw'")]
    [TestCase("'\\u123q'")]
    [TestCase("'\\z'")]
    [TestCase("'\r'")]
    [TestCase("'\n'")]
    [TestCase("'\''")]
    [TestCase("'a")]
    [TestCase("'ab'")]
    public void QuotedCharacter_Error(string input)
    {
        var parser = Literal.QuotedCharacter;
        Assert.That(parser.Parse(input).Success, Is.False);
    }
}
