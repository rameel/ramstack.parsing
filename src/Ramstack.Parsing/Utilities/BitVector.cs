namespace Ramstack.Parsing.Utilities;

/// <summary>
/// Represents a bit vector structure.
/// </summary>
internal readonly struct BitVector
{
    private readonly uint[] _values;

    /// <summary>
    /// Gets the total number of bits that this <see cref="BitVector"/> instance can represent.
    /// </summary>
    public int Length => _values.Length * 32;

    /// <summary>
    /// Initializes a new instance of the <see cref="BitVector"/> structure,
    /// capable of storing <paramref name="length"/> bits (from index 0 to <paramref name="length"/> - 1).
    /// </summary>
    /// <param name="length">The total number of bits this vector should be able to store.
    /// For example, if you pass <c>101</c>, you can set and check bits from index <c>0</c> up to <c>100</c>.</param>
    public BitVector(int length)
    {
        Argument.ThrowIfNegative(length);
        _values = new uint[(length + 31) >>> 5];
    }

    /// <summary>
    /// Sets the bit at the specified position to <c>1</c>.
    /// </summary>
    /// <param name="position">The bit position to set.</param>
    /// <remarks>
    /// Ensure that <paramref name="position"/> is within the maximum allowed bit position
    /// defined when creating this <see cref="BitVector"/>.
    /// </remarks>
    public void Set(int position)
    {
        var index = (uint)position >> 5;
        var value = 1u << position;
        _values[index] |= value;
    }

    /// <summary>
    /// Checks whether the bit at the specified position is set to <c>1</c>.
    /// </summary>
    /// <param name="position">The bit position to check.</param>
    /// <returns>
    /// <see langword="true" /> if the specified bit is set; otherwise, <see langword="false" />.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBitSet(int position)
    {
        var index = (uint)position >> 5;
        var array = _values;

        if (index < (uint)array.Length)
        {
            return (Unsafe.Add(
                ref MemoryMarshal.GetArrayDataReference(array),
                index) & (1u << position)) != 0;
        }

        return false;
    }

    /// <summary>
    /// Returns a <see cref="CharClass"/> object representing the internal bitset,
    /// applying the specified offset to each character value.
    /// </summary>
    /// <param name="offset">The offset to apply to the character values. Must be non-negative.</param>
    /// <returns>
    /// A <see cref="CharClass"/> object representing the characters
    /// indicated by the set bits in the internal representation.
    /// </returns>
    public CharClass GetCharClass(int offset)
    {
        Argument.ThrowIfNegative(offset);

        var count = Length;
        var array = new ArrayBuilder<CharClassRange>(count);

        for (var i = 0; i < count; i++)
        {
            if (IsBitSet(i))
            {
                var range = CharClassRange.Create((char)(i + offset));
                array.TryAdd(range);
            }
        }

        return new CharClass(array.ToArray());
    }
}
