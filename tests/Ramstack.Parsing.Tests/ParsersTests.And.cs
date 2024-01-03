using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void AndTest()
    {
        var parser = And(L('a'));

        Assert.That(parser.Parse("a").Success, Is.True);
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("a").Value, Is.EqualTo((0, 0)));

        Assert.That(parser.Parse("").Success, Is.False);
        Assert.That(parser.Parse("b").Success, Is.False);
    }
}
