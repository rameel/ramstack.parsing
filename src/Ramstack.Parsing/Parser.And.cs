namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a predicate parser that succeeds if the specified parser succeeds.
    /// </summary>
    /// <remarks>
    /// The predicate parser does not advance the parsing position.
    /// </remarks>
    /// <param name="parser">The parser that is expected to succeed.</param>
    /// <returns>
    /// A predicate parser that succeeds if the specified parser succeeds.
    /// </returns>
    public static Parser<Unit> And<T>(Parser<T> parser) =>
        new AssertParser<AndAssert>(parser.Void());

    #region Inner type: AssertParser

    /// <summary>
    /// Represents a predicate parser that evaluates based on the specified assertion type.
    /// </summary>
    /// <typeparam name="TAssert">
    /// The assertion type used to determine the result of the parse operation.
    /// Expected to be either <see cref="AndAssert"/> or <see cref="NotAssert"/>.
    /// </typeparam>
    /// <param name="parser">The underlying parser to evaluate.</param>
    private sealed class AssertParser<TAssert>(Parser<Unit> parser) : Parser<Unit>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, out Unit value)
        {
            var state = context.SuppressDiagnostics();
            var bookmark = context.BookmarkPosition();

            var result = parser.TryParse(ref context, out value);

            context.RestoreDiagnosticState(state);
            context.RestorePosition(bookmark);

            if (typeof(TAssert) == typeof(AndAssert))
                return result;

            return !result;
        }
    }

    /// <summary>
    /// A marker struct used to indicate that the parser should succeed only if the underlying parser succeeds.
    /// </summary>
    private struct AndAssert;

    /// <summary>
    /// A marker struct used to indicate that the parser should succeed only if the underlying parser fails.
    /// </summary>
    private struct NotAssert;

    #endregion
}
