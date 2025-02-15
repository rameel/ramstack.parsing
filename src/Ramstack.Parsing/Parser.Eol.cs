namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Gets a parser that matches the end of line.
    /// </summary>
    public static Parser<Unit> Eol { get; } = new EolParser();

    #region Inner type: EolParser

    /// <summary>
    /// Represents a parser that matches the end of line.
    /// </summary>
    private sealed class EolParser() : Parser<Unit>("end of line")
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, out Unit value)
        {
            Unsafe.SkipInit(out value);

            var s = context.Source;
            var p = context.Position;
            var count = 0;

            if ((uint)p < (uint)s.Length)
            {
                switch (s[p])
                {
                    case '\r' when (uint)p + 1 < (uint)s.Length && s[p + 1] == '\n':
                        count = 2;
                        break;

                    case '\n':
                    case '\u0085':
                    case '\u2028':
                    case '\u2029':
                    case '\r':
                        count = 1;
                        break;

                    default:
                        context.ReportExpected(Name);
                        return false;
                }
            }

            context.Advance(count);
            return true;
        }

        /// <inheritdoc />
        protected internal override Parser<Unit> ToNamedParser(string? name) =>
            new EolParser { Name = name };
    }

    #endregion
}
