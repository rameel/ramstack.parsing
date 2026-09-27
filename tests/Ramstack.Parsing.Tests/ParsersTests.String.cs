using System.Globalization;

using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void StringTest()
    {
        Assert.That(L("Apple").Parse("Apple").Success, Is.True);
        Assert.That(L("Apple").Parse("Apple").Value, Is.EqualTo("Apple"));
        Assert.That(L("Apple").Parse("Apples").Value, Is.EqualTo("Apple"));
        Assert.That(L("Apple").Map(m => (m.Index, m.Length)).Parse("Apple").Value, Is.EqualTo((0, 5)));
        Assert.That(L("Apple").Map(m => m.ToString()).Parse("Apple").Value, Is.EqualTo("Apple"));

        Assert.That(L("Apple", StringComparison.OrdinalIgnoreCase).Parse("apple").Value, Is.EqualTo("Apple"));

        Assert.That(L("Apple").Parse("apple").Success, Is.False);
        Assert.That(L("Apple").Parse("apple").ErrorMessage, Is.EqualTo("(1:1) Expected 'Apple'"));
        Assert.That(L("Apple").Parse("Fruit").Success, Is.False);
        Assert.That(L("Apple").Parse("").Success, Is.False);
    }

    [Test]
    public void OneOf_String_OverlappingWords()
    {
        var literals = new[]
        {
            "Sun",
            "Sunset",
            "Light",
            "Lighthouse",
            "Star",
            "Starlight",
            "Book",
            "Bookstore",
            "Home",
            "Homeland",
            "Sea",
            "Seashore",
            "1234",
            "12345",
            "Практика",
            "Практикант"
        };

        for (var count = 2; count <= literals.Length; count++)
        {
            var values = literals[..count];

            foreach (var value in values)
            {
                foreach (var comparison in Enum.GetValues<StringComparison>())
                {
                    var lower = ((int)comparison & 1) == 1
                        ? value.ToLower()
                        : value;

                    var upper = ((int)comparison & 1) == 1
                        ? value.ToUpper()
                        : value;

                    var parser = OneOf(values, comparison);

                    Assert.That(parser.Parse(value).Success, Is.True);
                    Assert.That(parser.Parse(lower).Success, Is.True);
                    Assert.That(parser.Parse(upper).Success, Is.True);
                    Assert.That(parser.Parse(value).Value, Is.EqualTo(value));
                    Assert.That(parser.Parse(lower).Value, Is.EqualTo(value));
                    Assert.That(parser.Parse(upper).Value, Is.EqualTo(value));

                    Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(value).Value, Is.EqualTo((0, value.Length)));
                    Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(lower).Value, Is.EqualTo((0, value.Length)));
                    Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(upper).Value, Is.EqualTo((0, value.Length)));
                }
            }
        }
    }

    [Test]
    public void OneOf_String()
    {
        var literals = new[]
        {
            "Sun",
            "Rain",
            "Lighthouse",
            "Starlight",
            "Bookstore",
            "Home",
            "Seashore",
            "12345",
            "Практика"
        };

        for (var count = 2; count <= literals.Length; count++)
        {
            var values = literals[..count];

            foreach (var value in values)
            {
                foreach (var comparison in Enum.GetValues<StringComparison>())
                {
                    var lower = ((int)comparison & 1) == 1
                        ? value.ToLower()
                        : value;

                    var upper = ((int)comparison & 1) == 1
                        ? value.ToUpper()
                        : value;

                    var parser = OneOf(values, comparison);

                    Assert.That(parser.Parse(value).Success, Is.True);
                    Assert.That(parser.Parse(lower).Success, Is.True);
                    Assert.That(parser.Parse(upper).Success, Is.True);
                    Assert.That(parser.Parse(value).Value, Is.EqualTo(value));
                    Assert.That(parser.Parse(lower).Value, Is.EqualTo(value));
                    Assert.That(parser.Parse(upper).Value, Is.EqualTo(value));

                    Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(value).Value, Is.EqualTo((0, value.Length)));
                    Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(lower).Value, Is.EqualTo((0, value.Length)));
                    Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(upper).Value, Is.EqualTo((0, value.Length)));
                }
            }
        }

        foreach (var text in new[] { "None", "Light", "123", "Практик" })
        {
            Assert.That(
                OneOf(literals).Parse(text).ErrorMessage,
                Is.EqualTo("(1:1) Expected 'Sun', 'Rain', 'Lighthouse', 'Starlight', 'Bookstore', 'Home', 'Seashore', '12345', or 'Практика'"));
        }
    }

    [Test]
    public void OneOf_UnicodeCaseVariants_AgreesWithStartsWith()
    {
        var cases = new (string Literal, string Input)[]
        {
            ("Μx", "µx"),       // Greek capital Mu / micro sign | ("\u039Cx", "\u00B5x")
            ("Μx", "μx"),       // Greek capital Mu / small Mu   | ("\u039Cx", "\u03BCx")
            ("Σx", "ςx"),       // Greek capital Sigma / final Sigma
            ("Σx", "σx"),       // Greek capital Sigma / small Sigma
            ("Вx", "вx"),       // Cyrillic capital Ve / small Ve
            ("Вx", "ᲀx"),       // Cyrillic capital Ve / historic letter
            ("Ιx", "ιx"),       // Greek capital Iota / small Iota
            ("Ιx", "\u0345x"),
            ("Ιx", "\u1FBEx")
        };

        var comparisons = new[]
        {
            StringComparison.Ordinal,
            StringComparison.OrdinalIgnoreCase,
            StringComparison.InvariantCulture,
            StringComparison.InvariantCultureIgnoreCase
        };

        foreach (var comparison in comparisons)
        {
            foreach (var (literal, input) in cases)
            {
                var expected = input.StartsWith(literal, comparison);
                var actual = OneOf([literal, "zz"], comparison).Parse(input).Success;

                Assert.That(
                    actual,
                    Is.EqualTo(expected),
                    $"comparison={comparison}, literal={literal}, input={input}");
            }
        }

        var result = OneOf(["Μx", "zz"], StringComparison.OrdinalIgnoreCase).Parse("µx");
        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo("Μx"));
    }

    [Test]
    public void OneOf_OrdinalIgnoreCase_DoesNotMatchLongSWithS()
    {
        // The ordinal comparer does not treat U+017F (long s) as equivalent to 's'.
        Assert.That("sun".StartsWith("ſun", StringComparison.OrdinalIgnoreCase), Is.False);
        Assert.That(OneOf(["sun", "moon"], StringComparison.OrdinalIgnoreCase).Parse("ſun").Success, Is.False);
    }

    [Test]
    public void OneOf_OrdinalIgnoreCase_MatchesEquivalentBmpInitials()
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        var hashes = new Dictionary<int, List<char>>();

        for (var i = 0; i <= char.MaxValue; i++)
        {
            var c = (char)i;

            if (char.IsSurrogate(c))
                continue;

            var hash = comparer.GetHashCode(c.ToString());
            if (!hashes.TryGetValue(hash, out var bucket))
            {
                bucket = [];
                hashes[hash] = bucket;
            }

            bucket.Add(c);
        }

        var classes = new List<List<char>>();
        foreach (var bucket in hashes.Values)
        {
            var local = new List<List<char>>();

            foreach (var c in bucket)
            {
                List<char>? cls = null;

                foreach (var candidate in local)
                {
                    if (comparer.Equals(candidate[0].ToString(), c.ToString()))
                    {
                        cls = candidate;
                        break;
                    }
                }

                if (cls is null)
                    local.Add(new List<char> { c });
                else
                    cls.Add(c);
            }

            classes.AddRange(local);
        }

        var checkedClasses = 0;
        foreach (var cls in classes)
        {
            if (cls.Count < 2)
                continue;

            checkedClasses++;

            // The bucket key must be identical for all characters
            // considered equal by the ordinal ignore case comparer.
            var key = char.ToUpperInvariant(cls[0]);
            foreach (var c in cls)
            {
                Assert.That(
                    char.ToUpperInvariant(c),
                    Is.EqualTo(key),
                    $"Equivalence class of U+{(int)cls[0]:X4} is split by U+{(int)c:X4}");
            }

            var parser = OneOf(new[] { cls[0] + "x", "zz" }, StringComparison.OrdinalIgnoreCase);
            foreach (var c in cls)
            {
                Assert.That(
                    parser.Parse(c + "x").Success,
                    Is.True,
                    $"U+{(int)c:X4} should match the literal starting with U+{(int)cls[0]:X4}");
            }
        }

        Assert.That(checkedClasses, Is.GreaterThan(0));
    }

    [Test]
    public void OneOf_IgnoreCase_MatchesDeseretCasePair()
    {
        var comparisons = new[]
        {
            StringComparison.OrdinalIgnoreCase,
            StringComparison.InvariantCultureIgnoreCase
        };

        foreach (var comparison in comparisons)
        {
            // Deseret capital and small letters share the same high surrogate.
            var value = "\U00010400x";
            var input = "\U00010428x";

            Assert.That(input.StartsWith(value, comparison), Is.True, $"comparison={comparison}");

            var actual = OneOf([value, "zz"], comparison).Parse(input).Success;
            Assert.That(actual, Is.True, $"comparison={comparison}");
        }
    }

    [Test]
    public void OneOf_String_CultureSpecificMatches()
    {
        var literals = new[] { "kelvin", "abc", "xyz", "\u200Babc" };

        Assert.That(L("kelvin", StringComparison.InvariantCultureIgnoreCase).Parse("\u212Aelvin").Success, Is.True);
        Assert.That(OneOf(literals, StringComparison.InvariantCultureIgnoreCase).Parse("\u212Aelvin").Success, Is.True);

        Assert.That(L("abc", StringComparison.CurrentCulture).Parse("\u200Babc").Success, Is.True);
        Assert.That(OneOf(literals, StringComparison.CurrentCulture).Parse("\u200Babc").Success, Is.True);

        var comparisons = new[]
        {
            StringComparison.CurrentCulture,
            StringComparison.CurrentCultureIgnoreCase,
            StringComparison.InvariantCulture,
            StringComparison.InvariantCultureIgnoreCase
        };

        foreach (var comparison in comparisons)
        {
            foreach (var input in new[] { "kelvin", "KELVIN", "\u212Aelvin", "abc", "\u200Babc", "\0abc", "\u200Babcx" })
            {
                var expected = literals.Any(l => input.StartsWith(l, comparison));
                var actual = OneOf(literals, comparison).Parse(input).Success;

                Assert.That(actual, Is.EqualTo(expected), $"comparison={comparison}, input={input}");
            }

            // A literal with zero collation weight matches the empty input.
            var parsers = new[]
            {
                L("\u200B", comparison),
                OneOf(["\u200B", "zz"], comparison)
            };

            foreach (var parser in parsers)
            {
                Assert.That(
                    parser.Parse("").Success,
                    Is.EqualTo("".StartsWith("\u200B", comparison)),
                    $"comparison={comparison}");
            }
        }
    }

    [TestCase(StringComparison.CurrentCultureIgnoreCase, "en-US", "tr-TR", "i", "I", true)]
    [TestCase(StringComparison.CurrentCultureIgnoreCase, "tr-TR", "en-US", "i", "I", false)]
    [TestCase(StringComparison.CurrentCulture, "en-US", "cs-CZ", "c", "ch", true)]
    [TestCase(StringComparison.CurrentCulture, "cs-CZ", "en-US", "c", "ch", false)]
    public void String_CurrentCultureIsCapturedAtConstruction(StringComparison comparison, string creationCulture, string parseCulture, string literal, string input, bool expected)
    {
        var original = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(creationCulture);
            var parsers = new[]
            {
                L(literal, comparison),
                OneOf([literal], comparison),
                OneOf([literal, "zz"], comparison)
            };

            foreach (var parser in parsers)
                Assert.That(parser.Parse(input).Success, Is.EqualTo(expected));

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(parseCulture);

            // Conversion after the culture switch must retain the original comparison.
            foreach (var parser in parsers)
            {
                foreach (var variant in new[] { parser, parser.As("literal") })
                {
                    var result = variant.Parse(input);
                    Assert.That(result.Success, Is.EqualTo(expected));

                    if (expected)
                        Assert.That(result.Value, Is.EqualTo(literal));

                    Assert.That(variant.Void().Parse(input).Success, Is.EqualTo(expected));
                }
            }

            Assert.That(L(literal, comparison).Parse(input).Success, Is.EqualTo(!expected));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [TestCase("i")]
    [TestCase("\u200Bi")]
    public void OneOf_String_CurrentCultureFreezesCandidateSelection(string literal)
    {
        var original = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            var literals = new[] { "I", literal };
            var parser = OneOf(literals, StringComparison.CurrentCultureIgnoreCase);

            Assert.That(parser.Parse("i").Value, Is.EqualTo("I"));

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");

            var actual = parser.Parse("i");
            Assert.That(actual.Success, Is.True);
            Assert.That(actual.Value, Is.EqualTo("I"));

            var fresh = OneOf(literals, StringComparison.CurrentCultureIgnoreCase).Parse("i");
            Assert.That(fresh.Success, Is.True);
            Assert.That(fresh.Value, Is.EqualTo(literal));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [TestCase("en-US", "tr-TR", "\u0131")]
    [TestCase("tr-TR", "en-US", "I")]
    public void OneOf_CurrentCultureChanges_PreservesSelectedLiteral(string creationCulture, string parseCulture, string expected)
    {
        var original = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(creationCulture);
            var parser = OneOf(["I", "\u0131"], StringComparison.CurrentCultureIgnoreCase);

            Assert.That(parser.Parse("\u0131").Value, Is.EqualTo(expected));

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(parseCulture);

            var result = parser.Parse("\u0131");
            Assert.That(result.Success, Is.True);
            Assert.That(result.Value, Is.EqualTo(expected));
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [TestCase(StringComparison.CurrentCulture)]
    [TestCase(StringComparison.CurrentCultureIgnoreCase)]
    [TestCase(StringComparison.InvariantCulture)]
    [TestCase(StringComparison.InvariantCultureIgnoreCase)]
    [SetCulture("en-US")]
    public void String_CultureComparison_ConsumesMatchedSourceLength(StringComparison comparison)
    {
        var cases = new (string Literal, string Input, string Matched, string Remaining)[]
        {
            ("abc", "\u200Babc!", "\u200Babc", "!"),
            ("abc", "\0abc!", "\0abc", "!"),
            ("\u200Babc", "abc!", "abc", "!"),
            ("abc", "abc\u200B!", "abc", "\u200B!"),
            ("\u200B", "!", "", "!"),
            ("\u200B", "", "", ""),
            ("é", "e\u0301!", "e\u0301", "!"),
            ("e\u0301", "é!", "é", "!"),
            ("\U00010400", "\u200B\U00010400!", "\u200B\U00010400", "!")
        };

        foreach (var (literal, input, matched, remaining) in cases)
        {
            var parsers = new[]
            {
                L(literal, comparison),
                OneOf([literal], comparison),
                OneOf([literal, "zz"], comparison)
            };

            foreach (var parser in parsers)
            {
                foreach (var variant in new[] { parser, parser.As("literal") })
                {
                    var context = new ParseContext("@" + input);
                    context.Advance(1);

                    Assert.That(variant.TryParse(ref context, out var value), Is.True);
                    Assert.That(value, Is.EqualTo(literal));
                    Assert.That(context.Position, Is.EqualTo(1 + matched.Length));
                    Assert.That(context.Remaining.ToString(), Is.EqualTo(remaining));

                    var tail = remaining.Length == 0 ? Eof : L(remaining).Void().ThenIgnore(Eof);
                    var composed = L("@")
                        .Then(variant.Map(m => (m.Index, m.Length, Text: m.ToString())))
                        .ThenIgnore(tail)
                        .Parse("@" + input);

                    Assert.That(composed.Success, Is.True);
                    Assert.That(composed.Value, Is.EqualTo((1, matched.Length, matched)));
                }
            }
        }
    }

    [Test]
    public void OneOf_String_MatchesSingleLiteralParsers()
    {
        var alphabet = new[]
        {
            'a', 'b', 'c', 'A', 'B', 'C',
            'µ', 'Μ', 'μ', 'σ', 'ς', 'Σ',
            'В', 'в', 'ᲀ', 'Ι', 'ι', '\u0345', '\u1FBE',
            '\u200B', '\u212A', '\0', 'ß'
        };

        var random = new Random(12);
        var comparisons = Enum.GetValues<StringComparison>();

        for (var iteration = 0; iteration < 300; iteration++)
        {
            var literals = new string[random.Next(2, 6)];
            for (var i = 0; i < literals.Length; i++)
                literals[i] = RandomString(random, alphabet, 1, 4);

            var input = RandomString(random, alphabet, 0, 5);

            foreach (var comparison in comparisons)
            {
                var expected = literals.Any(l => input.StartsWith(l, comparison));
                var actual = OneOf(literals, comparison).Parse(input).Success;

                Assert.That(
                    actual,
                    Is.EqualTo(expected),
                    $"comparison={comparison}, input={Display(input)}, literals={string.Join(", ", literals.Select(Display))}");
            }
        }

        static string RandomString(Random random, char[] alphabet, int minLength, int maxLength)
        {
            var chars = new char[random.Next(minLength, maxLength + 1)];
            for (var i = 0; i < chars.Length; i++)
                chars[i] = alphabet[random.Next(alphabet.Length)];

            return new string(chars);
        }

        static string Display(string value) =>
            string.Join("", value.Select(c => c is >= ' ' and < '\u007F' ? c.ToString() : $"\\u{(int)c:X4}"));
    }

    [Test]
    public void OneOf_String_ValidatesLiterals()
    {
        Assert.Throws<ArgumentException>(() => { _ = OneOf(["a", null!]); });
        Assert.Throws<ArgumentException>(() => { _ = OneOf(["a", ""]); });
        Assert.Throws<ArgumentException>(() => { _ = OneOf((string[])null!); });
        Assert.Throws<ArgumentException>(() => { _ = OneOf([]); });
    }
}
