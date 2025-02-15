namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that always fails with a report of a missing expected sequence or rule.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <param name="expected">The expected sequence or rule.</param>
    /// <returns>
    /// A parser that always fails with a report of a missing expected sequence or rule.
    /// </returns>
    public static Parser<T> Error<T>(string expected) =>
        new ErrorParser<T> { Name = expected };

    #region Inner type: ErrorParser

    /// <summary>
    /// Represents a parser that always fails with a report of a missing expected sequence or rule.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    private sealed class ErrorParser<T> : Parser<T>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            value = default;
            context.ReportExpected(Name);
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
