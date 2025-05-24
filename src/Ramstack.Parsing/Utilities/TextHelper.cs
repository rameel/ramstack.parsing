namespace Ramstack.Parsing.Utilities;

/// <summary>
/// Provides helper methods for working with text.
/// </summary>
internal static class TextHelper
{
    /// <summary>
    /// Calculates the line and column numbers for the specified index in the specified character span.
    /// </summary>
    /// <param name="source">The character span representing the text content.</param>
    /// <param name="index">The index within the span for which to determine the line and column numbers.</param>
    /// <returns>
    /// A tuple containing the line and column numbers corresponding to the specified index.
    /// </returns>
    public static (int Line, int Column) GetLineColumn(ReadOnlySpan<char> source, int index)
    {
        var line = 1;
        var column = 1;

        if ((uint)index < (uint)source.Length)
            source = source[..index];

        for (var i = 0; i < source.Length; i++)
        {
            column++;

            if (source[i] == '\n')
            {
                line++;
                column = 1;
            }
            else if (source[i] == '\r' && i + 1 < source.Length && source[i + 1] == '\n')
            {
                i++;

                line++;
                column = 1;
            }
        }

        return (line, column);
    }
}
