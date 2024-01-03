using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void AsTest()
    {
        Assert.That(
            Any.As("character").Parse("").ErrorMessage,
            Is.EqualTo("(1:1) Expected character"));
    }
}
