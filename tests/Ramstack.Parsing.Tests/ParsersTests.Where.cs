using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [TestCase("0", 0)]
    [TestCase("49", 49)]
    [TestCase("123", 123)]
    public void Where_AcceptedValue_ReturnsValue(string input, int expected)
    {
        var parser = L('!')
            .Then(
                Literal
                    .Number<int>()
                    .ThenIgnore(S))
            .Where(
                static n => n is >= 0 and <= 255,
                "octet");

        var context = new ParseContext($"!{input} \t?");

        Assert.That(parser.TryParse(ref context, out var value), Is.True);
        Assert.That(value, Is.EqualTo(expected));
        Assert.That(context.Position, Is.EqualTo(input.Length + 3));
        Assert.That(context.MatchedSegment.ToString(), Is.EqualTo(input));
        Assert.That(context.Remaining.ToString(), Is.EqualTo("?"));
    }

    [TestCase("-1")]
    [TestCase("256")]
    public void Where_RejectedValue_RestoresPosition(string input)
    {
        var parser = L('!')
            .Then(S)
            .Then(
                Literal
                    .Number<int>()
                    .ThenIgnore(S))
            .Where(
                static n => n is >= 0 and <= 255,
                "octet");

        var context = new ParseContext($"! \t{input} ?");

        Assert.That(parser.TryParse(ref context, out var value), Is.False);
        Assert.That(value, Is.Zero);
        Assert.That(context.Position, Is.Zero);
        Assert.That(context.MatchedSegment.Length, Is.Zero);
        Assert.That(context.MatchedSegment.ToString(), Is.Empty);
        Assert.That(context.Remaining.ToString(), Is.EqualTo(context.Source.ToString()));
        Assert.That(context.ToString(), Is.EqualTo("(1:1) Expected octet"));
    }

    [TestCase(false, "(1:2) Expected 'c'")]
    [TestCase(true,  "(1:4) Expected 'c'")]
    public void Where_InnerFailure_SkipsPredicateAndPreservesDiagnostics(bool composite, string error)
    {
        var calls = 0;
        var inner = composite
            ? L("ab").Then(L('c'))
            : L('c');

        var parser = L('!').Then(inner.Where(_ =>
        {
            calls++;
            return true;
        }, "validated character"));

        var context = new ParseContext("!abx");

        Assert.That(parser.TryParse(ref context, out var value), Is.False);
        Assert.That(value, Is.EqualTo('\0'));
        Assert.That(calls, Is.Zero);
        Assert.That(context.Position, Is.Zero);
        Assert.That(context.MatchedSegment.Length, Is.Zero);
        Assert.That(context.ToString(), Is.EqualTo(error));
    }

    [Test]
    public void Where_RejectedAlternative_ParsesFallbackFromOriginalPosition()
    {
        var number = Literal.Number<int>();
        var parser = L('!')
            .Then(number
                .Where(n => n <= 100, "small integer")
                .Or(number));

        var context = new ParseContext("!150?");

        Assert.That(parser.TryParse(ref context, out var value), Is.True);
        Assert.That(value, Is.EqualTo(150));
        Assert.That(context.Position, Is.EqualTo(4));
        Assert.That((ValueTuple<int, int>)context.MatchedSegment, Is.EqualTo((1, 3)));
    }

    [Test]
    public void Where_RejectedFirstAlternative_ParsesFallback()
    {
        var parserA = L('a').Do(_ => 1).Where(n => n == 2, "two");
        var parserB = L('a').Do(_ => 2);

        var parser = parserA.Or(parserB);
        var result = parser.Parse("a");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(2));
    }

    [Test]
    public void Where_OptionalRejection_LeavesInputForNextParser()
    {
        var parser = Seq(
            L('a').Where(_ => false, "accepted character").Optional(),
            L('a').Optional());

        var context = new ParseContext("a");

        Assert.That(parser.TryParse(ref context, out var value), Is.True);
        Assert.That(value.Value1.HasValue, Is.False);
        Assert.That(value.Value2.HasValue, Is.True);
        Assert.That(value.Value2.Value, Is.EqualTo('a'));
    }

    [Test]
    public void Where_RepetitionRejection_StopsBeforeRejectedValue()
    {
        var parser = Character
            .Digit.Where(
                static c => c != '3',
                "digit other than three")
            .ZeroOrMore();

        var context = new ParseContext("123");

        Assert.That(parser.TryParse(ref context, out var value), Is.True);
        Assert.That(value, Is.EqualTo(['1', '2']));

        Assert.That(context.Position, Is.EqualTo(2));
        Assert.That(context.MatchedSegment.ToString(), Is.EqualTo("12"));
        Assert.That(context.Remaining.ToString(), Is.EqualTo("3"));

        Assert.That(
            parser.Void().Then(L('3')).ThenIgnore(Eof).TryParse("123", out _),
            Is.True);
    }

    [TestCase("42", true, 42)]
    [TestCase("256", false, 256)]
    public void Where_VoidAndSuppressedDiagnostics_PreserveRequiredTransformations(string input, bool accepted, int expected)
    {
        var mapCalls = 0;
        var doCalls = 0;
        var predicateCalls = 0;

        var parser = Character
            .Digit.OneOrMore()
            .Map(m =>
            {
                mapCalls++;
                return m.ToString();
            })
            .Do(text =>
            {
                doCalls++;
                return int.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
            })
            .Where(n =>
            {
                predicateCalls++;
                Assert.That(n, Is.EqualTo(expected));
                return n is >= 0 and <= 255;
            }, "octet")
            .Void();

        var context = new ParseContext(input)
        {
            DiagnosticState = DiagnosticState.Suppressed
        };

        Assert.That(parser.TryParse(ref context, out _), Is.EqualTo(accepted));

        Assert.That(mapCalls, Is.EqualTo(1));
        Assert.That(doCalls, Is.EqualTo(1));
        Assert.That(predicateCalls, Is.EqualTo(1));

        Assert.That(context.Position, Is.EqualTo(accepted ? input.Length : 0));
        Assert.That(context.MatchedSegment.Length, Is.EqualTo(accepted ? input.Length : 0));
    }

    [TestCase("42", true, null)]
    [TestCase("256", false, "(1:1) Expected octet")]
    [TestCase("abc", false, "(1:1) Expected integer")]
    [TestCase("42xyz", false, "(1:3) Expected end of input")]
    public void Where_CompleteInput_ReportsPredicateInnerOrEofFailure(string input, bool accepted, string? error)
    {
        var parser = Literal
            .Number<int>("integer")
            .Where(
                n => n is >= 0 and <= 255,
                "octet")
            .ThenIgnore(Eof);

        var result = parser.Parse(input);
        var voidResult = parser.Void().Parse(input);

        Assert.That(result.Success, Is.EqualTo(accepted));
        Assert.That(result.ErrorMessage, Is.EqualTo(error));

        Assert.That(voidResult.Success, Is.EqualTo(accepted));
        Assert.That(voidResult.ErrorMessage, Is.EqualTo(error));

        Assert.That(parser.TryParse(input, out _), Is.EqualTo(accepted));
    }

    [Test]
    public void Where_AlternativeFailures_MergeExpectationsAtSamePosition()
    {
        var parser = L('a').Where(_ => false, "accepted character").Or(L('b'));

        Assert.That(parser.Parse("a").ErrorMessage, Is.EqualTo("(1:1) Expected accepted character or 'b'"));
    }

    [Test]
    public void Where_FartherInnerDiagnostic_TakesPrecedenceOverPredicateRejection()
    {
        var parser = L('a').ThenIgnore(L('b').Optional()).Where(_ => false, "accepted character");

        Assert.That(parser.Parse("a?").ErrorMessage, Is.EqualTo("(1:2) Expected 'b'"));
    }

    [Test]
    public void Where_FartherExistingDiagnostic_IsPreservedAfterRollback()
    {
        var parser =
            L("ab").Then(L('c'))
                .Or(L('a').Where(_ => false, "accepted character"));

        Assert.That(parser.Parse("ab?").ErrorMessage, Is.EqualTo("(1:3) Expected 'c'"));
    }

    [TestCase("ab")]
    [TestCase("ax")]
    public void Where_NamedParser_ReplacesInnerAndPredicateExpectations(string input)
    {
        var parser = L('a')
            .Then(L('b'))
            .Where(_ => false, "accepted character")
            .As("rule");

        Assert.That(parser.Parse(input).ErrorMessage, Is.EqualTo("(1:1) Expected rule"));
        Assert.That(parser.Void().Parse(input).ErrorMessage, Is.EqualTo("(1:1) Expected rule"));
        Assert.That(parser.Void().As("renamed").Parse(input).ErrorMessage, Is.EqualTo("(1:1) Expected renamed"));
    }

    [TestCase("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "(1:1) Expected identifier of at most 64 characters", 1)]
    [TestCase("int", "(1:1) Expected non-reserved identifier", 2)]
    [TestCase("foo__bar", "(1:1) Expected identifier without double underscore", 3)]
    [TestCase("foo_bar", null, 3)]
    public void Where_ChainedChecks_ReportFirstFailingStage(string input, string? error, int executedStages)
    {
        var stageCalls = new int[3];
        var reserved = new HashSet<string> { "class", "int", "return" };

        // An unnamed lexeme would leave the trailing expectation of ZeroOrMore
        // at the end of input, and by the farthest-error policy it would shadow
        // the expected messages of the chain.
        var parser = Seq(
                Set("A-Za-z_"),
                Set("A-Za-z0-9_").ZeroOrMore())
            .Text()
            .As("identifier")
            .Where(s =>
            {
                stageCalls[0]++;
                return s.Length <= 64;
            }, "identifier of at most 64 characters")
            .Where(s =>
            {
                stageCalls[1]++;
                return !reserved.Contains(s);
            }, "non-reserved identifier")
            .Where(s =>
            {
                stageCalls[2]++;
                return !s.Contains("__", StringComparison.Ordinal);
            }, "identifier without double underscore");

        var result = parser.Parse(input);

        Assert.That(result.Success, Is.EqualTo(error is null), result.ErrorMessage);
        Assert.That(result.ErrorMessage, Is.EqualTo(error));

        var expectedCalls = Enumerable
            .Range(0, stageCalls.Length)
            .Select(i => i < executedStages ? 1 : 0)
            .ToArray();

        Assert.That(stageCalls, Is.EqualTo(expectedCalls));
    }

    [Test]
    public void Where_ChainedChecks_KeepBaseValidatorMessages()
    {
        var octet = Literal
            .Number<int>("integer")
            .Where(n => n is >= 0 and <= 255, "octet");

        var nonZeroOctet = octet
            .Where(n => n != 0, "non-zero octet");

        Assert.That(octet.Parse("0").Success, Is.True);
        Assert.That(nonZeroOctet.Parse("7").Success, Is.True);
        Assert.That(nonZeroOctet.Parse("0").ErrorMessage, Is.EqualTo("(1:1) Expected non-zero octet"));
        Assert.That(nonZeroOctet.Parse("256").ErrorMessage, Is.EqualTo("(1:1) Expected octet"));
    }

    [Test]
    public void Where_PredicateThrows_UsesCallbackExceptionContract()
    {
        var exception = new InvalidOperationException("predicate failure");
        var fallbackCalls = 0;

        var parser = L('a')
            .Where(_ => throw exception, "accepted character")
            .Or(L('a').Do(c =>
            {
                fallbackCalls++;
                return c;
            }));

        var result = parser.Parse("a");

        Assert.That(result.Success, Is.False);
        Assert.That(result.Exception, Is.SameAs(exception));
        Assert.That(result.ErrorMessage, Does.StartWith("(1:2) ").And.Contain("predicate failure"));

        Assert.That(Assert.Throws<InvalidOperationException>(() => parser.TryParse("a", out _)), Is.SameAs(exception));
        Assert.That(parser.Void().Parse("a").Exception, Is.SameAs(exception));
        Assert.That(Assert.Throws<InvalidOperationException>(() => parser.Void().TryParse("a", out _)), Is.SameAs(exception));
        Assert.That(fallbackCalls, Is.Zero);
    }
}
