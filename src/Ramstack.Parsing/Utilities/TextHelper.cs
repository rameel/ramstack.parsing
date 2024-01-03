namespace Ramstack.Parsing.Utilities;

/// <summary>
/// Provides helper methods for working with text.
/// </summary>
internal static class TextHelper
{
    /// <summary>
    /// Returns the line number for the specified index in the given source string.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="index">The index in the source string for which to determine the line number.</param>
    /// <returns>
    /// The line number corresponding to the specified index.
    /// </returns>
    public static int GetLine(ReadOnlySpan<char> source, int index)
    {
        var line = 1;

        if (index >= source.Length)
            index = source.Length - 1;

        while (true)
        {
            if ((uint)index >= (uint)source.Length)
                break;

            if (source[index] == '\n')
                line++;

            index--;
        }

        return line;
    }

    /// <summary>
    /// Returns the column number for the specified index in the given source string.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="index">The index in the source string for which to determine the column number.</param>
    /// <returns>
    /// The column number corresponding to the specified index.
    /// </returns>
    public static int GetColumn(ReadOnlySpan<char> source, int index)
    {
        var column = 1;
        index--;

        if (index >= source.Length)
            index = source.Length - 1;

        while ((uint)index < (uint)source.Length
            && source[index] != '\n'
            && source[index] != '\r')
        {
            column++;
            index--;
        }

        return column;
    }
}
