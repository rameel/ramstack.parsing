namespace Ramstack.Parsing.Collections;

/// <summary>
/// Represents a debugger view for the <see cref="ArrayList{T}"/> structure.
/// </summary>
/// <typeparam name="T">The type of items in the <see cref="ArrayList{T}"/>.</typeparam>
internal sealed class ArrayListDebugView<T>(ArrayList<T> list)
{
    /// <summary>
    /// Gets the array of items contained in the <see cref="ArrayList{T}"/>.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items => list.ToArray();
}
