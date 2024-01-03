using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace Ramstack.Parsing.Utilities;

/// <summary>
/// Represents a character class that defines a set of characters based on Unicode categories or specific ranges.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
internal sealed class CharClass
{
    /// <summary>
    /// A dictionary mapping Unicode category designations to their corresponding <see cref="GeneralUnicodeCategory"/> values.
    /// </summary>
    public static readonly Dictionary<string, GeneralUnicodeCategory> Categories = new Dictionary<string, GeneralUnicodeCategory>
    {
        // Letters
        ["Lu"] = GeneralUnicodeCategory.UppercaseLetter,
        ["Ll"] = GeneralUnicodeCategory.LowercaseLetter,
        ["Lt"] = GeneralUnicodeCategory.TitlecaseLetter,
        ["Lm"] = GeneralUnicodeCategory.ModifierLetter,
        ["Lo"] = GeneralUnicodeCategory.OtherLetter,
        ["L" ] = GeneralUnicodeCategory.Letter,

        // Marks
        ["Mn"] = GeneralUnicodeCategory.NonSpacingMark,
        ["Mc"] = GeneralUnicodeCategory.SpacingCombiningMark,
        ["Me"] = GeneralUnicodeCategory.EnclosingMark,
        ["M" ] = GeneralUnicodeCategory.Mark,

        // Numbers
        ["Nd"] = GeneralUnicodeCategory.DecimalDigitNumber,
        ["Nl"] = GeneralUnicodeCategory.LetterNumber,
        ["No"] = GeneralUnicodeCategory.OtherNumber,
        ["N" ] = GeneralUnicodeCategory.Number,

        // Separators
        ["Zs"] = GeneralUnicodeCategory.SpaceSeparator,
        ["Zl"] = GeneralUnicodeCategory.LineSeparator,
        ["Zp"] = GeneralUnicodeCategory.ParagraphSeparator,
        ["Z" ] = GeneralUnicodeCategory.Separator,

        // Controls
        ["Cc"] = GeneralUnicodeCategory.Control,
        ["Cf"] = GeneralUnicodeCategory.Format,
        ["Cs"] = GeneralUnicodeCategory.Surrogate,
        ["Co"] = GeneralUnicodeCategory.PrivateUse,
        ["Cn"] = GeneralUnicodeCategory.OtherNotAssigned,
        ["C" ] = GeneralUnicodeCategory.Other,

        // Punctuations
        ["Pc"] = GeneralUnicodeCategory.ConnectorPunctuation,
        ["Pd"] = GeneralUnicodeCategory.DashPunctuation,
        ["Ps"] = GeneralUnicodeCategory.OpenPunctuation,
        ["Pe"] = GeneralUnicodeCategory.ClosePunctuation,
        ["Pi"] = GeneralUnicodeCategory.InitialQuotePunctuation,
        ["Pf"] = GeneralUnicodeCategory.FinalQuotePunctuation,
        ["Po"] = GeneralUnicodeCategory.OtherPunctuation,
        ["P" ] = GeneralUnicodeCategory.Punctuation,

        // Symbols
        ["Sm"] = GeneralUnicodeCategory.MathSymbol,
        ["Sc"] = GeneralUnicodeCategory.CurrencySymbol,
        ["Sk"] = GeneralUnicodeCategory.ModifierSymbol,
        ["So"] = GeneralUnicodeCategory.OtherSymbol,
        ["S" ] = GeneralUnicodeCategory.Symbol
    };

    /// <summary>
    /// An array of Unicode category group abbreviations.
    /// </summary>
    public static readonly string[] CategoryGroups = ["L", "M", "N", "Z", "C", "P", "S"];

    private readonly CharClassRange[] _source;
    private readonly CharClassRange[] _ranges;
    private readonly CharClassUnicodeCategory _categories;

    /// <summary>
    /// Gets the source (original) ranges of characters.
    /// </summary>
    public CharClassRange[] Source => _source;

    /// <summary>
    /// Gets the extended ranges of characters, which include additional
    /// characters based on the specified Unicode categories.
    /// </summary>
    public CharClassRange[] Ranges => _ranges;

    /// <summary>
    /// Gets the Unicode categories associated with this character class.
    /// </summary>
    public CharClassUnicodeCategory UnicodeCategories => _categories;

    /// <summary>
    /// Gets the total number of unique symbols across all ranges.
    /// </summary>
    /// <remarks>
    /// This property calculates the sum of all symbols included in the ranges.
    /// Each range contributes the count of its symbols, determined as <c>Hi - Lo + 1</c>.
    /// </remarks>
    public int SymbolCount { get; }

    /// <summary>
    /// Gets the total width between the lowest and highest characters across all ranges.
    /// </summary>
    /// <remarks>
    /// This property calculates the difference between the lowest character in the first range
    /// and the highest character in the last range, inclusive.
    /// The width represents the entire span covered by all ranges, including any gaps between them.
    /// This value is useful for determining the required size of data structures (like bit arrays)
    /// that need to represent the full range.
    /// </remarks>
    public int OverallWidth { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharClass"/> class using the specified elements.
    /// </summary>
    /// <param name="elements">The character class elements to initialize from.</param>
    public CharClass(CharClassElement[] elements)
        : this(ExtractRanges(elements), ExtractCategory(elements))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharClass"/> class using the specified ranges.
    /// </summary>
    /// <param name="ranges">The character ranges to initialize from.</param>
    public CharClass(CharClassRange[] ranges) : this(ranges, ranges, default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharClass"/> class using the specified ranges and Unicode categories.
    /// </summary>
    /// <param name="ranges">The character ranges to initialize from.</param>
    /// <param name="categories">The Unicode categories to initialize from.</param>
    public CharClass(CharClassRange[] ranges, CharClassUnicodeCategory categories) : this(ranges, ranges, categories)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharClass"/> class using the specified Unicode categories.
    /// </summary>
    /// <param name="categories">The Unicode categories to initialize from.</param>
    public CharClass(CharClassUnicodeCategory categories) : this([], [], categories)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharClass"/> class using the specified source ranges,
    /// optimized ranges, and Unicode categories.
    /// </summary>
    /// <param name="source">The source character ranges.</param>
    /// <param name="ranges">The optimized character ranges.</param>
    /// <param name="categories">The Unicode categories.</param>
    private CharClass(CharClassRange[] source, CharClassRange[] ranges, CharClassUnicodeCategory categories)
    {
        Array.Sort(ranges);

        // If Unicode categories are specified, extend the lower ranges
        // with characters belonging to the specified categories.
        if (categories.Value != 0 && ranges.Length != 0)
            ranges = EnrichRangesCore(ranges, categories);

        source = source.Optimize();
        ranges = ranges.Optimize();

        if (ranges.AsSpan().SequenceEqual(source.AsSpan()))
            source = ranges;

        if (ranges is [{ IsFullRange: true }])
            categories = default;

        foreach (var range in ranges)
            SymbolCount += range.SymbolCount;

        if (ranges.Length != 0)
            OverallWidth = ranges[^1].Hi - ranges[0].Lo + 1;

        (_source, _ranges, _categories) = (source, ranges, categories);
    }

    /// <summary>
    /// Returns a string containing all characters represented in the current character class.
    /// </summary>
    /// <param name="simdAligned">If set to <see langword="true"/>, the length of the resulting string is aligned
    /// to the nearest multiple of a SIMD-friendly size (16 or 32 bytes) if SIMD instructions are supported.
    /// </param>
    /// <returns>
    /// A string consisting of all characters within the defined ranges of this instance.
    /// </returns>
    public string GetSymbols(bool simdAligned = false)
    {
        var align = Avx2.IsSupported
            ? 16
            : Sse41.IsSupported || AdvSimd.IsSupported
                ? 8
                : 0;

        var count = simdAligned && align != 0
            ? SymbolCount + (align - 1) & -align
            : SymbolCount;

        return string.Create(count, (align, Ranges), (span, state) =>
        {
            var i = 0;
            foreach (var (lo, hi) in state.Ranges)
                for (int c = lo; c <= hi; c++)
                    if ((uint)i < (uint)span.Length)
                        span[i++] = (char)c;

            while ((uint)i < (uint)span.Length)
                span[i++] = span[0];
        });
    }

    /// <summary>
    /// Merges this character class with another character class.
    /// </summary>
    /// <param name="other">The other character class to merge with.</param>
    /// <returns>
    /// A new <see cref="CharClass"/> representing the merged result.
    /// </returns>
    public CharClass MergeClasses(CharClass other)
    {
        return new CharClass(
            [.._source, ..other._source],
            [.._ranges, ..other._ranges],
            _categories.Combine(other._categories)
            );
    }

    /// <summary>
    /// Populates the character class ranges with characters from the specified Unicode categories,
    /// ensuring that the ranges contain all relevant characters for optimized processing.
    /// </summary>
    /// <returns>
    /// A new <see cref="CharClass"/> instance with updated character ranges that include
    /// characters from the specified Unicode categories.
    /// If no Unicode categories are specified, returns the current instance unchanged.
    /// </returns>
    public CharClass EnrichRanges()
    {
        if (_categories.IsEmpty)
            return this;

        var ranges = EnrichRangesCore(_ranges, _categories);
        return new CharClass(_source, ranges, _categories);
    }

    /// <summary>
    /// Generates an extended set of character class ranges by adding characters from the specified Unicode categories.
    /// This is useful for cases where Unicode category-based classification is needed in addition to predefined ranges.
    /// </summary>
    /// <param name="categories">The Unicode character categories whose characters should be added to the ranges.</param>
    /// <param name="ranges">The existing character class ranges that will be extended.</param>
    /// <returns>
    /// An array of <see cref="CharClassRange"/> that includes the original ranges
    /// and additional characters from the specified Unicode categories.
    /// </returns>
    private static CharClassRange[] EnrichRangesCore(CharClassRange[] ranges, CharClassUnicodeCategory categories)
    {
        Debug.Assert(!categories.IsEmpty);

        var builder = new ArrayBuilder<CharClassRange>();

        // Identify used ASCII characters.
        for (var c = '\0'; c < 128; c++)
            if (categories.IsInclude(c))
                builder.Add(CharClassRange.Create(c));

        // Characters in the range [128..511].
        // Only if the specified range is used.
        if (ranges.Length != 0 && (int)ranges[^1].Hi is >= 128 and < 512)
            for (var c = (char)128; c < 512; c++)
                if (categories.IsInclude(c))
                    builder.Add(CharClassRange.Create(c));

        //
        // TODO Should we populate non-ASCII ranges if no ASCII values are present?
        //

        builder.AddRange(ranges);
        return builder.ToArray();
    }

    /// <inheritdoc />
    public override string ToString() =>
        this.ToPrintable();

    /// <summary>
    /// Extracts Unicode categories from the specified elements.
    /// </summary>
    /// <param name="elements">The elements to extract categories from.</param>
    /// <returns>
    /// A <see cref="CharClassUnicodeCategory"/> representing the combined categories.
    /// </returns>
    private static CharClassUnicodeCategory ExtractCategory(CharClassElement[] elements)
    {
        CharClassUnicodeCategory value = default;
        foreach (var element in elements)
            if (element.TryGetUnicodeCategory(out var category))
                value = value.Combine(category);

        return value;
    }

    /// <summary>
    /// Extracts character ranges from the specified elements.
    /// </summary>
    /// <param name="elements">The elements to extract ranges from.</param>
    /// <returns>
    /// An array of <see cref="CharClassRange"/> representing the extracted ranges.
    /// </returns>
    private static CharClassRange[] ExtractRanges(CharClassElement[] elements)
    {
        var builder = new ArrayBuilder<CharClassRange>();
        foreach (var element in elements)
            if (element.TryGetRange(out var range))
                builder.Add(range);

        return builder.ToArray();
    }
}
