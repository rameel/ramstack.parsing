using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [TestCase("", 0, 0, "")]
    [TestCase("1", 0, 0, "")]
    [TestCase(" ", 0, 1, " ")]
    [TestCase("\t", 0, 1, "\t")]
    [TestCase("\r", 0, 1, "\r")]
    [TestCase("\n", 0, 1, "\n")]
    [TestCase("\r\n", 0, 2, "\r\n")]
    [TestCase("    ", 0, 4, "    ")]
    [TestCase(" \r\n\t", 0, 4, " \r\n\t")]
    public void STest(string input, int index, int length, string value)
    {
        Assert.That(S.Parse(input).Success, Is.True);
        Assert.That(S.Map(m => (m.Index, m.Length)).Parse(input).Value, Is.EqualTo((index, length)));
        Assert.That(S.Map(m => m.ToString()).Parse(input).Value, Is.EqualTo(value));
    }
}
