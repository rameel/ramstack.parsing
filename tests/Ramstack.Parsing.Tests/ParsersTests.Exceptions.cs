using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void TryParse_FatalErrorParser_ReturnsFalse()
    {
        var parser = Fail<char>("failure message");

        Assert.That(parser.TryParse("x", out var value), Is.False);
        Assert.That(value, Is.EqualTo('\0'));
    }

    [Test]
    public void TryParse_FatalErrorCallback_ReturnsFalse()
    {
        var parser = L('a').Do(c =>
        {
            if (c == 'a')
                FatalError("forced failure");

            return c;
        });

        Assert.That(parser.TryParse("a", out var value), Is.False);
        Assert.That(value, Is.EqualTo('\0'));

        var result = parser.Parse("a");

        Assert.That(result.ErrorMessage, Is.EqualTo("(1:2) forced failure"));
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void TryParse_FailedMatch_ReturnsFalse()
    {
        var parser = L('a');

        Assert.That(parser.TryParse("b", out var value), Is.False);
        Assert.That(value, Is.EqualTo('\0'));
    }

    [Test]
    public void TryParse_UserCallbackThrows_PropagatesException()
    {
        var parser = L('a').Map<char, char>(_ => throw new InvalidOperationException("callback failure"));

        Assert.That(
            () => parser.TryParse("a", out _),
            Throws
                .TypeOf<InvalidOperationException>()
                .With.Message.EqualTo("callback failure"));
    }

    [Test]
    public void TryParse_UninitializedDeferredParser_ReturnsFalse()
    {
        var parser = Deferred<char>();

        Assert.That(parser.TryParse("x", out var value), Is.False);
        Assert.That(value, Is.EqualTo('\0'));

        Assert.That(
            parser.Parse("x").ErrorMessage,
            Is.EqualTo("(1:1) The deferred parser has not been initialized."));
    }

    [Test]
    public void Parse_UserCallbackThrows_ReportsException()
    {
        var parser = L('a').Map<char, char>(_ => throw new InvalidOperationException("callback failure"));

        var result = parser.Parse("a");

        Assert.That(result.Success, Is.False);
        Assert.That(result.Exception, Is.TypeOf<InvalidOperationException>());
        Assert.That(result.Exception!.Message, Is.EqualTo("callback failure"));
        Assert.That(result.ErrorMessage, Does.Contain("callback failure"));
    }
}
