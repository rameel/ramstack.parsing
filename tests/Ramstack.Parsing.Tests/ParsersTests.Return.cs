using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void Return_MatchedSegment_IsEmpty()
    {
        Assert.That(Return(42).Map(m => (m.Index, m.Length)).Parse("").Value, Is.EqualTo((0, 0)));
        Assert.That(Return(42).Map(m => m.ToString()).Parse("").Value, Is.Empty);
        Assert.That(Return(42).Text().Parse("abc").Value, Is.Empty);
    }

    [Test]
    public void Return_AfterMatch_DoesNotReuseStaleMatchedSegment()
    {
        var text = L("abc").Then(Return(42).Text());
        Assert.That(text.Parse("abc").Value, Is.Empty);

        var segment = L("abc").Then(Return(42).Map(m => (m.Index, m.Length)));
        Assert.That(segment.Parse("abc").Value, Is.EqualTo((3, 0)));
    }
}
