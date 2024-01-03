namespace Ramstack.Parsing.Utilities;

/// <summary>
/// Provides a collection of extension methods for performing various operations
/// on characters and character-related data structures.
/// </summary>
internal static class CharClassExtensions
{
    /// <summary>
    /// A mask that includes all valid <see cref="GeneralUnicodeCategory"/> values.
    /// Used for category normalization.
    /// </summary>
    public const GeneralUnicodeCategory NormalizedCategoryMask =
        GeneralUnicodeCategory.Letter
        | GeneralUnicodeCategory.Mark
        | GeneralUnicodeCategory.Number
        | GeneralUnicodeCategory.Separator
        | GeneralUnicodeCategory.Punctuation
        | GeneralUnicodeCategory.Symbol
        | GeneralUnicodeCategory.Other;

    /// <summary>
    /// A constant representing a collection of Unicode categories that are considered as printable characters.
    /// </summary>
    public static readonly CharClassUnicodeCategory PrintableCategories =
        GeneralUnicodeCategory.UppercaseLetter
        | GeneralUnicodeCategory.LowercaseLetter
        | GeneralUnicodeCategory.TitlecaseLetter
        | GeneralUnicodeCategory.DecimalDigitNumber
        | GeneralUnicodeCategory.LetterNumber
        | GeneralUnicodeCategory.OtherNumber
        | GeneralUnicodeCategory.DashPunctuation
        | GeneralUnicodeCategory.OpenPunctuation
        | GeneralUnicodeCategory.ClosePunctuation
        | GeneralUnicodeCategory.ConnectorPunctuation
        | GeneralUnicodeCategory.OtherPunctuation
        | GeneralUnicodeCategory.InitialQuotePunctuation
        | GeneralUnicodeCategory.FinalQuotePunctuation
        | GeneralUnicodeCategory.SpaceSeparator
        | GeneralUnicodeCategory.MathSymbol
        | GeneralUnicodeCategory.CurrencySymbol;

    /// <summary>
    /// Optimizes a specified array of character ranges by merging overlapping or adjacent ranges.
    /// </summary>
    /// <param name="ranges">The array of character ranges to optimize.</param>
    /// <returns>
    /// A new array with optimized character ranges.
    /// </returns>
    public static CharClassRange[] Optimize(this CharClassRange[] ranges)
    {
        if (CanOptimize(ranges))
            ranges = Optimize(ranges.AsSpan());

        return ranges;
    }

    /// <summary>
    /// Optimizes a specified span of character ranges by merging overlapping or adjacent ranges.
    /// </summary>
    /// <param name="ranges">The span of character ranges to optimize.</param>
    /// <returns>
    /// A new array with optimized character ranges.
    /// </returns>
    public static CharClassRange[] Optimize(this Span<CharClassRange> ranges)
    {
        if (!CanOptimize(ranges))
            return ranges.ToArray();

        var builder = new ArrayBuilder<CharClassRange>();
        foreach (var range in ranges)
        {
            if (builder.Count != 0)
            {
                var (lo, hi) = builder[^1];

                if (range.Lo <= hi + 1)
                {
                    if (range.Hi > hi)
                        hi = range.Hi;

                    builder[^1] = CharClassRange.CreateMasked(lo | (hi << 16));
                    continue;
                }
            }

            builder.Add(range);
        }

        return builder.ToArray();
    }

    // /// <summary>
    // /// Inverts the character ranges.
    // /// </summary>
    // /// <param name="ranges">The array of character ranges to invert.</param>
    // /// <returns>
    // /// A new array with inverted character ranges.
    // /// </returns>
    // public static CharClassRange[] Invert(this CharClassRange[] ranges) =>
    //     Invert(ranges.AsSpan());
    //
    // /// <summary>
    // /// Inverts the character ranges.
    // /// </summary>
    // /// <param name="ranges">The span of character ranges to invert.</param>
    // /// <returns>
    // /// A new array with inverted character ranges.
    // /// </returns>
    // public static CharClassRange[] Invert(this Span<CharClassRange> ranges)
    // {
    //     var array = new ArrayBuilder<CharClassRange>();
    //     var start = 0;
    //
    //     foreach (var (lo, hi) in ranges)
    //     {
    //         if (lo - 1 >= start)
    //         {
    //             var range = CharClassRange.Create((char)start, (char)(lo - 1));
    //             array.Add(range);
    //         }
    //
    //         start = hi + 1;
    //         if (start > char.MaxValue)
    //             break;
    //     }
    //
    //     if (start <= char.MaxValue)
    //     {
    //         var range = CharClassRange.Create((char)start, '\uFFFF');
    //         array.Add(range);
    //     }
    //
    //     return array.ToArray();
    // }

    /// <summary>
    /// Determines if the specified array of character ranges can be optimized by merging overlapping or adjacent ranges.
    /// </summary>
    /// <param name="ranges">The array of character ranges to check.</param>
    /// <returns>
    /// <see langword="true"/> if the ranges can be optimized, otherwise <see langword="false"/>.
    /// </returns>
    public static bool CanOptimize(this CharClassRange[] ranges) =>
        CanOptimize(ranges.AsSpan());

    /// <summary>
    /// Determines if the specified span of character ranges can be optimized by merging overlapping or adjacent ranges.
    /// </summary>
    /// <param name="ranges">The span of character ranges to check.</param>
    /// <returns>
    /// <see langword="true"/> if the ranges can be optimized, otherwise <see langword="false"/>.
    /// </returns>
    public static bool CanOptimize(this Span<CharClassRange> ranges)
    {
        ranges.Sort();

        for (var i = 1; i < ranges.Length; i++)
            if (ranges[i - 1].Hi + 1 >= ranges[i].Lo)
                return true;

        return false;
    }

    /// <summary>
    /// Determines whether a given character is printable.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>
    /// <see langword="true"/> if the character is printable, otherwise <see langword="false"/>.
    /// </returns>
    public static bool IsPrintable(this char c) =>
        c is >= ' ' and <= '~' || PrintableCategories.IsInclude(c);

    /// <summary>
    /// Returns the printable string representation of the specified character.
    /// </summary>
    /// <param name="c">The character to format.</param>
    /// <param name="controls">The control characters that should be escaped.</param>
    /// <returns>
    /// The printable string representation of the specified character.
    /// </returns>
    public static string ToPrintable(this char c, string controls = "'")
    {
        return c switch
        {
            '\0' => @"'\0'",
            '\a' => @"'\a'",
            '\b' => @"'\b'",
            '\e' => @"'\e'",
            '\f' => @"'\f'",
            '\n' => @"'\n'",
            '\r' => @"'\r'",
            '\t' => @"'\t'",
            '\v' => @"'\v'",
            '\'' => @"'\''",
            '\\' => @"'\\'",
            _ when controls.Contains(c) => $"'\\{c}'",
            _ when IsPrintable(c) => $"'{c}'",
            _ => $"'\\u{(ushort)c:X4}'"
        };
    }

    /// <summary>
    /// Returns the printable string representation of the specified range of characters.
    /// </summary>
    /// <param name="range">The range of characters to format.</param>
    /// <returns>
    /// The printable string representation of the specified range of characters.
    /// </returns>
    public static string ToPrintable(this CharClassRange range)
    {
        if (range.IsSingleCharacter)
            return range.Lo.ToPrintable();

        var sb = new StringBuffer();
        sb.Append('[');
        Format(ref sb, range.Lo, "-");
        if (range.Lo + 1 < range.Hi)
            sb.Append('-');
        Format(ref sb, range.Hi, "-");
        sb.Append(']');
        return sb.ToString();
    }

    /// <summary>
    /// Returns the printable string representation of the specified Unicode categories.
    /// </summary>
    /// <param name="category">The Unicode categories to format.</param>
    /// <returns>
    /// The printable string representation of the specified Unicode categories.
    /// </returns>
    public static string ToPrintable(this GeneralUnicodeCategory category)
    {
        category &= NormalizedCategoryMask;
        if (category == 0)
            return "";

        var sb = new StringBuffer();
        Format(ref sb, category);
        return sb.ToString();
    }

    /// <summary>
    /// Returns the printable string representation of the specified string literal.
    /// </summary>
    /// <param name="literal">The string literal to format.</param>
    /// <returns>
    /// The printable string representation of the specified string literal.
    /// </returns>
    public static string ToPrintable(this string literal)
    {
        var sb = new StringBuffer();
        sb.Append('\'');
        foreach (var c in literal)
            Format(ref sb, c, "'");

        sb.Append('\'');
        return sb.ToString();
    }

    /// <summary>
    /// Returns the printable string representation of the specified <see cref="CharClass"/> object.
    /// </summary>
    /// <param name="class">The <see cref="CharClass"/> object to format.</param>
    /// <returns>
    /// The printable string representation of the specified <see cref="CharClass"/> object.
    /// </returns>
    internal static string ToPrintable(this CharClass @class)
    {
        if (@class is { Source: [{ IsSingleCharacter: true }], UnicodeCategories.IsEmpty: true })
            return @class.Source[0].ToPrintable();

        var sb = new StringBuffer();
        sb.Append('[');

        var ranges = @class.Source;

        for (var i = 0; i < ranges.Length; i++)
        {
            var (lo, hi) = ranges[i];
            if (lo != hi)
            {
                Format(ref sb, lo, "-");
                if (lo + 1 < hi)
                    sb.Append('-');
                Format(ref sb, hi, "-");
            }
            else
            {
                var controls = i + 1 != ranges.Length ? "-" : "";
                Format(ref sb, lo, controls);
            }
        }

        Format(ref sb, (GeneralUnicodeCategory)@class.UnicodeCategories.Value);

        sb.Append(']');
        return sb.ToString();
    }

    /// <summary>
    /// Formats the specified character as a printable string.
    /// </summary>
    /// <param name="sb">The string buffer to append the formatted string.</param>
    /// <param name="c">The character to format.</param>
    /// <param name="controls">The control characters that should be escaped.</param>
    private static void Format(ref StringBuffer sb, char c, string controls = "")
    {
        var s = c switch
        {
            '\0' => @"\0",
            '\a' => @"\a",
            '\b' => @"\b",
            '\e' => @"\e",
            '\f' => @"\f",
            '\n' => @"\n",
            '\r' => @"\r",
            '\t' => @"\t",
            '\v' => @"\v",
            '\'' => @"\'",
            '\\' => @"\\",
            _ when controls.Contains(c) => "\\" + c,
            _ => null
        };

        if (s is not null)
        {
            sb.Append(s);
        }
        else if (IsPrintable(c))
        {
            sb.Append(c);
        }
        else
        {
            sb.Append("\\u");
            sb.Append(((ushort)c).ToString("x4"));
        }
    }

    /// <summary>
    /// Formats the specified Unicode categories as a printable string.
    /// </summary>
    /// <param name="sb">The string buffer to append the formatted string.</param>
    /// <param name="category">The Unicode categories to format.</param>
    private static void Format(ref StringBuffer sb, GeneralUnicodeCategory category)
    {
        category &= NormalizedCategoryMask;

        foreach (var group in CharClass.CategoryGroups)
        {
            if (category == 0)
                return;

            if (!CharClass.Categories.TryGetValue(group, out var c))
                continue;

            var inverted = ~c & NormalizedCategoryMask;
            if ((category & inverted) != inverted)
                continue;

            sb.Append(@"\P{");
            sb.Append(group);
            sb.Append('}');
            category &= c;
        }

        foreach (var group in CharClass.CategoryGroups)
        {
            if (category == 0)
                return;

            if (!CharClass.Categories.TryGetValue(group, out var c))
                continue;

            if ((category & c) != c)
                continue;

            sb.Append(@"\p{");
            sb.Append(group);
            sb.Append('}');
            category &= ~c;
        }

        foreach (var (group, c) in CharClass.Categories)
        {
            if (category == 0)
                return;

            if ((category & c) == c)
            {
                sb.Append(@"\p{");
                sb.Append(group);
                sb.Append('}');

                category &= ~c;
            }
        }
    }
}
