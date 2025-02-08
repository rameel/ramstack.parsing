namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that forcibly terminates parsing with the specified error message.
    /// </summary>
    /// <typeparam name="T">The type of value produced by the parser.</typeparam>
    /// <param name="message">The error message to be thrown.</param>
    /// <returns>
    /// A parser that forcibly terminates parsing with the specified error message.
    /// </returns>
    public static Parser<T> Fail<T>(string message)
    {
        Argument.ThrowIfNullOrEmpty(message);
        return new FailParser<T> { Name = message };
    }

    /// <summary>
    /// Forcibly terminates parsing with the specified error message.
    /// </summary>
    /// <param name="message">The error message to be thrown.</param>
    [DoesNotReturn]
    public static void FatalError(string message) =>
        throw new FatalErrorException(message);

    #region Inner type: FailParser

    /// <summary>
    /// Represents a parser that always throws a <see cref="FatalErrorException"/>
    /// with the specified error message when invoked.
    /// </summary>
    /// <typeparam name="T">The type of value produced by the parser.</typeparam>
    private sealed class FailParser<T> : Parser<T>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value) =>
            throw new FatalErrorException(Name!);

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new FailParser<Unit> { Name = Name };
    }

    #endregion
}
