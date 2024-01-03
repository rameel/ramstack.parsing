namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that applies the specified parser both before and after the main parser,
    /// returning the result of the main parser.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the main parser.</typeparam>
    /// <typeparam name="TAround">The type of the value produced by the parser to be applied around the main parser (ignored in the final result).</typeparam>
    /// <param name="parser">The main parser whose result will be returned.</param>
    /// <param name="around">The parser to apply before and after the main parser.</param>
    /// <returns>
    /// A new <see cref="Parser{T}"/> that applies <paramref name="around"/> before and after <paramref name="parser"/>,
    /// returning the result of the main parser.
    /// </returns>
    public static Parser<T> Between<T, TAround>(this Parser<T> parser, Parser<TAround> around) =>
        new BetweenParser<T>(parser, around.Void());

    /// <summary>
    /// Creates a parser that applies the specified parsers before and after the main parser,
    /// returning the result of the main parser.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the main parser.</typeparam>
    /// <typeparam name="TBefore">The type of the parser applied before the main parser (ignored in the final result).</typeparam>
    /// <typeparam name="TAfter">The type of the parser applied after the main parser (ignored in the final result).</typeparam>
    /// <param name="parser">The main parser whose result will be returned.</param>
    /// <param name="before">The parser to apply before the main parser.</param>
    /// <param name="after">The parser to apply after the main parser.</param>
    /// <returns>
    /// A new <see cref="Parser{T}"/> that applies <paramref name="before"/> and then <paramref name="parser"/>,
    /// followed by <paramref name="after"/>, returning the result of the main parser.
    /// </returns>
    public static Parser<T> Between<T, TBefore, TAfter>(this Parser<T> parser, Parser<TBefore> before, Parser<TAfter> after) =>
        new BetweenParser<T>(parser, before.Void(), after.Void());

    #region Inner type: BetweenParser

    /// <summary>
    /// Represents a parser that applies the specified parsers before and after the main parser,
    /// returning the result of the main parser.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the main parser.</typeparam>
    /// <param name="parser">The main parser whose result will be returned.</param>
    /// <param name="before">The parser to apply before the main parser.</param>
    /// <param name="after">The parser to apply after the main parser.</param>
    private sealed class BetweenParser<T>(Parser<T> parser, Parser<Unit> before, Parser<Unit> after) : Parser<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BetweenParser{T}"/> class,
        /// applying the same parser both before and after the main parser.
        /// </summary>
        /// <param name="parser">The main parser whose result will be returned.</param>
        /// <param name="around">The parser to apply before and after the main parser.</param>
        public BetweenParser(Parser<T> parser, Parser<Unit> around) : this(parser, around, around)
        {
        }

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var bookmark = context.BookmarkPosition();

            if (before.TryParse(ref context, out _) && parser.TryParse(ref context, out value))
            {
                var (index, length) = context.MatchedSegment;

                if (after.TryParse(ref context, out _))
                {
                    context.SetMatched(index, length);
                    return true;
                }
            }

            context.RestorePosition(bookmark);
            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser()
        {
            return ReferenceEquals(before, after)
                ? new BetweenParser<Unit>(parser.Void(), before.Void())
                : new BetweenParser<Unit>(parser.Void(), before.Void(), after.Void());
        }
    }

    #endregion
}
