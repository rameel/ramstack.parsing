using System.Numerics;

namespace Ramstack.Parsing;

partial class Literal
{
    /// <summary>
    /// Creates a parser for numeric types.
    /// </summary>
    /// <typeparam name="T">The numeric type to parse.</typeparam>
    /// <param name="kind">The numeric format to parse. Defaults to <see cref="NumberKind.Auto"/>.</param>
    /// <returns>
    /// A parser for the specified numeric type.
    /// </returns>
    public static Parser<T> Number<T>(NumberKind kind = NumberKind.Auto) where T : struct
    #if NET7_0_OR_GREATER
    , INumber<T>
    #endif
    {
        return Number<T>(null, kind);
    }

    /// <summary>
    /// Creates a parser for numeric types.
    /// </summary>
    /// <typeparam name="T">The numeric type to parse.</typeparam>
    /// <param name="name">An optional name for the parser.</param>
    /// <param name="kind">The numeric format to parse. Defaults to <see cref="NumberKind.Auto"/>.</param>
    /// <returns>
    /// A parser for the specified numeric type.
    /// </returns>
    public static Parser<T> Number<T>(string? name, NumberKind kind = NumberKind.Auto) where T : struct
    #if NET7_0_OR_GREATER
    , INumber<T>
    #endif
    {
        if (typeof(T) == typeof(float)
            || typeof(T) == typeof(double)
            || typeof(T) == typeof(decimal)
            || typeof(T) == typeof(Half))
        {
            return kind is NumberKind.Auto or NumberKind.Float
                ? new NumberLiteral<T, FloatLiteralKind>(name ?? "floating-point number", NumberStyles.Float)
                : throw new ArgumentException($"The number kind {kind} are not supported on floating-point types.", nameof(kind));
        }

        if (typeof(T) == typeof(sbyte)
            || typeof(T) == typeof(byte)
            || typeof(T) == typeof(short)
            || typeof(T) == typeof(ushort)
            || typeof(T) == typeof(int)
            || typeof(T) == typeof(uint)
            || typeof(T) == typeof(long)
            || typeof(T) == typeof(ulong)
            || typeof(T) == typeof(nint)
            || typeof(T) == typeof(nuint)
            #if NET7_0_OR_GREATER
            || typeof(T) == typeof(Int128)
            || typeof(T) == typeof(UInt128)
            #endif
            || typeof(T) == typeof(BigInteger))
        {
            if (kind is NumberKind.Auto or NumberKind.Integer)
                return new NumberLiteral<T, IntegerLiteralKind>(name ?? "number", NumberStyles.Integer);

            if (kind == NumberKind.HexNumber)
                return new NumberLiteral<T, HexNumberLiteralKind>(name ?? "hexadecimal number", NumberStyles.HexNumber);

            #if NET8_0_OR_GREATER
            if (kind == NumberKind.BinaryNumber)
                return new NumberLiteral<T, BinaryNumberLiteralKind>(name ?? "binary number", NumberStyles.BinaryNumber);
            #endif

            throw new ArgumentException(
                $"The number kind {kind} are not supported on integer numeric types.",
                nameof(kind));
        }

        throw new InvalidOperationException($"The specified type {typeof(T)} are not supported.");
    }

    private static int TryParseNumber<TKind>(ref char span, int length, int p)
    {
        var s = MemoryMarshal.CreateReadOnlySpan(ref span, length);
        if ((uint)p >= (uint)s.Length)
            return 0;

        if (typeof(TKind) == typeof(IntegerLiteralKind) || typeof(TKind) == typeof(FloatLiteralKind))
        {
            //
            // This line checks if s[p] is '+' or '-'.
            //
            // 1. Subtracting '+' maps:
            //    '+' (ASCII 43) -> 0
            //    '-' (ASCII 45) -> 2
            //
            // 2. Applying & -3 means: -3 is 11111...11111101:
            //    - bit #1 is zero, ensuring both 0 and 2 become 0 when we do & -3:
            //         0b00000...00000000 & 0b11111...11111101 = 0
            //         0b00000...00000010 & 0b11111...11111101 = 0
            //
            // 3. If (((s[p] - '+') & -3) == 0) is true,
            //    it means the character was either '+' or '-'.
            //

            if ((s[p] - '+' & -3) == 0)
            {
                p++;
                if ((uint)p >= (uint)s.Length)
                    return 0;
            }

            if (!IsAsciiDigit(s[p]))
                return 0;

            p++;
            while ((uint)p < (uint)s.Length && IsAsciiDigit(s[p]))
                p++;

            //
            // Float number pattern: \d+(\.\d+)?([Ee][-+]?\d+)?
            //

            if (typeof(TKind) == typeof(FloatLiteralKind) && (uint)p < s.Length)
            {
                if (s[p] == '.')
                {
                    var index = p + 1;

                    if ((uint)index < (uint)s.Length && IsAsciiDigit(s[index]))
                    {
                        index++;
                        while ((uint)index < (uint)s.Length && IsAsciiDigit(s[index]))
                            index++;

                        p = index;
                    }
                }

                if ((uint)p < (uint)s.Length && (s[p] | 0x20) == 'e')
                {
                    var index = p + 1;

                    if ((uint)index < (uint)s.Length)
                    {
                        // c == '+' || c == '-'
                        if ((s[index] - '+' & -3) == 0)
                            index++;

                        if ((uint)index < (uint)s.Length && IsAsciiDigit(s[index]))
                        {
                            index++;
                            while ((uint)index < (uint)s.Length && IsAsciiDigit(s[index]))
                                index++;

                            p = index;
                        }
                    }
                }
            }
        }

        if (typeof(TKind) == typeof(HexNumberLiteralKind))
        {
            if (!IsAsciiHexDigit(s[p]))
                return 0;

            while ((uint)p < (uint)s.Length && IsAsciiHexDigit(s[p]))
                p++;
        }

        if (typeof(TKind) == typeof(BinaryNumberLiteralKind))
        {
            if ((uint)s[p] - '0' > 1)
                return 0;

            while ((uint)p < (uint)s.Length && (uint)s[p] - '0' <= 1)
                p++;
        }

        return p;
    }

    #region Helper methods

    private static bool IsAsciiDigit(char c) =>
        IsBetween(c, '0', '9');

    private static bool IsAsciiHexDigit(char c)
    {
        #if NET7_0_OR_GREATER
        return char.IsAsciiHexDigit(c);
        #else
        return IsBetween(c, '0', '9') | IsBetween((uint)c | 0x20, 'a', 'f');
        #endif
    }

    private static bool IsBetween(uint c, char min, char max) =>
        c - min <= (uint)(max - min);

    #endregion

    #region Inner type: NumberLiteral

    /// <summary>
    /// Represents a parser for numeric literals based on the specified numeric type and format.
    /// </summary>
    /// <typeparam name="T">The numeric type to parse.</typeparam>
    /// <typeparam name="TKind">The kind of numeric literal being parsed.</typeparam>
    /// <param name="name">A name for the parser.</param>
    /// <param name="style">A bitwise combination of enumeration values that indicates the permitted format of the numeric literal.</param>
    private sealed class NumberLiteral<T, TKind>(string name, NumberStyles style) : Parser<T>(name) where TKind : struct where T : struct
    #if NET7_0_OR_GREATER
    , INumber<T>
    #endif
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, out T value)
        {
            Unsafe.SkipInit(out value);

            var bookmark = context.BookmarkPosition();
            var position = TryParseNumber<TKind>(
                ref MemoryMarshal.GetReference(context.Source),
                context.Source.Length,
                context.Position);

            if (position != 0)
            {
                context.Advance(position - context.Position);

                #if NET7_0_OR_GREATER
                if (T.TryParse(context.MatchedSegment, style, CultureInfo.InvariantCulture, out value))
                    return true;
                #else
                if (typeof(T) == typeof(sbyte))
                    if (sbyte.TryParse(context.MatchedSegment, style, CultureInfo.InvariantCulture, out Unsafe.As<T, sbyte>(ref value)))
                        return true;

                if (typeof(T) == typeof(byte))
                    if (byte.TryParse(context.MatchedSegment, style, CultureInfo.InvariantCulture, out Unsafe.As<T, byte>(ref value)))
                        return true;

                if (typeof(T) == typeof(short))
                    if (short.TryParse(context.MatchedSegment, style, CultureInfo.InvariantCulture, out Unsafe.As<T, short>(ref value)))
                        return true;

                if (typeof(T) == typeof(ushort))
                    if (ushort.TryParse(context.MatchedSegment, style, CultureInfo.InvariantCulture, out Unsafe.As<T, ushort>(ref value)))
                        return true;

                if (typeof(T) == typeof(int))
                    if (int.TryParse(context.MatchedSegment, style, CultureInfo.InvariantCulture, out Unsafe.As<T, int>(ref value)))
                        return true;

                if (typeof(T) == typeof(uint))
                    if (uint.TryParse(context.MatchedSegment, style, CultureInfo.InvariantCulture, out Unsafe.As<T, uint>(ref value)))
                        return true;

                if (typeof(T) == typeof(nint))
                    if (nint.TryParse(context.MatchedSegment, style, CultureInfo.InvariantCulture, out Unsafe.As<T, nint>(ref value)))
                        return true;

                if (typeof(T) == typeof(nuint))
                    if (nuint.TryParse(context.MatchedSegment, style, CultureInfo.InvariantCulture, out Unsafe.As<T, nuint>(ref value)))
                        return true;

                if (typeof(T) == typeof(long))
                    if (long.TryParse(context.MatchedSegment, style, CultureInfo.InvariantCulture, out Unsafe.As<T, long>(ref value)))
                        return true;

                if (typeof(T) == typeof(ulong))
                    if (ulong.TryParse(context.MatchedSegment, style, CultureInfo.InvariantCulture, out Unsafe.As<T, ulong>(ref value)))
                        return true;

                if (typeof(T) == typeof(BigInteger))
                    if (BigInteger.TryParse(context.MatchedSegment, style, CultureInfo.InvariantCulture, out Unsafe.As<T, BigInteger>(ref value)))
                        return true;

                if (typeof(T) == typeof(double))
                    if (double.TryParse(context.MatchedSegment, style, CultureInfo.InvariantCulture, out Unsafe.As<T, double>(ref value)))
                        return true;

                if (typeof(T) == typeof(float))
                    if (float.TryParse(context.MatchedSegment, style, CultureInfo.InvariantCulture, out Unsafe.As<T, float>(ref value)))
                        return true;

                if (typeof(T) == typeof(decimal))
                    if (decimal.TryParse(context.MatchedSegment, style, CultureInfo.InvariantCulture, out Unsafe.As<T, decimal>(ref value)))
                        return true;

                if (typeof(T) == typeof(Half))
                    if (Half.TryParse(context.MatchedSegment, style, CultureInfo.InvariantCulture, out Unsafe.As<T, Half>(ref value)))
                        return true;
                #endif
            }

            context.RestorePosition(bookmark);
            context.AddError(Name);
            value = default;
            return false;
        }
    }

    #endregion

    /// <summary>
    /// A marker struct used to indicate the kind of number literal being parsed as an integer.
    /// </summary>
    private struct IntegerLiteralKind;

    /// <summary>
    /// A marker struct used to indicate the kind of number literal being parsed as a hexadecimal integer number.
    /// </summary>
    private struct HexNumberLiteralKind;

    /// <summary>
    /// A marker struct used to indicate the kind of number literal being parsed as a binary integer number.
    /// </summary>
    private struct BinaryNumberLiteralKind;

    /// <summary>
    /// A marker struct used to indicate the kind of number literal being parsed as a floating-point number.
    /// </summary>
    private struct FloatLiteralKind;
}
