namespace Ramstack.Parsing;

partial class Literal
{
    /// <summary>
    /// A lookup table for unescaping characters. Each entry in the table corresponds
    /// to a character code, and the value indicates the unescaped representation of that character.
    /// A value of <c>-1</c> indicates that the character is not escapable
    /// or does not require unescaping.
    /// </summary>
    /// <remarks>
    /// The table is indexed by character codes (e.g., ASCII values), and the values
    /// represent the corresponding unescaped characters.
    /// For example: Character code <c>110</c> (<c>n</c>) maps to <c>10</c>,
    /// indicating it is unescaped as a newline (<c>\n</c>).
    /// </remarks>
    internal static ReadOnlySpan<sbyte> UnescapeTable =>
    [
        /* 000   */ -1,
        /* 001   */ -1,
        /* 002   */ -1,
        /* 003   */ -1,
        /* 004   */ -1,
        /* 005   */ -1,
        /* 006   */ -1,
        /* 007   */ -1,
        /* 008   */ -1,
        /* 009   */ -1,
        /* 010   */ -1,
        /* 011   */ -1,
        /* 012   */ -1,
        /* 013   */ -1,
        /* 014   */ -1,
        /* 015   */ -1,
        /* 016   */ -1,
        /* 017   */ -1,
        /* 018   */ -1,
        /* 019   */ -1,
        /* 020   */ -1,
        /* 021   */ -1,
        /* 022   */ -1,
        /* 023   */ -1,
        /* 024   */ -1,
        /* 025   */ -1,
        /* 026   */ -1,
        /* 027   */ -1,
        /* 028   */ -1,
        /* 029   */ -1,
        /* 030   */ -1,
        /* 031   */ -1,
        /* 032   */ -1,
        /* 033 ! */ -1,
        /* 034 " */ 34,
        /* 035 # */ -1,
        /* 036 $ */ -1,
        /* 037 % */ -1,
        /* 038 & */ -1,
        /* 039 ' */ 39,
        /* 040 ( */ -1,
        /* 041 ) */ -1,
        /* 042 * */ -1,
        /* 043 + */ -1,
        /* 044 , */ -1,
        /* 045 - */ -1,
        /* 046 . */ -1,
        /* 047 / */ 47,
        /* 048 0 */ 00,
        /* 049 1 */ -1,
        /* 050 2 */ -1,
        /* 051 3 */ -1,
        /* 052 4 */ -1,
        /* 053 5 */ -1,
        /* 054 6 */ -1,
        /* 055 7 */ -1,
        /* 056 8 */ -1,
        /* 057 9 */ -1,
        /* 058 : */ -1,
        /* 059 ; */ -1,
        /* 060 < */ -1,
        /* 061 = */ -1,
        /* 062 > */ -1,
        /* 063 ? */ -1,
        /* 064 @ */ -1,
        /* 065 A */ -1,
        /* 066 B */ -1,
        /* 067 C */ -1,
        /* 068 D */ -1,
        /* 069 E */ -1,
        /* 070 F */ -1,
        /* 071 G */ -1,
        /* 072 H */ -1,
        /* 073 I */ -1,
        /* 074 J */ -1,
        /* 075 K */ -1,
        /* 076 L */ -1,
        /* 077 M */ -1,
        /* 078 N */ -1,
        /* 079 O */ -1,
        /* 080 P */ -1,
        /* 081 Q */ -1,
        /* 082 R */ -1,
        /* 083 S */ -1,
        /* 084 T */ -1,
        /* 085 U */ -1,
        /* 086 V */ -1,
        /* 087 W */ -1,
        /* 088 X */ -1,
        /* 089 Y */ -1,
        /* 090 Z */ -1,
        /* 091 [ */ -1,
        /* 092 \ */ 92,
        /* 093 ] */ -1,
        /* 094 ^ */ -1,
        /* 095 _ */ -1,
        /* 096 ` */ -1,
        /* 097 a */ 07,
        /* 098 b */ 08,
        /* 099 c */ -1,
        /* 100 d */ -1,
        /* 101 e */ 27,
        /* 102 f */ 12,
        /* 103 g */ -1,
        /* 104 h */ -1,
        /* 105 i */ -1,
        /* 106 j */ -1,
        /* 107 k */ -1,
        /* 108 l */ -1,
        /* 109 m */ -1,
        /* 110 n */ 10,
        /* 111 o */ -1,
        /* 112 p */ -1,
        /* 113 q */ -1,
        /* 114 r */ 13,
        /* 115 s */ -1,
        /* 116 t */ 09,
        /* 117 u */ -1,
        /* 118 v */ 11,
        /* 119 w */ -1,
        /* 120 x */ -1,
        /* 121 y */ -1,
        /* 122 z */ -1,
        /* 123 { */ -1,
        /* 124 | */ -1,
        /* 125 } */ -1,
        /* 126 ~ */ -1
    ];

    /// <summary>
    /// Gets a parser that matches the escape sequence.
    /// </summary>
    public static Parser<char> EscapeSequence { get; } = new EscapeSequenceParser<char>();

    #region Inner type: EscapeSequenceParser

    /// <summary>
    /// Represents a parser that matches escape sequences, where each sequence is denoted by a backslash ('\').
    /// </summary>
    private sealed class EscapeSequenceParser<T>() : Parser<T>("escape sequence") where T : struct
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, out T value)
        {
            var s = context.Source;
            var p = context.Position;

            if ((uint)p < (uint)s.Length && s[p] == '\\')
            {
                if ((uint)(p + 1) < (uint)s.Length && s[p + 1] < UnescapeTable.Length)
                {
                    var c = UnescapeTable[s[p + 1]];
                    if (c >= 0)
                    {
                        if (typeof(T) == typeof(char))
                            value = (T)(object)(char)c;
                        else
                            value = default;

                        context.Advance(2);
                        return true;
                    }
                }
            }

            value = default;
            context.AddError(Name);
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new EscapeSequenceParser<T> { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new EscapeSequenceParser<Unit> { Name = Name };
    }

    #endregion
}
