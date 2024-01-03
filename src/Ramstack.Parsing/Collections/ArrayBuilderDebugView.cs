namespace Ramstack.Parsing.Collections;

/// <summary>
/// Represents a debugger view for the <see cref="ArrayBuilder{T}"/> structure.
/// </summary>
/// <typeparam name="T">The type of items in the <see cref="ArrayBuilder{T}"/>.</typeparam>
internal sealed class ArrayBuilderDebugView<T>(ArrayBuilder<T> builder)
{
    /// <summary>
    /// Gets the array of items contained in the <see cref="ArrayBuilder{T}"/>.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items => builder.ToArray();
}
