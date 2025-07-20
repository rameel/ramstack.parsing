using System.Globalization;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Text;

using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void Set_Any()
    {
        var parser = Choice(Set("0-9"), Set("\0-\uFFFF"));
        Assert.That(parser, Is.SameAs(Any));
    }

    [Test]
    public void Set_UC()
    {
        var parser = Choice(L(GeneralUnicodeCategory.OtherLetter), L(GeneralUnicodeCategory.FinalQuotePunctuation));
        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+UnicodeCategoryParser`1[System.Char]"));

        IncludeTest(parser, c => char.GetUnicodeCategory(c) is UnicodeCategory.OtherLetter or UnicodeCategory.FinalQuotePunctuation);
    }

    [Test]
    public void Set_SingeChar()
    {
        var parser = Set("a");
        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+CharParser`1[System.Char]"));
    }

    [Test]
    public void Set_SingleRange()
    {
        var parser1 = Set("a-z");
        var parser2 = OneOf("0123456789");
        var parser3 = Choice(OneOf("0123456789"), L(UnicodeCategory.DecimalDigitNumber));
        var parser4 = Choice(Set("0-9"), L(UnicodeCategory.DecimalDigitNumber));

        Assert.That(parser1.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+RangeSearcher]"));
        Assert.That(parser2.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+RangeSearcher]"));
        Assert.That(parser3.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+RangeSearcher]"));
        Assert.That(parser4.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+RangeSearcher]"));

        IncludeTest(
            OneOf("0123456789"),
            c => c is >= '0' and <= '9');

        IncludeTest(
            Choice(OneOf("0123456789"), L(UnicodeCategory.DecimalDigitNumber)),
            c => c is >= '0' and <= '9' || char.GetUnicodeCategory(c) == UnicodeCategory.DecimalDigitNumber);
    }

    [Test]
    public void Set_MultipleRanges_OverallWidth_128()
    {
        var parser1 = Set("a-z0-9");
        var parser2 = Set("а-жА-Ж");
        var parser3 = Set("a-z0-9\u0085\u00A0");
        Assert.That(parser1.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block128Bit]]"));
        Assert.That(parser2.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block128Bit]]"));
        Assert.That(parser3.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block128Bit]]"));

        IncludeTest(
            Set("a-z0-9\u0085\u00A0"),
            c => c is >= '0' and <= '9' or >= 'a' and <= 'z' or '\u0085' or '\u00A0');

        IncludeTest(
            Set("а-жА-Ж"),
            c => c is >= 'а' and <= 'ж' or >= 'А' and <= 'Ж');
    }

    [Test]
    public void Set_MultipleRanges_OverallWidth_256()
    {
        var parser1 = Set("\0\u0085\u00A0a-z0-9");
        var parser2 = Set("\0\u0085\u00FFa-z0-9");
        var parser3 = Set("\u3040-\u309F\u3100-\u312F");

        Assert.That(parser1.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block256Bit]]"));
        Assert.That(parser2.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block256Bit]]"));
        Assert.That(parser3.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block256Bit]]"));

        IncludeTest(
            Set("\0\u0085\u00A0\u00FFa-z0-9"),
            c => c is >= 'a' and <= 'z' or >= '0' and <= '9' or '\0' or '\u0085' or '\u00A0' or '\u00FF');

        IncludeTest(
            Set("\u3040-\u309F\u3100-\u312F"),
            c => c is >= '\u3040' and <= '\u309F' or >= '\u3100' and <= '\u312f');
    }

    [Test]
    public void Set_MultipleRanges_OverallWidth_512()
    {
        var parser1 = Set("\0\u0085\u00A0\u0190a-z0-9");
        var parser2 = Set("\0\u0085\u00A0\u01FFa-z0-9");
        var parser3 = Set("\u3040-\u309F\u3130-\u318F");

        Assert.That(parser1.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block512Bit]]"));
        Assert.That(parser2.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block512Bit]]"));
        Assert.That(parser3.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block512Bit]]"));

        IncludeTest(
            Set("\0\u0085\u00A0\u0190a-z0-9"),
            c => c is >= 'a' and <= 'z' or >= '0' and <= '9' or '\0' or '\u0085' or '\u00A0' or '\u0190');

        IncludeTest(
            Set("\u3040-\u309F\u3130-\u318F"),
            c => c is >= '\u3040' and <= '\u309F' or >= '\u3130' and <= '\u318f');
    }

    [Test]
    public void Set_MultipleRanges_ContainsSearch()
    {
        var sb = new StringBuilder();
        for (var c = '\0'; c < 128*5; c = (char)(c + 5))
            sb.Append($"\\u{(int)c:x4}");

        var parser = Set(sb.ToString());

        Assert.That(parser.GetType().ToString(),
            Avx2.IsSupported
                ? Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+ContainsSearcher]")
                : Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+BinaryRangeSearcher]"));
        IncludeTest(parser, c => c < 128*5 && c % 5 == 0);
    }

    [Test]
    public void Set_MultipleRanges_Simd256()
    {
        if (!Avx2.IsSupported)
            return;

        var sb = new StringBuilder();
        var chars = new HashSet<char> { char.MinValue };

        sb.Append('\0');

        for (var c = (char)1000; c < 1050; c = (char)(c + 5))
        {
            sb.Append($"\\u{c+0:x4}");
            sb.Append($"\\u{c+1:x4}");
            sb.Append($"\\u{c+2:x4}");

            chars.Add(c);
            chars.Add((char)(c + 1));
            chars.Add((char)(c + 2));
        }

        var parser = Set(sb.ToString());

        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+SimdRangeSearcher`1[Ramstack.Parsing.Utilities.Block256Bit]]"));
        IncludeTest(parser, chars.Contains);
    }

    [Test]
    public void Set_MultipleRanges_Simd128()
    {
        if (!Sse41.IsSupported && !AdvSimd.IsSupported)
            return;

        var sb = new StringBuilder();
        var chars = new HashSet<char> { char.MinValue };

        sb.Append('\0');

        for (var c = (char)1000; c < 1030; c = (char)(c + 5))
        {
            sb.Append($"\\u{c+0:x4}");
            sb.Append($"\\u{c+1:x4}");
            sb.Append($"\\u{c+2:x4}");

            chars.Add((char)(c + 0));
            chars.Add((char)(c + 1));
            chars.Add((char)(c + 2));
        }

        var parser = Set(sb.ToString());

        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+SimdRangeSearcher`1[Ramstack.Parsing.Utilities.Block128Bit]]"));
        IncludeTest(parser, chars.Contains);
    }

    [Test]
    public void Set_MultipleRanges_BinarySearch()
    {
        var sb = new StringBuilder();
        for (var c = '\0'; c < 10240; c = (char)(c + 2))
            sb.Append($"\\u{(int)c:x4}");

        var parser = Set(sb.ToString());

        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RangeParser`2[System.Char,Ramstack.Parsing.Parser+BinaryRangeSearcher]"));
        IncludeTest(parser, c => c < 10240 && c % 2 == 0);
    }

    private static void IncludeTest(Parser<char> parser, Func<char, bool> included)
    {
        for (var c = 0; c <= 65535; c++)
        {
            var s = new string((char)c, 1);

            if (parser.TryParse(s, out var v))
            {
                Assert.That(included((char)c), Is.True, $"Char: \\u{c:x4}");
                Assert.That(v, Is.EqualTo((char)c), $"Char: \\u{c:x4}");
            }
            else
            {
                Assert.That(included((char)c), Is.False, $"Char: \\u{c:x4}");
            }
        }
    }
}
