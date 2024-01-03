namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Converts a specified parser into a void parser, which matches the input without storing the result.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <param name="parser">The parser to convert.</param>
    /// <returns>
    /// A parser instance that performs the same parsing as the original parser, but does not capture the result.
    /// </returns>
    public static Parser<Unit> Void<T>(this Parser<T> parser) =>
        parser as Parser<Unit> ?? parser.ToVoidParser();

    #region Inner type: VoidParser

    /// <summary>
    /// Represents a parser that wraps a specified parser that cannot provide a void parser for itself.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the underlying parser.</typeparam>
    /// <param name="parser">The parser to wrap.</param>
    internal sealed class VoidParser<T>(Parser<T> parser) : Parser<Unit>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, out Unit value)
        {
            Unsafe.SkipInit(out value);
            return parser.TryParse(ref context, out _);
        }

        /// <inheritdoc />
        protected internal override Parser<Unit> ToNamedParser(string? name) =>
            new VoidParser<T>(parser.ToNamedParser(name)) { Name = name };
    }

    #endregion
}
