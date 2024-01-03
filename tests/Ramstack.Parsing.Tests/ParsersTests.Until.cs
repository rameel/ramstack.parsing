using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void UntilTest()
    {
        var parser = Choice(
            Set("0-9"),
            Set("a-zA-Z")
            ).Until(Seq(L(','), S));

        Assert.That(parser.Parse("123,").Success, Is.True);
        Assert.That(parser.Parse("abc,").Success, Is.True);
        Assert.That(parser.Parse("abc,").Value, Is.EquivalentTo("abc"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("123,abc").Value, Is.EqualTo((0, 3)));
    }
}
