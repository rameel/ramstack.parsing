using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void NotTest()
    {
        var parser = Not(L('a'));

        Assert.That(parser.Parse("b").Success, Is.True);
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("b").Value, Is.EqualTo((0, 0)));

        Assert.That(parser.Parse("").Success, Is.True);
        Assert.That(parser.Parse("a").Success, Is.False);
    }
}
