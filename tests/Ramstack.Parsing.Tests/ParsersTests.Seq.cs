using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void SeqTest()
    {
        var parser = Seq(
            Seq(
                Seq(Any, Any),
                Seq(Any, Any)),
            Seq(
                Seq(Any, Any),
                Seq(Any, Any)));

        Assert.That(parser.Map(m => m.ToString()).Parse("12345678").Value, Is.EqualTo("12345678"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("12345678").Value, Is.EqualTo((0, 8)));
    }

    [Test]
    public void Seq2Test()
    {
        var parser = Seq(Any, Any);

        Assert.That(parser.Parse("12").Success, Is.True);
        Assert.That(parser.Parse("123").Success, Is.True);
        parser.Do((c1, c2) =>
        {
            Assert.That(c1, Is.EqualTo('1'));
            Assert.That(c2, Is.EqualTo('2'));
            return (c1, c2);
        }).Parse("12");

        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("12").Value, Is.EqualTo((0, 2)));
        Assert.That(parser.Map(m => m.ToString()).Parse("12").Value, Is.EqualTo("12"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("123").Value, Is.EqualTo((0, 2)));
        Assert.That(parser.Parse("12").Value, Is.EqualTo(('1', '2')));
        Assert.That(parser.Parse("123").Value, Is.EqualTo(('1', '2')));

        Assert.That(parser.Parse("").Success, Is.False);
        Assert.That(parser.Parse("").ErrorMessage, Is.EqualTo("(1:1) Expected any character"));

        Assert.That(parser.Parse("1").Success, Is.False);
        Assert.That(parser.Parse("1").ErrorMessage, Is.EqualTo("(1:2) Expected any character"));
    }

    [Test]
    public void Seq3Test()
    {
        var parser = Seq(Any, Any, Any);

        Assert.That(parser.Parse("123").Success, Is.True);
        Assert.That(parser.Parse("1234").Success, Is.True);
        parser.Do((c1, c2, c3) =>
        {
            Assert.That(c1, Is.EqualTo('1'));
            Assert.That(c2, Is.EqualTo('2'));
            Assert.That(c3, Is.EqualTo('3'));
            return (c1, c2, c3);
        }).Parse("123");

        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("123").Value, Is.EqualTo((0, 3)));
        Assert.That(parser.Map(m => m.ToString()).Parse("123").Value, Is.EqualTo("123"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("1234").Value, Is.EqualTo((0, 3)));
        Assert.That(parser.Parse("123").Value, Is.EqualTo(('1', '2', '3')));
        Assert.That(parser.Parse("1234").Value, Is.EqualTo(('1', '2', '3')));

        Assert.That(parser.Parse("").Success, Is.False);
        Assert.That(parser.Parse("").ErrorMessage, Is.EqualTo("(1:1) Expected any character"));

        Assert.That(parser.Parse("1").Success, Is.False);
        Assert.That(parser.Parse("1").ErrorMessage, Is.EqualTo("(1:2) Expected any character"));

        Assert.That(parser.Parse("12").Success, Is.False);
        Assert.That(parser.Parse("12").ErrorMessage, Is.EqualTo("(1:3) Expected any character"));
    }

    [Test]
    public void Seq4Test()
    {
        var parser = Seq(Any, Any, Any, Any);

        Assert.That(parser.Parse("1234").Success, Is.True);
        Assert.That(parser.Parse("12345").Success, Is.True);
        parser.Do((c1, c2, c3, c4) =>
        {
            Assert.That(c1, Is.EqualTo('1'));
            Assert.That(c2, Is.EqualTo('2'));
            Assert.That(c3, Is.EqualTo('3'));
            Assert.That(c4, Is.EqualTo('4'));
            return (c1, c2, c3, c4);
        }).Parse("1234");

        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("1234").Value, Is.EqualTo((0, 4)));
        Assert.That(parser.Map(m => m.ToString()).Parse("1234").Value, Is.EqualTo("1234"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("12345").Value, Is.EqualTo((0, 4)));
        Assert.That(parser.Parse("1234").Value, Is.EqualTo(('1', '2', '3', '4')));
        Assert.That(parser.Parse("12345").Value, Is.EqualTo(('1', '2', '3', '4')));

        Assert.That(parser.Parse("").Success, Is.False);
        Assert.That(parser.Parse("").ErrorMessage, Is.EqualTo("(1:1) Expected any character"));

        Assert.That(parser.Parse("1").Success, Is.False);
        Assert.That(parser.Parse("1").ErrorMessage, Is.EqualTo("(1:2) Expected any character"));

        Assert.That(parser.Parse("12").Success, Is.False);
        Assert.That(parser.Parse("12").ErrorMessage, Is.EqualTo("(1:3) Expected any character"));

        Assert.That(parser.Parse("123").Success, Is.False);
        Assert.That(parser.Parse("123").ErrorMessage, Is.EqualTo("(1:4) Expected any character"));
    }

    [Test]
    public void Seq5Test()
    {
        var parser = Seq(Any, Any, Any, Any, Any);

        Assert.That(parser.Parse("12345").Success, Is.True);
        Assert.That(parser.Parse("123456").Success, Is.True);
        parser.Do((c1, c2, c3, c4, c5) =>
        {
            Assert.That(c1, Is.EqualTo('1'));
            Assert.That(c2, Is.EqualTo('2'));
            Assert.That(c3, Is.EqualTo('3'));
            Assert.That(c4, Is.EqualTo('4'));
            Assert.That(c5, Is.EqualTo('5'));
            return (c1, c2, c3, c4, c5);
        }).Parse("12345");

        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("12345").Value, Is.EqualTo((0, 5)));
        Assert.That(parser.Map(m => m.ToString()).Parse("12345").Value, Is.EqualTo("12345"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("123456").Value, Is.EqualTo((0, 5)));
        Assert.That(parser.Parse("12345").Value, Is.EqualTo(('1', '2', '3', '4', '5')));
        Assert.That(parser.Parse("123456").Value, Is.EqualTo(('1', '2', '3', '4', '5')));

        Assert.That(parser.Parse("").Success, Is.False);
        Assert.That(parser.Parse("").ErrorMessage, Is.EqualTo("(1:1) Expected any character"));

        Assert.That(parser.Parse("1").Success, Is.False);
        Assert.That(parser.Parse("1").ErrorMessage, Is.EqualTo("(1:2) Expected any character"));

        Assert.That(parser.Parse("12").Success, Is.False);
        Assert.That(parser.Parse("12").ErrorMessage, Is.EqualTo("(1:3) Expected any character"));

        Assert.That(parser.Parse("123").Success, Is.False);
        Assert.That(parser.Parse("123").ErrorMessage, Is.EqualTo("(1:4) Expected any character"));

        Assert.That(parser.Parse("1234").Success, Is.False);
        Assert.That(parser.Parse("1234").ErrorMessage, Is.EqualTo("(1:5) Expected any character"));
    }

    [Test]
    public void Seq6Test()
    {
        var parser = Seq(Any, Any, Any, Any, Any, Any);

        Assert.That(parser.Parse("123456").Success, Is.True);
        Assert.That(parser.Parse("1234567").Success, Is.True);
        parser.Do((c1, c2, c3, c4, c5, c6) =>
        {
            Assert.That(c1, Is.EqualTo('1'));
            Assert.That(c2, Is.EqualTo('2'));
            Assert.That(c3, Is.EqualTo('3'));
            Assert.That(c4, Is.EqualTo('4'));
            Assert.That(c5, Is.EqualTo('5'));
            Assert.That(c6, Is.EqualTo('6'));
            return (c1, c2, c3, c4, c5, c6);
        }).Parse("123456");

        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("123456").Value, Is.EqualTo((0, 6)));
        Assert.That(parser.Map(m => m.ToString()).Parse("123456").Value, Is.EqualTo("123456"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("1234567").Value, Is.EqualTo((0, 6)));
        Assert.That(parser.Parse("123456").Value, Is.EqualTo(('1', '2', '3', '4', '5', '6')));
        Assert.That(parser.Parse("1234567").Value, Is.EqualTo(('1', '2', '3', '4', '5', '6')));

        Assert.That(parser.Parse("").Success, Is.False);
        Assert.That(parser.Parse("").ErrorMessage, Is.EqualTo("(1:1) Expected any character"));

        Assert.That(parser.Parse("1").Success, Is.False);
        Assert.That(parser.Parse("1").ErrorMessage, Is.EqualTo("(1:2) Expected any character"));

        Assert.That(parser.Parse("12").Success, Is.False);
        Assert.That(parser.Parse("12").ErrorMessage, Is.EqualTo("(1:3) Expected any character"));

        Assert.That(parser.Parse("123").Success, Is.False);
        Assert.That(parser.Parse("123").ErrorMessage, Is.EqualTo("(1:4) Expected any character"));

        Assert.That(parser.Parse("1234").Success, Is.False);
        Assert.That(parser.Parse("1234").ErrorMessage, Is.EqualTo("(1:5) Expected any character"));

        Assert.That(parser.Parse("12345").Success, Is.False);
        Assert.That(parser.Parse("12345").ErrorMessage, Is.EqualTo("(1:6) Expected any character"));
    }

    [Test]
    public void Seq7Test()
    {
        var parser = Seq(Any, Any, Any, Any, Any, Any, Any);

        Assert.That(parser.Parse("1234567").Success, Is.True);
        Assert.That(parser.Parse("12345678").Success, Is.True);
        parser.Do((c1, c2, c3, c4, c5, c6, c7) =>
        {
            Assert.That(c1, Is.EqualTo('1'));
            Assert.That(c2, Is.EqualTo('2'));
            Assert.That(c3, Is.EqualTo('3'));
            Assert.That(c4, Is.EqualTo('4'));
            Assert.That(c5, Is.EqualTo('5'));
            Assert.That(c6, Is.EqualTo('6'));
            Assert.That(c7, Is.EqualTo('7'));
            return (c1, c2, c3, c4, c5, c6, c7);
        }).Parse("1234567");

        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("1234567").Value, Is.EqualTo((0, 7)));
        Assert.That(parser.Map(m => m.ToString()).Parse("1234567").Value, Is.EqualTo("1234567"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("12345678").Value, Is.EqualTo((0, 7)));
        Assert.That(parser.Parse("1234567").Value, Is.EqualTo(('1', '2', '3', '4', '5', '6', '7')));
        Assert.That(parser.Parse("12345678").Value, Is.EqualTo(('1', '2', '3', '4', '5', '6', '7')));

        Assert.That(parser.Parse("").Success, Is.False);
        Assert.That(parser.Parse("").ErrorMessage, Is.EqualTo("(1:1) Expected any character"));

        Assert.That(parser.Parse("1").Success, Is.False);
        Assert.That(parser.Parse("1").ErrorMessage, Is.EqualTo("(1:2) Expected any character"));

        Assert.That(parser.Parse("12").Success, Is.False);
        Assert.That(parser.Parse("12").ErrorMessage, Is.EqualTo("(1:3) Expected any character"));

        Assert.That(parser.Parse("123").Success, Is.False);
        Assert.That(parser.Parse("123").ErrorMessage, Is.EqualTo("(1:4) Expected any character"));

        Assert.That(parser.Parse("1234").Success, Is.False);
        Assert.That(parser.Parse("1234").ErrorMessage, Is.EqualTo("(1:5) Expected any character"));

        Assert.That(parser.Parse("12345").Success, Is.False);
        Assert.That(parser.Parse("12345").ErrorMessage, Is.EqualTo("(1:6) Expected any character"));

        Assert.That(parser.Parse("123456").Success, Is.False);
        Assert.That(parser.Parse("123456").ErrorMessage, Is.EqualTo("(1:7) Expected any character"));
    }

    [Test]
    public void Seq8Test()
    {
        var parser = Seq(Any, Any, Any, Any, Any, Any, Any, Any);

        Assert.That(parser.Parse("12345678").Success, Is.True);
        Assert.That(parser.Parse("123456789").Success, Is.True);
        parser.Do((c1, c2, c3, c4, c5, c6, c7, c8) =>
        {
            Assert.That(c1, Is.EqualTo('1'));
            Assert.That(c2, Is.EqualTo('2'));
            Assert.That(c3, Is.EqualTo('3'));
            Assert.That(c4, Is.EqualTo('4'));
            Assert.That(c5, Is.EqualTo('5'));
            Assert.That(c6, Is.EqualTo('6'));
            Assert.That(c7, Is.EqualTo('7'));
            Assert.That(c8, Is.EqualTo('8'));
            return (c1, c2, c3, c4, c5, c6, c7, c8);
        }).Parse("12345678");

        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("12345678").Value, Is.EqualTo((0, 8)));
        Assert.That(parser.Map(m => m.ToString()).Parse("12345678").Value, Is.EqualTo("12345678"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("123456789").Value, Is.EqualTo((0, 8)));
        Assert.That(parser.Parse("12345678").Value, Is.EqualTo(('1', '2', '3', '4', '5', '6', '7', '8')));
        Assert.That(parser.Parse("123456789").Value, Is.EqualTo(('1', '2', '3', '4', '5', '6', '7', '8')));

        Assert.That(parser.Parse("").Success, Is.False);
        Assert.That(parser.Parse("").ErrorMessage, Is.EqualTo("(1:1) Expected any character"));

        Assert.That(parser.Parse("1").Success, Is.False);
        Assert.That(parser.Parse("1").ErrorMessage, Is.EqualTo("(1:2) Expected any character"));

        Assert.That(parser.Parse("12").Success, Is.False);
        Assert.That(parser.Parse("12").ErrorMessage, Is.EqualTo("(1:3) Expected any character"));

        Assert.That(parser.Parse("123").Success, Is.False);
        Assert.That(parser.Parse("123").ErrorMessage, Is.EqualTo("(1:4) Expected any character"));

        Assert.That(parser.Parse("1234").Success, Is.False);
        Assert.That(parser.Parse("1234").ErrorMessage, Is.EqualTo("(1:5) Expected any character"));

        Assert.That(parser.Parse("12345").Success, Is.False);
        Assert.That(parser.Parse("12345").ErrorMessage, Is.EqualTo("(1:6) Expected any character"));

        Assert.That(parser.Parse("123456").Success, Is.False);
        Assert.That(parser.Parse("123456").ErrorMessage, Is.EqualTo("(1:7) Expected any character"));

        Assert.That(parser.Parse("1234567").Success, Is.False);
        Assert.That(parser.Parse("1234567").ErrorMessage, Is.EqualTo("(1:8) Expected any character"));
    }
}
