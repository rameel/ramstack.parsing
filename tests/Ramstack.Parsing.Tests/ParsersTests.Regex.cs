#if NET7_0_OR_GREATER

using System.Text.RegularExpressions;

using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void RegexTest()
    {
        var parser = L('-').Then(Regex(new Regex(@"^\d+")));

        Assert.That(parser.Parse("-12345abc").Success, Is.True);
        Assert.That(parser.Parse("-a12345abc").Success, Is.False);
        Assert.That(parser.Parse("-12345abc").Value, Is.EqualTo("12345"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("-12345abc").Value, Is.EqualTo((1, 5)));
    }

    [Test]
    public void Regex_Failed_IfNotStartsWithAnchor()
    {
        var list = new[] { new Regex(@"^\d+") };
        foreach (var expr in list)
        {
            var parser = L('-').Then(Regex(expr));

            Assert.That(parser.Parse("-12345abc").Success, Is.True);
            Assert.That(parser.Parse("-a12345abc").Success, Is.False);
            Assert.That(parser.Parse("-12345abc").Value, Is.EqualTo("12345"));
            Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("-12345abc").Value, Is.EqualTo((1, 5)));
        }
    }
}

#endif
