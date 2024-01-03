namespace Ramstack.Parsing.Utilities;

/// <summary>
/// Represents a bit vector structure.
/// </summary>
internal readonly unsafe struct BitVector<T> where T : unmanaged
{
    // ReSharper disable once UnassignedReadonlyField
    private readonly T _storage;

    /// <summary>
    /// Gets the total number of bits that this <see cref="BitVector{T}"/> instance can represent.
    /// </summary>
    public static int Length => sizeof(T) * 8;

    /// <summary>
    /// Sets the bit at the specified position (0..N-1) to <c>1</c>.
    /// </summary>
    /// <param name="position">The bit position to set. Must be within the range [0..N-1].</param>
    public void Set(int position)
    {
        Argument.ThrowIfGreaterThanOrEqual((uint)position, (uint)Length);

        var index = (uint)position >> 5;
        var value = 1u << position;

        Unsafe.Add(
            ref Unsafe.As<T, uint>(ref Unsafe.AsRef(in _storage)),
            index) |= value;
    }

    /// <summary>
    /// Determines whether the bit at the specified position is set to <c>1</c>.
    /// </summary>
    /// <param name="position">The bit position to check.</param>
    /// <returns>
    /// <see langword="true" /> if the specified bit is set; otherwise, <see langword="false" />.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsBitSet(int position)
    {
        if ((uint)position >= (uint)sizeof(T) * 8)
            return false;

        var index = sizeof(T) != 32/8 ? (uint)position >> 5 : 0;
        var value = Unsafe.Add(
            ref Unsafe.As<T, uint>(ref Unsafe.AsRef(in _storage)),
            index) & (1u << position);

        return value != 0;
    }

    /// <summary>
    /// Returns an array of the <see cref="CharClassRange"/> object representing the internal bitset,
    /// applying the specified offset to each character value.
    /// </summary>
    /// <param name="offset">The offset to apply to the character values. Must be non-negative.</param>
    /// <returns>
    /// An array of the <see cref="CharClassRange"/> object representing the characters indicated by the set bits
    /// in the internal representation.
    /// </returns>
    public CharClassRange[] ToCharClassRanges(int offset)
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

        return array.ToArray();
    }
}

[StructLayout(LayoutKind.Explicit, Size = 128/8)]
internal readonly struct Block128Bit;

[StructLayout(LayoutKind.Explicit, Size = 256/8)]
internal readonly struct Block256Bit;

[StructLayout(LayoutKind.Explicit, Size = 512/8)]
internal readonly struct Block512Bit;
