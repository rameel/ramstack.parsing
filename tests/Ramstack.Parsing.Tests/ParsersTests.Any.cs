using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void AnyTest()
    {
        Assert.That(Any.Parse("1").Success, Is.True);
        Assert.That(Any.Map(m => (m.Index, m.Length)).Parse("1").Value, Is.EqualTo((0, 1)));
        Assert.That(Any.Map(m => m.ToString()).Parse("1").Value, Is.EqualTo("1"));
        Assert.That(Any.Parse("1").Value, Is.EqualTo('1'));
        Assert.That(Any.Parse("").Success, Is.False);
        Assert.That(Any.Then(Any).Parse("12").Success, Is.True);
        Assert.That(Any.Then(Any).Map(m => (m.Index, m.Length)).Parse("12").Value, Is.EqualTo((1, 1)));
        Assert.That(Any.Then(Any).Map(m => m.ToString()).Parse("12").Value, Is.EqualTo("2"));
    }
}
