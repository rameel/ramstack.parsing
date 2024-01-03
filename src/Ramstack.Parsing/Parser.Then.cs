namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that sequentially applies the current parser and a specified second parser,
    /// returning the result of the second parser with ignoring the result of the first.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the first parser, which is ignored.</typeparam>
    /// <typeparam name="TResult">The type of the value produced by the second parser.</typeparam>
    /// <param name="parser">The initial parser.</param>
    /// <param name="then">The subsequent parser whose result is returned.</param>
    /// <returns>
    /// A parser that sequentially applies the current parser and a specified second parser,
    /// returning the result of the second parser with ignoring the result of the first.
    /// </returns>
    public static Parser<TResult> Then<T, TResult>(this Parser<T> parser, Parser<TResult> then) =>
        new ThenParser<TResult>(parser.Void(), then);

    #region Inner type: ThenParser

    /// <summary>
    /// Represents a parser that sequentially applies an initial parser and a specified second parser,
    /// returning the result of the second parser with ignoring the result of the first.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the second parser.</typeparam>
    /// <param name="parser">The initial parser.</param>
    /// <param name="then">The subsequent parser whose result is returned.</param>
    private sealed class ThenParser<T>(Parser<Unit> parser, Parser<T> then) : Parser<T>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var position = context.BookmarkPosition();

            if (parser.TryParse(ref context, out _) && then.TryParse(ref context, out value))
                return true;

            value = default;
            context.RestorePosition(position);

            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new ThenParser<Unit>(parser, then.Void());
    }

    #endregion
}
