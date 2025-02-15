namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that repeatedly applies the main parser, interleaved with a separator specified by another parser.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the main parser.</typeparam>
    /// <typeparam name="TSeparator">The type of the value produced by the separator parser.</typeparam>
    /// <param name="parser">The main parser.</param>
    /// <param name="separator">The parser that identifies the separators placed between the elements parsed by the main parser.</param>
    /// <param name="allowTrailing"><see langword="true" /> if trailing separator is allowed; otherwise, <see langword="false" />.</param>
    /// <param name="min">The minimum number of repetitions.</param>
    /// <param name="max">The maximum number of repetitions.</param>
    /// /// <returns>
    /// A parser that repeatedly applies the main parser, interleaved with the specified separator.
    /// </returns>
    public static Parser<ArrayList<T>> Separated<T, TSeparator>(this Parser<T> parser, Parser<TSeparator> separator, bool allowTrailing = false, int min = 0, int max = int.MaxValue) =>
        new SeparatedParser<T>(parser, separator.Void(), allowTrailing, min, max);

    #region Inner type: SeparatedParser

    /// <summary>
    /// Represents a parser that repeatedly applies the main parser, interleaved with a separator specified by another parser.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the main parser.</typeparam>
    /// <param name="parser">The main parser.</param>
    /// <param name="separator">The parser that identifies the separators placed between the elements parsed by the main parser.</param>
    /// <param name="allowTrailing"><see langword="true" /> if trailing separator is allowed; otherwise, <see langword="false" />.</param>
    /// <param name="min">The minimum number of repetitions.</param>
    /// <param name="max">The maximum number of repetitions.</param>
    private sealed class SeparatedParser<T>(Parser<T> parser, Parser<Unit> separator, bool allowTrailing, int min, int max) : Parser<ArrayList<T>>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out ArrayList<T>? value)
        {
            var list = new ArrayList<T>();
            var bookmark = context.BookmarkPosition();
            var separatorBookmark = bookmark;

            do
            {
                if (!parser.TryParse(ref context, out var result))
                    break;

                list.Add(result);

                separatorBookmark = context.BookmarkPosition();
                if (!separator.TryParse(ref context, out _))
                    break;
            }
            while (list.Count < max);

            if (list.Count >= min)
            {
                if (!allowTrailing)
                    context.RestorePosition(separatorBookmark);

                context.SetMatched(bookmark);
                value = list;
                return true;
            }

            value = null;
            context.RestorePosition(bookmark);
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new VoidSeparatedParser(parser.Void(), separator, allowTrailing, min, max);
    }

    #endregion

    #region Inner type: VoidSeparatorParser

    /// <summary>
    /// Represents a specialized parser used as an optimization that discards the parsed result,
    /// that repeatedly applies the main parser, interleaved with a separator specified by another parser.
    /// </summary>
    private sealed class VoidSeparatedParser : Parser<Unit>
    {
        private readonly Parser<Unit> _parser;
        private readonly Parser<Unit> _separator;
        private readonly bool _allowTrailing;
        private readonly int _min;
        private readonly int _max;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoidSeparatedParser"/> class.
        /// </summary>
        /// <param name="parser">The main parser.</param>
        /// <param name="separator">The parser that identifies the separators placed between the elements parsed by the main parser.</param>
        /// <param name="allowTrailing"><see langword="true" /> if trailing separator is allowed; otherwise, <see langword="false" />.</param>
        /// <param name="min">The minimum number of repetitions.</param>
        /// <param name="max">The maximum number of repetitions.</param>
        public VoidSeparatedParser(Parser<Unit> parser, Parser<Unit> separator, bool allowTrailing, int min, int max)
        {
            _parser = parser;
            _separator = separator;
            _allowTrailing = allowTrailing;
            _min = min;
            _max = max;
        }

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, out Unit value)
        {
            var count = 0;
            var bookmark = context.BookmarkPosition();
            var separatorBookmark = bookmark;

            do
            {
                if (!_parser.TryParse(ref context, out value))
                    break;

                count++;

                separatorBookmark = context.BookmarkPosition();
                if (!_separator.TryParse(ref context, out value))
                    break;
            }
            while (count < _max);

            if (count >= _min)
            {
                if (!_allowTrailing)
                    context.RestorePosition(separatorBookmark);

                context.SetMatched(bookmark);
                return true;
            }

            context.RestorePosition(bookmark);
            return false;
        }
    }

    #endregion
}
