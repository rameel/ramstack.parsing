using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [TestCase("")]
    [TestCase("1")]
    [TestCase("\n")]
    public void EofTest(string input)
    {
        Assert.That(Eof.Parse(input).Success, Is.EqualTo(input.Length == 0));

        if (input.Length != 0)
            return;

        Assert.That(Eol.Map(m => (m.Index, m.Length)).Parse(input).Value, Is.EqualTo((0, 0)));
        Assert.That(Eol.Map(m => m.ToString()).Parse(input).Value, Is.Empty);
    }

    [Test]
    public void Eof_TrailingInput_ReportsEndOfInput()
    {
        var parser = L('4').ThenIgnore(Eof);

        Assert.That(parser.Parse("4x").ErrorMessage, Is.EqualTo("(1:2) Expected end of input"));
    }
}
