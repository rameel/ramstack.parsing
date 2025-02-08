namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that returns the string representation of the matched segment (ignoring the parsed value).
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the initial parser.</typeparam>
    /// <param name="parser">The parser instance used to match the input.</param>
    /// <returns>
    /// A parser that returns the string representation of the matched segment.
    /// </returns>
    public static Parser<string> Text<T>(this Parser<T> parser) =>
        parser.Map(m => m.ToString());
}
