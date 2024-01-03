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
}
