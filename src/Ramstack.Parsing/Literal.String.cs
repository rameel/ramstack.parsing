namespace Ramstack.Parsing;

partial class Literal
{
    #if NET8_0_OR_GREATER
    private static System.Buffers.SearchValues<char> SingleQuoteStopChars { get; } = System.Buffers.SearchValues.Create(['\'', '\\', '\r', '\n']);
    private static System.Buffers.SearchValues<char> DoubleQuoteStopChars { get; } = System.Buffers.SearchValues.Create(['\"', '\\', '\r', '\n']);
    #elif NET7_0
    private static ReadOnlySpan<char> SingleQuoteStopChars => ['\'', '\\', '\r', '\n'];
    private static ReadOnlySpan<char> DoubleQuoteStopChars => ['\"', '\\', '\r', '\n'];
    #else
    private static char[] SingleQuoteStopChars { get; } = ['\'', '\\', '\r', '\n'];
    private static char[] DoubleQuoteStopChars { get; } = ['\"', '\\', '\r', '\n'];
    #endif

    private static Parser<string>? _doubleQuotedString;
    private static Parser<string>? _singleQuotedString;

    /// <summary>
    /// Gets a parser that matches a string enclosed in double quotes.
    /// </summary>
    public static Parser<string> DoubleQuotedString => _doubleQuotedString ??= new QuotedStringParser<DoubleQuoteString, string>("double-quoted string");

    /// <summary>
    /// Gets a parser that matches a string enclosed in single quotes.
    /// </summary>
    public static Parser<string> SingleQuotedString => _singleQuotedString ??= new QuotedStringParser<SingleQuoteString, string>("single-quoted string");

    #region Inner type: QuotedStringParser

    /// <summary>
    /// Represents a parser that matches a string enclosed in double or single quotes.
    /// </summary>
    /// <typeparam name="TStringKind">The type of the string kind to parse.</typeparam>
    /// <typeparam name="TResult">The type of the value produced by the parser.</typeparam>
    /// <param name="name">The name to assign to the parser.</param>
    private sealed class QuotedStringParser<TStringKind, TResult>(string? name) : Parser<TResult>(name) where TStringKind : struct
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out TResult? value)
        {
            var count = TryParseImpl(context.Remaining, out value);
            if (count == 0)
            {
                context.AddError(Name);
            }
            else
            {
                context.Advance(count);
            }

            return count != 0;
        }

        /// <inheritdoc />
        protected internal override Parser<TResult> ToNamedParser(string? name) =>
            new QuotedStringParser<TStringKind, TResult>(name);

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new QuotedStringParser<TStringKind, Unit>(Name);

        private static int TryParseImpl(ReadOnlySpan<char> s, out TResult? value)
        {
            var quote = typeof(TStringKind) == typeof(DoubleQuoteString)
                ? '"' : '\'';

            value = default;

            var result = new StringBuffer();
            var offset = 0;

            if (s.Length == 0 || s[0] != quote)
                goto FAIL;

            s = s.Slice(1);

            while (true)
            {
                var index = typeof(TStringKind) == typeof(SingleQuoteString)
                    ? s.IndexOfAny(SingleQuoteStopChars)
                    : s.IndexOfAny(DoubleQuoteStopChars);

                if ((uint)index >= (uint)s.Length)
                    goto FAIL;

                if (s[index] == quote)
                {
                    if (typeof(TResult) != typeof(Unit))
                    {
                        if (result.Length == 0)
                        {
                            Unsafe.As<TResult, string>(ref value!) = new string(s[..index]);
                        }
                        else
                        {
                            result.Append(s[..index]);
                            Unsafe.As<TResult, string>(ref value!) = result.ToString();
                        }
                    }

                    return offset + index + 2;
                }

                if (s[index] is '\n' or '\r')
                    goto FAIL;

                if ((uint)index + 1 >= (uint)s.Length)
                    goto FAIL;

                if (typeof(TResult) != typeof(Unit))
                    if (index != 0)
                        result.Append(s[..index]);

                char ch;

                var escaped = s[index + 1];
                if (escaped != 'u')
                {
                    if (escaped >= (uint)UnescapeTable.Length)
                        goto FAIL;

                    var c = UnescapeTable[escaped];
                    if (c < 0)
                        goto FAIL;

                    ch = (char)c;
                }
                else
                {
                    if (index + 5 >= s.Length)
                        goto FAIL;

                    ref var r = ref Unsafe.AsRef(in s[index]);

                    var v1 = (nint)Unsafe.Add(ref r, 2);
                    var v2 = (nint)Unsafe.Add(ref r, 3);
                    var v3 = (nint)Unsafe.Add(ref r, 4);
                    var v4 = (nint)Unsafe.Add(ref r, 5);

                    if ((uint)(v1 | v2 | v3 | v4) > 127)
                        goto FAIL;

                    ref var table = ref MemoryMarshal.GetReference(HexTable);

                    var r1 = (int)Unsafe.Add(ref table, v1);
                    var r2 = (int)Unsafe.Add(ref table, v2);
                    var r3 = (int)Unsafe.Add(ref table, v3);
                    var r4 = (int)Unsafe.Add(ref table, v4);

                    r1 <<= 12;
                    r2 <<= 08;
                    r3 <<= 04;
                    var c = r1 | r2 | r3 | r4;

                    if ((uint)c > 0xFFFF)
                        goto FAIL;

                    ch = (char)c;
                    index += 4;
                }

                if (typeof(TResult) != typeof(Unit))
                    result.Append(ch);

                // Micro-optimization:
                // While bounds check here is technically redundant (no out-of-bounds access here),
                // this explicit check helps reduce code size by preventing the JIT from generating
                // CORINFO_HELP_RNGCHKFAIL for the Slice() call below
                if ((uint)index + 2 > (uint)s.Length)
                    goto FAIL;

                s = s.Slice(index + 2);
                offset += index + 2;
            }

            FAIL:
            if (typeof(TResult) != typeof(Unit))
                result.Dispose();

            return 0;
        }
    }

    #endregion

    /// <summary>
    /// A marker struct used to indicate a string literal enclosed in single quotes (' ').
    /// </summary>
    private struct SingleQuoteString;

    /// <summary>
    /// A marker struct used to indicate a string literal enclosed in double quotes (" ").
    /// </summary>
    private struct DoubleQuoteString;
}
