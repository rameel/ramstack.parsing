using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void Fold_NoInputConsumed_StopsWithoutReducing([Values] bool rightAssociative, [Values] bool discardResult)
    {
        var operand = new BoundedParser<int>(Return(2));
        var op = Return('-');

        var reductions = 0;
        var reduce = (int l, int r, char _) =>
        {
            reductions++;
            return l - r;
        };

        var parser = rightAssociative
            ? operand.FoldR(op, reduce)
            : operand.Fold(op, reduce);

        var context = new ParseContext("!tail");
        context.Advance(1);

        if (discardResult)
        {
            Assert.That(parser.Void().TryParse(ref context, out _), Is.True);
        }
        else
        {
            Assert.That(parser.TryParse(ref context, out var value), Is.True);
            Assert.That(value, Is.EqualTo(2));
        }

        Assert.That(reductions, Is.Zero);
        Assert.That(context.Position, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Index, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Length, Is.Zero);
        Assert.That(context.Remaining.ToString(), Is.EqualTo("tail"));
    }

    [TestCase(false, 6)]
    [TestCase(true, 8)]
    public void Fold_EmptyPairAfterProgress_SkipsReduction(bool rightAssociative, int expected)
    {
        var operand = new BoundedParser<int>(
            Literal.Number<int>().DefaultOnFail(2)
            );

        var op = L('-').DefaultOnFail('-');

        var reductions = 0;
        var reduce = (int l, int r, char _) =>
        {
            reductions++;
            return l - r;
        };

        var parser = rightAssociative
            ? operand.FoldR(op, reduce)
            : operand.Fold(op, reduce);

        var result = parser.Parse("10-3-1!");
        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(expected));
        Assert.That(result.Length, Is.EqualTo(6));
        Assert.That(reductions, Is.EqualTo(2));

        var discarded = parser.Void().Parse("10-3-1!");
        Assert.That(discarded.Success, Is.True);
        Assert.That(discarded.Length, Is.EqualTo(6));
        Assert.That(reductions, Is.EqualTo(2));
    }

    [TestCase(true, false, "123!", -4)]
    [TestCase(true, true, "123!", 2)]
    [TestCase(false, false, "--!", -2)]
    [TestCase(false, true, "--!", 2)]
    public void Fold_OnlyOneParserConsumesInput_Continues(bool operandConsumes, bool rightAssociative, string source, int expected)
    {
        var operand = operandConsumes
            ? Set('0', '9').Do(c => c - '0')
            : Return(2);

        var op = operandConsumes
            ? Return('-')
            : L('-');

        var parser = rightAssociative
            ? operand.FoldR(op, (l, r, _) => l - r)
            : operand.Fold(op, (l, r, _) => l - r);

        var result = parser.Parse(source);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(expected));
        Assert.That(result.Length, Is.EqualTo(source.Length - 1));

        var discarded = parser.Void().Parse(source);
        Assert.That(discarded.Success, Is.True);
        Assert.That(discarded.Length, Is.EqualTo(result.Length));
    }

    [TestCase(0, 1, false, false)]
    [TestCase(0, 1, false, true)]
    [TestCase(0, 1, true, false)]
    [TestCase(0, 1, true, true)]
    [TestCase(1, 1, false, false)]
    [TestCase(1, 1, false, true)]
    [TestCase(1, 1, true, false)]
    [TestCase(1, 1, true, true)]
    [TestCase(3, 3, false, false)]
    [TestCase(3, 3, false, true)]
    [TestCase(3, 3, true, false)]
    [TestCase(3, 3, true, true)]
    public void Separated_NoInputConsumed_StopsAfterMinimumReached(int min, int expectedCount, bool allowTrailing, bool discardResult)
    {
        var item = new BoundedParser<int>(Return(7));
        var parser = item.Separated(Return(','), allowTrailing, min);
        var context = new ParseContext("!tail");
        context.Advance(1);

        if (discardResult)
        {
            Assert.That(parser.Void().TryParse(ref context, out _), Is.True);
        }
        else
        {
            Assert.That(parser.TryParse(ref context, out var value), Is.True);
            Assert.That(value, Is.EqualTo(Enumerable.Repeat(7, expectedCount)));
        }

        Assert.That(context.Position, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Index, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Length, Is.Zero);
        Assert.That(context.Remaining.ToString(), Is.EqualTo("tail"));
    }

    [TestCase(true, "abc!", "abc", false)]
    [TestCase(true, "abc!", "abc", true)]
    [TestCase(false, ",,!", "xxx", false)]
    [TestCase(false, ",,!", "xxx", true)]
    public void Separated_OnlyOneParserConsumesInput_Continues(bool itemConsumes, string source, string expected, bool allowTrailing)
    {
        var item = itemConsumes ? Set('a', 'z') : Return('x');
        var separator = itemConsumes ? Return(',') : L(',');
        var parser = item.Separated(separator, allowTrailing);

        var result = parser.Parse(source);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(expected.ToCharArray()));
        Assert.That(result.Length, Is.EqualTo(source.Length - 1));

        var discarded = parser.Void().Parse(source);
        Assert.That(discarded.Success, Is.True);
        Assert.That(discarded.Length, Is.EqualTo(result.Length));
    }

    [TestCase(false, 1, false)]
    [TestCase(false, 1, true)]
    [TestCase(true, 2, false)]
    [TestCase(true, 2, true)]
    public void Separated_MaximumReached_RespectsTrailingSeparator(bool allowTrailing, int expectedLength, bool discardResult)
    {
        var parser = L('a').Separated(L(','), allowTrailing, min: 1, max: 1);
        var context = new ParseContext("a,a");

        if (discardResult)
        {
            Assert.That(parser.Void().TryParse(ref context, out _), Is.True);
        }
        else
        {
            Assert.That(parser.TryParse(ref context, out var value), Is.True);
            Assert.That(value, Is.EqualTo(new[] { 'a' }));
        }

        Assert.That(context.Position, Is.EqualTo(expectedLength));
        Assert.That(context.MatchedSegment.Index, Is.Zero);
        Assert.That(context.MatchedSegment.Length, Is.EqualTo(expectedLength));
    }

    [Test]
    public void Separated_MinimumNotReached_RestoresPosition([Values] bool allowTrailing, [Values] bool discardResult)
    {
        var parser = L('a').Separated(L(','), allowTrailing, min: 2);
        var context = new ParseContext("!a,?");
        context.Advance(1);

        if (discardResult)
        {
            Assert.That(parser.Void().TryParse(ref context, out _), Is.False);
        }
        else
        {
            Assert.That(parser.TryParse(ref context, out var value), Is.False);
            Assert.That(value, Is.Null);
        }

        Assert.That(context.Position, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Length, Is.Zero);
        Assert.That(context.Remaining.ToString(), Is.EqualTo("a,?"));
        Assert.That(context.ToString(), Is.EqualTo("(1:4) Expected 'a'"));
    }

    [TestCase(-1, 1, "min")]
    [TestCase(0, -1, "max")]
    [TestCase(0, 0, "max")]
    [TestCase(2, 1, "min")]
    public void Separated_InvalidBounds_ThrowsArgumentOutOfRangeException(int min, int max, string parameter)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => L('a').Separated(L(','), min: min, max: max));
        Assert.That(exception!.ParamName, Is.EqualTo(parameter));
    }

    #region Inner type: BoundedParser

    // Bound parser invocations so a missing progress check fails instead of hanging the test run.
    private sealed class BoundedParser<T>(Parser<T> parser) : Parser<T>
    {
        private int _attempts;

        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            if (++_attempts > 16)
                throw new InvalidOperationException(
                    "The repetition did not stop after matching empty input.");

            return parser.TryParse(ref context, out value);
        }
    }

    #endregion
}
