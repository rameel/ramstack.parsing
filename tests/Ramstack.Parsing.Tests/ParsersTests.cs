namespace Ramstack.Parsing;

[TestFixture]
public partial class ParsersTests
{
    [TestCase("", 0)]
    [TestCase("a\nb", 2)]
    public void ToString_NoDiagnostics_ReturnsEmptyString(string source, int position)
    {
        var context = new ParseContext(source);
        context.Advance(position);

        Assert.That(context.ToString(), Is.Empty);
    }

    [Test]
    public void ToString_RecordedDiagnostics_PreservesErrorPositionAfterRollback()
    {
        var context = new ParseContext("a\nb");
        var bookmark = context.BookmarkPosition();

        context.Advance(2);
        context.ReportExpected("digit");
        context.RestorePosition(bookmark);

        Assert.That(context.Position, Is.Zero);
        Assert.That(context.ToString(), Is.EqualTo("(2:1) Expected digit"));
    }

    [Test]
    public void ToString_SuccessfulOptionalMatch_PreservesDiagnostics()
    {
        var parser = Parser.L('a').Optional();
        var context = new ParseContext("b");

        Assert.That(parser.TryParse(ref context, out var value), Is.True);
        Assert.That(value.HasValue, Is.False);
        Assert.That(context.ToString(), Is.EqualTo("(1:1) Expected 'a'"));

        var result = parser.Parse("b");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value.HasValue, Is.False);
        Assert.That(result.Length, Is.Zero);
        Assert.That(result.ErrorMessage, Is.Null);
    }

    [Test]
    public void Parse_FailedMatchWithoutDiagnostics_ReturnsGenericFailure()
    {
        var result = new NonReportingParser().Parse("x");

        Assert.That(result.Success, Is.False);
        Assert.That(result.Length, Is.Zero);
        Assert.That(result.ErrorMessage, Is.EqualTo("Parsing failed."));
        Assert.That(result.ToString(), Is.EqualTo("Parsing failed."));
        Assert.That(result.Exception, Is.Null);
    }

    #region Inner type: NonReportingParser

    private sealed class NonReportingParser : Parser<char>
    {
        public override bool TryParse(ref ParseContext context, out char value)
        {
            value = '\0';
            return false;
        }
    }

    #endregion

    #region Inner type: BoundedParser

    // Bound parser invocations so a missing progress check fails instead of hanging the test run.
    private sealed class BoundedParser<T>(Parser<T> parser) : Parser<T>
    {
        public int Attempts { get; private set; }

        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            if (++Attempts > 16)
                throw new InvalidOperationException(
                    "The repetition did not stop after matching empty input.");

            return parser.TryParse(ref context, out value);
        }
    }

    #endregion
}
