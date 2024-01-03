namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Gets a parser that matches the end of input.
    /// </summary>
    public static Parser<Unit> Eof { get; } = new EofParser();

    #region Inner type: EofParser

    /// <summary>
    /// Represents a parser that matches the end of input.
    /// </summary>
    private sealed class EofParser() : Parser<Unit>("end of input")
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, out Unit value)
        {
            Unsafe.SkipInit(out value);

            if (context.Position >= context.Source.Length)
            {
                context.Advance(0);
                return true;
            }

            context.AddError(Name);
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<Unit> ToNamedParser(string? name) =>
            new EofParser { Name = name };
    }

    #endregion
}
