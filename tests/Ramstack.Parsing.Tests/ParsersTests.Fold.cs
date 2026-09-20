using System.Numerics;

using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [TestCase("1", 1, 1)]
    [TestCase("1+", 1, 1)]
    [TestCase("1+2$", 3, 3)]
    [TestCase("1+2-", 3, 3)]
    [TestCase("1+2+3+4-2", 8, 9)]
    [TestCase("1+2+3+4-2$", 8, 9)]
    public void FoldTest(string expr, int result, int length)
    {
        var number = Literal.Number<int>();
        var parser = number.Fold(OneOf("+-"), (l, r, o) => o == '+' ? l + r : l - r);

        Assert.That(parser.Parse(expr).Success, Is.True);
        Assert.That(parser.Parse(expr).Value, Is.EqualTo(result));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(expr).Value, Is.EqualTo((0, length)));
    }

    [TestCase("2", "2", 1)]
    [TestCase("2$", "2", 1)]
    [TestCase("2**", "2", 1)]
    [TestCase("2**3", "8", 4)]
    [TestCase("2**3**", "8", 4)]
    [TestCase("2**3**4**1$", "2417851639229258349412352", 10)]
    public void FoldRTest(string expr, string result, int length)
    {
        var number = Literal.Number<BigInteger>();
        var parser = number.FoldR(L("**"), (l, r, _) => BigInteger.Pow(l, (int)r));

        Assert.That(parser.Parse(expr).Success, Is.True);
        Assert.That(parser.Parse(expr).Value, Is.EqualTo(BigInteger.Parse(result)));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(expr).Value, Is.EqualTo((0, length)));
    }

    [Test]
    public void Fold_NoInputConsumed_StopsWithoutReducing([Values] bool rightAssociative)
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

        var success = parser.TryParse(ref context, out var value);

        Assert.That(success, Is.True);
        Assert.That(value, Is.EqualTo(2));
        Assert.That(reductions, Is.Zero);

        Assert.That(context.Position, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Index, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Length, Is.Zero);
        Assert.That(context.Remaining.ToString(), Is.EqualTo("tail"));
    }

    [Test]
    public void FoldVoid_NoInputConsumed_StopsWithoutReducing([Values] bool rightAssociative)
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

        var success = parser.Void().TryParse(ref context, out _);

        Assert.That(success, Is.True);
        Assert.That(reductions, Is.Zero);

        Assert.That(context.Position, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Index, Is.EqualTo(1));
        Assert.That(context.MatchedSegment.Length, Is.Zero);
        Assert.That(context.Remaining.ToString(), Is.EqualTo("tail"));
    }

    [TestCase(false, 6)]
    [TestCase(true, 8)]
    public void Fold_TrailingEmptyPair_StopsWithoutReducing(bool rightAssociative, int expected)
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
    }

    [Test]
    public void FoldVoid_TrailingEmptyPair_StopsWithoutReducing([Values] bool rightAssociative)
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

        var result = parser.Void().Parse("10-3-1!");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Length, Is.EqualTo(6));
        Assert.That(reductions, Is.Zero);
    }

    [TestCase(false, -4)]
    [TestCase(true, 2)]
    public void Fold_OnlyOperandConsumesInput_ContinuesParsing(bool rightAssociative, int expected)
    {
        var operand = Set('0', '9').Do(c => c - '0');
        var op = Return('-');

        var parser = rightAssociative
            ? operand.FoldR(op, (l, r, _) => l - r)
            : operand.Fold(op, (l, r, _) => l - r);

        var result = parser.Parse("123!");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(expected));
        Assert.That(result.Length, Is.EqualTo(3));
    }

    [Test]
    public void FoldVoid_OnlyOperandConsumesInput_ContinuesParsing([Values] bool rightAssociative)
    {
        var operand = Set('0', '9').Do(c => c - '0');
        var op = Return('-');

        var parser = rightAssociative
            ? operand.FoldR(op, (l, r, _) => l - r)
            : operand.Fold(op, (l, r, _) => l - r);

        var result = parser.Void().Parse("123!");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Length, Is.EqualTo(3));
    }

    [TestCase(false, -2)]
    [TestCase(true, 2)]
    public void Fold_OnlyOperatorConsumesInput_ContinuesParsing(bool rightAssociative, int expected)
    {
        var operand = Return(2);
        var op = L('-');

        var parser = rightAssociative
            ? operand.FoldR(op, (l, r, _) => l - r)
            : operand.Fold(op, (l, r, _) => l - r);

        var result = parser.Parse("--!");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo(expected));
        Assert.That(result.Length, Is.EqualTo(2));
    }

    [Test]
    public void FoldVoid_OnlyOperatorConsumesInput_ContinuesParsing([Values] bool rightAssociative)
    {
        var operand = Return(2);
        var op = L('-');

        var parser = rightAssociative
            ? operand.FoldR(op, (l, r, _) => l - r)
            : operand.Fold(op, (l, r, _) => l - r);

        var result = parser.Void().Parse("--!");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Length, Is.EqualTo(2));
    }
}
