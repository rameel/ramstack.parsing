using System.Buffers;

namespace Ramstack.Parsing.Utilities;

/// <summary>
/// Represents a mutable buffer for building strings.
/// </summary>
internal struct StringBuffer : IDisposable
{
    private char[] _chars;
    private int _count;

    /// <summary>
    /// Gets the current length of the string buffer.
    /// </summary>
    public int Length => _count;

    /// <summary>
    /// Initializes a new instance of the <see cref="StringBuffer"/> structure.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StringBuffer() =>
        _chars = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="StringBuffer"/> structure.
    /// </summary>
    /// <param name="capacity">The initial capacity of the buffer.</param>
    public StringBuffer(int capacity) =>
        _chars = ArrayPool<char>.Shared.Rent(capacity);

    /// <summary>
    /// Appends a single character to the string buffer.
    /// </summary>
    /// <param name="c">The character to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c)
    {
        var chars = _chars;
        var count = _count;

        if ((uint)count < (uint)chars.Length)
        {
            chars[count] = c;
        }
        else
        {
            _chars = AppendSlow(chars, count, c);
        }

        _count = count + 1;
    }

    /// <summary>
    /// Appends a span of characters to the string buffer.
    /// </summary>
    /// <param name="text">The span of characters to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(ReadOnlySpan<char> text)
    {
        if (text.Length != 0)
        {
            var chars = _chars;
            var count = _count;

            if ((uint)(count + text.Length) <= (uint)chars.Length)
            {
                ref var p = ref Unsafe.Add(
                    ref MemoryMarshal.GetArrayDataReference(chars),
                    (nint)(uint)count);

                if (text.Length <= 2)
                {
                    p = text[0];
                    if (text.Length == 2)
                        Unsafe.Add(ref p, 1) = text[1];
                }
                else
                {
                    text.TryCopyTo(MemoryMarshal.CreateSpan(ref p, text.Length));
                }
            }
            else
            {
                _chars = AppendSlow(chars, count, ref MemoryMarshal.GetReference(text), text.Length);
            }

            _count = count + text.Length;
        }
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return ToStringImpl(_chars, _count);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static string ToStringImpl(char[] buffer, int length)
        {
            var result = new string(buffer.AsSpan(0, length));
            if (buffer.Length != 0)
                ArrayPool<char>.Shared.Return(buffer);

            return result;
        }
    }

    /// <summary>
    /// Releases resources used by the string buffer,
    /// returning its internal buffer to the pool if applicable.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        if (_count != 0)
            DisposeImpl(_chars);

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void DisposeImpl(char[] buffer)
        {
            if (buffer.Length != 0)
                ArrayPool<char>.Shared.Return(buffer);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static char[] AppendSlow(char[] rented, int index, char c)
    {
        var capacity = rented.Length != 0
            ? rented.Length * 2
            : 128;

        var buffer = ArrayPool<char>.Shared.Rent(capacity);
        _ = buffer.Length;

        rented.AsSpan().TryCopyTo(buffer);
        if ((uint)index < (uint)buffer.Length)
            buffer[index] = c;

        if (rented.Length != 0)
            ArrayPool<char>.Shared.Return(rented);

        return buffer;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static char[] AppendSlow(char[] rented, int index, ref char s, int length)
    {
        var required = checked(rented.Length + length);
        var capacity = rented.Length != 0
            ? rented.Length * 2
            : 128;

        if (capacity < required)
            capacity = required;

        var buffer = ArrayPool<char>.Shared.Rent(capacity);
        _ = buffer.Length;

        rented.AsSpan(0, index).TryCopyTo(buffer);
        MemoryMarshal
            .CreateSpan(ref s, length)
            .TryCopyTo(buffer.AsSpan(index));

        if (rented.Length != 0)
            ArrayPool<char>.Shared.Return(rented);

        return buffer;
    }
}
