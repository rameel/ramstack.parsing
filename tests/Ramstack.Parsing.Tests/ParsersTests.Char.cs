using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void CharTest()
    {
        var parser = L('a');

        Assert.That(parser.Parse("a").Success, Is.True);
        Assert.That(parser.Parse("a").Value, Is.EqualTo('a'));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("a").Value, Is.EqualTo((0, 1)));
        Assert.That(parser.Map(m => m.ToString()).Parse("a").Value, Is.EqualTo("a"));

        Assert.That(parser.Parse("1").Success, Is.False);
        Assert.That(parser.Parse("").Success, Is.False);
    }

    [TestCase("a")]
    [TestCase("ab")]
    [TestCase("ace")]
    [TestCase("adgl")]
    [TestCase("adglp")]
    [TestCase("adglpv")]
    [TestCase("abc")]
    [TestCase("abcd")]
    [TestCase("abcde")]
    [TestCase("abcdef")]
    [TestCase("abcdefgh")]
    [TestCase("0123456789")]
    public void OneOf_CharTest(string chars)
    {
        var parser = OneOf(chars);

        foreach (var c in chars)
        {
            var source = c.ToString();

            Assert.That(parser.Parse(source).Value, Is.EqualTo(c));
            Assert.That(parser.Parse(source).Success, Is.True);
            Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(source).Value, Is.EqualTo((0, 1)));
            Assert.That(parser.Map(m => m.ToString()).Parse(source).Value, Is.EqualTo(source));
        }

        Assert.That(parser.Parse("*").Success, Is.False);
        Assert.That(parser.Parse("").Success, Is.False);
    }
}
