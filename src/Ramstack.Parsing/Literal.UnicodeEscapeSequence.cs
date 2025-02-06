namespace Ramstack.Parsing;

partial class Literal
{
    /// <summary>
    /// A lookup table for converting hexadecimal characters to their corresponding
    /// integer values. Each entry in the table corresponds to a character code,
    /// and the value indicates the integer value of the hexadecimal digit (0-15).
    /// A value of <c>-1</c> indicates that the character is not a valid hexadecimal digit.
    /// </summary>
    internal static ReadOnlySpan<sbyte> HexTable =>
    [
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
         0,  1,  2,  3,  4,  5,  6,  7,  8,  9, -1, -1, -1, -1, -1, -1,
        -1, 10, 11, 12, 13, 14, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, 10, 11, 12, 13, 14, 15, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1
    ];

    private static Parser<char>? _unicodeEscapeSequence;

    /// <summary>
    /// Gets a parser that matches a Unicode escape sequence in the format <c>\uXXXX</c>.
    /// </summary>
    /// <remarks>
    /// The parser expects a Unicode escape sequence in the format <c>\uXXXX</c> where <c>XXXX</c>
    /// is a 4-digit hexadecimal number.
    /// For example: <c>\u0041</c> represents the character <c>'A'</c>.
    /// </remarks>
    public static Parser<char> UnicodeEscapeSequence => _unicodeEscapeSequence ??= new UnicodeEscapeSequenceParser<char>();

    #region Inner type: UnicodeEscapeSequenceParser

    /// <summary>
    /// Represents a parser that converts unicode escape sequences to their corresponding characters.
    /// </summary>
    private sealed class UnicodeEscapeSequenceParser<T>() : Parser<T>("unicode escape") where T : struct
    {
        /// <inheritdoc/>
        public override bool TryParse(ref ParseContext context, out T value)
        {
            var s = context.Remaining;

            if ((uint)s.Length > 5 && s.StartsWith("\\u"))
            {
                var v1 = (nint)s[2];
                var v2 = (nint)s[3];
                var v3 = (nint)s[4];
                var v4 = (nint)s[5];

                if ((uint)(v1 | v2 | v3 | v4) <= 127)
                {
                    ref var table = ref MemoryMarshal.GetReference(HexTable);

                    var r1 = (int)Unsafe.Add(ref table, v1);
                    var r2 = (int)Unsafe.Add(ref table, v2);
                    var r3 = (int)Unsafe.Add(ref table, v3);
                    var r4 = (int)Unsafe.Add(ref table, v4);

                    r1 <<= 12;
                    r2 <<= 08;
                    r3 <<= 04;
                    var ch = r1 | r2 | r3 | r4;

                    if ((uint)ch <= 0xFFFF)
                    {
                        if (typeof(T) == typeof(char))
                            value = (T)(object)(char)ch;
                        else
                            value = default;

                        context.Advance(6);
                        return true;
                    }
                }
            }

            context.AddError(Name);
            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new UnicodeEscapeSequenceParser<T> { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new UnicodeEscapeSequenceParser<Unit> { Name = Name };
    }

    #endregion
}
