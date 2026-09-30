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

    [Test]
    public void As_Success_PreservesLaterDiagnostics()
    {
        var parser = L('a').As("letter").ThenIgnore(L('b'));

        Assert.That(parser.Parse("ax").ErrorMessage, Is.EqualTo("(1:2) Expected 'b'"));
    }
}
