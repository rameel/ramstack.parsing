namespace Ramstack.Parsing;

partial class LiteralTests
{
    [TestCase(@"\""", '"')]
    [TestCase(@"\'", '\'')]
    [TestCase(@"\/", '/')]
    [TestCase(@"\\", '\\')]
    [TestCase(@"\0", '\0')]
    [TestCase(@"\a", '\a')]
    [TestCase(@"\b", '\b')]
    [TestCase(@"\e", '\e')]
    [TestCase(@"\f", '\f')]
    [TestCase(@"\n", '\n')]
    [TestCase(@"\r", '\r')]
    [TestCase(@"\t", '\t')]
    [TestCase(@"\v", '\v')]
    public void EscapeSequenceTest(string source, char expected)
    {
        Assert.That(
            Literal.EscapeSequence.Parse(source).Value,
            Is.EqualTo(expected));
    }
}
