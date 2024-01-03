using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [TestCase("", true, 0, 0, "")]
    [TestCase("1", false, 0, 0, null)]
    [TestCase("\n", true, 0, 1, "\n")]
    [TestCase("\r", true, 0, 1, "\r")]
    [TestCase("\r\n", true, 0, 2, "\r\n")]
    [TestCase("\n\n", true, 0, 1, "\n")]
    [TestCase("\n\r", true, 0, 1, "\n")]
    [TestCase("\r\r", true, 0, 1, "\r")]
    [TestCase("\u0085", true, 0, 1, "\u0085")]
    [TestCase("\u2028", true, 0, 1, "\u2028")]
    [TestCase("\u2029", true, 0, 1, "\u2029")]
    public void EolTest(string input, bool succeed, int index, int length, string value)
    {
        Assert.That(Eol.Parse(input).Success, Is.EqualTo(succeed));

        if (!succeed)
            return;

        Assert.That(Eol.Map(m => (m.Index, m.Length)).Parse(input).Value, Is.EqualTo((index, length)));
        Assert.That(Eol.Map(m => m.ToString()).Parse(input).Value, Is.EqualTo(value));
    }
}
