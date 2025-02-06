using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that matches a single character within the specified range.
    /// </summary>
    /// <param name="lo">The start of the range.</param>
    /// <param name="hi">The end of the range.</param>
    /// <returns>
    /// A parser that matches any single character within the specified range.
    /// </returns>
    public static Parser<char> Set(char lo, char hi)
    {
        Argument.ThrowIfGreaterThan(lo, hi);

        if (lo == hi)
            return L(lo);

        if (lo == char.MinValue && hi == char.MaxValue)
            return Any;

        var name = CharClassRange.Create(lo, hi).ToPrintable();
        return new RangeParser<char, RangeSearcher>(
            new RangeSearcher(lo, hi),
            default) { Name = name };
    }

    /// <summary>
    /// Creates a parser that matches any single character within the specified range set.
    /// For example, use <c>Set("a-z0-9")</c> to parse any alphanumeric character.
    /// </summary>
    /// <remarks>
    /// The <c>Set</c> method supports a wide range of character classes and escape sequences, including:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       Unicode general categories:<br/>
    ///       - <c>\p{</c>name<c>}</c>: any character in Unicode general category
    ///       (e.g., <c>\p{L}</c> for letters, <c>\p{Nd}</c> for decimal digits).<br/>
    ///       Where <c>name</c> is a Unicode general category: https://www.unicode.org/reports/tr44/#General_Category_Values
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Negation of Unicode general categories:<br/>
    ///       - <c>\P{</c>name<c>}</c>: any character not in the specified Unicode general category.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Shorthand character classes:<br/>
    ///         - <c>\w</c>: word character<br/>
    ///         - <c>\W</c>: non-word character<br/>
    ///         - <c>\s</c>: whitespace character<br/>
    ///         - <c>\S</c>: non-whitespace character<br/>
    ///         - <c>\d</c>: decimal digit<br/>
    ///         - <c>\D</c>: non-decimal digit.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///         Character escapes:<br/>
    ///         - <c>\a</c>: bell character (<c>\u0007</c>)<br/>
    ///         - <c>\b</c>: backspace (<c>\u0008</c>)<br/>
    ///         - <c>\t</c>: tab (<c>\u0009</c>)<br/>
    ///         - <c>\r</c>: carriage return (<c>\u000D</c>)<br/>
    ///         - <c>\v</c>: vertical tab (<c>\u000B</c>)<br/>
    ///         - <c>\f</c>: form feed (<c>\u000C</c>)<br/>
    ///         - <c>\n</c>: new line (<c>\u000A</c>)<br/>
    ///         - <c>\e</c>: escape (<c>\u001B</c>)<br/>
    ///         - <c>\u</c>nnnn: Unicode character (exactly four hexadecimal digits represented by <c>nnnn</c>).<br/>
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Character ranges: <c>0-9</c>, <c>a-z</c>, etc.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       Literal characters: <c>abcdefABCDEF</c>, etc.
    ///     </description>
    ///   </item>
    /// </list>
    ///
    /// The following features are **not supported**:
    /// <list type="bullet">
    ///     <item><description>Named Unicode blocks (e.g., <c>\p{Cyrillic}</c>).</description></item>
    ///     <item><description>Negation within ranges (e.g., <c>^0-9</c>).</description></item>
    ///     <item><description>Exclude-style ranges (e.g., <c>a-z-[a-f]</c>).</description></item>
    ///     <item><description>Anchors (e.g., <c>^</c>, <c>$</c>, <c>\A</c>, <c>\Z</c>, <c>\z</c>, <c>\G</c>, <c>\b</c>, <c>\B</c>).</description></item>
    /// </list>
    /// </remarks>
    /// <param name="set">A string representing a character classes, e.g., <c>"A-Za-z\p{Nd}"</c>.</param>
    /// <returns>
    /// A parser that matches any single character within the specified range sets.
    /// </returns>
    public static Parser<char> Set(string set)
    {
        var @class = CharClassParser.Parse(set);
        return Set(@class);
    }

    internal static Parser<char> Set(CharClass @class)
    {
        var name = @class.ToPrintable();
        var categories = @class.UnicodeCategories;
        var ranges = @class.Ranges;

        if (ranges.Length == 0)
            return new UnicodeCategoryParser<char>(categories) { Name = name };

        if (ranges.Length == 1)
        {
            if (ranges[0].IsFullRange)
                return Any;

            if (categories.IsEmpty && ranges[0].IsSingleCharacter)
                return L(ranges[0].Lo);

            return new RangeParser<char, RangeSearcher>(
                new RangeSearcher(ranges[0].Lo, ranges[0].Hi),
                categories) { Name = name };
        }

        if (@class.OverallWidth <= 128)
            return new RangeParser<char, BitVectorSearcher<Block128Bit>>(
                new BitVectorSearcher<Block128Bit>(ranges),
                categories) { Name = name };

        if (@class.OverallWidth <= 256)
            return new RangeParser<char, BitVectorSearcher<Block256Bit>>(
                new BitVectorSearcher<Block256Bit>(ranges),
                categories) { Name = name };

        if (@class.OverallWidth <= 512)
            return new RangeParser<char, BitVectorSearcher<Block512Bit>>(
                new BitVectorSearcher<Block512Bit>(ranges),
                categories) { Name = name };

        var list = new ArrayList<(int comparisons, int consumption, Parser<char> parser)>();

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
                    parser: new RangeParser<char, ContainsSearcher>(
                        new ContainsSearcher(@class.GetSymbols(simdAligned: true)),
                        categories) { Name = name }
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
                    parser: new RangeParser<char, SimdRangeSearcher<Block256Bit>>(
                        new SimdRangeSearcher<Block256Bit>(ranges),
                        categories) { Name = name }
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
                    parser: new RangeParser<char, SimdRangeSearcher<Block128Bit>>(
                        new SimdRangeSearcher<Block128Bit>(ranges),
                        categories) { Name = name }
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
                    parser: new RangeParser<char, BinaryRangeSearcher>(
                        new BinaryRangeSearcher(ranges),
                        categories) { Name = name }
                )
            );
        }

        return list
            .OrderBy(v => v.comparisons)
            .ThenBy(v => v.consumption)
            .First()
            .parser;
    }

    #region Inner type: UnicodeCategoryParser

    /// <summary>
    /// Represents a parser that attempts to parse a single character by checking
    /// whether it belongs to a specified Unicode category.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the parser.</typeparam>
    /// <param name="categories">The Unicode categories against which a character is matched.</param>
    private sealed class UnicodeCategoryParser<T>(CharClassUnicodeCategory categories) : Parser<T>, ICharClassSupport where T : struct
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, out T value)
        {
            var s = context.Source;
            var p = context.Position;

            if ((uint)p < (uint)s.Length)
            {
                var c = s[p];
                if (categories.IsInclude(c))
                {
                    if (typeof(T) == typeof(char))
                        value = (T)(object)c;
                    else
                        value = default;

                    context.Advance(1);
                    return true;
                }
            }

            value = default;
            context.AddError(Name);
            return false;
        }

        /// <inheritdoc />
        public CharClass GetCharClass() =>
            new CharClass(categories);

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new UnicodeCategoryParser<T>(categories) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new UnicodeCategoryParser<Unit>(categories) { Name = Name };
    }

    #endregion

    #region Inner type: RangeParser

    /// <summary>
    /// Represents a parser that attempts to parse a single character using a character class range searcher.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the parser.</typeparam>
    /// <typeparam name="TSearcher">The type of the character class range searcher implementing <see cref="ICharClassRangeSearcher"/>.</typeparam>
    private sealed class RangeParser<T, TSearcher> : Parser<T>, ICharClassSupport where T : struct where TSearcher : struct, ICharClassRangeSearcher
    {
        private TSearcher _searcher;
        private readonly CharClassUnicodeCategory _categories;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeParser{T, TSearcher}"/> class
        /// with the specified searcher and character class categories.
        /// </summary>
        /// <param name="searcher">The character class range searcher used for parsing.</param>
        /// <param name="categories">The Unicode character categories against which the character is matched
        /// if it is not found in the searcher.</param>
        public RangeParser(TSearcher searcher, CharClassUnicodeCategory categories)
        {
            Debug.Assert(typeof(T) == typeof(char) || typeof(T) == typeof(Unit));

            _searcher = searcher;
            _categories = categories;
        }

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, out T value)
        {
            var s = context.Source;
            var p = context.Position;

            if ((uint)p < (uint)s.Length)
            {
                var c = s[p];
                if (_searcher.Contains(c) || _categories.IsInclude(c))
                {
                    if (typeof(T) == typeof(char))
                        value = (T)(object)c;
                    else
                        value = default;

                    context.Advance(1);
                    return true;
                }
            }

            value = default;
            context.AddError(Name);
            return false;
        }

        /// <inheritdoc />
        public CharClass GetCharClass() =>
            _searcher.GetCharClass(_categories);

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new RangeParser<T, TSearcher>(_searcher, _categories) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new RangeParser<Unit, TSearcher>(_searcher, _categories) { Name = Name };
    }

    #endregion

    #region Inner type: RangeSearcher

    /// <summary>
    /// Represents a searcher that checks whether a character falls within a single range.
    /// </summary>
    private readonly struct RangeSearcher : ICharClassRangeSearcher
    {
        private readonly uint _start;
        private readonly uint _count;

        /// <summary>
        /// Initializes a new instance of the <see cref="RangeSearcher"/> structure
        /// with the specified lower and upper character bounds.
        /// </summary>
        /// <param name="lo">The lower bound of the character range.</param>
        /// <param name="hi">The upper bound of the character range.</param>
        public RangeSearcher(char lo, char hi)
        {
            Debug.Assert(lo <= hi);

            _start = lo;
            _count = (uint)(hi - lo + 1);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(char ch) =>
            ch - _start < _count;

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfAnyExcept(ReadOnlySpan<char> s) =>
            IndexOfAnyExcept(ref MemoryMarshal.GetReference(s), s.Length, _start, _count);

        /// <inheritdoc />
        public CharClass GetCharClass(CharClassUnicodeCategory categories)
        {
            var lo = _start;
            var hi = lo + _count - 1;

            return new CharClass(
                [CharClassRange.Create((char)lo, (char)hi)],
                categories);
        }

        private static int IndexOfAnyExcept(ref char s, int length, uint start, uint count)
        {
            ref var p = ref s;

            for (; length > 0; length--)
            {
                if (p - start >= count)
                    // ReSharper disable once RedundantCast
                    return (int)((nint)Unsafe.ByteOffset(ref s, ref p) >>> 1);

                p = ref Unsafe.Add(ref p, 1);
            }

            return -1;
        }
    }

    #endregion

    #region Inner type: ContainsSearcher

    /// <summary>
    /// Represents a searcher that determines whether a character is contained
    /// within a predefined set using direct lookup.
    /// </summary>
    /// <param name="chars">The string containing the set of characters to search within.</param>
    private readonly struct ContainsSearcher(string chars) : ICharClassRangeSearcher
    {
        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(char ch) =>
            chars.Contains(ch);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfAnyExcept(ReadOnlySpan<char> s) =>
            IndexOfAnyExcept(ref MemoryMarshal.GetReference(s), s.Length, chars);

        /// <inheritdoc />
        public CharClass GetCharClass(CharClassUnicodeCategory categories) =>
            new CharClass(CharClassRange.Create(chars), categories);

        private static int IndexOfAnyExcept(ref char s, int length, string chars)
        {
            _ = chars.Length;

            #if NET7_0_OR_GREATER

            return MemoryMarshal.CreateReadOnlySpan(ref s, length).IndexOfAnyExcept(chars);

            #else

            ref var p = ref s;

            for (; length > 0; length--)
            {
                if (!chars.Contains(p))
                    return (int)((nint)Unsafe.ByteOffset(ref s, ref p) >>> 1);

                p = ref Unsafe.Add(ref p, 1);
            }

            return -1;

            #endif
        }
    }

    #endregion

    #region Inner type: BitVectorSearcher

    /// <summary>
    /// Represents a searcher that uses a bit vector to efficiently determine
    /// whether a character belongs to a specified set of ranges.
    /// </summary>
    /// <typeparam name="TStorage">The underlying storage type for the bit vector, which must be an unmanaged type.</typeparam>
    private readonly struct BitVectorSearcher<TStorage> : ICharClassRangeSearcher where TStorage : unmanaged
    {
        private readonly BitVector<TStorage> _vector;
        private readonly int _offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="BitVectorSearcher{TStorage}"/> structure
        /// with the specified character class ranges.
        /// </summary>
        /// <param name="ranges">The character class ranges used for searching.</param>
        public BitVectorSearcher(CharClassRange[] ranges)
        {
            var vector = default(BitVector<TStorage>);
            var offset = ranges.Length != 0 ? ranges[0].Lo : 0;

            foreach (var range in ranges)
                for (var i = range.Lo; i <= range.Hi; i++)
                    vector.Set(i - offset);

            _vector = vector;
            _offset = offset;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(char ch) =>
            _vector.IsBitSet(ch - _offset);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfAnyExcept(ReadOnlySpan<char> s) =>
            IndexOfAnyExcept(ref MemoryMarshal.GetReference(s), s.Length, in _vector, _offset);

        /// <inheritdoc />
        public CharClass GetCharClass(CharClassUnicodeCategory categories) =>
            new CharClass(_vector.ToCharClassRanges(_offset), categories);

        private static int IndexOfAnyExcept(ref char s, int length, in BitVector<TStorage> vector, int offset)
        {
            ref var p = ref s;

            for (; length > 0; length--)
            {
                if (!vector.IsBitSet(p - offset))
                    // ReSharper disable once RedundantCast
                    return (int)((nint)Unsafe.ByteOffset(ref s, ref p) >>> 1);

                p = ref Unsafe.Add(ref p, 1);
            }

            return -1;
        }
    }

    #endregion

    #region Inner type: SimdRangeSearcher

    /// <summary>
    /// Represents a searcher that utilizes SIMD operations to efficiently determine
    /// whether a character falls within a specified set of ranges.
    /// </summary>
    /// <typeparam name="TWidth">The SIMD vector width, which must be an unmanaged type.</typeparam>
    private readonly struct SimdRangeSearcher<TWidth> : ICharClassRangeSearcher where TWidth : unmanaged
    {
        private readonly ushort[] _ranges;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimdRangeSearcher{TWidth}"/> structure
        /// with the specified character class ranges.
        /// </summary>
        /// <param name="ranges">The character class ranges used for searching.</param>
        public SimdRangeSearcher(CharClassRange[] ranges)
        {
            var result = new ArrayBuilder<ushort>();
            var stride = Unsafe.SizeOf<TWidth>() / 2;

            for (var i = 0; i < ranges.Length; i += stride)
            {
                for (var j = i; j < i + stride; j++) result.Add(ranges[(uint)j < (uint)ranges.Length ? j : 0].Lo);
                for (var j = i; j < i + stride; j++) result.Add(ranges[(uint)j < (uint)ranges.Length ? j : 0].Hi);
            }

            _ranges = result.ToArray();
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(char ch) =>
            ContainsCore(ch, _ranges);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfAnyExcept(ReadOnlySpan<char> s) =>
            IndexOfAnyExcept(ref MemoryMarshal.GetReference(s), s.Length, _ranges);

        /// <inheritdoc />
        public CharClass GetCharClass(CharClassUnicodeCategory categories)
        {
            var ranges = _ranges;
            var length = ranges.Length;
            var @class = new ArrayBuilder<CharClassRange>();
            var stride = Unsafe.SizeOf<TWidth>() / 2;

            for (var i = 0; i < length; i += stride * 2)
            for (var j = i; j < i + stride; j++)
                @class.Add(
                    CharClassRange.Create(
                        (char)ranges[j],
                        (char)ranges[j + stride]));

            return new CharClass(@class.ToArray(), categories);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ContainsCore(char ch, ushort[] ranges)
        {
            var length = ranges.Length;

            if (Unsafe.SizeOf<TWidth>() == 256/8)
            {
                // ReSharper disable once RedundantCast
                var c = Vector256.Create((ushort)ch);
                ref var r = ref MemoryMarshal.GetArrayDataReference(ranges);

                for (; length > 0; r = ref Unsafe.Add(ref r, 32), length -= 32)
                {
                    var x = Unsafe.ReadUnaligned<Vector256<ushort>>(ref Unsafe.As<ushort, byte>(ref r));
                    var y = Unsafe.ReadUnaligned<Vector256<ushort>>(ref Unsafe.As<ushort, byte>(ref Unsafe.Add(ref r, 16)));

                    var v = Avx2.CompareEqual(
                        Avx2.Min(Avx2.Max(c, x), y),
                        c);

                    if (!Avx.TestZ(v, v))
                        return true;
                }
            }
            else if (Unsafe.SizeOf<TWidth>() == 128/8)
            {
                // ReSharper disable once RedundantCast
                var c = Vector128.Create((ushort)ch);
                ref var r = ref MemoryMarshal.GetArrayDataReference(ranges);

                for (; length > 0; r = ref Unsafe.Add(ref r, 16), length -= 16)
                {
                    var x = Unsafe.ReadUnaligned<Vector128<ushort>>(ref Unsafe.As<ushort, byte>(ref r));
                    var y = Unsafe.ReadUnaligned<Vector128<ushort>>(ref Unsafe.As<ushort, byte>(ref Unsafe.Add(ref r, 8)));

                    if (Sse41.IsSupported)
                    {
                        var v = Sse2.CompareEqual(
                            Sse41.Min(Sse41.Max(c, x), y),
                            c);

                        if (!Sse41.TestZ(v, v))
                            return true;
                    }
                    else
                    {
                        var v = AdvSimd.CompareLessThanOrEqual(
                            AdvSimd.Subtract(c, x),
                            y);

                        if (!v.Equals(Vector128<ushort>.Zero))
                            return true;
                    }
                }
            }

            return false;
        }

        private static int IndexOfAnyExcept(ref char s, int length, ushort[] ranges)
        {
            ref var p = ref s;

            for (; length > 0; length--)
            {
                if (!ContainsCore(p, ranges))
                    // ReSharper disable once RedundantCast
                    return (int)((nint)Unsafe.ByteOffset(ref s, ref p) >>> 1);

                p = ref Unsafe.Add(ref p, 1);
            }

            return -1;
        }
    }

    #endregion

    #region Inner type: BinaryRangeSearcher

    /// <summary>
    /// Represents a searcher that uses a binary search algorithm to determine
    /// whether a character falls within a specified set of ranges.
    /// </summary>
    private readonly struct BinaryRangeSearcher : ICharClassRangeSearcher
    {
        private readonly (uint lo, uint hi)[] _ranges;

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryRangeSearcher"/> structure
        /// with the specified character class ranges.
        /// </summary>
        /// <param name="ranges">The character class ranges used for searching.</param>
        public BinaryRangeSearcher(CharClassRange[] ranges)
        {
            var list = new (uint lo, uint hi)[ranges.Length];

            // Note: The following condition is redundant, but it is included specifically
            // to eliminate bounds checks generated by the JIT compiler.
            if (list.Length == ranges.Length)
                for (var i = 0; i < ranges.Length; i++)
                    list[i] = ranges[i];

            _ranges = list;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(char ch) =>
            ContainsCore(ch, _ranges);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfAnyExcept(ReadOnlySpan<char> s) =>
            IndexOfAnyExcept(ref MemoryMarshal.GetReference(s), s.Length, _ranges);

        /// <inheritdoc />
        public CharClass GetCharClass(CharClassUnicodeCategory categories)
        {
            var ranges = _ranges;
            var @class = new CharClassRange[ranges.Length];

            // Note: The following condition is redundant, but it is included specifically
            // to eliminate bounds checks generated by the JIT compiler.
            if (@class.Length == ranges.Length)
                for (var i = 0; i < ranges.Length; i++)
                    @class[i] = CharClassRange.Create((char)ranges[i].lo, (char)ranges[i].hi);

            return new CharClass(@class, categories);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ContainsCore(char ch, (uint lo, uint hi)[] ranges)
        {
            var lo = 0;
            var hi = ranges.Length - 1;
            var nc = (uint)ch;

            while (lo <= hi)
            {
                var mi = (lo + hi) >>> 1;
                if ((uint)mi >= (uint)ranges.Length)
                    break;

                var range = ranges[mi];

                if (nc < range.lo)
                {
                    hi = mi - 1;
                }
                else if (nc > range.hi)
                {
                    lo = mi + 1;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        private static int IndexOfAnyExcept(ref char s, int length, (uint lo, uint hi)[] ranges)
        {
            ref var p = ref s;

            for (; length > 0; length--)
            {
                if (!ContainsCore(p, ranges))
                    // ReSharper disable once RedundantCast
                    return (int)((nint)Unsafe.ByteOffset(ref s, ref p) >>> 1);

                p = ref Unsafe.Add(ref p, 1);
            }

            return -1;
        }
    }

    #endregion

    #region Inner type: SearchValuesSearcher

    #if NET8_0_OR_GREATER

    /// <summary>
    /// Represents a searcher that utilizes <see cref="System.Buffers.SearchValues{T}"/> to efficiently
    /// determine whether a character belongs to a predefined set.
    /// </summary>
    /// <param name="values">The set of characters against which searches are performed.</param>
    private readonly struct SearchValuesSearcher(System.Buffers.SearchValues<char> values) : ICharClassRangeSearcher
    {
        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(char ch) =>
            values.Contains(ch);

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int IndexOfAnyExcept(ReadOnlySpan<char> s) =>
            s.IndexOfAnyExcept(values);

        /// <inheritdoc />
        public CharClass GetCharClass(CharClassUnicodeCategory categories)
        {
            var list = new ArrayBuilder<CharClassRange>();
            for (var c = 0; c <= 127; c++)
                if (values.Contains((char)c))
                    list.Add(CharClassRange.Create((char)c));

            return new CharClass(list.ToArray(), categories);
        }
    }

    #endif

    #endregion

    #region Inner type: ICharClassRangeSearcher

    /// <summary>
    /// Provides support for searching character class ranges.
    /// </summary>
    private interface ICharClassRangeSearcher
    {
        /// <summary>
        /// Determines whether the specified character is contained within the character class range.
        /// </summary>
        /// <param name="ch">The character to search for.</param>
        /// <returns>
        /// <see langword="true"/> if the character is contained within the range;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        bool Contains(char ch);

        /// <summary>
        /// Searches for the first occurrence of any character in the specified span
        /// that is not contained within the character class range.
        /// </summary>
        /// <param name="s">The span of characters to search.</param>
        /// <returns>
        /// The zero-based index of the first character in <paramref name="s"/>
        /// that is not contained within the range; <c>-1</c> if all characters
        /// in <paramref name="s"/> are contained within the range.
        /// </returns>
        int IndexOfAnyExcept(ReadOnlySpan<char> s);

        /// <summary>
        /// Returns a <see cref="CharClass"/> object representing the current instance.
        /// </summary>
        /// <param name="categories">The Unicode categories to add to the character classes.</param>
        /// <returns>
        /// A <see cref="CharClass"/> object representing the current instance.
        /// </returns>
        CharClass GetCharClass(CharClassUnicodeCategory categories);
    }

    #endregion
}
