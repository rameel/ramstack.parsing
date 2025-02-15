namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that matches a single instance of the specified character.
    /// </summary>
    /// <param name="ch">The character to match.</param>
    /// <returns>
    /// A <see cref="Parser{T}"/> that matches the specified character.
    /// </returns>
    public static Parser<char> L(char ch) =>
        new CharParser<char>(ch) { Name = ch.ToPrintable() };

    /// <summary>
    /// Creates a parser that matches any character in the specified string.
    /// </summary>
    /// <param name="chars">A string containing one or more characters to match.</param>
    /// <returns>
    /// A <see cref="Parser{T}"/> that matches any character in <paramref name="chars"/>.
    /// </returns>
    public static Parser<char> OneOf(string chars)
    {
        Argument.ThrowIfNullOrEmpty(chars);

        if (chars.Length == 1)
            return L(chars[0]);

        var @class = new CharClass(
            CharClassRange.Create(chars)
            );
        return Set(@class);
    }

    #region Inner type: CharParser

    /// <summary>
    /// Represents a parser that matches a single character.
    /// </summary>
    private sealed class CharParser<T> : Parser<T>, ICharClassSupport where T : struct
    {
        private readonly int _c;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharParser{T}"/> class that matches the specified character.
        /// </summary>
        /// <param name="c">The character to match.</param>
        public CharParser(char c) =>
            _c = c;

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, out T value)
        {
            var s = context.Source;
            var p = context.Position;

            if ((uint)p < (uint)s.Length && s[p] == _c)
            {
                if (typeof(T) == typeof(char))
                    value = (T)(object)s[p];
                else
                    value = default;

                context.Advance(1);
                return true;
            }

            value = default;
            context.ReportExpected(Name);

            return false;
        }

        /// <inheritdoc />
        public CharClass GetCharClass() =>
            new CharClass([CharClassRange.Create((char)_c)]);

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new CharParser<T>((char)_c) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new CharParser<Unit>((char)_c) { Name = Name };
    }

    #endregion
}
