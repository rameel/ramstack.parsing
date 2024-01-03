namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Gets a parser that matches any single character.
    /// </summary>
    /// <remarks>
    /// This parser succeeds if any single character is available in the source text.
    /// </remarks>
    public static Parser<char> Any { get; } = new AnyParser<char>();

    #region Inner type: AnyParser

    /// <summary>
    /// Represents a parser that matches any single character.
    /// </summary>
    private sealed class AnyParser<T>() : Parser<T>("any character"), ICharClassSupport where T : struct
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, out T value)
        {
            var s = context.Source;
            var p = context.Position;

            if ((uint)p < (uint)s.Length)
            {
                if (typeof(T) == typeof(char))
                    value = (T)(object)s[p];
                else
                    value = default;

                context.Advance(1);
                return true;
            }

            context.AddError(Name);

            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new AnyParser<T> { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new AnyParser<Unit> { Name = Name };

        /// <inheritdoc />
        public CharClass GetCharClass() =>
            new CharClass([CharClassRange.Create('\0', '\xFFFF')]);
    }

    #endregion
}
