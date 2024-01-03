using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void DefaultOnFail_ValueType()
    {
        var parser = L('a').DefaultOnFail();

        Assert.That(parser.Parse("a").Success, Is.True);
        Assert.That(parser.Parse("b").Success, Is.True);
        Assert.That(parser.Parse("a").Value, Is.EqualTo('a'));
        Assert.That(parser.Parse("b").Value, Is.EqualTo('\0'));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("abc").Value, Is.EqualTo((0, 1)));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("bac").Value, Is.EqualTo((0, 0)));

        parser = parser.DefaultOnFail('!');
        Assert.That(parser.Parse("b").Value, Is.EqualTo('!'));
    }

    [Test]
    public void DefaultOnFail_ReferenceType()
    {
        var parser = L("true").DefaultOnFail();

        Assert.That(parser.Parse("true").Success, Is.True);
        Assert.That(parser.Parse("test").Success, Is.True);
        Assert.That(parser.Parse("true").Value, Is.EqualTo("true"));
        Assert.That(parser.Parse("test").Value, Is.Null);
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("true").Value, Is.EqualTo((0, 4)));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("test").Value, Is.EqualTo((0, 0)));

        Parser<string> newParser = parser.DefaultOnFail("Wow!")!;
        Assert.That(newParser.Parse("test").Value, Is.EqualTo("Wow!"));
    }
}
