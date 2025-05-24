namespace Ramstack.Parsing;

/// <summary>
/// Represents the context in which parsing operations are performed.
/// </summary>
public ref struct ParseContext
{
    private readonly ReadOnlySpan<char> _source;
    private (int Index, int Length) _match;
    private ErrorSet _diagnostics;
    private int _position;

    /// <summary>
    /// Gets or sets the diagnostic state of this parsing context.
    /// </summary>
    public DiagnosticState DiagnosticState { get; set; }

    /// <summary>
    /// Gets the source text being parsed.
    /// </summary>
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    public readonly ReadOnlySpan<char> Source => _source;

    /// <summary>
    /// Gets the portion of the source text that has not yet been parsed.
    /// </summary>
    public readonly ReadOnlySpan<char> Remaining
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var source = _source;
            var length = source.Length - _position;

            Debug.Assert(_source[_position..].Length == length);

            ref var reference = ref Unsafe.Add(
                ref MemoryMarshal.GetReference(source),
                (nint)(uint)_position);

            return MemoryMarshal.CreateReadOnlySpan(ref reference, length);
        }
    }

    /// <summary>
    /// Gets the current position in the source text.
    /// </summary>
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWithPrivateSetter")]
    public readonly int Position => _position;

    /// <summary>
    /// Gets the most recently matched segment of the source text.
    /// </summary>
    public readonly Match MatchedSegment => new Match(_source, _match.Index, _match.Length);

    /// <summary>
    /// Initializes a new instance of the <see cref="ParseContext"/> structure.
    /// </summary>
    /// <param name="source">The source text to parse.</param>
    public ParseContext(ReadOnlySpan<char> source)
    {
        _source = source;
        _diagnostics = new ErrorSet();
    }

    /// <summary>
    /// Advances the current position by the specified number of characters.
    /// </summary>
    /// <param name="count">The number of characters to advance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        var pos = _position;
        var len = _source.Length;

        if ((uint)count > (uint)(len - pos))
            count = len - pos;

        _match.Index = pos;
        _match.Length = count;
        _position = pos + count;
    }

    /// <summary>
    /// Sets the most recently matched segment in the source text, spanning from the specified bookmark position to the current position.
    /// </summary>
    /// <param name="bookmark">The bookmark that marks the start position of the matched segment.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetMatched(PositionBookmark bookmark)
    {
        var length = Position - bookmark.Position;
        _match.Index = bookmark.Position;
        _match.Length = length;
    }

    /// <summary>
    /// Sets the most recently matched segment in the source text.
    /// </summary>
    /// <param name="index">The zero-based start position of the matched segment.</param>
    /// <param name="length">The length of the matched segment.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetMatched(int index, int length)
    {
        Debug.Assert(_source.Slice(index, length).Length == length);

        var len = _source.Length;

        if ((uint)index > (uint)len)
        {
            (index, length) = (len, 0);
        }
        else if ((uint)length > (uint)(len - index))
        {
            length = len - index;
        }

        _match.Index = index;
        _match.Length = length;
    }

    /// <summary>
    /// Reports a missing expected sequence or rule at the current position.
    /// The expected sequence or rule is added only if diagnostics are enabled.
    /// </summary>
    /// <param name="expected">The expected sequence or rule.</param>
    public void ReportExpected(string? expected)
    {
        if (DiagnosticState == DiagnosticState.Normal && expected is not null)
            _diagnostics.ReportExpected(_position, expected);
    }

    /// <summary>
    /// Reports a missing expected sequence or rule at the specified position.
    /// The expected sequence or rule is added only if diagnostics are enabled.
    /// </summary>
    /// <param name="index">The position in the source text where the error occurred.</param>
    /// <param name="expected">The expected sequence or rule.</param>
    internal void ReportExpected(int index, string? expected)
    {
        if (DiagnosticState == DiagnosticState.Normal && expected is not null)
            _diagnostics.ReportExpected(index, expected);
    }

    /// <summary>
    /// Reports multiple missing expected sequences or rules at the current position.
    /// The expected sequences or rules are added only if diagnostics are enabled.
    /// </summary>
    /// <param name="expected">An array of expected sequences or rules.</param>
    public void ReportExpected(string[] expected)
    {
        if (DiagnosticState == DiagnosticState.Normal)
            _diagnostics.ReportExpected(_position, expected);
    }

    /// <summary>
    /// Suppresses diagnostics and returns the previous diagnostic state.
    /// </summary>
    /// <returns>
    /// The previous <see cref="DiagnosticState"/>, which can be used to restore
    /// the diagnostic state later via <see cref="RestoreDiagnosticState"/>.
    /// </returns>
    public DiagnosticState SuppressDiagnostics()
    {
        var state = DiagnosticState;
        DiagnosticState = DiagnosticState.Suppressed;
        return state;
    }

    /// <summary>
    /// Conditionally suppresses diagnostic messages if the parser is named.
    /// If <paramref name="name"/> is not <see langword="null"/>, diagnostic messages
    /// will be suppressed until <see cref="RestoreDiagnosticState"/> is called.
    /// </summary>
    /// <param name="name">The name of the parser. If this value is not <see langword="null"/>,
    /// diagnostics will be suppressed for its sub-parsers.</param>
    /// <returns>
    /// The previous <see cref="DiagnosticState"/>, which can be used to restore
    /// the diagnostic state later via <see cref="RestoreDiagnosticState"/>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method is useful in scenarios where multiple sub-parsers
    /// could generate redundant diagnostic messages, yet the top-level (named)
    /// parser only needs to present a single, consolidated error message.
    /// </para>
    ///
    /// <para>
    /// Consider, for example, a grammar for parsing a float value:
    /// <code>
    /// // ('+' / '-')? [0-9]+  (.[0-9]+)?
    /// // [   sign   ] [int ]  [fraction]
    ///
    /// var parser = Seq(
    ///     Set("+-").Optional(),       // sign
    ///     Set("0-9").OneOrMore(),     // integer
    ///     Seq(
    ///         L('.'),
    ///         Set("0-9").OneOrMore()
    ///     ).Optional()                // fraction
    /// ).As("float");
    /// </code>
    ///
    /// We can visualize this parser, where <c>float</c> is the top-level
    /// named parser and <c>sign</c>, <c>integer</c>, and <c>fraction</c> are its sub-parsers:
    /// <code>
    /// float
    /// ├─ sign (optional)
    /// │  └─ [ '+' | '-' ]
    /// ├─ integer
    /// │  └─ [0-9]+
    /// └─ fraction (optional)
    ///    ├─ '.'
    ///    └─ [0-9]+
    /// </code>
    /// When a user input is missing digits or has a misplaced decimal point,
    /// each of these sub-parsers could produce an individual diagnostic:
    /// "Expected digit" or "Expected '.'". However, all these messages are eventually
    /// discarded in favor of a single top-level error from the <c>float</c> parser.<br/><br/>
    ///
    /// By suppressing sub-parsers diagnostics and only producing a concise top-level message
    /// like "(1:5) Expected float" we avoid unnecessary memory allocations and generally
    /// do less work, which ultimately improves performance.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var previousState = context.SuppressDiagnosticsIfNamed(Name);
    /// // ... parsing logic ...
    /// context.RestoreDiagnostics(previousState);
    /// </code>
    /// </example>
    public DiagnosticState SuppressDiagnosticsIfNamed(string? name)
    {
        var state = DiagnosticState;
        if (name is not null)
            DiagnosticState = DiagnosticState.Suppressed;
        return state;
    }

    /// <summary>
    /// Restores the diagnostic state to the specified value.
    /// </summary>
    /// <param name="state">The <see cref="DiagnosticState"/> to restore.</param>
    public void RestoreDiagnosticState(DiagnosticState state) =>
        DiagnosticState = state;

    /// <summary>
    /// Returns a bookmark for the current parser position.
    /// </summary>
    /// <returns>
    /// A <see cref="PositionBookmark"/> representing the current position.
    /// </returns>
    public readonly PositionBookmark BookmarkPosition() =>
        new PositionBookmark(_position);

    /// <summary>
    /// Restores the parser position using the specified bookmark.
    /// </summary>
    /// <param name="bookmark">The bookmark to restore from.</param>
    public void RestorePosition(PositionBookmark bookmark)
    {
        _position = bookmark.Position;
        _match.Length = 0;
    }

    /// <inheritdoc />
    public readonly override string ToString() =>
        GenerateErrorMessage(_source, _diagnostics.Index, _diagnostics.ToString());

    /// <summary>
    /// Generates a formatted error message that includes line and column information.
    /// </summary>
    /// <param name="source">The source text.</param>
    /// <param name="position">The position in the source text where the error occurred.</param>
    /// <param name="message">The error message.</param>
    /// <returns>
    /// A formatted error message that includes line and column details.
    /// </returns>
    internal static string GenerateErrorMessage(ReadOnlySpan<char> source, int position, string message)
    {
        var (line, column) = TextHelper.GetLineColumn(source, position);
        return $"({line}:{column}) {message}";
    }
}
