namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that applies the specified map function to the matched segment (ignoring the parsed value).
    /// </summary>
    /// <typeparam name="T">The type of value produced by the initial parser.</typeparam>
    /// <typeparam name="TResult">The type of the value produced by applying the map function.</typeparam>
    /// <param name="parser">The <see cref="Parser{T}"/> used to match the input.</param>
    /// <param name="func">The function that maps the matched segment to a <typeparamref name="TResult"/>.</param>
    /// <returns>
    /// A parser that produces <typeparamref name="TResult"/> by mapping the matched segment through <paramref name="func"/>.
    /// </returns>
    public static Parser<TResult> Map<T, TResult>(this Parser<T> parser, MapFunc<TResult> func) =>
        new MapParser<TResult>(parser.Void(), func) { Name = parser.Name };

    /// <summary>
    /// Creates a parser that applies the specified map function to both the matched segment and the parsed value.
    /// </summary>
    /// <typeparam name="T">The type of value produced by the initial parser.</typeparam>
    /// <typeparam name="TResult">The type of the value produced by applying the map function.</typeparam>
    /// <param name="parser">The <see cref="Parser{T}"/> used to match and parse the input.</param>
    /// <param name="func">The function that maps the matched segment and the parsed value to a <typeparamref name="TResult"/>.</param>
    /// <returns>
    /// A parser that produces <typeparamref name="TResult"/> by mapping the matched segment
    /// and the parsed value through <paramref name="func"/>.
    /// </returns>
    public static Parser<TResult> Map<T, TResult>(this Parser<T> parser, MapFunc<T, TResult> func) =>
        new MapParser<T, TResult>(parser, func) { Name = parser.Name };

    #region Inner type: MapParser<T>

    /// <summary>
    /// Represents a parser that applies a <paramref name="func"/> to the matched segment,
    /// ignoring the original parsed value.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by applying the map function.</typeparam>
    /// <param name="parser">A parser that matches the input but discards its value.</param>
    /// <param name="func">A function transforming the matched segment into <typeparamref name="T"/>.</param>
    private sealed class MapParser<T>(Parser<Unit> parser, MapFunc<T> func) : Parser<T>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            if (parser.TryParse(ref context, out _))
            {
                value = func(context.MatchedSegment)!;
                return true;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new MapParser<T>(parser.ToNamedParser(name), func) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            parser.Void();
    }

    #endregion

    #region Inner type: MapParser<T, TResult>

    /// <summary>
    /// Represents a parser that applies a <paramref name="func"/> to both the matched segment and the parsed value.
    /// </summary>
    /// <typeparam name="T">The type of value produced by the initial parser.</typeparam>
    /// <typeparam name="TResult">The type of the value produced by applying the map function.</typeparam>
    /// <param name="parser">A parser that matches and parses the input to a value of type <typeparamref name="T"/>.</param>
    /// <param name="func">A function transforming the matched segment and the parsed value into <typeparamref name="TResult"/>.</param>
    private sealed class MapParser<T, TResult>(Parser<T> parser, MapFunc<T, TResult> func) : Parser<TResult>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out TResult? value)
        {
            if (parser.TryParse(ref context, out var parsedValue))
            {
                value = func(context.MatchedSegment, parsedValue)!;
                return true;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<TResult> ToNamedParser(string? name) =>
            new MapParser<T, TResult>(parser.ToNamedParser(name), func) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            parser.Void();
    }

    #endregion
}
