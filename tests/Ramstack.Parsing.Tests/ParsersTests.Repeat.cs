using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void Repeat_0_1_Test()
    {
        var parser = L('a').Repeat(0, 1);

        Assert.That(parser.Parse("").Success, Is.True);
        Assert.That(parser.Parse("").Value, Is.Empty);
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("").Value, Is.EqualTo((0, 0)));

        Assert.That(parser.Parse("a").Success, Is.True);
        Assert.That(parser.Parse("a").Value, Is.EquivalentTo("a"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("a").Value, Is.EqualTo((0, 1)));
    }

    [Test]
    public void Repeat_1_1_Test()
    {
        var parser = L('a').Repeat(1, 1);

        Assert.That(parser.Parse("a").Success, Is.True);
        Assert.That(parser.Parse("a").Value, Is.EquivalentTo("a"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("a").Value, Is.EqualTo((0, 1)));

        Assert.That(parser.Parse("").Success, Is.False);
    }

    [Test]
    public void Repeat_5_10_Test()
    {
        var parser = L('a').Repeat(5, 10);

        Assert.That(parser.Parse("aaaaa").Success, Is.True);
        Assert.That(parser.Parse("aaaaaa").Success, Is.True);
        Assert.That(parser.Parse("aaaaaaa").Success, Is.True);
        Assert.That(parser.Parse("aaaaaaaa").Success, Is.True);
        Assert.That(parser.Parse("aaaaaaaaa").Success, Is.True);
        Assert.That(parser.Parse("aaaaaaaaaa").Success, Is.True);
        Assert.That(parser.Parse("aaaaaaaaaaa").Success, Is.True);

        Assert.That(parser.Parse("aaaaa").Value, Is.EquivalentTo("aaaaa"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("aaaaa").Value, Is.EqualTo((0, 5)));

        Assert.That(parser.Parse("aaaaaaaaaaaaaaaaaaaa").Value, Is.EquivalentTo("aaaaaaaaaa"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("aaaaaaaaaaaaaaa").Value, Is.EqualTo((0, 10)));

        Assert.That(parser.Parse("aaaa").Success, Is.False);
    }
}
