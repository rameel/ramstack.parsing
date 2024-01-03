using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void OptionalTest()
    {
        var parser = L('a').Optional();

        Assert.That(parser.Parse("a").Success, Is.True);
        Assert.That(parser.Parse("b").Success, Is.True);
        Assert.That(parser.Parse("a").Value.HasValue, Is.True);
        Assert.That(parser.Parse("a").Value.Value, Is.EqualTo('a'));
        Assert.That(parser.Parse("b").Value.HasValue, Is.False);
        Assert.That(parser.Parse("b").Value.Value, Is.EqualTo('\0'));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("abc").Value, Is.EqualTo((0, 1)));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("bac").Value, Is.EqualTo((0, 0)));
    }
}
