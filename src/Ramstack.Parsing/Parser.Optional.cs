namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates an optional parser that always succeeds,
    /// regardless of whether the specified parser succeeds or fails.
    /// </summary>
    /// <typeparam name="T">The type of value produced by the initial parser.</typeparam>
    /// <param name="parser">The parser to be treated as optional.</param>
    /// <returns>
    /// A parser representing an optional version of the specified parser.
    /// </returns>
    public static Parser<OptionalValue<T>> Optional<T>(this Parser<T> parser) =>
        new OptionalParser<T>(parser);

    /// <summary>
    /// Returns the input parser as-is when it is already an optional parser.
    /// </summary>
    /// <typeparam name="T">The type of value produced by the initial parser.</typeparam>
    /// <param name="parser">The optional parser that will be returned unchanged.</param>
    /// <returns>
    /// The same parser that was passed in as an argument.
    /// </returns>
    public static Parser<OptionalValue<T>> Optional<T>(this Parser<OptionalValue<T>> parser) =>
        parser;

    #region Inner type: OptionalParser

    /// <summary>
    /// Represents a parser that always succeeds, regardless of whether the specified parser succeeds or fails.
    /// </summary>
    /// <typeparam name="T">The type of value produced by the initial parser.</typeparam>
    /// <param name="parser">The parser to be treated as optional.</param>
    private sealed class OptionalParser<T>(Parser<T> parser) : Parser<OptionalValue<T>>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, out OptionalValue<T> value)
        {
            value = default;

            if (parser.TryParse(ref context, out var result))
            {
                Unsafe.AsRef(in value.Value) = result;
                Unsafe.AsRef(in value.HasValue) = true;
            }
            else
            {
                context.Advance(0);
            }

            return true;
        }

        /// <inheritdoc />
        protected internal override Parser<OptionalValue<T>> ToNamedParser(string? name) =>
            new OptionalParser<T>(parser.ToNamedParser(name)) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new DefaultOnFailParser<Unit>(parser.Void(), Unit.Value) { Name = Name };
    }

    #endregion
}
