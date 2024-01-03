using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void FailTest()
    {
        var parser = Fail<Unit>("failure message");

        Assert.That(parser.Parse("1").Success, Is.False);
        Assert.That(parser.Parse("1").ErrorMessage, Is.EqualTo("(1:1) failure message"));
    }
}
