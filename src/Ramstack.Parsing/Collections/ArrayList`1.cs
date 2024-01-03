using System.Collections;

namespace Ramstack.Parsing.Collections;

/// <summary>
/// Represents a strongly typed list of objects that can be accessed by index.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(ArrayListDebugView<>))]
public sealed class ArrayList<T> : IList<T>, IReadOnlyList<T>
{
    private T[] _array;
    private int _count;

    /// <summary>
    /// Gets the number of items contained in the <see cref="ArrayList{T}"/>.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int Count => _count;

    /// <summary>
    /// Gets an item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to get.</param>
    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var array = _array;
            var count = _count;

            if ((uint)index >= (uint)count)
                ThrowHelper.ThrowArgumentOutOfRangeException();

            return ref Unsafe.Add(
                ref MemoryMarshal.GetArrayDataReference(array),
                (nint)(uint)index);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayList{T}"/>.
    /// </summary>
    public ArrayList()
    {
        _count = 0;
        _array = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayList{T}"/> with the specified buffer capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity of the buffer.</param>
    public ArrayList(int capacity)
    {
        _count = 0;
        _array = new T[capacity];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayList{T}"/> class that contains elements copied
    /// from the specified span and has sufficient capacity to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new list.</param>
    public ArrayList(ReadOnlySpan<T> collection)
    {
        _count = collection.Length;
        _array = collection.ToArray();
    }

    /// <inheritdoc />
    public int IndexOf(T item) =>
        Array.IndexOf(_array, item, 0, _count);

    /// <inheritdoc />
    public bool Contains(T item) =>
        IndexOf(item) >= 0;

    /// <inheritdoc />
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
    /// Adds the items of the specified span to the end of the list.
    /// </summary>
    /// <param name="collection">The readonly span of items to add.</param>
    public void AddRange(ReadOnlySpan<T> collection)
    {
        var array = _array;
        var count = _count;

        if (array.Length - count < collection.Length)
            array = Grow(checked(count + collection.Length));

        _count = count + collection.Length;
        _ = array.Length;
        collection.TryCopyTo(array.AsSpan(count));
    }

    /// <inheritdoc />
    public void Insert(int index, T item)
    {
        var count = _count;
        var array = _array;

        Argument.ThrowIfGreaterThan((uint)index, (uint)count);

        if (count == array.Length)
            array = Grow(index);

        Array.Copy(
            sourceArray: array,
            sourceIndex: index,
            destinationArray: array,
            destinationIndex: index + 1,
            length: count - index);

        _count = count + 1;

        if ((uint)index < (uint)array.Length)
            array[index] = item;
    }

    /// <inheritdoc />
    public bool Remove(T item)
    {
        var index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        Argument.ThrowIfGreaterThanOrEqual((uint)index, (uint)_count);

        if (index < --_count)
            Array.Copy(
                sourceArray: _array,
                sourceIndex: index + 1,
                destinationArray: _array,
                destinationIndex: index,
                length: _count - index);

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            _array[_count] = default!;
    }

    /// <summary>
    /// Removes all elements from the <see cref="ArrayList{T}" />.
    /// </summary>
    public void Clear()
    {
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            _count = ClearImpl(_array, _count);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static int ClearImpl(T[] array, int count)
            {
                if ((uint)count <= (uint)array.Length)
                    array.AsSpan(0, count).Clear();

                return 0;
            }
        }
        else
        {
            _count = 0;
        }
    }

    /// <summary>
    /// Copies the contents of this list into a destination <see cref="Span{T}"/>.
    /// </summary>
    /// <param name="destination">The destination <see cref="Span{T}"/> object.</param>
    public void CopyTo(Span<T> destination) =>
        AsSpan().CopyTo(destination);

    /// <summary>
    /// Attempts to copy the current list to a destination <see cref="Span{T}"/>
    /// and returns a value that indicates whether the copy operation succeeded.
    /// </summary>
    /// <param name="destination">The destination <see cref="Span{T}"/> object.</param>
    /// <returns>
    /// <see langword="true"/> if copy operations succeeded; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryCopyTo(Span<T> destination) =>
        AsSpan().TryCopyTo(destination);

    /// <summary>
    /// Returns an enumerator that iterates through the <see cref="ArrayList{T}"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="Enumerator"/> for the <see cref="ArrayList{T}"/>.
    /// </returns>
    public Enumerator GetEnumerator() =>
        new Enumerator(this);

    /// <summary>
    /// Returns a <see cref="Span{T}"/> representing the written data of the current instance.
    /// </summary>
    /// <returns>
    /// A <see cref="Span{T}"/> that represents the data within the current instance.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> AsSpan()
    {
        var array = _array;
        var count = _count;
        _ = array.Length;

        return MemoryMarshal.CreateSpan(
            ref MemoryMarshal.GetArrayDataReference(array),
            count);
    }

    /// <summary>
    /// Copies the elements of the <see cref="ArrayList{T}"/> to a new array.
    /// </summary>
    /// <returns>
    /// An array containing copies of the elements of the <see cref="ArrayList{T}"/>.
    /// </returns>
    public T[] ToArray() =>
        AsSpan().ToArray();

    #region Explicit interface implementations

    /// <inheritdoc />
    T IList<T>.this[int index]
    {
        get => this[index];
        set => this[index] = value;
    }

    /// <inheritdoc />
    T IReadOnlyList<T>.this[int index] => this[index];

    /// <inheritdoc />
    bool ICollection<T>.IsReadOnly => false;

    /// <inheritdoc />
    void ICollection<T>.CopyTo(T[] array, int arrayIndex) =>
        AsSpan().CopyTo(array.AsSpan(arrayIndex));

    /// <inheritdoc />
    IEnumerator<T> IEnumerable<T>.GetEnumerator() =>
        GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    #endregion

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

        // ReSharper disable once RedundantOverflowCheckingContext
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

    #region Inner type: Enumerator

    /// <summary>
    /// Enumerates the items of a <see cref="ArrayList{T}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly T[] _array;
        private readonly int _count;
        private int _index;

        /// <summary>
        /// Gets the item at the current position of the enumerator.
        /// </summary>
        public readonly ref readonly T Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)_index >= (uint)_count)
                    ThrowHelper.ThrowArgumentOutOfRangeException();

                Debug.Assert((uint)_index < (uint)_array.Length);

                return ref Unsafe.Add(
                    ref MemoryMarshal.GetArrayDataReference(_array),
                    (nint)(uint)_index);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Enumerator"/> structure.
        /// </summary>
        /// <param name="list">The list to initialize from.</param>
        internal Enumerator(ArrayList<T> list)
        {
            _index = -1;
            _count = list._count;
            _array = list._array;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext() =>
            (uint)++_index < (uint)_count;

        /// <inheritdoc />
        public void Reset() =>
            _index = -1;

        /// <inheritdoc />
        T IEnumerator<T>.Current
        {
            get
            {
                if ((uint)_index >= (uint)_count)
                    ThrowHelper.ThrowArgumentOutOfRangeException();

                return _array[_index];
            }
        }

        /// <inheritdoc />
        object? IEnumerator.Current => Current;

        /// <inheritdoc />
        public void Dispose() =>
            _index = -1;
    }

    #endregion
}
