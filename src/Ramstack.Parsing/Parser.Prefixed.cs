namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that matches zero or more prefix operators followed by an operand.
    /// </summary>
    /// <remarks>
    /// <code>
    /// // Example: ('-')* Number
    /// // - - - 5 => -(-(-5))
    /// var unary = number.Prefixed(L('-'), (x, op) => -x);
    /// </code>
    /// <para>
    ///   Operators are applied from right to left: the operator closest to the operand is applied first.
    /// </para>
    /// <para>
    ///   If the operator parser succeeds without consuming input, its result is discarded
    ///   and the search for operators stops without calling the reduction function for that result.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of the value produced by the main parser.</typeparam>
    /// <typeparam name="TOperator">The type of the operator token produced by the parser.</typeparam>
    /// <param name="parser">The main parser that matches an operand.</param>
    /// <param name="op">The parser that matches a prefix operator token.</param>
    /// <param name="reduce">A reduction function that receives the operand value as its first argument
    /// and the operator token as its second argument, and returns the updated operand value.</param>
    /// <returns>
    /// A parser that matches prefix operators followed by an operand.
    /// </returns>
    public static Parser<T> Prefixed<T, TOperator>(this Parser<T> parser, Parser<TOperator> op, Func<T, TOperator, T> reduce) =>
        new PrefixedParser<T, TOperator>(parser, op, reduce);

    #region Inner type: PrefixedParser

    /// <summary>
    /// Represents a parser that matches zero or more prefix operators followed by an operand.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the main parser.</typeparam>
    /// <typeparam name="TOperator">The type of the operator token produced by the parser.</typeparam>
    /// <param name="parser">The main parser that matches an operand.</param>
    /// <param name="op">The parser that matches a prefix operator token.</param>
    /// <param name="reduce">A reduction function that receives the operand value as its first argument
    /// and the operator token as its second argument, and returns the updated operand value.</param>
    private sealed class PrefixedParser<T, TOperator>(Parser<T> parser, Parser<TOperator> op, Func<T, TOperator, T> reduce) : Parser<T>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var bookmark = context.BookmarkPosition();

            //
            // Most chains contain zero or one operator. Keep the first operator in a local variable
            // so that the buffer is allocated only when a second operator appears.
            //
            var count = 0;
            var first = default(TOperator)!;
            var other = new ArrayBuilder<TOperator>();

            while (true)
            {
                var last = context.Position;

                if (!op.TryParse(ref context, out var o))
                    break;

                //
                // Prevent infinite loop
                //
                if (context.Position == last)
                    break;

                if (count == 0)
                {
                    count++;
                    first = o;
                }
                else
                {
                    other.Add(o);
                }
            }

            if (parser.TryParse(ref context, out var v))
            {
                for (var i = other.Count - 1; i >= 0; i--)
                    v = reduce(v, other[i]);

                if (count != 0)
                    v = reduce(v, first);

                context.SetMatched(bookmark);
                value = v!;
                return true;
            }

            context.RestorePosition(bookmark);
            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            Seq(op.Void().ZeroOrMore(), parser.Void());
    }

    #endregion
}
