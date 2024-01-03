namespace Ramstack.Parsing;

/// <summary>
/// Represents the matched segment of the source text.
/// </summary>
[SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
public readonly ref struct Match
{
    #if NET7_0_OR_GREATER
    private readonly ref char _source;
    #else
    private readonly ReadOnlySpan<char> _source;
    #endif

    private readonly int _index;
    private readonly int _length;

    /// <summary>
    /// Gets the zero-based position of the matched segment in the source.
    /// </summary>
    public int Index => _index;

    /// <summary>
    /// Gets the number of characters that were matched.
    /// </summary>
    public int Length => _length;

    /// <summary>
    /// Gets a read-only span of the matched characters.
    /// </summary>
    public ReadOnlySpan<char> Value => this;

    /// <summary>
    /// Initializes a new instance of the <see cref="Match"/> structure.
    /// </summary>
    /// <param name="source">The source containing the matched text.</param>
    /// <param name="index">The zero-based position of the matched segment.</param>
    /// <param name="length">The number of matched characters.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Match(ReadOnlySpan<char> source, int index, int length)
    {
        #if NET7_0_OR_GREATER
        _source = ref MemoryMarshal.GetReference(source);
        #else
        _source = source;
        #endif

        _index = index;
        _length = length;
    }

    /// <summary>
    /// Deconstructs the current <see cref="Match"/> into its individual components.
    /// </summary>
    /// <param name="index">When this method returns, contains the zero-based position of the matched segment.</param>
    /// <param name="length">When this method returns, contains the number of matched characters.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void Deconstruct(out int index, out int length)
    {
        index = Index;
        length = Length;
    }

    /// <inheritdoc />
    public override string ToString() =>
        new string(Value);

    /// <summary>
    /// Implicit conversion from a <see cref="Match"/> to a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <param name="match">The <see cref="Match"/> value.</param>
    /// <returns>
    /// A <see cref="ReadOnlySpan{T}"/> representing the matched segment.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<char>(Match match)
    {
        #if NET7_0_OR_GREATER
        return MemoryMarshal.CreateReadOnlySpan(
            ref Unsafe.Add(ref match._source, (nint)(uint)match._index),
            match._length);
        #else
        return MemoryMarshal.CreateReadOnlySpan(
            ref Unsafe.Add(
                ref MemoryMarshal.GetReference(match._source),
                (nint)(uint)match._index),
            match._length);
        #endif
    }

    /// <summary>
    /// Implicit conversion from a <see cref="Match"/> to a <see cref="ValueTuple{T1,T2}"/>.
    /// This tuple contains the zero-based index and the length of the matched segment.
    /// </summary>
    /// <param name="match">The <see cref="Match"/> value.</param>
    /// <returns>
    /// A <see cref="ValueTuple{T1,T2}"/> containing the index and length of the matched segment.
    /// </returns>
    public static implicit operator (int Index, int Length)(Match match) =>
        (match.Index, match.Length);
}
