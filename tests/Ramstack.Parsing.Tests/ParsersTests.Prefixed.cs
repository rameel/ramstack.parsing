using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [TestCase("x", "x")]
    [TestCase("-x", "(-x)")]
    [TestCase("!-x", "(!(-x))")]
    [TestCase("-+~!x", "(-(+(~(!x))))")]
    public void Prefixed_MultipleOperators_AppliesRightToLeft(string source, string expected)
    {
        var operand = Set('a', 'z').Map(m => m.ToString());
        var parser = operand.Prefixed(
            OneOf("-+~!"),
            static (x, op) => $"({op}{x})");

        var result = parser.Parse(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(expected));
        Assert.That(result.Length, Is.EqualTo(source.Length));
    }

    [TestCase("")]
    [TestCase("-")]
    [TestCase("-x")]
    public void Prefixed_MissingOperand_FailsAndRestoresPosition(string source)
    {
        var operand = Set('0', '9').Do(c => c - '0');
        var parser = operand.Prefixed(L('-'), (x, _) => -x);

        var context = new ParseContext("!" + source);
        context.Advance(1);

        Assert.That(parser.TryParse(ref context, out _), Is.False);

        Assert.That(context.Position, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Length, Is.Zero);
    }

    [Test]
    public void Prefixed_MissingOperand_ReportsErrorAfterOperators()
    {
        var operand = Set('0', '9').Do(c => c - '0').As("digit");
        var parser = operand.Prefixed(L('-'), (x, _) => -x);

        var result = parser.Parse("--");

        Assert.That(result.Success, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("(1:3) Expected '-' or digit"));
    }

    [Test]
    public void Prefixed_Success_SetsMatchedSegment()
    {
        var digit = Set('0', '9').Do(c => c - '0');
        var parser = digit.Prefixed(L('-'), (x, _) => -x);

        var context = new ParseContext("!--5tail");
        context.Advance(1);

        Assert.That(parser.TryParse(ref context, out var value), Is.True);
        Assert.That(value, Is.EqualTo(5));

        Assert.That(context.MatchedSegment.Index, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Length, Is.EqualTo(3));
        Assert.That(context.Remaining.ToString(), Is.EqualTo("tail"));
    }

    [Test]
    public void Prefixed_OperatorConsumesNoInput_StopsWithoutReducing()
    {
        var op = new BoundedParser<char>(Return('-'));
        var digit = Set('0', '9').Do(c => c - '0');

        var reductions = 0;
        var parser = digit.Prefixed(op, (x, _) =>
        {
            reductions++;
            return -x;
        });

        var result = parser.Parse("5!");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(5));
        Assert.That(result.Length, Is.EqualTo(1));

        Assert.That(reductions, Is.Zero);
    }

    [Test]
    public void Prefixed_TrailingEmptyOperator_StopsWithoutReducing()
    {
        var op = new BoundedParser<char>(L('-').DefaultOnFail('-'));
        var digit = Set('0', '9').Do(c => c - '0');

        var reductions = 0;
        var parser = digit.Prefixed(op, (x, _) =>
        {
            reductions++;
            return x + 1;
        });

        var result = parser.Parse("--5!");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(7));
        Assert.That(result.Length, Is.EqualTo(3));

        Assert.That(reductions, Is.EqualTo(2));
    }

    [TestCase("5!", 1)]
    [TestCase("-5!", 2)]
    [TestCase("---5!", 4)]
    public void PrefixedVoid_Success_SkipsReduction(string source, int length)
    {
        var digit = Set('0', '9').Do(c => c - '0');

        var reductions = 0;
        var parser = digit.Prefixed(L('-'), (x, _) =>
        {
            reductions++;
            return -x;
        });

        var result = parser.Void().Parse(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Length, Is.EqualTo(length));

        Assert.That(reductions, Is.Zero);
    }

    [TestCase("")]
    [TestCase("-")]
    [TestCase("--x")]
    public void PrefixedVoid_MissingOperand_FailsAndRestoresPosition(string source)
    {
        var digit = Set('0', '9').Do(c => c - '0');
        var parser = digit.Prefixed(L('-'), (x, _) => -x).Void();

        var context = new ParseContext("!" + source);
        context.Advance(1);

        var success = parser.TryParse(ref context, out _);

        Assert.That(success, Is.False);
        Assert.That(context.Position, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Length, Is.Zero);
    }

    [Test]
    public void PrefixedVoid_TrailingEmptyOperator_StopsParsing()
    {
        var op = new BoundedParser<char>(L('-').DefaultOnFail('-'));
        var digit = Set('0', '9').Do(c => c - '0');
        var parser = digit.Prefixed(op, (x, _) => -x).Void();

        var result = parser.Parse("--5!");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Length, Is.EqualTo(3));
    }

    [Test]
    public void Prefixed_VeryLongChain_DoesNotOverflowStack()
    {
        var digit = Set('0', '9').Do(c => c - '0');
        var parser = digit.Prefixed(L('-'), (x, _) => -x);

        var source = new string('-', 1_000_001) + "1";
        var result = parser.Parse(source);

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(-1));
        Assert.That(result.Length, Is.EqualTo(source.Length));

        Assert.That(
            parser.Void().Parse(source).Length,
            Is.EqualTo(source.Length));
    }
}
