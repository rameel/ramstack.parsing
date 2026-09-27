namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that attempts to match the specified literal.
    /// </summary>
    /// <param name="literal">The text to match.</param>
    /// <param name="comparison">The string comparison type to use for matching the literal.
    /// Defaults to <see cref="StringComparison.Ordinal"/>.</param>
    /// <returns>
    /// A parser that attempts to match the specified literal using the provided comparison type.
    /// </returns>
    /// <remarks>
    /// Comparisons that use the current culture capture it when the parser is created.
    /// Later changes to the current culture do not affect the parser or its named and void variants.
    /// Culture-aware matches consume the matched source segment, whose length may differ from
    /// the literal length, including zero for literals with no collation weight.
    /// </remarks>
    public static Parser<string> L(string literal, StringComparison comparison = StringComparison.Ordinal)
    {
        Argument.ThrowIfNullOrEmpty(literal);

        var expected = literal.ToPrintable();
        if (comparison == StringComparison.Ordinal)
            return new OrdinalStringParser<string, CompareOptionsNone>(literal) { Name = expected };

        if (comparison == StringComparison.OrdinalIgnoreCase)
            return new OrdinalStringParser<string, CompareOptionsIgnoreCase>(literal) { Name = expected };

        var culture = comparison switch
        {
            StringComparison.CurrentCulture or
            StringComparison.CurrentCultureIgnoreCase => CultureInfo.CurrentCulture,
            _ => CultureInfo.InvariantCulture
        };

        var options = comparison switch
        {
            StringComparison.CurrentCulture or StringComparison.InvariantCulture => CompareOptions.None,
            _ => CompareOptions.IgnoreCase,
        };

        return new StringParser<string>(literal, culture.CompareInfo, options) { Name = expected };
    }

    /// <summary>
    /// Creates a parser that matches one of the specified literals.
    /// </summary>
    /// <param name="literal1">The first text to match.</param>
    /// <param name="literal2">The second text to match.</param>
    /// <param name="comparison">The string comparison type to use for matching the literal.
    /// Defaults to <see cref="StringComparison.Ordinal"/>.</param>
    /// <returns>
    /// A parser that parses one of the specified literals using the provided comparison type.
    /// </returns>
    public static Parser<string> OneOf(string literal1, string literal2, StringComparison comparison = StringComparison.Ordinal) =>
        OneOf([literal1, literal2], comparison);

    /// <summary>
    /// Creates a parser that matches one of the specified literals.
    /// </summary>
    /// <param name="literal1">The first text to match.</param>
    /// <param name="literal2">The second text to match.</param>
    /// <param name="literal3">The third text to match.</param>
    /// <param name="comparison">The string comparison type to use for matching the literal.
    /// Defaults to <see cref="StringComparison.Ordinal"/>.</param>
    /// <returns>
    /// A parser that parses one of the specified literals using the provided comparison type.
    /// </returns>
    public static Parser<string> OneOf(string literal1, string literal2, string literal3, StringComparison comparison = StringComparison.Ordinal) =>
        OneOf([literal1, literal2, literal3], comparison);

    /// <summary>
    /// Creates a parser that matches one of the specified literals.
    /// </summary>
    /// <param name="literal1">The first text to match.</param>
    /// <param name="literal2">The second text to match.</param>
    /// <param name="literal3">The third text to match.</param>
    /// <param name="literal4">The fourth text to match.</param>
    /// <param name="comparison">The string comparison type to use for matching the literal.
    /// Defaults to <see cref="StringComparison.Ordinal"/>.</param>
    /// <returns>
    /// A parser that parses one of the specified literals using the provided comparison type.
    /// </returns>
    public static Parser<string> OneOf(string literal1, string literal2, string literal3, string literal4, StringComparison comparison = StringComparison.Ordinal) =>
        OneOf([literal1, literal2, literal3, literal4], comparison);

    /// <summary>
    /// Creates a parser that matches one of the specified literals.
    /// </summary>
    /// <param name="literals">The array of literals to match.</param>
    /// <param name="comparison">The string comparison type to use for matching the literal.
    /// Defaults to <see cref="StringComparison.Ordinal"/>.</param>
    /// <returns>
    /// A parser that parses one of the specified literals using the provided comparison type.
    /// </returns>
    /// <remarks>
    /// When multiple literals match the beginning of the source, the parser returns the one that comes
    /// first when the literals are sorted in descending order using the specified comparison.
    /// For ordinal comparisons, this means the longest matching literal.
    /// Ties are resolved by the original order of the literals.
    /// Comparisons that use the current culture capture it when the parser is created,
    /// using the same culture for ordering, deduplication and matching.
    /// Later changes to the current culture do not affect the parser or its named and void variants.
    /// Culture-aware matches consume the matched source segment, whose length may differ from
    /// the literal length, including zero for literals with no collation weight.
    /// </remarks>
    public static Parser<string> OneOf(string[] literals, StringComparison comparison = StringComparison.Ordinal)
    {
        Argument.ThrowIfNullOrEmpty(literals);

        for (var i = 0; i < literals.Length; i++)
            Argument.ThrowIfNullOrEmpty(literals[i], $"{nameof(literals)}[{i}]");

        if (literals.Length == 1)
            return L(literals[0], comparison);

        var expected = literals.Select(l => l.ToPrintable()).ToArray();

        if (comparison is StringComparison.Ordinal or StringComparison.OrdinalIgnoreCase)
        {
            var ordinalComparer = StringComparer.FromComparison(comparison);
            var ignoreCase = comparison == StringComparison.OrdinalIgnoreCase;
            var dictionary = new CharMap(
                literals
                    .GroupBy(l => GetBucketKey(l[0], ignoreCase))
                    .ToDictionary(
                        g => g.Key,
                        g => g
                            .Distinct(ordinalComparer)
                            .OrderDescending(ordinalComparer)
                            .ToArray()));

            return ignoreCase
                ? new OrdinalStringDictionaryParser<string, CompareOptionsIgnoreCase>(dictionary, expected)
                : new OrdinalStringDictionaryParser<string, CompareOptionsNone>(dictionary, expected);
        }

        var culture = comparison switch
        {
            StringComparison.CurrentCulture or StringComparison.CurrentCultureIgnoreCase => CultureInfo.CurrentCulture,
            StringComparison.InvariantCulture or StringComparison.InvariantCultureIgnoreCase => CultureInfo.InvariantCulture,
            _ => throw new ArgumentOutOfRangeException(nameof(comparison))
        };

        var options = comparison switch
        {
            StringComparison.CurrentCultureIgnoreCase or
            StringComparison.InvariantCultureIgnoreCase => CompareOptions.IgnoreCase,
            _ => CompareOptions.None
        };

        var comparer = StringComparer.Create(culture, options);
        var candidates = literals
            .Distinct(comparer)
            .OrderDescending(comparer)
            .ToArray();

        return new CultureStringParser<string>(candidates, culture.CompareInfo, options, expected);
    }

    #region Inner type: OrdinalStringParser

    /// <summary>
    /// Represents a parser that attempts to match the specified literal using ordinal string comparison.
    /// This parser is specialized for performance with ordinal comparison.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <typeparam name="TCompareOptions">The compare options used for matching strings.</typeparam>
    private sealed class OrdinalStringParser<T, TCompareOptions> : Parser<T>
    {
        private readonly string _literal;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrdinalStringParser{T,TCompareOptions}"/> class.
        /// </summary>
        /// <param name="literal">The string to match.</param>
        public OrdinalStringParser(string literal) =>
            _literal = literal;

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var literal = _literal;
            // Null-check trick: prove to the JIT that literal is non-null so the
            // string -> ReadOnlySpan<char> conversion stays branch-free.
            _ = literal.Length;

            var result = typeof(TCompareOptions) == typeof(CompareOptionsIgnoreCase)
                ? context.Remaining.StartsWith(literal.AsSpan(), StringComparison.OrdinalIgnoreCase)
                : context.Remaining.StartsWith(literal.AsSpan());

            if (result)
            {
                if (typeof(T) != typeof(Unit))
                    value = (T)(object)literal;
                else
                    value = default!;

                context.Advance(literal.Length);
                return true;
            }

            value = default;
            context.ReportExpected(Name);

            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new OrdinalStringParser<T, TCompareOptions>(_literal) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new OrdinalStringParser<Unit, TCompareOptions>(_literal) { Name = Name };
    }

    #endregion

    #region Inner type: StringParser

    /// <summary>
    /// Represents a parser that attempts to match a specified literal using a specified string comparison type.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    private sealed class StringParser<T> : Parser<T>
    {
        private readonly string _literal;
        private readonly CompareInfo _compareInfo;
        private readonly CompareOptions _compareOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringParser{T}"/> class.
        /// </summary>
        /// <param name="literal">The string to match.</param>
        /// <param name="compareInfo">The comparison information captured when the parser is created.</param>
        /// <param name="compareOptions">The comparison options to use for matching the literal.</param>
        public StringParser(string literal, CompareInfo compareInfo, CompareOptions compareOptions) =>
            (_literal, _compareInfo, _compareOptions) = (literal, compareInfo, compareOptions);

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            if (_compareInfo.IsPrefix(context.Remaining, _literal, _compareOptions, out var length))
            {
                if (typeof(T) != typeof(Unit))
                    value = (T)(object)_literal;
                else
                    value = default!;

                context.Advance(length);
                return true;
            }

            value = default;
            context.ReportExpected(Name);
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new StringParser<T>(_literal, _compareInfo, _compareOptions) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new StringParser<Unit>(_literal, _compareInfo, _compareOptions) { Name = Name };
    }

    #endregion

    #region Inner type: OrdinalStringDictionaryParser

    /// <summary>
    /// Represents a parser that attempts to match one of the specified literals.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <typeparam name="TCompareOptions">The compare options used for matching strings.</typeparam>
    private sealed class OrdinalStringDictionaryParser<T, TCompareOptions> : Parser<T>
    {
        private readonly CharMap _literals;
        private readonly string[] _expected;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrdinalStringDictionaryParser{T,TCompareOptions}"/> class.
        /// </summary>
        /// <param name="literals">A dictionary mapping initial characters to arrays of string literals expected at the beginning of the source.</param>
        /// <param name="expected">An array of error messages describing expected literals, used when parsing fails.</param>
        public OrdinalStringDictionaryParser(CharMap literals, string[] expected)
        {
            _literals = literals;
            _expected = expected;
        }

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var s = context.Remaining;
            if (s.Length != 0)
            {
                var literals = typeof(TCompareOptions) == typeof(CompareOptionsIgnoreCase)
                    ? _literals[GetBucketKey(s[0], ignoreCase: true)]
                    : _literals[s[0]];

                foreach (var literal in literals ?? [])
                {
                    _ = literal.Length;

                    var result = typeof(TCompareOptions) == typeof(CompareOptionsIgnoreCase)
                        ? s.StartsWith(literal.AsSpan(), StringComparison.OrdinalIgnoreCase)
                        : s.StartsWith(literal.AsSpan());

                    if (result)
                    {
                        if (typeof(T) != typeof(Unit))
                            value = (T)(object)literal;
                        else
                            value = default!;

                        context.Advance(literal.Length);
                        return true;
                    }
                }
            }

            switch (Name)
            {
                case not null:
                    context.ReportExpected(Name);
                    break;
                default:
                    context.ReportExpected(_expected);
                    break;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new OrdinalStringDictionaryParser<T, TCompareOptions>(_literals, _expected) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new OrdinalStringDictionaryParser<Unit, TCompareOptions>(_literals, _expected) { Name = Name };
    }

    #endregion

    #region Inner type: CultureStringParser

    /// <summary>
    /// Represents a parser that attempts to match one of the specified literals using culture-specific string comparison.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    private sealed class CultureStringParser<T> : Parser<T>
    {
        private readonly string[] _literals;
        private readonly CompareInfo _compareInfo;
        private readonly CompareOptions _compareOptions;
        private readonly string[] _expected;

        /// <summary>
        /// Initializes a new instance of the <see cref="CultureStringParser{T}"/> class.
        /// </summary>
        /// <param name="literals">An array of literals sorted in descending order using the captured comparison.</param>
        /// <param name="compareInfo">The comparison information captured when the parser is created.</param>
        /// <param name="compareOptions">An optional combination of <see cref="CompareOptions"/> enumeration values to use during the match.</param>
        /// <param name="expected">An array of error messages describing expected literals, used when parsing fails.</param>
        public CultureStringParser(string[] literals, CompareInfo compareInfo, CompareOptions compareOptions, string[] expected)
        {
            _literals = literals;
            _compareInfo = compareInfo;
            _compareOptions = compareOptions;
            _expected = expected;
        }

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var s = context.Remaining;

            foreach (var literal in _literals)
            {
                _ = literal.Length;

                if (_compareInfo.IsPrefix(s, literal, _compareOptions, out var length))
                {
                    if (typeof(T) != typeof(Unit))
                        value = (T)(object)literal;
                    else
                        value = default!;

                    context.Advance(length);
                    return true;
                }
            }

            switch (Name)
            {
                case not null:
                    context.ReportExpected(Name);
                    break;
                default:
                    context.ReportExpected(_expected);
                    break;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new CultureStringParser<T>(_literals, _compareInfo, _compareOptions, _expected) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new CultureStringParser<Unit>(_literals, _compareInfo, _compareOptions, _expected) { Name = Name };
    }

    #endregion

    private static char GetBucketKey(char c, bool ignoreCase)
    {
        if (!ignoreCase)
            return c;

        // All high surrogates share a single bucket.
        // char.ToUpperInvariant cannot fold supplementary characters,
        // so this keeps both parts of a case pair in the same bucket
        // even if their leading code units would not match otherwise.
        return char.IsHighSurrogate(c) ? '\uD800' : char.ToUpperInvariant(c);
    }

    /// <summary>
    /// A marker struct used to indicate that string comparisons should be case-sensitive.
    /// </summary>
    private struct CompareOptionsNone;

    /// <summary>
    /// A marker struct used to indicate that string comparisons should be case-insensitive.
    /// </summary>
    private struct CompareOptionsIgnoreCase;
}
