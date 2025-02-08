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
}
