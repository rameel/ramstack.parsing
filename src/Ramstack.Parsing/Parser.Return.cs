namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that does not consume any input and returns the <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <param name="value">The value to return.</param>
    /// <returns>
    /// A parser that does not consume any input and returns the <paramref name="value"/>.
    /// </returns>
    public static Parser<T> Return<T>(T value) =>
        new ReturnParser<T>(value);

    #region Inner type: ReturnParser

    /// <summary>
    /// Creates a parser that does not consume any input and returns the <paramref name="returnValue"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <param name="returnValue">The value to return.</param>
    private sealed class ReturnParser<T>(T returnValue) : Parser<T>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            value = returnValue!;
            return true;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new ReturnParser<T>(returnValue) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new ReturnParser<Unit>(Unit.Value) { Name = Name };
    }

    #endregion
}
