using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void BetweenTest()
    {
        var parser = Any.Between(L("<["), L("]>"));

        Assert.That(parser.Parse("<[a]>").Success, Is.True);
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("<[a]>").Value, Is.EqualTo((2, 1)));
        Assert.That(parser.Map(m => m.ToString()).Parse("<[a]>").Value, Is.EqualTo("a"));
        Assert.That(parser.Parse("<[a]>").Value, Is.EqualTo('a'));

        Assert.That(parser.Parse("<[]>").Success, Is.False);
        Assert.That(parser.Parse("[<a>]").Success, Is.False);
        Assert.That(parser.Parse("<[a]").Success, Is.False);
        Assert.That(parser.Parse("<[a>").Success, Is.False);
        Assert.That(parser.Parse("[a]>").Success, Is.False);
        Assert.That(parser.Parse("<a]>").Success, Is.False);
        Assert.That(parser.Parse("<a>").Success, Is.False);
        Assert.That(parser.Parse("[a]").Success, Is.False);
        Assert.That(parser.Parse("").Success, Is.False);
    }

    [Test]
    public void BetweenTest_2()
    {
        var parser = Character.Digit.Between(L('[').Repeat(1, 2), L(']'));
        Assert.That(parser.Parse("[1]").Success, Is.EqualTo(true));
        Assert.That(parser.Parse("[[1]").Success, Is.EqualTo(true));
        Assert.That(parser.Parse("[c]").Success, Is.EqualTo(false));
        Assert.That(parser.Parse("[c]").ToString(), Is.EqualTo("(1:2) Expected '[' or digit"));
        Assert.That(parser.Parse("1]").ToString(), Is.EqualTo("(1:1) Expected '['"));
        Assert.That(parser.Parse("(1]").ToString(), Is.EqualTo("(1:1) Expected '['"));
        Assert.That(parser.Parse("[1)").ToString(), Is.EqualTo("(1:3) Expected ']'"));
        Assert.That(parser.Parse("[1").ToString(), Is.EqualTo("(1:3) Expected ']'"));
        Assert.That(parser.Parse("[[1").ToString(), Is.EqualTo("(1:4) Expected ']'"));
        Assert.That(parser.Parse("[(1]").ToString(), Is.EqualTo("(1:2) Expected '[' or digit"));
    }
}
