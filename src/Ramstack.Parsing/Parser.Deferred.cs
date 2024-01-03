namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that can be defined later.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <returns>
    /// A parser that can be defined later.
    /// </returns>
    public static DeferredParser<T> Deferred<T>() =>
        new DeferredParser<T>();

    /// <summary>
    /// Creates a parser that references itself to support recursive definitions.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <param name="parser">A function that accepts a reference to the deferred parser and returns the resulting parser.</param>
    /// <returns>
    /// A parser that can be recursively defined.
    /// </returns>
    public static Parser<T> Recursive<T>(Func<DeferredParser<T>, Parser<T>> parser) =>
        new DeferredParser<T>(parser);
}
