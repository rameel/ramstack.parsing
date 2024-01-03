namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Gets a parser that matches zero or more whitespace characters.
    /// </summary>
    /// <remarks>
    /// <c>Character.WhiteSpace.ZeroOrMore().Void()</c>
    /// </remarks>
    public static Parser<Unit> S { get; } = new SParser();

    #region Inner type: SParser

    /// <summary>
    /// Represents a parser that matches zero or more whitespace characters.
    /// </summary>
    /// <remarks>
    /// This parser is functionally equivalent to <c>Character.WhiteSpace.ZeroOrMore().Void()</c>,
    /// but it is only 1-3% faster. The only reason for its existence is that,
    /// unlike the general implementation, it does not allocate memory for internal data,
    /// saving about 36 bytes.
    /// </remarks>
    private sealed class SParser : Parser<Unit>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, out Unit value)
        {
            Unsafe.SkipInit(out value);

            var s = context.Remaining;

            for (var i = 0; ; i++)
            {
                if ((uint)i < (uint)s.Length && char.IsWhiteSpace(s[i]))
                    continue;

                context.Advance(i);
                return true;
            }
        }

        /// <inheritdoc />
        protected internal override Parser<Unit> ToNamedParser(string? name) =>
            new SParser { Name = name };
    }

    #endregion
}
