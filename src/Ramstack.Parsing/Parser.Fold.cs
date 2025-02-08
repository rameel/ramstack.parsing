namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a left-associative parser.
    /// </summary>
    /// <remarks>
    /// <code>
    /// // Example: Number ([+-] Number)*
    /// // 1 + 2 + 3 + 4 => (((1 + 2) + 3) + 4)
    /// var sum = number.Fold(OneOf("+-"), (l, r, op) => op == '+' ? l + r : l - r);
    /// </code>
    /// </remarks>
    /// <typeparam name="T">The type of the value produced by the main parser.</typeparam>
    /// <typeparam name="TOperator">The type of the operator token produced by the parser.</typeparam>
    /// <param name="parser">The main parser that matches a value.</param>
    /// <param name="op">The parser that matches an operator token.</param>
    /// <param name="reduce">A reduction function.</param>
    /// <returns>
    /// A parser that performs left-associative folding.
    /// </returns>
    public static Parser<T> Fold<T, TOperator>(this Parser<T> parser, Parser<TOperator> op, Func<T, T, TOperator, T> reduce) =>
        new FoldParser<T, TOperator>(parser, op, reduce);

    /// <summary>
    /// Creates a right-associative parser.
    /// </summary>
    /// <remarks>
    /// <code>
    /// // 2 ^ 3 ^ 4 ^ 1 => (2 ^ (3 ^ (4 ^ 1)))
    /// // a = b = c = d => (a = (b = (c = d)))
    ///
    /// // Number ("^" Number)*
    /// var power = number.FoldR(L('^'), (l, r, op) => Math.Pow(l, r));
    /// </code>
    /// </remarks>
    /// <typeparam name="T">The type of value produces by the main parser.</typeparam>
    /// <typeparam name="TOperator">The type of the operator token produced by the parser.</typeparam>
    /// <param name="parser">The main parser that matches a value.</param>
    /// <param name="op">The operator parser that matches an operator token.</param>
    /// <param name="reduce">A reduction function.</param>
    /// <returns>
    /// A parser that performs right-associative folding.
    /// </returns>
    public static Parser<T> FoldR<T, TOperator>(this Parser<T> parser, Parser<TOperator> op, Func<T, T, TOperator, T> reduce) =>
        new FoldRParser<T, TOperator>(parser, op, reduce);

    #region Inner type: FoldParser

    /// <summary>
    /// Represents a left-associative parser.
    /// </summary>
    /// <typeparam name="T">The type of value produces by the main parser.</typeparam>
    /// <typeparam name="TOperator">The type of the operator token produced by the parser.</typeparam>
    /// <param name="parser">The main parser that matches a value.</param>
    /// <param name="op">The operator parser that matches an operator token.</param>
    /// <param name="reduce">A reduction function.</param>
    private sealed class FoldParser<T, TOperator>(Parser<T> parser, Parser<TOperator> op, Func<T, T, TOperator, T> reduce) : Parser<T>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var bookmark = context.BookmarkPosition();

            if (parser.TryParse(ref context, out var v))
            {
                var result = v;

                while (true)
                {
                    var rollback = context.BookmarkPosition();

                    if (op.TryParse(ref context, out var o) && parser.TryParse(ref context, out v))
                    {
                        result = reduce(result, v, o);
                        continue;
                    }

                    context.RestorePosition(rollback);
                    break;
                }

                context.SetMatched(bookmark);
                value = result!;
                return true;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser()
        {
            var p = parser.Void();
            return Seq(p, Seq(op.Void(), p).ZeroOrMore());
        }
    }

    #endregion

    #region Inner type: FoldRParser

    /// <summary>
    /// Represents a right-associative parser.
    /// </summary>
    /// <typeparam name="T">The type of value produces by the main parser.</typeparam>
    /// <typeparam name="TOperator">The type of the operator token produced by the parser.</typeparam>
    /// <param name="parser">The main parser that matches a value.</param>
    /// <param name="op">The operator parser that matches an operator token.</param>
    /// <param name="reduce">A reduction function.</param>
    private sealed class FoldRParser<T, TOperator>(Parser<T> parser, Parser<TOperator> op, Func<T, T, TOperator, T> reduce) : Parser<T>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var bookmark = context.BookmarkPosition();

            if (parser.TryParse(ref context, out var v))
            {
                var list = new ArrayBuilder<(TOperator op, T value)>();
                list.Add((default!, v));

                while (true)
                {
                    var rollback = context.BookmarkPosition();

                    if (op.TryParse(ref context, out var o) && parser.TryParse(ref context, out v))
                    {
                        list.Add((o, v));
                        continue;
                    }

                    context.RestorePosition(rollback);
                    break;
                }

                context.SetMatched(bookmark);

                var result = list[^1].value;
                for (var i = list.Count - 1; i > 0; i--)
                    result = reduce(list[i - 1].value, result, list[i].op);

                value = result!;
                return true;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser()
        {
            var p = parser.Void();
            return Seq(p, Seq(op.Void(), p).ZeroOrMore());
        }
    }

    #endregion
}
