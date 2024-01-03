namespace Ramstack.Parsing.Collections;

/// <summary>
/// Represents an array builder for elements of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of elements stored in the array builder.</typeparam>
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(ArrayBuilderDebugView<>))]
internal struct ArrayBuilder<T>
{
    private T[] _array;
    private int _count;

    /// <summary>
    /// Gets the number of items contained in the <see cref="ArrayBuilder{T}"/>.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public readonly int Count => _count;

    /// <summary>
    /// Gets the underlying buffer.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public readonly T[] InnerBuffer => _array;

    /// <summary>
    /// Gets an item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to get.</param>
    public readonly ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var array = _array;
            if ((uint)index >= (uint)_count)
                ThrowHelper.ThrowArgumentOutOfRangeException();

            return ref Unsafe.Add(
                ref MemoryMarshal.GetArrayDataReference(array),
                (nint)(uint)index);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayBuilder{T}"/>.
    /// </summary>
    public ArrayBuilder()
    {
        _count = 0;
        _array = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayBuilder{T}"/> with the specified buffer capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity of the buffer.</param>
    public ArrayBuilder(int capacity)
    {
        _count = 0;
        _array = new T[capacity];
    }

    /// <summary>
    /// Adds an item to the underlying array, resizing it if necessary.
    /// </summary>
    /// <param name="item">The item to add.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        var array = _array;
        var count = _count;

        if ((uint)count < (uint)array.Length)
        {
            array[count] = item;
            _count = count + 1;
        }
        else
        {
            AddSlow(item);
        }
    }

    /// <summary>
    /// Attempts to add an item to the underlying array, without throwing an exception if there is no room.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <remarks>
    /// Use this method if you know there is enough space in the <see cref="ArrayBuilder{T}"/>
    /// for another item, and you are writing performance-sensitive code.
    /// </remarks>
    /// <returns>
    /// <see langword="true"/> if the item was added to the underlying array; otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryAdd(T item)
    {
        var array = _array;
        var count = _count;

        if ((uint)count < (uint)array.Length)
        {
            array[count] = item;
            _count = count + 1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Adds the elements of the specified array to the end of the underlying array.
    /// </summary>
    /// <param name="items">The array of elements to add.</param>
    public void AddRange(T[] items)
    {
        var array = _array;
        var count = _count;

        if (array.Length - count < items.Length)
            array = Grow(checked(count + items.Length));

        _count = count + items.Length;
        _ = array.Length;
        items.AsSpan().TryCopyTo(array.AsSpan(count));
    }

    /// <summary>
    /// Removes all elements from the <see cref="ArrayBuilder{T}" />.
    /// </summary>
    public void Clear()
    {
        // The array is not cleared (e.g., references or reference-containing data are not set to null)
        // intentionally for performance reasons. This type is designed to be used as a temporary object
        // and solely for building arrays. Therefore, there is no risk of memory leaks,
        // and clearing unused elements would be unnecessary.
        _count = 0;
    }

    /// <summary>
    /// Returns a <see cref="Span{T}"/> representing the written data of the current instance.
    /// </summary>
    /// <returns>
    /// A <see cref="Span{T}"/> that represents the data within the current instance.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T> AsSpan()
    {
        var array = _array;
        var count = _count;
        _ = array.Length;

        return MemoryMarshal.CreateSpan(
            ref MemoryMarshal.GetArrayDataReference(array),
            count);
    }

    /// <summary>
    /// Copies the elements of the <see cref="ArrayBuilder{T}"/> to a new array.
    /// </summary>
    /// <returns>
    /// An array containing copies of the elements of the <see cref="ArrayBuilder{T}"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T[] ToArray() =>
        AsSpan().ToArray();

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void AddSlow(T item)
    {
        var array = Grow(1);
        var count = _count;

        if ((uint)count < (uint)array.Length)
            array[count] = item;

        _count = count + 1;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private T[] Grow(int extra)
    {
        var array = _array;

        var required = checked(array.Length + extra);
        var capacity = array.Length != 0 ? array.Length * 2 : 4;

        if ((uint)capacity > (uint)Array.MaxLength)
            capacity = Array.MaxLength;

        if (capacity < required)
            capacity = required;

        var destination = new T[capacity];
        array.AsSpan().TryCopyTo(destination);

        _array = destination;
        return destination;
    }
}
