namespace Ramstack.Parsing;

/// <summary>
/// Represents a bookmark for a particular parsing position in the input text.
/// </summary>
[DebuggerDisplay("Position = {Position}")]
public readonly struct PositionBookmark
{
    /// <summary>
    /// Gets the zero-based parsing position.
    /// </summary>
    public readonly int Position;

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionBookmark"/> structure.
    /// </summary>
    /// <param name="position">The zero-based parsing position.</param>
    internal PositionBookmark(int position) =>
        Position = position;
}
