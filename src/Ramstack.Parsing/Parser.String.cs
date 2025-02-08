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
    public static Parser<string> L(string literal, StringComparison comparison = StringComparison.Ordinal)
    {
        Argument.ThrowIfNullOrEmpty(literal);

        var expected = literal.ToPrintable();
        return comparison == StringComparison.Ordinal
            ? new OrdinalStringParser<string>(literal) { Name = expected }
            : new StringParser<string>(literal, comparison) { Name = expected };
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
    public static Parser<string> OneOf(string[] literals, StringComparison comparison = StringComparison.Ordinal)
    {
        Argument.ThrowIfNullOrEmpty(literals);

        if (literals.Length == 1)
            return L(literals[0], comparison);

        var expected = literals.Select(l => l.ToPrintable()).ToArray();
        var comparer = StringComparer.FromComparison(comparison);

        var dictionary = new CharMap(
            Permute(literals, comparison)
                .GroupBy(l => l.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .Select(p => p.Value)
                        .OrderDescending(comparer)
                        .Distinct(comparer)
                        .ToArray()));

        return comparison switch
        {
            StringComparison.CurrentCulture => new StringDictionaryParser<string>(in dictionary, CultureInfo.CurrentCulture, CompareOptions.None, expected),
            StringComparison.CurrentCultureIgnoreCase => new StringDictionaryParser<string>(in dictionary, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase, expected),
            StringComparison.InvariantCulture => new StringDictionaryParser<string>(in dictionary, CultureInfo.InvariantCulture, CompareOptions.None, expected),
            StringComparison.InvariantCultureIgnoreCase => new StringDictionaryParser<string>(in dictionary, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase, expected),
            StringComparison.Ordinal => new OrdinalStringDictionaryParser<string, CompareOptionsNone>(in dictionary, expected),
            _ => new OrdinalStringDictionaryParser<string, CompareOptionsIgnoreCase>(in dictionary, expected)
        };
    }

    #region Inner type: OrdinalStringParser

    /// <summary>
    /// Represents a parser that attempts to match the specified literal using ordinal string comparison.
    /// This parser is specialized for performance with ordinal comparison.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    private sealed class OrdinalStringParser<T> : Parser<T>
    {
        private readonly string _literal;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrdinalStringParser{T}"/> class.
        /// </summary>
        /// <param name="literal">The string to match.</param>
        public OrdinalStringParser(string literal) =>
            _literal = literal;

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            if (context.Remaining.StartsWith(_literal))
            {
                if (typeof(T) != typeof(Unit))
                    value = ((T)(object)_literal)!;
                else
                    value = default!;

                context.Advance(_literal.Length);
                return true;
            }

            value = default;
            context.AddError(Name);

            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new OrdinalStringParser<T>(_literal) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new OrdinalStringParser<Unit>(_literal) { Name = Name };
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
        private readonly StringComparison _comparison;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringParser{T}"/> class.
        /// </summary>
        /// <param name="literal">The string to match.</param>
        /// <param name="comparison">The string comparison type to use for matching the literal.</param>
        public StringParser(string literal, StringComparison comparison) =>
            (_literal, _comparison) = (literal, comparison);

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            if (context.Remaining.StartsWith(_literal, _comparison))
            {
                if (typeof(T) != typeof(Unit))
                    value = ((T)(object)_literal)!;
                else
                    value = default!;

                context.Advance(_literal.Length);
                return true;
            }

            value = default;
            context.AddError(Name);
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new StringParser<T>(_literal, _comparison) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new StringParser<Unit>(_literal, _comparison) { Name = Name };
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
        public OrdinalStringDictionaryParser(in CharMap literals, string[] expected)
        {
            _literals = literals;
            _expected = expected;
        }

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var s = context.Remaining;

            if (s.Length != 0 && _literals[s[0]] is {} literals)
            {
                foreach (var literal in literals)
                {
                    _ = literal.Length;

                    bool result;
                    if (typeof(TCompareOptions) == typeof(CompareOptionsIgnoreCase))
                        result = s.StartsWith(literal, StringComparison.OrdinalIgnoreCase);
                    else
                        result = s.StartsWith(literal);

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
                    context.AddError(Name);
                    break;
                default:
                    context.AddErrors(_expected);
                    break;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new OrdinalStringDictionaryParser<T, TCompareOptions>(in _literals, _expected) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new OrdinalStringDictionaryParser<Unit, TCompareOptions>(in _literals, _expected) { Name = Name };
    }

    #endregion

    #region Inner type: StringDictionaryParser

    /// <summary>
    /// Represents a parser that attempts to match one of the specified literals based on specific cultural and comparison options.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    private sealed class StringDictionaryParser<T> : Parser<T>
    {
        private readonly CharMap _literals;
        private readonly CultureInfo _culture;
        private readonly CompareInfo _comparer;
        private readonly string[] _expected;
        private readonly CompareOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringDictionaryParser{T}"/> class.
        /// </summary>
        /// <param name="literals">A dictionary mapping initial characters to arrays of string literals expected at the beginning of the source.</param>
        /// <param name="culture">The culture to use for comparisons, which affects how strings are compared and ordered.</param>
        /// <param name="options">An optional combination of <see cref="CompareOptions"/> enumeration values to use during the match.</param>
        /// <param name="expected">An array of error messages describing expected literals, used when parsing fails.</param>
        public StringDictionaryParser(in CharMap literals, CultureInfo culture, CompareOptions options, string[] expected)
        {
            _literals = literals;
            _culture = culture;
            _comparer = culture.CompareInfo;
            _expected = expected;
            _options = options;
        }

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var s = context.Remaining;

            if (s.Length != 0 && _literals[s[0]] is {} literals)
            {
                foreach (var literal in literals)
                {
                    _ = literal.Length;

                    if (_comparer.IsPrefix(s, literal, _options))
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
                    context.AddError(Name);
                    break;
                default:
                    context.AddErrors(_expected);
                    break;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new StringDictionaryParser<T>(in _literals, _culture, _options, _expected) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new StringDictionaryParser<Unit>(in _literals, _culture, _options, _expected) { Name = Name };
    }

    #endregion

    private static IEnumerable<(char Key, string Value)> Permute(string[] literals, StringComparison comparison)
    {
        var list = literals.Select(s => (Key: s[0], Value: s)).ToArray();

        if (comparison is StringComparison.Ordinal
            or StringComparison.InvariantCulture
            or StringComparison.CurrentCulture)
            return list.Distinct();

        var ci = comparison == StringComparison.CurrentCultureIgnoreCase
            ? CultureInfo.CurrentCulture
            : CultureInfo.InvariantCulture;

        return list
            .Concat(list.Select(s => s with { Key = ci.TextInfo.ToUpper(s.Key) }))
            .Concat(list.Select(s => s with { Key = ci.TextInfo.ToLower(s.Key) }));
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
