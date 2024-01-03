using System.Globalization;

using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void UnicodeCategoryTest()
    {
        var parser = L(UnicodeCategory.DecimalDigitNumber);

        for (var c = '0'; c <= '9'; c++)
        {
            var s = c.ToString();
            Assert.That(parser.Parse(s).Success, Is.True);
            Assert.That(parser.Parse(s).Value, Is.EqualTo(c));
            Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(s).Value, Is.EqualTo((0, 1)));
        }

        Assert.That(parser.Parse("a").Success, Is.False);
        Assert.That(parser.Parse("*").Success, Is.False);
    }

    // [Test]
    // public void UnicodeCategoriesTest()
    // {
    //     var parser = OneOf(
    //         UnicodeCategory.DecimalDigitNumber,
    //         UnicodeCategory.LowercaseLetter,
    //         UnicodeCategory.UppercaseLetter);
    //
    //     foreach (var c in "0123456789abcdefghABCDEFGH")
    //     {
    //         var s = c.ToString();
    //         Assert.That(parser.Parse(s).Success, Is.True);
    //         Assert.That(parser.Parse(s).Value, Is.EqualTo(c));
    //         Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(s).Value, Is.EqualTo((0, 1)));
    //     }
    //
    //     Assert.That(parser.Parse(" ").Success, Is.False);
    //     Assert.That(parser.Parse("*").Success, Is.False);
    // }
}
