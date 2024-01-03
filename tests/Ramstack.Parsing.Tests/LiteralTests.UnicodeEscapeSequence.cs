using static Ramstack.Parsing.Parser;
using static Ramstack.Parsing.Literal;

namespace Ramstack.Parsing;

partial class LiteralTests
{
    [TestCase(@"\u0063", '\u0063')]
    [TestCase(@"\u0041", '\u0041')]
    [TestCase(@"\u0020", '\u0020')]
    [TestCase(@"\uDD20", '\uDD20')]
    [TestCase(@"\u0000", '\u0000')]
    [TestCase(@"\uFFFF", '\uFFFF')]
    [TestCase(@"\uBEEF", '\uBEEF')]
    [TestCase(@"\ubeef", '\uBEEF')]
    [TestCase(@"\u1234", '\u1234')]
    [TestCase(@"\u4321", '\u4321')]
    [TestCase(@"\uabcd", '\uABCD')]
    [TestCase(@"\uDCBA", '\uDCBA')]
    public void UnicodeEscapeSequenceTest(string input, char symbol)
    {
        var parser = UnicodeEscapeSequence.Before(Eof);
        Assert.That(parser.Parse(input).Success, Is.True);
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(input).Value, Is.EqualTo((0, 6)));
        Assert.That(parser.Parse(input).Value, Is.EqualTo(symbol));
        Assert.That(parser.Map(m => m.ToString()).Parse(input).Value, Is.EqualTo(input));
    }

    [TestCase(@"\u")]
    [TestCase(@"\u0")]
    [TestCase(@"\u01")]
    [TestCase(@"\u012")]
    [TestCase(@"\u012g")]
    [TestCase(@"\uqqqq")]
    [TestCase(@"\u000q")]
    [TestCase(@"\u00q0")]
    [TestCase(@"\u0q00")]
    [TestCase(@"\uq000")]
    [TestCase(@"\uя000")]
    [TestCase(@"\u0я00")]
    [TestCase(@"\u00я0")]
    [TestCase(@"\u000я")]
    public void UnicodeEscapeSequence_InvalidInput(string input)
    {
        var parser = UnicodeEscapeSequence.Before(Eof);
        Assert.That(parser.Parse(input).Success, Is.False);
    }
}
