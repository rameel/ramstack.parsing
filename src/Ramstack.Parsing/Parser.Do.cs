namespace Ramstack.Parsing;

public static partial class Parser
{
    /// <summary>
    /// Creates a parser that applies a transformation function to the value produced by the specified parser.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the initial parser.</typeparam>
    /// <typeparam name="TResult">The type of the value produced after applying the transformation function.</typeparam>
    /// <param name="parser">The parser whose output will be transformed.</param>
    /// <param name="func">The function used to transform the parser's output.</param>
    /// <returns>
    /// A parser that applies a transformation function to the value produced by the specified parser.
    /// </returns>
    public static Parser<TResult> Do<T, TResult>(this Parser<T> parser, Func<T, TResult> func) =>
        new DoParser<T, TResult>(parser, func);

    #region Inner type: DoParser<T, TResult>

    /// <summary>
    /// Represents a parser that applies a transformation function to the value produced by the specified parser.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <typeparam name="TResult">The type of the value produced after applying the transformation function.</typeparam>
    /// <param name="parser">The parser whose output will be transformed.</param>
    /// <param name="func">The function used to transform the parser's output.</param>
    private sealed class DoParser<T, TResult>(Parser<T> parser, Func<T, TResult> func) : Parser<TResult>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out TResult? value)
        {
            if (parser.TryParse(ref context, out var v))
            {
                value = func(v)!;
                return true;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<TResult> ToNamedParser(string? name) =>
            new DoParser<T, TResult>(parser.ToNamedParser(name), func) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            parser.Void();
    }

    #endregion
}
