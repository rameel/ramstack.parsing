namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that accepts a parsed value only when it satisfies the specified predicate.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <param name="parser">The parser that produces the value to check.</param>
    /// <param name="predicate">The condition that the parsed value must satisfy.</param>
    /// <param name="expected">A non-empty description of the expected value for diagnostics.</param>
    /// <returns>
    /// A parser that returns the original value on success, or an ordinary parse failure otherwise.
    /// </returns>
    /// <remarks>
    ///   <para>
    ///     Prefer a side-effect-free predicate: restoring input does not undo changes to external state.
    ///   </para>
    ///   <para>
    ///     Exceptions are not converted into predicate rejections.
    ///   </para>
    /// </remarks>
    public static Parser<T> Where<T>(this Parser<T> parser, Func<T, bool> predicate, string expected)
    {
        Argument.ThrowIfNull(parser);
        Argument.ThrowIfNull(predicate);
        Argument.ThrowIfNullOrEmpty(expected);

        return new WhereParser<T>(parser, predicate, expected);
    }

    /// <summary>
    /// Represents a parser that checks a condition on the parsed value.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <param name="parser">The parser that produces the value to check.</param>
    /// <param name="predicate">The condition that the parsed value must satisfy.</param>
    /// <param name="expected">A non-empty description of the expected value for diagnostics.</param>
    private sealed class WhereParser<T>(Parser<T> parser, Func<T, bool> predicate, string expected) : Parser<T>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var bookmark = context.BookmarkPosition();

            if (!parser.TryParse(ref context, out value))
                return false;

            if (predicate(value))
                return true;

            context.ReportExpected(
                bookmark.Position, expected);

            context.RestorePosition(bookmark);
            value = default;
            return false;
        }

        //
        // NOTE: The default void wrapper preserves the value needed by the predicate.
        //
    }
}
