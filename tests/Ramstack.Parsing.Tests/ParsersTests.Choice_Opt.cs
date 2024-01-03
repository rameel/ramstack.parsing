using System.Globalization;
using System.Reflection;

using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void Choice_CharClass_ShouldMerged_1()
    {
        var parser = Choice(
            L('b'),
            Set("a-c"),
            Set("d-z"),
            Set("0-5"),
            Set("3-7"),
            Set("6-9"),
            OneOf("abcdefg"),
            L(UnicodeCategory.DecimalDigitNumber),
            L(GeneralUnicodeCategory.DecimalDigitNumber));

        Assert.That(parser.Name, Is.EqualTo(@"[0-9a-z\p{Nd}]"));
        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block128Bit]]"));
    }

    [Test]
    public void Choice_CharClass_ShouldMerged_2()
    {
        var parser = Choice(
            L('b'),
            Set("a-c"),
            Set("d-z"),
            Literal.EscapeSequence,
            Set("0-5"),
            Set("3-7"),
            Set("6-9"),
            OneOf("abcdefg"),
            L(GeneralUnicodeCategory.DecimalDigitNumber));

        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+DeferredDiagnosticChoiceParser`1[System.Char]"));

        var parsers = (Parser<char>[])parser.GetType().GetField("_parsers", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(parser)!;
        Assert.That(parsers.Length, Is.EqualTo(2));

        Assert.That(parsers[0].Name, Is.EqualTo(@"[0-9a-z\p{Nd}]"));
        Assert.That(parsers[0].GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block128Bit]]"));

        Assert.That(parsers[1].Name, Is.EqualTo(@"escape sequence"));
        Assert.That(parsers[1].GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Literal+EscapeSequenceParser`1[System.Char]"));
    }

    [Test]
    public void Choice_CharClass_ShouldMerged_3()
    {
        var parser = Choice(
            L('c'),
            Set("a-c"),
            Set("d-z"),
            Not(Eof).Then(Literal.EscapeSequence),
            Set("0-5"),
            Set("3-7"),
            Literal.UnicodeEscapeSequence,
            Set("6-9"),
            OneOf("abcdefg"),
            L(GeneralUnicodeCategory.DecimalDigitNumber));

        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+ChoiceParser`1[System.Char]"));

        var parsers = (Parser<char>[])parser.GetType().GetProperty("Parsers", BindingFlags.Instance | BindingFlags.Public)!.GetValue(parser)!;

        Assert.That(parsers.Length, Is.EqualTo(3));

        Assert.That(parsers[0].Name, Is.EqualTo(@"[0-9a-z\p{Nd}]"));
        Assert.That(parsers[0].GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block128Bit]]"));
        Assert.That(parsers[1].GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+ThenParser`1[System.Char]"));
        Assert.That(parsers[2].GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Literal+UnicodeEscapeSequenceParser`1[System.Char]"));
    }

    [Test]
    public void ChoiceTest_UnicodeCategories()
    {
        var parser = Choice(
            Character.Uppercase,
            Character.Digit,
            L(UnicodeCategory.LowercaseLetter),
            L(GeneralUnicodeCategory.DecimalDigitNumber));

        Assert.That(parser.Name, Is.EqualTo(@"[\p{Lu}\p{Ll}\p{Nd}]"));

        Assert.That(
            parser.GetType().ToString(),
            Is.EqualTo("Ramstack.Parsing.Parser+UnicodeCategoryParser`1[System.Char]"));

        Assert.That(parser.Parse("A").Success, Is.True);
        Assert.That(parser.Parse("A").Value, Is.EqualTo('A'));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("A").Value, Is.EqualTo((0, 1)));

        Assert.That(parser.Parse("a").Success, Is.True);
        Assert.That(parser.Parse("a").Value, Is.EqualTo('a'));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("a").Value, Is.EqualTo((0, 1)));

        Assert.That(parser.Parse("1").Success, Is.True);
        Assert.That(parser.Parse("1").Value, Is.EqualTo('1'));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("1").Value, Is.EqualTo((0, 1)));

        Assert.That(parser.Parse("*").Success, Is.False);
        Assert.That(parser.Parse("*").ToString(), Is.EqualTo(@"(1:1) Expected [\p{Lu}\p{Ll}\p{Nd}]"));
        Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("*").Value, Is.EqualTo((0, 0)));
    }

    [Test]
    public void ChoiceTest_CharClass()
    {
        var parser1 = Choice(Set("a-e"), Set("A-C"), Set("0-9"), L('~'));
        var parser2 = Choice(Character.Control, Set("a-z"), OneOf("0123456789"), Set('\0', '\u007f'));
        var parser3 = Choice(Character.Control, Set("A-E"), L('z'), OneOf("0123456789"), Set('\0', '\u007f'));
        var parser4 = Choice(Character.Control, Set('\0', '\u007f'));

        Assert.That(parser1.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block128Bit]]"));
        Assert.That(parser2.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+RangeSearcher]"));
        Assert.That(parser3.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+RangeSearcher]"));
        Assert.That(parser4.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+RangeSearcher]"));

        Assert.That(parser1.Name, Is.EqualTo("[0-9A-Ca-e~]"));
        Assert.That(parser2.Name, Is.EqualTo(@"[\0-\u007f\p{Cc}]"));
        Assert.That(parser3.Name, Is.EqualTo(@"[\0-\u007f\p{Cc}]"));
        Assert.That(parser4.Name, Is.EqualTo(@"[\0-\u007f\p{Cc}]"));

        foreach (var parser in new[] { parser1, parser2, parser3, parser4 })
        {
            Assert.That(parser.Parse("A").Success, Is.True);
            Assert.That(parser.Parse("A").Value, Is.EqualTo('A'));
            Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("A").Value, Is.EqualTo((0, 1)));

            Assert.That(parser.Parse("a").Success, Is.True);
            Assert.That(parser.Parse("a").Value, Is.EqualTo('a'));
            Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("a").Value, Is.EqualTo((0, 1)));

            Assert.That(parser.Parse("1").Success, Is.True);
            Assert.That(parser.Parse("1").Value, Is.EqualTo('1'));
            Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("1").Value, Is.EqualTo((0, 1)));

            Assert.That(parser.Parse("~").Success, Is.True);
            Assert.That(parser.Parse("~").Value, Is.EqualTo('~'));
            Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("~").Value, Is.EqualTo((0, 1)));

            Assert.That(parser.Parse("Ф").Success, Is.False);
            Assert.That(parser.Map(m => (m.Index, m.Length)).Parse("Ф").Value, Is.EqualTo((0, 0)));
        }
    }
}
