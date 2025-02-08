namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates an optional parser that always succeeds, returning the parsed value if successful,
    /// or a default value of the <typeparamref name="T"/> if the specified parser fails.
    /// </summary>
    /// <typeparam name="T">The type of value produced by the initial parser.</typeparam>
    /// <param name="parser">The parser to be treated as optional.
    /// If this parser fails to parse, the method will return the default value of the type.</param>
    /// <returns>
    /// A parser that always succeeds, either by producing a value from the original parser
    /// or by returning the default value of the <typeparamref name="T"/>.
    /// </returns>
    public static Parser<T?> DefaultOnFail<T>(this Parser<T> parser) =>
        parser.DefaultOnFail(default!)!;

    /// <summary>
    /// Creates an optional parser that always succeeds, returning the parsed value if successful,
    /// or a default value if the specified parser fails.
    /// </summary>
    /// <typeparam name="T">The type of value produced by the initial parser.</typeparam>
    /// <param name="parser">The parser to be treated as optional.
    /// If this parser fails to parse, the method will return the default value of the type.</param>
    /// <param name="defaultValue">The default value to return if the specified parser fails to parse.</param>
    /// <returns>
    /// A parser that always succeeds, either by producing a value from the initial parser
    /// or by returning the specified default value.
    /// </returns>
    public static Parser<T> DefaultOnFail<T>(this Parser<T> parser, T defaultValue)
    {
        if (parser is DefaultOnFailParser<T> p)
            parser = p.Parser;

        return new DefaultOnFailParser<T>(parser, defaultValue);
    }

    #region Inner type: DefaultOnFailParser

    /// <summary>
    /// Represents a parser that always succeeds, returning the parsed value if successful,
    /// or a default value if the specified parser fails.
    /// </summary>
    /// <typeparam name="T">The type of value produced by the initial parser.</typeparam>
    /// <param name="parser">The parser to be treated as optional.
    /// If this parser fails to parse, the method will return the default value of the type.</param>
    /// <param name="defaultValue">The default value to return if the initial parser fails to parse.</param>
    private sealed class DefaultOnFailParser<T>(Parser<T> parser, T defaultValue) : Parser<T>
    {
        /// <summary>
        /// Gets the initial parser.
        /// </summary>
        public Parser<T> Parser => parser;

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            if (!parser.TryParse(ref context, out value))
            {
                value = defaultValue!;
                context.Advance(0);
            }

            return true;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new DefaultOnFailParser<T>(parser.ToNamedParser(name), defaultValue) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new DefaultOnFailParser<Unit>(parser.Void(), Unit.Value) { Name = Name };
    }

    #endregion
}
