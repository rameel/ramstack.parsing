namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that repeatedly applies the specified parser zero or more times
    /// until the specified <paramref name="terminator"/> parser succeeds.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <typeparam name="TTerminator">The type of the value produced by the terminator parser (ignored).</typeparam>
    /// <param name="parser">The main parser to apply repeatedly.</param>
    /// <param name="terminator">The parser that determines when to stop.</param>
    /// <returns>
    /// A parser that applies <paramref name="parser"/> zero or more times, stopping when
    /// <paramref name="terminator"/> succeeds, and returns an array of the results produced by <paramref name="parser"/>.
    /// </returns>
    public static Parser<ArrayList<T>> Until<T, TTerminator>(this Parser<T> parser, Parser<TTerminator> terminator) =>
        new UntilParser<T>(parser, terminator.Void());

    /// <summary>
    /// Creates a parser that repeatedly applies the specified parser zero or more times
    /// until the specified <paramref name="terminator"/> parser succeeds.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <param name="parser">The main parser to apply repeatedly.</param>
    /// <param name="terminator">The parser that determines when to stop.</param>
    /// <returns>
    /// A parser that applies <paramref name="parser"/> zero or more times, stopping when
    /// <paramref name="terminator"/> succeeds, and returns an array of the results produced by <paramref name="parser"/>.
    /// </returns>
    public static Parser<ArrayList<T>> Until<T>(this Parser<T> parser, Parser<Unit> terminator) =>
        new UntilParser<T>(parser, terminator);

    #region Inner type: UntilParser

    /// <summary>
    /// Represents a parser that repeatedly applies the specified parser zero or more times
    /// until the specified <paramref name="terminator"/> parser succeeds.
    /// </summary>
    /// <param name="parser">The main parser to apply repeatedly.</param>
    /// <param name="terminator">The parser that determines when to stop.</param>
    private sealed class UntilParser<T>(Parser<T> parser, Parser<Unit> terminator) : Parser<ArrayList<T>>
    {
        private readonly Parser<Unit> _isTerminator = And(terminator);

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out ArrayList<T>? value)
        {
            var list = new ArrayList<T>();
            var bookmark = context.BookmarkPosition();

            while (true)
            {
                if (_isTerminator.TryParse(ref context, out _))
                {
                    context.SetMatched(bookmark);
                    value = list;
                    return true;
                }

                var position = context.Position;

                if (!parser.TryParse(ref context, out var result))
                    break;

                list.Add(result);

                //
                // Prevent infinite loop
                //
                if (context.Position == position)
                {
                    //
                    // Parsing failed in this case because:
                    // 1. The main parser matched, but the position remained unchanged.
                    // 2. The terminator didn't match before, and it won't match now.
                    // 3. Rechecking would yield the same result, making it redundant.
                    // 4. Without a terminator, parsing is considered unsuccessful, and it will never match now.
                    // 5. If a parser matches but the position remains unchanged, it results in an infinite loop.
                    //
                    break;
                }
            }

            value = null;
            context.RestorePosition(bookmark);
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new VoidUntilParser(parser.Void(), terminator);
    }

    #endregion

    #region Inner type: VoidUntilParser

    /// <summary>
    /// Represents a parser that repeatedly applies the specified parser zero or more times
    /// until the specified <paramref name="terminator"/> parser succeeds.
    /// </summary>
    /// <param name="parser">The main parser to apply repeatedly.</param>
    /// <param name="terminator">The parser that determines when to stop.</param>
    private sealed class VoidUntilParser(Parser<Unit> parser, Parser<Unit> terminator) : Parser<Unit>
    {
        private readonly Parser<Unit> _isTerminator = And(terminator);

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, out Unit value)
        {
            var bookmark = context.BookmarkPosition();

            while (true)
            {
                if (_isTerminator.TryParse(ref context, out value))
                {
                    context.SetMatched(bookmark);
                    return true;
                }

                var position = context.Position;

                if (!parser.TryParse(ref context, out value))
                    break;

                //
                // Prevent infinite loop
                //
                if (context.Position == position)
                {
                    //
                    // Parsing failed in this case because:
                    // 1. The main parser matched, but the position remained unchanged.
                    // 2. The terminator didn't match before, and it won't match now.
                    // 3. Rechecking would yield the same result, making it redundant.
                    // 4. Without a terminator, parsing is considered unsuccessful, and it will never match now.
                    // 5. If a parser matches but the position remains unchanged, it results in an infinite loop.
                    //
                    break;
                }
            }

            context.RestorePosition(bookmark);
            return false;
        }
    }

    #endregion
}
