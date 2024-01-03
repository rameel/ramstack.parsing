namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that attempts to parse the input with the specified parser
    /// or its alternative in sequence, returning the result of the first one that succeeds.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by each parser.</typeparam>
    /// <param name="parser">The main parser.</param>
    /// <param name="alternative">The alternative parser.</param>
    /// <returns>
    /// A <see cref="Parser{T}"/> that tries each parser sequentially and returns the result
    /// of the first parser that successfully parses the input.
    /// </returns>
    public static Parser<T> Or<T>(this Parser<T> parser, Parser<T> alternative) =>
        Choice(parser, alternative);
}
