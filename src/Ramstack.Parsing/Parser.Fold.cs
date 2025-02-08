namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a left-associative parser that applies the <paramref name="reduce"/> function
    /// to reduce values from the parsed items.
    /// </summary>
    /// <remarks>
    /// <code>
    /// // Number ([+-] Number)*
    /// var sum = number.Fold(
    ///     Seq(OneOf("+-"), number),
    ///     (r, op, d) => op == '+' ? r + n : r - n);
    /// </code>
    /// </remarks>
    /// <typeparam name="T">The type of value produces by the initial parser.</typeparam>
    /// <typeparam name="TItem">The type of the values being accumulated.</typeparam>
    /// <param name="parser">The initial parser.</param>
    /// <param name="item">The parser for values to accumulate.</param>
    /// <param name="reduce">A reduction function.</param>
    /// <returns>
    /// A parser that performs left-associative folding over parsed values.
    /// </returns>
    public static Parser<T> Fold<T, TItem>(this Parser<T> parser, Parser<TItem> item, Func<T, TItem, T> reduce) =>
        new FoldParser<T, TItem>(parser, item, reduce);

    /// <summary>
    /// Creates a right-associative parser that applies the <paramref name="reduce"/> function
    /// to reduce values from the parsed items.
    /// </summary>
    /// <remarks>
    /// <code>
    /// // Number ("^" Number)*
    /// var power = number.FoldR(
    ///     Seq(L('^'), number),
    ///     (r, _, d) => Math.Pow(r, d));
    /// </code>
    /// </remarks>
    /// <typeparam name="T">The type of value produces by the initial parser.</typeparam>
    /// <typeparam name="TItem">The type of the values being accumulated.</typeparam>
    /// <param name="parser">The initial parser.</param>
    /// <param name="item">The parser for values to accumulate.</param>
    /// <param name="reduce">A reduction function.</param>
    /// <returns>
    /// A parser that performs right-associative folding over parsed values.
    /// </returns>
    public static Parser<T> FoldR<T, TItem>(this Parser<T> parser, Parser<TItem> item, Func<T, TItem, T> reduce) =>
        new FoldRParser<T, TItem>(parser, item, reduce);

    #region Inner type: FoldParser

    /// <summary>
    /// Represents a left-associative parser that accumulates parsed items using a specified reduction function.
    /// </summary>
    /// <typeparam name="T">The type of value produces by the initial parser.</typeparam>
    /// <typeparam name="TItem">The type of the values being accumulated.</typeparam>
    /// <param name="parser">The initial parser.</param>
    /// <param name="item">The parser for values to accumulate.</param>
    /// <param name="reduce">A reduction function.</param>
    private sealed class FoldParser<T, TItem>(Parser<T> parser, Parser<TItem> item, Func<T, TItem, T> reduce) : Parser<T>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var bookmark = context.BookmarkPosition();

            if (parser.TryParse(ref context, out var result))
            {
                while (item.TryParse(ref context, out var r))
                    result = reduce(result, r);

                context.SetMatched(bookmark);
                value = result!;

                return true;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            Seq(parser.Void(), item.Void().ZeroOrMore());
    }

    #endregion

    #region Inner type: FoldRParser

    /// <summary>
    /// Represents a right-associative parser that accumulates parsed items using a specified reduction function.
    /// </summary>
    /// <typeparam name="T">The type of value produces by the initial parser.</typeparam>
    /// <typeparam name="TItem">The type of the values being accumulated.</typeparam>
    /// <param name="parser">The initial parser.</param>
    /// <param name="item">The parser for values to accumulate.</param>
    /// <param name="reduce">A reduction function.</param>
    private sealed class FoldRParser<T, TItem>(Parser<T> parser, Parser<TItem> item, Func<T, TItem, T> reduce) : Parser<T>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var bookmark = context.BookmarkPosition();

            if (parser.TryParse(ref context, out var result))
            {
                var values = new ArrayList<TItem>();
                while (item.TryParse(ref context, out var v))
                    values.Add(v);

                for (var i = values.Count - 1; i >= 0; i--)
                    result = reduce(result, values[i]);

                context.SetMatched(bookmark);
                value = result!;

                return true;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            Seq(parser.Void(), item.Void().ZeroOrMore());
    }

    #endregion
}
