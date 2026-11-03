using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [TestCase("x")]
    [TestCase("xb")]
    [TestCase("xa")]
    [TestCase("xac")]
    public void ThenIgnore_FailedMatch_ResetsValue(string source)
    {
        AssertFailedValue(L('a').ThenIgnore(L('b')), source);
        AssertFailedValue(L('a').Do(c => c.ToString()).ThenIgnore(L('b')), source);
        AssertFailedValue(L('a').Do(c => (c, 42)).ThenIgnore(L('b')), source);
    }

    [TestCase("named")]
    [TestCase("where")]
    [TestCase("deferred")]
    public void TryParse_WrappedFailedMatch_ResetsValue(string wrapper)
    {
        var parser = L('a').ThenIgnore(L('b'));
        switch (wrapper)
        {
            case "named":
                parser = parser.As("token");
                break;
            case "where":
                parser = parser.Where(_ => true, "token");
                break;
            case "deferred":
                var deferred = Deferred<char>();
                deferred.Parser = parser;
                parser = deferred;
                break;
        }

        AssertFailedValue(parser, "xac");
    }

    private static void AssertFailedValue<T>(Parser<T> parser, string source)
    {
        var context = new ParseContext(source);
        context.Advance(1);

        Assert.That(parser.TryParse(ref context, out var value), Is.False);
        Assert.That(value, Is.EqualTo(default(T)));
        Assert.That(context.Position, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Length, Is.Zero);

        Assert.That(parser.TryParse(source.AsSpan(1), out value), Is.False);
        Assert.That(value, Is.EqualTo(default(T)));
    }
}
