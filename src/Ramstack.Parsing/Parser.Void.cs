namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Converts a specified parser into a void parser, which matches the input without storing the result.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <param name="parser">The parser to convert.</param>
    /// <returns>
    /// A parser that matches the input while discarding the parsed value.
    /// </returns>
    /// <remarks>
    /// Transformations whose results are discarded may be skipped, including callbacks passed to
    /// <c>Map</c>, <c>Do</c>, <c>FoldL</c>, <c>FoldR</c>, and <c>Prefixed</c>.
    /// Side effects and exceptions in skipped callbacks do not occur.
    /// This also applies when a combinator discards a parser's result internally.
    /// </remarks>
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
