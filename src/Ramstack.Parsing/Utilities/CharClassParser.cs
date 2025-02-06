namespace Ramstack.Parsing.Utilities;

/// <summary>
/// Represents a character class expression parser.
/// </summary>
internal static class CharClassParser
{
    /// <summary>
    /// Parses a character class from the specified regular expression pattern.
    /// </summary>
    /// <param name="pattern">The string containing the character class to parse.</param>
    /// <returns>
    /// A <see cref="CharClass"/> object representing the parsed character class.
    /// </returns>
    public static CharClass Parse(string pattern)
    {
        // // TODO: Add special handling for patterns enclosed in square brackets
        // // Temporary workaround to handle patterns enclosed in square brackets.
        // if (pattern is ['[', .., ']'])
        //     pattern = pattern[1..^1];

        Argument.ThrowIfNullOrEmpty(nameof(pattern));

        var elements = new List<CharClassElement>();
        var p = 0;

        while (p < pattern.Length)
        {
            if (ProcessCategory())
                continue;

            if (ProcessShorthand())
                continue;

            ProcessRange();
        }

        return new CharClass(elements.ToArray());

        bool ProcessCategory()
        {
            var s = pattern.AsSpan(p);
            if (!s.StartsWith("\\p{") && !s.StartsWith("\\P{"))
                return false;

            var end = pattern.IndexOf('}', p + 3);
            if (end < 0)
                Error_InvalidPattern(pattern);

            // I skipped adding support for named Unicode blocks (like \p{Hiragana})
            // since it seems many parser users don't need it. If I'm wrong about that,
            // it can be fixed quickly - just a couple lines of code. For now though,
            // range definitions can be used instead.
            // Unicode Block Names: https://www.unicode.org/Public/16.0.0/ucd/Blocks.txt

            var key = pattern[(p + 3)..end];

            if (!CharClass.Categories.TryGetValue(key, out var category))
                Error_UnknownProperty(pattern, key);

            if (pattern[p + 1] == 'P')
                category = ~category;

            p = end + 1;

            var element = CharClassUnicodeCategory.Create(category);
            elements.Add(element);
            return true;
        }

        bool ProcessShorthand()
        {
            var s = pattern.AsSpan(p);
            if (s.Length > 1 && s[0] == '\\')
            {
                switch (s[1])
                {
                    case 's':
                        elements.Add(CharClassRange.Create('\u0009'));
                        elements.Add(CharClassRange.Create('\u000a'));
                        elements.Add(CharClassRange.Create('\u000b'));
                        elements.Add(CharClassRange.Create('\u000c'));
                        elements.Add(CharClassRange.Create('\u000d'));
                        elements.Add(CharClassRange.Create('\u0020'));
                        elements.Add(CharClassRange.Create('\u0085'));
                        elements.Add(
                            CharClassUnicodeCategory.Create(
                                GeneralUnicodeCategory.Separator));

                        p += 2;
                        return true;

                    case 'S':
                        elements.Add(CharClassRange.Create('\u0000', '\u0008'));
                        elements.Add(CharClassRange.Create('\u000e', '\u001f'));
                        elements.Add(CharClassRange.Create('\u0021', '\u0084'));
                        elements.Add(
                            CharClassUnicodeCategory.Create(
                                ~GeneralUnicodeCategory.Separator));

                        p += 2;
                        return true;

                    case 'd':
                    case 'D':
                        var digits = GeneralUnicodeCategory.DecimalDigitNumber;
                        if (s[1] == 'D')
                            digits = ~digits;

                        elements.Add(
                            CharClassUnicodeCategory.Create(
                                digits));

                        p += 2;
                        return true;

                    case 'w':
                    case 'W':
                        var word = GeneralUnicodeCategory.Letter
                            | GeneralUnicodeCategory.NonSpacingMark
                            | GeneralUnicodeCategory.DecimalDigitNumber
                            | GeneralUnicodeCategory.ConnectorPunctuation;

                        if (s[1] == 'W')
                            word = ~word;

                        elements.Add(
                            CharClassUnicodeCategory.Create(
                                word));

                        p += 2;
                        return true;
                }
            }

            return false;
        }

        void ProcessRange()
        {
            var lo = ParseSymbol();
            var hi = lo;

            if (p + 1 < pattern.Length && pattern[p] == '-')
            {
                p++;

                hi = ParseSymbol();
                if (lo > hi)
                    Error_InvalidRange(pattern);
            }

            var range = CharClassRange.Create(lo, hi);
            elements.Add(range);
        }

        char ParseSymbol()
        {
            var s = pattern.AsSpan(p);
            if (s.StartsWith(@"\u"))
                return ParseUnicodeSequence();

            if (s[0] == '\\')
                return ParseEscapeSequence();

            p++;
            return s[0];
        }

        char ParseEscapeSequence()
        {
            if ((uint)p + 1 >= (uint)pattern.Length
                || !"0abeftnvr\\-".Contains(pattern[++p]))
                Error_InvalidEscapeSequence(pattern);

            return pattern[p++] switch
            {
                '0' => '\0',
                'a' => '\a',
                'b' => '\b',
                'e' => '\u001b',
                'f' => '\f',
                't' => '\t',
                'n' => '\n',
                'v' => '\v',
                'r' => '\r',
                var c => c
            };
        }

        char ParseUnicodeSequence()
        {
            if (p + 5 >= pattern.Length)
                Error_InvalidUnicodeSequence(pattern);

            if (!uint.TryParse(pattern.AsSpan(p + 2, 4), NumberStyles.AllowHexSpecifier, null, out var n))
                Error_InvalidUnicodeSequence(pattern);

            p += 6;
            return (char)n;
        }
    }

    [DoesNotReturn]
    private static void Error_InvalidPattern(string pattern) =>
        throw new ArgumentException($"Invalid pattern '{pattern}'.");

    [DoesNotReturn]
    private static void Error_InvalidEscapeSequence(string pattern) =>
        throw new ArgumentException($"Invalid pattern '{pattern}'. Unrecognized escape sequence.");

    [DoesNotReturn]
    private static void Error_InvalidUnicodeSequence(string pattern) =>
        throw new ArgumentException($"Invalid pattern '{pattern}'. Unrecognized unicode sequence.");

    [DoesNotReturn]
    private static void Error_UnknownProperty(string pattern, string name) =>
        throw new ArgumentException($"Invalid pattern '{pattern}'. Unknown property '{name}'.");

    [DoesNotReturn]
    private static void Error_InvalidRange(string pattern) =>
        throw new ArgumentException($"Invalid pattern '{pattern}'. Range in reverse order.");
}
