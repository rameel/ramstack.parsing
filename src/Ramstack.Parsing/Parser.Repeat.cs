using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that applies the specified parser zero or more times.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the specified parser.</typeparam>
    /// <param name="parser">The <see cref="Parser{T}"/> to apply zero or more times.</param>
    /// <returns>
    /// A parser that applies the specified parser zero or more times.
    /// </returns>
    public static Parser<List<T>> ZeroOrMore<T>(this Parser<T> parser) =>
        Repeat(parser, 0, int.MaxValue);

    /// <summary>
    /// Creates a parser that applies the specified parser zero or more times.
    /// </summary>
    /// <param name="parser">The <see cref="Parser{T}"/> to apply zero or more times.</param>
    /// <returns>
    /// A parser that applies the specified parser zero or more times.
    /// </returns>
    public static Parser<Unit> ZeroOrMore(this Parser<Unit> parser) =>
        Repeat(parser, 0, int.MaxValue);

    /// <summary>
    /// Creates a parser that applies the specified parser at least once.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the specified parser.</typeparam>
    /// <param name="parser">The <see cref="Parser{T}"/> to apply at least once.</param>
    /// <returns>
    /// A parser that applies the specified parser at least once.
    /// </returns>
    public static Parser<List<T>> OneOrMore<T>(this Parser<T> parser) =>
        Repeat(parser, 1, int.MaxValue);

    /// <summary>
    /// Creates a parser that applies the specified parser at least once.
    /// </summary>
    /// <param name="parser">The <see cref="Parser{T}"/> to apply at least once.</param>
    /// <returns>
    /// A parser that applies the specified parser at least once.
    /// </returns>
    public static Parser<Unit> OneOrMore(this Parser<Unit> parser) =>
        Repeat(parser, 1, int.MaxValue);

    /// <summary>
    /// Creates a parser that applies the specified parser zero or more times.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the specified parser.</typeparam>
    /// <param name="parser">The <see cref="Parser{T}"/> to apply zero or more times.</param>
    /// <returns>
    /// A parser that applies the specified parser zero or more times.
    /// </returns>
    public static Parser<List<T>> Many<T>(this Parser<T> parser) =>
        Repeat(parser, 0, int.MaxValue);

    /// <summary>
    /// Creates a parser that applies the specified parser zero or more times.
    /// </summary>
    /// <param name="parser">The <see cref="Parser{T}"/> to apply zero or more times.</param>
    /// <returns>
    /// A parser that applies the specified parser zero or more times.
    /// </returns>
    public static Parser<Unit> Many(this Parser<Unit> parser) =>
        Repeat(parser, 0, int.MaxValue);

    /// <summary>
    /// Creates a parser that applies the specified parser at least the defined number of times.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the specified parser.</typeparam>
    /// <param name="parser">The <see cref="Parser{T}"/> to apply at least the specified number of times.</param>
    /// <param name="count">The minimum number of repetitions.</param>
    /// <returns>
    /// A parser that applies the specified parser at least a defined number of times.
    /// </returns>
    public static Parser<List<T>> AtLeast<T>(this Parser<T> parser, int count) =>
        Repeat(parser, count, int.MaxValue);

    /// <summary>
    /// Creates a parser that applies the specified parser at least the specified number of times.
    /// </summary>
    /// <param name="parser">The <see cref="Parser{T}"/> to apply at least the specified number of times.</param>
    /// <param name="count">The minimum number of repetitions.</param>
    /// <returns>
    /// A parser that applies the specified parser at least a defined number of times.
    /// </returns>
    public static Parser<Unit> AtLeast(this Parser<Unit> parser, int count) =>
        Repeat(parser, count, int.MaxValue);

    /// <summary>
    /// Creates a parser that applies the specified parser a defined number of times.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the specified parser.</typeparam>
    /// <param name="parser">The <see cref="Parser{T}"/> to apply the specified number of times.</param>
    /// <param name="count">The number of repetitions.</param>
    /// <returns>
    /// A parser that applies the specified parser a defined number of times.
    /// </returns>
    public static Parser<List<T>> Repeat<T>(this Parser<T> parser, int count) =>
        Repeat(parser, count, count);

    /// <summary>
    /// Creates a parser that applies the specified parser a defined number of times.
    /// </summary>
    /// <param name="parser">The <see cref="Parser{T}"/> to apply the specified number of times.</param>
    /// <param name="count">The number of repetitions.</param>
    /// <returns>
    /// A parser that applies the specified parser a defined number of times.
    /// </returns>
    public static Parser<Unit> Repeat(this Parser<Unit> parser, int count) =>
        Repeat(parser, count, count);

    /// <summary>
    /// Creates a parser that applies the specified parser a defined number of times.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the specified parser.</typeparam>
    /// <param name="parser">The <see cref="Parser{T}"/> to apply the specified number of times.</param>
    /// <param name="min">Minimum number of repetitions.</param>
    /// <param name="max">Maximum number of repetitions.</param>
    /// <returns>
    /// A parser that applies the specified parser a defined number of times.
    /// </returns>
    public static Parser<List<T>> Repeat<T>(this Parser<T> parser, int min, int max)
    {
        Argument.ThrowIfNegative(min);
        Argument.ThrowIfNegativeOrZero(max);
        Argument.ThrowIfGreaterThan(min, max);

        return parser is ICharClassSupport s
            ? (Parser<List<T>>)(object)CreateRepeatParser(s.GetCharClass(), min, max)
            : new RepeatParser<T>(parser, min, max);
    }

    /// <summary>
    /// Creates a parser that applies the specified parser a defined number of times.
    /// </summary>
    /// <param name="parser">The <see cref="Parser{T}"/> to apply the specified number of times.</param>
    /// <param name="min">Minimum number of repetitions.</param>
    /// <param name="max">Maximum number of repetitions.</param>
    /// <returns>
    /// A parser that applies the specified parser a defined number of times.
    /// </returns>
    public static Parser<Unit> Repeat(this Parser<Unit> parser, int min, int max)
    {
        Argument.ThrowIfNegative(min);
        Argument.ThrowIfNegativeOrZero(max);
        Argument.ThrowIfGreaterThan(min, max);

        return parser is ICharClassSupport s
            ? CreateRepeatParser(s.GetCharClass(), min, max).Void()
            : new VoidRepeatParser(parser, min, max);
    }

    private static Parser<List<char>> CreateRepeatParser(CharClass @class, int min, int max)
    {
        if (@class.Ranges.Length == 0)
            @class = @class.EnrichRanges();

        var errors = @class.ToPrintable();
        var ranges = @class.Ranges;

        #if NET8_0_OR_GREATER
        if (ranges.Length != 0)
        {
            // SearchValues.IndexOfAnyExcept is highly optimized for:
            // - A single range, e.g. [A..Z]
            // - The ASCII character set ([0..127])
            // - A total character count between 1 and 5

            // While we could use SearchValues for other cases as well,
            // it may allocate a significant amount of memory
            // and may not be as efficient as desired.
            // Therefore, for other cases, we use a different search strategy.

            if (ranges.Length == 1 || ranges[^1].Hi <= 127 || @class.SymbolCount <= 5)
            {
                return new RepeatCharClassParser<SearchValuesSearcher>(
                    new SearchValuesSearcher(System.Buffers.SearchValues.Create(@class.GetSymbols())),
                    @class.UnicodeCategories,
                    min, max) { Name = errors };
            }
        }
        #endif

        //
        // TODO: For ASCII, it might be worth creating a specialized version with SIMD support for .NET6 and .NET7
        //

        if (@class.OverallWidth <= 128)
            return new RepeatCharClassParser<BitVectorSearcher<Block128Bit>>(
                new BitVectorSearcher<Block128Bit>(ranges),
                @class.UnicodeCategories, min, max) { Name = errors };

        if (@class.OverallWidth <= 256)
            return new RepeatCharClassParser<BitVectorSearcher<Block256Bit>>(
                new BitVectorSearcher<Block256Bit>(ranges),
                @class.UnicodeCategories, min, max) { Name = errors };

        if (@class.OverallWidth <= 512)
            return new RepeatCharClassParser<BitVectorSearcher<Block512Bit>>(
                new BitVectorSearcher<Block512Bit>(ranges),
                @class.UnicodeCategories, min, max) { Name = errors };

        var list = new List<(int comparisons, int consumption, Parser<List<char>> parser)>();

        if (true)
        {
            var count = @class.SymbolCount;
            var width = Avx2.IsSupported
                ? Vector256<ushort>.Count
                : Sse41.IsSupported || AdvSimd.IsSupported
                    ? Vector128<ushort>.Count
                    : 0;

            list.Add(
                (
                    comparisons: width != 0 ? (count + width - 1 & -width) >>> BitOperations.Log2((uint)width) : count,
                    consumption: width != 0 ? (count + width - 1) & -width : count,
                    parser: new RepeatCharClassParser<ContainsSearcher>(
                        new ContainsSearcher(@class.GetSymbols(simdAligned: true)),
                        @class.UnicodeCategories, min, max) { Name = errors }
                )
            );
        }

        if (Avx2.IsSupported)
        {
            var count = ranges.Length + 15 & -16;
            list.Add(
                (
                    comparisons: count >>> 4,
                    consumption: count * 2,
                    parser: new RepeatCharClassParser<SimdRangeSearcher<Block256Bit>>(
                        new SimdRangeSearcher<Block256Bit>(ranges),
                        @class.UnicodeCategories, min, max) { Name = errors }
                )
            );
        }

        if (Sse41.IsSupported || AdvSimd.IsSupported)
        {
            var count = ranges.Length + 7 & -8;
            list.Add(
                (
                    comparisons: count >>> 3,
                    consumption: count * 2,
                    parser: new RepeatCharClassParser<SimdRangeSearcher<Block128Bit>>(
                        new SimdRangeSearcher<Block128Bit>(ranges),
                        @class.UnicodeCategories, min, max) { Name = errors }
                )
            );
        }

        if (true)
        {
            var count = (int)BitOperations.RoundUpToPowerOf2(
                (uint)BitOperations.Log2((uint)ranges.Length)
                );

            var overhead = Avx2.IsSupported
                ? 3
                : Sse41.IsSupported || AdvSimd.IsSupported
                    ? 2
                    : 0;

            list.Add(
                (
                    comparisons: count + overhead,
                    consumption: ranges.Length * 2,
                    parser: new RepeatCharClassParser<BinaryRangeSearcher>(
                        new BinaryRangeSearcher(ranges),
                        @class.UnicodeCategories, min, max) { Name = errors }
                )
            );
        }

        return list
            .OrderBy(v => v.comparisons)
            .ThenBy(v => v.consumption)
            .First()
            .parser;
    }


    #region Inner type: RepeatParser

    /// <summary>
    /// Represents a parser that applies the specified parser a defined number of times.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the specified parser.</typeparam>
    private sealed class RepeatParser<T> : Parser<List<T>>
    {
        private readonly Parser<T> _parser;
        private readonly int _min;
        private readonly int _max;

        /// <summary>
        /// Represents a parser that applies the specified parser a defined number of times.
        /// </summary>
        /// <param name="parser">The <see cref="Parser{T}"/> to be applied a specified number of times.</param>
        /// <param name="min">The minimum number of repetitions.</param>
        /// <param name="max">The maximum number of repetitions.</param>
        public RepeatParser(Parser<T> parser, int min, int max) =>
            (_parser, _min, _max) = (parser, min, max);

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out List<T>? value)
        {
            var list = new List<T>();
            var bookmark = context.BookmarkPosition();
            var parser = _parser;

            do
            {
                var last = context.Position;

                if (!parser.TryParse(ref context, out var result))
                    break;

                list.Add(result);

                //
                // Prevent infinite loop
                //
                if (list.Count >= _min)
                {
                    //
                    // Parsing failed in this case because:
                    // 1. The parser matched, but the position remained unchanged.
                    // 2. Rechecking would yield the same result, making it redundant.
                    // 3. If a parser matches but the position remains unchanged, it results in an infinite loop.
                    //
                    if (context.Position == last)
                        break;
                }
            }
            while (list.Count < _max);

            if (list.Count >= _min)
            {
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
            new VoidRepeatParser(_parser.Void(), _min, _max) { Name = Name };
    }

    #endregion

    #region Inner type: VoidRepeatParser

    /// <summary>
    /// Represents a specialized parser used as an optimization that discards the parsed result,
    /// while applying the specified parser a defined number of times.
    /// </summary>
    private sealed class VoidRepeatParser : Parser<Unit>
    {
        private readonly Parser<Unit> _parser;
        private readonly int _min;
        private readonly int _max;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoidRepeatParser"/> class.
        /// </summary>
        /// <param name="parser">The <see cref="Parser{T}"/> to be applied a specified number of times.</param>
        /// <param name="min">The minimum number of repetitions.</param>
        /// <param name="max">The maximum number of repetitions.</param>
        public VoidRepeatParser(Parser<Unit> parser, int min, int max) =>
            (_parser, _min, _max) = (parser, min, max);

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, out Unit value)
        {
            var count = 0;
            var bookmark = context.BookmarkPosition();
            var parser = _parser;

            do
            {
                var last = context.Position;
                if (!parser.TryParse(ref context, out value))
                    break;

                //
                // Prevent infinite loop
                //
                if (context.Position != last)
                    continue;

                count = _min;
                break;
            }
            while (++count < _max);

            if (count >= _min)
            {
                context.SetMatched(bookmark);
                return true;
            }

            context.RestorePosition(bookmark);
            return false;
        }
    }

    #endregion

    #region Inner type: RepeatCharClassParser

    private sealed class RepeatCharClassParser<TSearcher> : Parser<List<char>> where TSearcher : struct, ICharClassRangeSearcher
    {
        private TSearcher _searcher;
        private readonly CharClassUnicodeCategory _categories;
        private readonly int _min;
        private readonly int _max;

        public RepeatCharClassParser(TSearcher searcher, CharClassUnicodeCategory categories, int min, int max)
        {
            Debug.Assert(min >= 0 && min <= max);

            _searcher = searcher;
            _categories = categories;
            _min = min;
            _max = max;
        }

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out List<char>? value)
        {
            value = null;

            var s = context.Remaining;
            var count = 0;

            while (s.Length != 0)
            {
                var index = _searcher.IndexOfAnyExcept(s);

                if ((uint)index < (uint)s.Length)
                {
                    count += index;

                    if (!_categories.IsInclude(s[index]))
                        break;

                    index++;
                    count++;

                    // At this point, "index" is guaranteed to be within [0..s.Length].
                    // While the Slice method performs its own bounds check and would throw if out of range,
                    // our explicit check helps the JIT recognize that the call to Slice is safe.
                    // This allows the JIT to eliminate the internal bounds check
                    // and avoid generating unnecessary exception-handling code inside Slice.
                    //
                    // In other words, the check would happen anyway, but by placing it here explicitly,
                    // we eliminate the need for exception code inside Slice itself.
                    //
                    // Even though logically (uint)index < (uint)s.Length implies (uint)index <= (uint)s.Length,
                    // the JIT cannot currently prove this identity and therefore cannot optimize away
                    // the redundant check in Slice.

                    if ((uint)index <= (uint)s.Length)
                        s = s.Slice(index);
                }
                else
                {
                    count += s.Length;
                    break;
                }
            }

            if (count >= _min)
            {
                if (count < _max)
                    context.ReportExpected(context.Position + count, Name);

                count = Math.Min(count, _max);
                context.Advance(count);

                value = ListFactory<char>.CreateList(context.MatchedSegment);
            }
            else
            {
                context.ReportExpected(context.Position + count, Name);
            }

            return count >= _min;
        }

        /// <inheritdoc />
        protected internal override Parser<List<char>> ToNamedParser(string? name) =>
            new RepeatCharClassParser<TSearcher>(_searcher, _categories, _min, _max) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new VoidRepeatCharClassParser<TSearcher>(_searcher, _categories, _min, _max) { Name = Name };
    }

    #endregion

    #region Inner type: VoidRepeatCharClassParser

    private sealed class VoidRepeatCharClassParser<TSearcher> : Parser<Unit> where TSearcher : struct, ICharClassRangeSearcher
    {
        private TSearcher _searcher;
        private readonly CharClassUnicodeCategory _categories;
        private readonly int _min;
        private readonly int _max;

        public VoidRepeatCharClassParser(TSearcher searcher, CharClassUnicodeCategory categories, int min, int max)
        {
            Debug.Assert(min >= 0 && min <= max);

            _searcher = searcher;
            _categories = categories;
            _min = min;
            _max = max;
        }

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, out Unit value)
        {
            Unsafe.SkipInit(out value);

            var s = context.Remaining;
            var count = 0;

            while (s.Length != 0)
            {
                var index = _searcher.IndexOfAnyExcept(s);
                if ((uint)index < (uint)s.Length)
                {
                    count += index;

                    if (!_categories.IsInclude(s[index]))
                        break;

                    index++;
                    count++;

                    // At this point, "index" is guaranteed to be within [0..s.Length].
                    // While the Slice method performs its own bounds check and would throw if out of range,
                    // our explicit check helps the JIT recognize that the call to Slice is safe.
                    // This allows the JIT to eliminate the internal bounds check
                    // and avoid generating unnecessary exception-handling code inside Slice.
                    //
                    // In other words, the check would happen anyway, but by placing it here explicitly,
                    // we eliminate the need for exception code inside Slice itself.
                    //
                    // Even though logically (uint)index < (uint)s.Length implies (uint)index <= (uint)s.Length,
                    // the JIT cannot currently prove this identity and therefore cannot optimize away
                    // the redundant check in Slice.

                    if ((uint)index <= (uint)s.Length)
                        s = s.Slice(index);
                }
                else
                {
                    count += s.Length;
                    break;
                }
            }

            if (count >= _min)
            {
                if (count < _max)
                    context.ReportExpected(context.Position + count, Name);

                count = Math.Min(count, _max);
                context.Advance(count);
            }
            else
            {
                context.ReportExpected(context.Position + count, Name);
            }

            return count >= _min;
        }

        /// <inheritdoc />
        protected internal override Parser<Unit> ToNamedParser(string? name) =>
            new VoidRepeatCharClassParser<TSearcher>(_searcher, _categories, _min, _max) { Name = name };
    }

    #endregion
}
