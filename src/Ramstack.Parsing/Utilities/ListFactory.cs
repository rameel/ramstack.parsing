namespace Ramstack.Parsing.Utilities;

/// <summary>
/// Represents the helper class responsible for creating <see cref="List{T}"/> instances
/// with optimized initialization based on the target framework version.
/// </summary>
/// <typeparam name="T">The type of the elements in the list.</typeparam>
internal static class ListFactory<T>
{
    /// <summary>
    /// Creates a new <see cref="List{T}"/> initialized with the elements from the specified <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <param name="items">The span of items to populate the list with.</param>
    /// <returns>
    /// A new <see cref="List{T}"/> containing the specified elements.
    /// </returns>
    /// <remarks>
    /// This method leverages internal optimizations depending on the target framework version
    /// to efficiently populate the list with the specified items.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static List<T> CreateList(ReadOnlySpan<T> items)
    {
        #if NET9_0_OR_GREATER
        var list = new List<T>();
        GetCount(list) = items.Length;
        GetArray(list) = items.ToArray();
        return list;
        #elif NET8_0_OR_GREATER
        var list = new List<T>(items.Length);
        if (items.Length != 0)
        {
            CollectionsMarshal.SetCount(list, items.Length);
            items.TryCopyTo(CollectionsMarshal.AsSpan(list));
        }
        return list;
        #else
        var list = new List<T>(items.Length);
        foreach (var item in items)
            list.Add(item);
        return list;
        #endif
    }

    #if NET9_0_OR_GREATER
    /// <summary>
    /// Returns a reference to the internal array buffer of the specified <see cref="List{T}"/>.
    /// </summary>
    /// <param name="list">The list whose internal buffer is to be accessed.</param>
    /// <returns>
    /// A reference to the internal array used by the list.
    /// </returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_items")]
    private static extern ref T[] GetArray(List<T> list);

    /// <summary>
    /// Returns a reference to the internal "_size" field of the specified <see cref="List{T}"/>
    /// representing the number of elements in the list.
    /// </summary>
    /// <param name="list">The list whose internal "_size" field is to be accessed.</param>
    /// <returns>
    /// A reference to the internal "_size" field representing the number of elements in the list.
    /// </returns>
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_size")]
    private static extern ref int GetCount(List<T> list);
    #endif
}

