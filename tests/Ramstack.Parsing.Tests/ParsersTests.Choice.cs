using System.Reflection;

using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void ChoiceTest_1()
    {
        var parsers = Choice(L("Sun"), L("Sunset"));

        Assert.That(
            parsers.Parse("Sunset").Success,
            Is.True);

        Assert.That(
            parsers.Parse("Sunset").Value,
            Is.EqualTo("Sun"));

        Assert.That(
            parsers.Parse("Sun").Value,
            Is.EqualTo("Sun"));

        Assert.That(
            parsers.Map(m => (m.Index, m.Length)).Parse("Sunset").Value,
            Is.EqualTo((0, 3)));
    }

    [Test]
    public void ChoiceTest_2()
    {
        var parsers = Choice(L("Sun"), L("Sunset"), L("Book"), L("Bookstore"));

        Assert.That(
            parsers.Parse("Sunset").Success,
            Is.True);

        Assert.That(
            parsers.Parse("Sunset").Value,
            Is.EqualTo("Sun"));

        Assert.That(
            parsers.Parse("Sun").Value,
            Is.EqualTo("Sun"));

        Assert.That(
            parsers.Map(m => (m.Index, m.Length)).Parse("Sunset").Value,
            Is.EqualTo((0, 3)));
    }

    [Test]
    public void Choice_NamedParser_ReportsName()
    {
        // The unnamed alternative makes this a ChoiceParser rather than
        // DeferredDiagnosticChoiceParser.
        var parser = Choice(L('a'), L('b').Between(L('('), L(')'))).As("value");

        Assert.That(
            parser.Parse("?").ErrorMessage,
            Is.EqualTo("(1:1) Expected value"));
    }

    [Test]
    public void Choice_UnnamedParser_ReportsAlternatives()
    {
        var parser = Choice(L('a'), L('b').Between(L('('), L(')')));

        Assert.That(
            parser.Parse("?").ErrorMessage,
            Is.EqualTo("(1:1) Expected 'a' or '('"));
    }

    [Test]
    public void Choice_NestedParser_FlattensAlternatives()
    {
        var parser1 =
            Choice(
                Choice(
                    L('a'),
                    L('b').Between(L('('), L(')'))),
            L('c'));

        var parser2 =
            L('a')
                .Or(L('b').Between(L('('), L(')')))
                .Or(L('c'));

        Assert.That(parser1.Parse("a").Value, Is.EqualTo('a'));
        Assert.That(parser1.Parse("(b)").Value, Is.EqualTo('b'));
        Assert.That(parser1.Parse("c").Value, Is.EqualTo('c'));

        Assert.That(parser2.Parse("a").Value, Is.EqualTo('a'));
        Assert.That(parser2.Parse("(b)").Value, Is.EqualTo('b'));
        Assert.That(parser2.Parse("c").Value, Is.EqualTo('c'));
    }

    [Test]
    public void Choice_NestedNamedParser_KeepsName()
    {
        var p = Choice(L('a'), L('b').Between(L('('), L(')'))).As("letter");
        var parser = Choice(p, L('c'));

        Assert.That(
            parser.Parse("?").ErrorMessage,
            Is.EqualTo("(1:1) Expected letter or 'c'"));
    }

    [Test]
    public void Choice_NestedParsers_NonCharValue_FlattensTheWholeTree()
    {
        var p1 = Choice(
            L("a").Do(_ => 1),
            L("b").Do(_ => 2));

        var p2 = Choice(
            p1,
            L("c").Do(_ => 3));

        var p3 = Choice(
            L("d").Do(_ => 4),
            p2);

        var parsers = (Parser<int>[])p3.GetType()
            .GetProperty("Parsers", BindingFlags.Instance | BindingFlags.Public)!
            .GetValue(p3)!;

        Assert.That(parsers.Length, Is.EqualTo(4));
        Assert.That(parsers, Has.None.SameAs(p1));
        Assert.That(parsers, Has.None.SameAs(p2));
        Assert.That(p3.Parse("d").Value, Is.EqualTo(4));
    }

    [Test]
    public void Choice_LongerLiteralBeforeCharClass_KeepsAlternativeOrder()
    {
        var parser = Choice(
            L("==").Void(),
            L('=').Void(),
            L('!').Void());

        Assert.That(parser.Parse("==").Length, Is.EqualTo(2));
        Assert.That(parser.Parse("==").Success, Is.True);

        Assert.That(parser.Parse("=").Length, Is.EqualTo(1));
        Assert.That(parser.Parse("!").Length, Is.EqualTo(1));
    }

    [Test]
    public void Choice_NonCharClassParserBetweenCharClasses_KeepsAlternativeOrder()
    {
        var parser = Choice(
            L('!').Void(),
            L("==").Void(),
            L('=').Void());

        Assert.That(parser.Parse("==").Length, Is.EqualTo(2));
        Assert.That(parser.Parse("==").Success, Is.True);

        Assert.That(parser.Parse("!").Length, Is.EqualTo(1));
        Assert.That(parser.Parse("=").Length, Is.EqualTo(1));
    }
}
