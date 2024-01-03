namespace Ramstack.Parsing.Utilities;

/// <summary>
/// Provides a mapping structure for character-to-string-array lookups.
/// </summary>
internal readonly struct CharMap
{
    private readonly string[][] _array;
    private readonly Dictionary<char, string[]>? _dictionary;

    /// <summary>
    /// Character offset used to normalize array indices in the array-based storage strategy.
    /// </summary>
    private readonly int _offset;

    /// <summary>
    /// Provides indexed access to string arrays mapped to specific characters.
    /// </summary>
    /// <param name="c">The character to look up.</param>
    /// <returns>
    /// The string array associated with the character, or <see langword="null"/> if no mapping exists.
    /// </returns>
    public string[]? this[char c]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var index = c - _offset;
            var array = _array;

            if ((uint)index < (uint)array.Length)
                return array[index];

            var dictionary = _dictionary;
            if (dictionary is not null)
            {
                dictionary.TryGetValue(c, out var result);
                return result;
            }

            return null;
        }
    }

    /// <summary>
    /// Initializes a new instance of the CharMap struct with the specified character mappings.
    /// </summary>
    /// <param name="dictionary">A dictionary containing the initial character-to-string-array mappings.</param>
    public CharMap(Dictionary<char, string[]> dictionary)
    {
        var chars = dictionary.Keys.ToArray();
        Array.Sort(chars);

        if ((uint)chars[^1] - chars[0] + 1 < 128u)
        {
            _offset = chars[0];
            _array = new string[chars[^1] - chars[0] + 1][];

            foreach (var c in chars)
                _array[c - _offset] = dictionary[c];
        }
        else
        {
            _array = [];
            _dictionary = dictionary;
        }
    }
}
