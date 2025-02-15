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

    [Test]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public void Repeat_InfiniteLoopPrevention_ZeroLength()
    {
        var digit = Character.Digit;
        var digit_list = digit.Separated(L(','));
        var index_list = digit_list.Between(L('['), L(']')).Many();

        Assert.That(index_list.Parse("[]").Length, Is.EqualTo(2));
        Assert.That(index_list.Parse("[][]").Length, Is.EqualTo(4));
        Assert.That(index_list.Parse("[][][]").Length, Is.EqualTo(6));

        Assert.That(index_list.Parse("[1,2][1,2][2,6]").Length, Is.EqualTo(15));
        Assert.That(index_list.Parse("[][][1,2][][][1,2][][][2,6][][]").Length, Is.EqualTo(31));
    }

    [Test]
    public void Repeat_InfiniteLoopPrevention_ZeroConsuming()
    {
        var parser = And(Character.Digit).AtLeast(2);

        Assert.That(parser.Parse("1234567890").Success, Is.True);
        Assert.That(parser.Parse("1234567890").Length, Is.Zero);
        Assert.That(parser.Text().Parse("1234567890").Value, Is.Empty);
    }
}
