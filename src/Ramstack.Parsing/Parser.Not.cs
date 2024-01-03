namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a predicate parser that succeeds only if the specified parser fails.
    /// </summary>
    /// <remarks>
    /// The predicate parser does not advance the parsing position.
    /// </remarks>
    /// <param name="parser">The parser that is expected to fail.</param>
    /// <returns>
    /// A predicate parser that succeeds if the specified parser fails.
    /// </returns>
    public static Parser<Unit> Not<T>(Parser<T> parser) =>
        new AssertParser<NotAssert>(parser.Void());
}
