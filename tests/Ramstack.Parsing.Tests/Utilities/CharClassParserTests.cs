#if TEST_INTERNALS

namespace Ramstack.Parsing.Utilities;

[TestFixture]
public class CharClassParserTests
{
    [TestCase(@"\0", @"'\0'")]
    [TestCase(@"\a", @"'\a'")]
    [TestCase(@"\b", @"'\b'")]
    [TestCase(@"\e", @"'\e'")]
    [TestCase(@"\f", @"'\f'")]
    [TestCase(@"\t", @"'\t'")]
    [TestCase(@"\n", @"'\n'")]
    [TestCase(@"\v", @"'\v'")]
    [TestCase(@"\r", @"'\r'")]
    [TestCase(@"\\", @"'\\'")]
    [TestCase(@"\-", @"'-'")]

    [TestCase("-", "'-'")]
    [TestCase("9", "'9'")]

    [TestCase("*+-",  "[*+-]")]
    [TestCase("0*+-", "[*+\\-0]")]


    [TestCase("0-12-34-56-78-9", "[0-9]")]
    [TestCase("0123456789",      "[0-9]")]
    [TestCase("6-72-30-18-94-5", "[0-9]")]
    [TestCase("0-234-5678-9",    "[0-9]")]
    [TestCase("0-24-68-9",       "[0-24-689]")]
    [TestCase("01",              "[01]")]

    [TestCase(@"\u0030\u0031\u0032\u0033", "[0-3]")]

    [TestCase(@"\p{L}\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}",             @"[\p{L}]")]
    [TestCase(@"\p{M}\p{Mn}\p{Mc}\p{Me}",                         @"[\p{M}]")]
    [TestCase(@"\p{N}\p{Nd}\p{Nl}\p{No}",                         @"[\p{N}]")]
    [TestCase(@"\p{Z}\p{Zs}\p{Zl}\p{Zp}",                         @"[\p{Z}]")]
    [TestCase(@"\p{P}\p{Pc}\p{Pd}\p{Ps}\p{Pe}\p{Pi}\p{Pf}\p{Po}", @"[\p{P}]")]
    [TestCase(@"\p{S}\p{Sm}\p{Sc}\p{Sk}\p{So}",                   @"[\p{S}]")]
    [TestCase(@"\p{C}\p{Cc}\p{Cf}\p{Cs}\p{Co}\p{Cn}",             @"[\p{C}]")]

    [TestCase(@"\P{L}", @"[\P{L}]")]
    [TestCase(@"\P{M}", @"[\P{M}]")]
    [TestCase(@"\P{N}", @"[\P{N}]")]
    [TestCase(@"\P{Z}", @"[\P{Z}]")]
    [TestCase(@"\P{P}", @"[\P{P}]")]
    [TestCase(@"\P{S}", @"[\P{S}]")]
    [TestCase(@"\P{C}", @"[\P{C}]")]

    [TestCase(@"\p{M}\p{N}\p{Z}\p{C}\p{P}\p{S}", @"[\P{L}]")]
    [TestCase(@"\p{L}\p{N}\p{Z}\p{C}\p{P}\p{S}", @"[\P{M}]")]
    [TestCase(@"\p{L}\p{M}\p{Z}\p{C}\p{P}\p{S}", @"[\P{N}]")]
    [TestCase(@"\p{L}\p{M}\p{N}\p{C}\p{P}\p{S}", @"[\P{Z}]")]
    [TestCase(@"\p{L}\p{M}\p{N}\p{Z}\p{C}\p{S}", @"[\P{P}]")]
    [TestCase(@"\p{L}\p{M}\p{N}\p{Z}\p{C}\p{P}", @"[\P{S}]")]
    [TestCase(@"\p{L}\p{M}\p{N}\p{Z}\p{P}\p{S}", @"[\P{C}]")]

    [TestCase(@"a-z\p{L}",                 @"[a-z\p{L}]")]
    [TestCase(@"\p{L}a-z",                 @"[a-z\p{L}]")]
    [TestCase(@"\p{Lu}\p{Ll}\p{Lt}\p{Nd}", @"[\p{Lu}\p{Ll}\p{Lt}\p{Nd}]")]

    [TestCase(@"\w", @"[\p{L}\p{Mn}\p{Nd}\p{Pc}]")]
    [TestCase(@"\d", @"[\p{Nd}]")]
    [TestCase(@"\s", @"[\t-\r \u0085\p{Z}]")]
    [TestCase(@"\W", @"[\p{Z}\p{C}\p{S}\p{Mc}\p{Me}\p{Nl}\p{No}\p{Pd}\p{Ps}\p{Pe}\p{Pi}\p{Pf}\p{Po}]")]
    [TestCase(@"\D", @"[\P{N}\p{Nl}\p{No}]")]
    [TestCase(@"\S", @"[\0-\b\u000e-\u001f!-\u0084\P{Z}]")]

    [TestCase(@"\p{L}\p{M}\p{Z}\p{C}\p{P}\p{S}\p{Nl}\p{No}",               @"[\P{N}\p{Nl}\p{No}]")]

    [TestCase(@"\0-\b\u000e-\u001f!-\u0084\p{L}\p{M}\p{N}\p{C}\p{P}\p{S}", @"[\0-\b\u000e-\u001f!-\u0084\P{Z}]")]
    [TestCase(@"\0-\b\u000e-\u001f!-\u0084\P{Z}",                          @"[\0-\b\u000e-\u001f!-\u0084\P{Z}]")]
    public void ParseTest(string set, string expected)
    {
        var @class = CharClassParser.Parse(set);
        Assert.That(@class.ToPrintable(), Is.EqualTo(expected));
    }

    [TestCase("2-1",     "Invalid pattern '2-1'. Range in reverse order.")]
    [TestCase("\\p{Q}",  "Invalid pattern '\\p{Q}'. Unknown property 'Q'.")]
    [TestCase("\\p{Lu",  "Invalid pattern '\\p{Lu'.")]
    [TestCase("\\T",     "Invalid pattern '\\T'. Unrecognized escape sequence.")]
    [TestCase("\\ur234", "Invalid pattern '\\ur234'. Unrecognized unicode sequence.")]
    [TestCase("\\u1r34", "Invalid pattern '\\u1r34'. Unrecognized unicode sequence.")]
    [TestCase("\\u12r4", "Invalid pattern '\\u12r4'. Unrecognized unicode sequence.")]
    [TestCase("\\u123r", "Invalid pattern '\\u123r'. Unrecognized unicode sequence.")]
    public void ParseTest_Invalid(string set, string message)
    {
        Assert.That(
            () => CharClassParser.Parse(set),
            Throws.ArgumentException.With.Message.EqualTo(message));
    }
}

#endif
