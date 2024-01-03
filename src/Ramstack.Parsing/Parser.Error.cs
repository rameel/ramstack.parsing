namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that always fails with the specified error message.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <param name="message">The error message that this parser will produce upon failure.</param>
    /// <returns>
    /// A parser that always fails with the specified error message.
    /// </returns>
    public static Parser<T> Error<T>(string message) =>
        new ErrorParser<T> { Name = message };

    #region Inner type: ErrorParser

    /// <summary>
    /// Represents a parser that always fails with the specified error message.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    private sealed class ErrorParser<T> : Parser<T>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            value = default;
            context.AddError(Name);
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new ErrorParser<T> { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new ErrorParser<Unit> { Name = Name };
    }

    #endregion
}
