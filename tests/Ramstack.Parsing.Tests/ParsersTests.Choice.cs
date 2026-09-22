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
}
