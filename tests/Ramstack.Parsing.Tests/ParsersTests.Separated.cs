using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void Separated__0_1_DisallowTrailing()
    {
        var parser = Set("0-9").Separated(L(','), max: 1);

        Assert.That(parser.Parse("1").Success, Is.True);
        Assert.That(parser.Parse("1").Value, Is.EquivalentTo("1"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("1").Value, Is.EqualTo((0, 1)));

        Assert.That(parser.Parse("1,").Success, Is.True);
        Assert.That(parser.Parse("1,").Value, Is.EquivalentTo("1"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("1,").Value, Is.EqualTo((0, 1)));

        Assert.That(parser.Parse("1,2").Success, Is.True);
        Assert.That(parser.Parse("1,2").Value, Is.EquivalentTo("1"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("1,2").Value, Is.EqualTo((0, 1)));

        Assert.That(parser.Parse("12").Success, Is.True);
        Assert.That(parser.Parse("12").Value, Is.EquivalentTo("1"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("12").Value, Is.EqualTo((0, 1)));

        foreach (var s in new[] { "", "a", ",", ",1" })
        {
            Assert.That(parser.Parse(s).Success, Is.True);
            Assert.That(parser.Parse(s).Value, Is.Empty);
            Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(s).Value, Is.EqualTo((0, 0)));
        }
    }

    [Test]
    public void Separated__0_1_AllowTrailing()
    {
        var parser = Set("0-9").Separated(L(','), max: 1, allowTrailing: true);

        Assert.That(parser.Parse("1").Success, Is.True);
        Assert.That(parser.Parse("1").Value, Is.EquivalentTo("1"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("1").Value, Is.EqualTo((0, 1)));

        Assert.That(parser.Parse("1,").Success, Is.True);
        Assert.That(parser.Parse("1,").Value, Is.EquivalentTo("1"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("1,").Value, Is.EqualTo((0, 2)));

        Assert.That(parser.Parse("1,2").Success, Is.True);
        Assert.That(parser.Parse("1,2").Value, Is.EquivalentTo("1"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("1,2").Value, Is.EqualTo((0, 2)));

        Assert.That(parser.Parse("12").Success, Is.True);
        Assert.That(parser.Parse("12").Value, Is.EquivalentTo("1"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("1").Value, Is.EqualTo((0, 1)));

        foreach (var s in new[] { "", "a", ",", ",1" })
        {
            Assert.That(parser.Parse(s).Success, Is.True);
            Assert.That(parser.Parse(s).Value, Is.Empty);
            Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(s).Value, Is.EqualTo((0, 0)));
        }
    }

    [Test]
    public void Separated_DisallowTrailing()
    {
        var parser = Choice(
            Set("0-9"),
            Set("a-zA-Z")
            ).OneOrMore()
             .Separated(
                 Seq(L(','), S));

        foreach (var s in new[] { "123", "123,", "123,abc", "123,abc,", "123, abc", "123, abc, "})
        {
            Assert.That(
                parser.Parse(s).Success,
                Is.True);

            Assert.That(
                parser.Map(m => (m.Index, m.Length)).Parse(s).Value,
                Is.EqualTo((0, s.TrimEnd(' ', ',').Length)));
        }
    }

    [Test]
    public void Separated_AllowTrailing()
    {
        var parser = Choice(
            Set("0-9"),
            Set("a-zA-Z")
            ).OneOrMore()
             .Separated(
                 Seq(L(','), S),
                 allowTrailing: true);

        foreach (var s in new[] { "123", "123,", "123,abc", "123,abc,", "123, abc", "123, abc, "})
        {
            Assert.That(parser.Parse(s).Success, Is.True);
            Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(s).Value, Is.EqualTo((0, s.Length)));
        }
    }

    [TestCase(0, 1, false)]
    [TestCase(0, 1, true)]
    [TestCase(1, 1, false)]
    [TestCase(1, 1, true)]
    [TestCase(3, 3, false)]
    [TestCase(3, 3, true)]
    public void Separated_NoInputConsumed_StopsAfterMinimumReached(int min, int expectedCount, bool allowTrailing)
    {
        var item = new BoundedParser<int>(Return(7));
        var parser = item.Separated(Return(','), allowTrailing, min);
        var context = new ParseContext("!tail");
        context.Advance(1);

        var success = parser.TryParse(ref context, out var value);

        Assert.That(success, Is.True);
        Assert.That(value, Is.EqualTo(Enumerable.Repeat(7, expectedCount)));
        Assert.That(context.Position, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Index, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Length, Is.Zero);
        Assert.That(context.Remaining.ToString(), Is.EqualTo("tail"));
    }

    [TestCase(0, 1, false)]
    [TestCase(0, 1, true)]
    [TestCase(1, 1, false)]
    [TestCase(1, 1, true)]
    [TestCase(3, 3, false)]
    [TestCase(3, 3, true)]
    public void SeparatedVoid_NoInputConsumed_StopsAfterMinimumReached(int min, int expectedCount, bool allowTrailing)
    {
        var item = new BoundedParser<int>(Return(7));
        var parser = item.Separated(Return(','), allowTrailing, min).Void();
        var context = new ParseContext("!tail");
        context.Advance(1);

        var success = parser.TryParse(ref context, out _);

        Assert.That(success, Is.True);
        Assert.That(item.Attempts, Is.EqualTo(expectedCount));
        Assert.That(context.Position, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Index, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Length, Is.Zero);
        Assert.That(context.Remaining.ToString(), Is.EqualTo("tail"));
    }

    [Test]
    public void Separated_TrailingEmptyPair_StopsAfterEmptyItem([Values] bool allowTrailing)
    {
        var item = new BoundedParser<int>(
            Literal.Number<int>().DefaultOnFail(7)
            );

        var parser = item.Separated(Return(','), allowTrailing);
        var context = new ParseContext("!12?");
        context.Advance(1);

        var success = parser.TryParse(ref context, out var value);

        Assert.That(success, Is.True);
        Assert.That(value, Is.EqualTo(new[] { 12, 7 }));
        Assert.That(item.Attempts, Is.EqualTo(2));
        Assert.That(context.Position, Is.EqualTo(3));
        Assert.That(context.MatchedSegment.Index, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Length, Is.EqualTo(2));
        Assert.That(context.Remaining.ToString(), Is.EqualTo("?"));
    }

    [Test]
    public void SeparatedVoid_TrailingEmptyPair_StopsAfterEmptyItem([Values] bool allowTrailing)
    {
        var item = new BoundedParser<int>(
            Literal.Number<int>().DefaultOnFail(7)
            );

        var parser = item.Separated(Return(','), allowTrailing).Void();
        var context = new ParseContext("!12?");
        context.Advance(1);

        var success = parser.TryParse(ref context, out _);

        Assert.That(success, Is.True);
        Assert.That(item.Attempts, Is.EqualTo(2));
        Assert.That(context.Position, Is.EqualTo(3));
        Assert.That(context.MatchedSegment.Index, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Length, Is.EqualTo(2));
        Assert.That(context.Remaining.ToString(), Is.EqualTo("?"));
    }

    [Test]
    public void Separated_OnlyItemConsumesInput_ContinuesParsing([Values] bool allowTrailing)
    {
        var item = Set('a', 'z');
        var separator = Return(',');
        var parser = item.Separated(separator, allowTrailing);

        var result = parser.Parse("abc!");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(new[] { 'a', 'b', 'c' }));
        Assert.That(result.Length, Is.EqualTo(3));
    }

    [Test]
    public void SeparatedVoid_OnlyItemConsumesInput_ContinuesParsing([Values] bool allowTrailing)
    {
        var item = Set('a', 'z');
        var separator = Return(',');
        var parser = item.Separated(separator, allowTrailing).Void();

        var result = parser.Parse("abc!");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Length, Is.EqualTo(3));
    }

    [Test]
    public void Separated_OnlySeparatorConsumesInput_ContinuesParsing([Values] bool allowTrailing)
    {
        var item = Return('x');
        var separator = L(',');
        var parser = item.Separated(separator, allowTrailing);

        var result = parser.Parse(",,!");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(new[] { 'x', 'x', 'x' }));
        Assert.That(result.Length, Is.EqualTo(2));
    }

    [Test]
    public void SeparatedVoid_OnlySeparatorConsumesInput_ContinuesParsing([Values] bool allowTrailing)
    {
        var item = Return('x');
        var separator = L(',');
        var parser = item.Separated(separator, allowTrailing).Void();

        var result = parser.Parse(",,!");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Length, Is.EqualTo(2));
    }

    [TestCase(false, 1)]
    [TestCase(true, 2)]
    public void Separated_MaximumReached_ConsumesTrailingSeparatorOnlyWhenAllowed(bool allowTrailing, int expectedLength)
    {
        var parser = L('a').Separated(L(','), allowTrailing, min: 1, max: 1);
        var context = new ParseContext("a,a");

        var success = parser.TryParse(ref context, out var value);

        Assert.That(success, Is.True);
        Assert.That(value, Is.EqualTo(new[] { 'a' }));
        Assert.That(context.Position, Is.EqualTo(expectedLength));
        Assert.That(context.MatchedSegment.Index, Is.Zero);
        Assert.That(context.MatchedSegment.Length, Is.EqualTo(expectedLength));
    }

    [TestCase(false, 1)]
    [TestCase(true, 2)]
    public void SeparatedVoid_MaximumReached_ConsumesTrailingSeparatorOnlyWhenAllowed(bool allowTrailing, int expectedLength)
    {
        var parser = L('a').Separated(L(','), allowTrailing, min: 1, max: 1).Void();
        var context = new ParseContext("a,a");

        var success = parser.TryParse(ref context, out _);

        Assert.That(success, Is.True);
        Assert.That(context.Position, Is.EqualTo(expectedLength));
        Assert.That(context.MatchedSegment.Index, Is.Zero);
        Assert.That(context.MatchedSegment.Length, Is.EqualTo(expectedLength));
    }

    [Test]
    public void Separated_MinimumNotReached_RestoresPosition([Values] bool allowTrailing)
    {
        var parser = L('a').Separated(L(','), allowTrailing, min: 2);
        var context = new ParseContext("!a,?");
        context.Advance(1);

        var success = parser.TryParse(ref context, out var value);

        Assert.That(success, Is.False);
        Assert.That(value, Is.Null);
        Assert.That(context.Position, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Length, Is.Zero);
        Assert.That(context.Remaining.ToString(), Is.EqualTo("a,?"));
        Assert.That(context.ToString(), Is.EqualTo("(1:4) Expected 'a'"));
    }

    [Test]
    public void SeparatedVoid_MinimumNotReached_RestoresPosition([Values] bool allowTrailing)
    {
        var parser = L('a').Separated(L(','), allowTrailing, min: 2).Void();
        var context = new ParseContext("!a,?");
        context.Advance(1);

        var success = parser.TryParse(ref context, out _);

        Assert.That(success, Is.False);
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
}
