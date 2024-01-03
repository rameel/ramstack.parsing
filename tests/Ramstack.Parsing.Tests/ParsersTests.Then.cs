using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void ThenTest()
    {
        var parser = Any.Then(Any);

        Assert.That(parser.Parse("12").Success, Is.True);
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("12").Value, Is.EqualTo((1, 1)));
        Assert.That(parser.Map(m => m.ToString()).Parse("12").Value, Is.EqualTo("2"));

        Assert.That(parser.Parse("1").Success, Is.False);
        Assert.That(parser.Parse("").Success, Is.False);
    }
}
