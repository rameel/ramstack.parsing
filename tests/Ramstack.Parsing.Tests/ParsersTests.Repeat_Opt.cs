using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Text;

using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void Repeat_SingleRange()
    {
        var parser = Set(@"\u3040-\u309F").Many();

        #if NET8_0_OR_GREATER
        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RepeatCharClassParser`1[Ramstack.Parsing.Parser+SearchValuesSearcher]"));
        #else
        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RepeatCharClassParser`1[Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block128Bit]]"));
        #endif

        var text = parser.Text();

        Assert.That(text.Parse("\u3040\u3041\u3042\u3043").Value, Is.EqualTo("\u3040\u3041\u3042\u3043"));
        Assert.That(text.Parse("\u30FF\u3041\u3042\u3043").Value, Is.Empty);
        Assert.That(text.Parse("\u3040\u3041\u3042\u30FF").Value, Is.EqualTo("\u3040\u3041\u3042"));
    }

    [Test]
    public void Repeat_Ascii()
    {
        var parser = Set("0-9A-Za-z").Many();
        var p = parser.Text();

        #if NET8_0_OR_GREATER
        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RepeatCharClassParser`1[Ramstack.Parsing.Parser+SearchValuesSearcher]"));
        #else
        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RepeatCharClassParser`1[Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block128Bit]]"));
        #endif

        Assert.That(p.Parse("1234567890").Value, Is.EqualTo("1234567890"));
        Assert.That(p.Parse("!123456789").Value, Is.Empty);
        Assert.That(p.Parse("123456789!").Value, Is.EqualTo("123456789"));
    }

    [TestCase("1")]
    [TestCase("12")]
    [TestCase("123")]
    [TestCase("1234")]
    [TestCase("12345")]
    public void Repeat_SymbolCount(string chars)
    {
        var parser = OneOf(chars).Many();
        var p = parser.Text();

        #if NET8_0_OR_GREATER
        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RepeatCharClassParser`1[Ramstack.Parsing.Parser+SearchValuesSearcher]"));
        #else
        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RepeatCharClassParser`1[Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block128Bit]]"));
        #endif

        Assert.That(p.Parse(chars).Value, Is.EqualTo(chars));
        Assert.That(p.Parse("!" + chars).Value, Is.Empty);
        Assert.That(p.Parse(chars + "!").Value, Is.EqualTo(chars));
    }

    [Test]
    public void Repeat_BitVectorSearcher()
    {
        var parser = Set("а-жА-Ж").Many();
        var p = parser.Text();

        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RepeatCharClassParser`1[Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block128Bit]]"));

        Assert.That(p.Parse("абвгд").Value, Is.EqualTo("абвгд"));
        Assert.That(p.Parse("!абвгд").Value, Is.Empty);
        Assert.That(p.Parse("абвгд!").Value, Is.EqualTo("абвгд"));
    }

    [Test]
    public void Repeat_BitVectorSearcher_UnicodeCategories()
    {
        var parser = Choice(
            Set("a-z0-9"),
            Set(@"\p{L}\p{Nd}")
            ).Many();
        var p = parser.Text();

        #if NET8_0_OR_GREATER
        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RepeatCharClassParser`1[Ramstack.Parsing.Parser+SearchValuesSearcher]"));
        #else
        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RepeatCharClassParser`1[Ramstack.Parsing.Parser+BitVectorSearcher`1[Ramstack.Parsing.Utilities.Block128Bit]]"));
        #endif

        Assert.That(p.Parse("абвгд").Value, Is.EqualTo("абвгд"));
        Assert.That(p.Parse("aаbбcвdгeдf").Value, Is.EqualTo("aаbбcвdгeдf"));
        Assert.That(p.Parse("!абвгд").Value, Is.Empty);
        Assert.That(p.Parse("абвгд0123456789абвгд!").Value, Is.EqualTo("абвгд0123456789абвгд"));
    }

    [Test]
    public void Repeat_Simd256Searcher()
    {
        if (!Avx2.IsSupported)
            return;

        var set = new StringBuilder();
        var chars = new StringBuilder();

        set.Append('\0');

        for (var c = (char)1000; c < 1050; c = (char)(c + 5))
        {
            set.Append($"\\u{c+0:x4}");
            set.Append($"\\u{c+1:x4}");
            set.Append($"\\u{c+2:x4}");

            chars.Append(c);
            chars.Append((char)(c + 1));
            chars.Append((char)(c + 2));
        }

        var parser = Set(set.ToString()).Many();
        var p = parser.Text();

        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RepeatCharClassParser`1[Ramstack.Parsing.Parser+SimdRangeSearcher`1[Ramstack.Parsing.Utilities.Block256Bit]]"));

        var s = chars.ToString();
        var r = string.Join("", s.Reverse());

        Assert.That(p.Parse(s).Value, Is.EqualTo(s));
        Assert.That(p.Parse(r).Value, Is.EqualTo(r));
        Assert.That(p.Parse(s + "!").Value, Is.EqualTo(s));
        Assert.That(p.Parse("!" + s).Value, Is.Empty);
    }

    [Test]
    public void Repeat_Simd128Searcher()
    {
        if (!Sse41.IsSupported && !AdvSimd.IsSupported)
            return;

        var set = new StringBuilder();
        var chars = new StringBuilder();

        set.Append('\0');

        for (var c = (char)1000; c < 1030; c = (char)(c + 5))
        {
            set.Append($"\\u{c+0:x4}");
            set.Append($"\\u{c+1:x4}");
            set.Append($"\\u{c+2:x4}");

            chars.Append(c);
            chars.Append((char)(c + 1));
            chars.Append((char)(c + 2));
        }

        var parser = Set(set.ToString()).Many();
        var p = parser.Text();

        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RepeatCharClassParser`1[Ramstack.Parsing.Parser+SimdRangeSearcher`1[Ramstack.Parsing.Utilities.Block128Bit]]"));

        var s = chars.ToString();
        var r = string.Join("", s.Reverse());

        Assert.That(p.Parse(s).Value, Is.EqualTo(s));
        Assert.That(p.Parse(r).Value, Is.EqualTo(r));
        Assert.That(p.Parse(s + "!").Value, Is.EqualTo(s));
        Assert.That(p.Parse("!" + s).Value, Is.Empty);
    }

    [Test]
    public void Repeat_BinarySearcher()
    {
        var set = new StringBuilder();
        var chars = new StringBuilder();

        for (var c = '\0'; c < 10240; c = (char)(c + 2))
        {
            set.Append($"\\u{(int)c:x4}");
            chars.Append(c);
        }

        var parser = Set(set.ToString()).Many();
        var p = parser.Text();

        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RepeatCharClassParser`1[Ramstack.Parsing.Parser+BinaryRangeSearcher]"));

        var s = chars.ToString();
        var r = string.Join("", s.Reverse());

        Assert.That(p.Parse(s).Value, Is.EqualTo(s));
        Assert.That(p.Parse(r).Value, Is.EqualTo(r));
        Assert.That(p.Parse(s + "!").Value, Is.EqualTo(s));
        Assert.That(p.Parse("!" + s).Value, Is.Empty);
    }

    [Test]
    public void Repeat_ContainsSearcher()
    {
        var set = new StringBuilder();
        var chars = new StringBuilder();

        for (var c = '\0'; c < 128*5; c = (char)(c + 5))
        {
            set.Append($"\\u{(int)c:x4}");
            chars.Append(c);
        }

        var parser = Set(set.ToString()).Many();
        var p = parser.Text();

        Assert.That(parser.GetType().ToString(), Is.EqualTo("Ramstack.Parsing.Parser+RepeatCharClassParser`1[Ramstack.Parsing.Parser+ContainsSearcher]"));

        var s = chars.ToString();
        var r = string.Join("", s.Reverse());

        Assert.That(p.Parse(s).Value, Is.EqualTo(s));
        Assert.That(p.Parse(r).Value, Is.EqualTo(r));
        Assert.That(p.Parse(s + "!").Value, Is.EqualTo(s));
        Assert.That(p.Parse("!" + s).Value, Is.Empty);
    }
}
