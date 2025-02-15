namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that sequentially applies the current parser and a specified second parser,
    /// returning the result of the first parser with ignoring the result of the second.
    /// </summary>
    /// <typeparam name="TResult">The type of the value produced by the first parser.</typeparam>
    /// <typeparam name="T">The type of the value produced by the second parser, which is ignored.</typeparam>
    /// <param name="parser">The initial parser whose result is returned.</param>
    /// <param name="ignore">The subsequent parser, applied after the initial parser.</param>
    /// <returns>
    /// A parser that sequentially applies the current parser and a specified second parser,
    /// returning the result of the first parser with ignoring the result of the second.
    /// </returns>
    public static Parser<TResult> ThenIgnore<TResult, T>(this Parser<TResult> parser, Parser<T> ignore) =>
        new ThenIgnoreParser<TResult>(parser, ignore.Void());

    #region Inner type: ThenIgnoreParser

    /// <summary>
    /// Represents a parser that sequentially applies an initial parser and a specified second parser,
    /// returning the result of the first parser with ignoring the result of the second.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the first parser.</typeparam>
    /// <param name="parser">The initial parser whose result is returned.</param>
    /// <param name="ignore">The subsequent parser, applied after the initial parser.</param>
    private sealed class ThenIgnoreParser<T>(Parser<T> parser, Parser<Unit> ignore) : Parser<T>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var bookmark = context.BookmarkPosition();

            if (parser.TryParse(ref context, out value))
            {
                var (index, length) = context.MatchedSegment;

                if (ignore.TryParse(ref context, out _))
                {
                    context.SetMatched(index, length);
                    return true;
                }
            }

            context.RestorePosition(bookmark);
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new ThenIgnoreParser<Unit>(parser.Void(), ignore);
    }

    #endregion
}
