namespace Ramstack.Parsing.Utilities;

/// <summary>
/// Represents an element of a character class, which can either be a range of characters or a Unicode category.
/// </summary>
internal readonly struct CharClassElement
{
    private readonly long _value;

    /// <summary>
    /// Gets a value indicating whether the element represents a character range.
    /// </summary>
    public bool IsRange => _value >>> 63 != 0;

    /// <summary>
    /// Gets a value indicating whether the element represents a Unicode category.
    /// </summary>
    public bool IsUnicodeCategory => _value >>> 63 == 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharClassElement"/> struct with the specified value.
    /// </summary>
    /// <param name="value">The raw value representing the character class element.</param>
    private CharClassElement(long value) =>
        _value = value;

    /// <summary>
    /// Attempts to retrieve the Unicode category represented by this element.
    /// </summary>
    /// <param name="category">When this method returns, contains the <see cref="CharClassUnicodeCategory"/> if successful;
    /// otherwise, the default value.</param>
    /// <returns>
    /// <see langword="true"/> if the element represents a Unicode category;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetUnicodeCategory(out CharClassUnicodeCategory category)
    {
        return TryGetUnicodeCategoryImpl(_value, out category);

        static bool TryGetUnicodeCategoryImpl(long value, out CharClassUnicodeCategory category)
        {
            if (value >>> 63 == 0)
            {
                category = CharClassUnicodeCategory.Create((int)(value >>> 32));
                return true;
            }

            category = default;
            return false;
        }
    }

    /// <summary>
    /// Attempts to retrieve the character range represented by this element.
    /// </summary>
    /// <param name="range">When this method returns, contains the <see cref="CharClassRange"/> if successful;
    /// otherwise, the default value.</param>
    /// <returns>
    /// <see langword="true"/> if the element represents a character range;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetRange(out CharClassRange range)
    {
        return TryGetRangeImpl(_value, out range);

        static bool TryGetRangeImpl(long value, out CharClassRange range)
        {
            if (value >>> 63 != 0)
            {
                range = CharClassRange.CreateMasked((int)value);
                return true;
            }

            range = default;
            return false;
        }
    }

    /// <summary>
    /// Creates a new <see cref="CharClassElement"/> representing the specified character range.
    /// </summary>
    /// <param name="range">The character range to represent.</param>
    /// <returns>
    /// A new <see cref="CharClassElement"/> representing the specified character range.
    /// </returns>
    public static CharClassElement Create(CharClassRange range) =>
        new CharClassElement((1L << 63) | (uint)range.Value);

    /// <summary>
    /// Creates a new <see cref="CharClassElement"/> representing the specified Unicode category.
    /// </summary>
    /// <param name="category">The Unicode category to represent.</param>
    /// <returns>
    /// A new <see cref="CharClassElement"/> representing the specified Unicode category.
    /// </returns>
    public static CharClassElement Create(CharClassUnicodeCategory category) =>
        Create((GeneralUnicodeCategory)category.Value);

    /// <summary>
    /// Creates a new <see cref="CharClassElement"/> representing the specified general Unicode category.
    /// </summary>
    /// <param name="category">The general Unicode category to represent.</param>
    /// <returns>
    /// A new <see cref="CharClassElement"/> representing the specified Unicode category.
    /// </returns>
    public static CharClassElement Create(GeneralUnicodeCategory category) =>
        new CharClassElement((long)(uint)category << 32);

    /// <summary>
    /// Implicitly converts a <see cref="CharClassRange"/> to a <see cref="CharClassElement"/>.
    /// </summary>
    /// <param name="range">The <see cref="CharClassRange"/> to convert.</param>
    /// <returns>
    /// A <see cref="CharClassElement"/> representing the specified character range.
    /// </returns>
    public static implicit operator CharClassElement(CharClassRange range) =>
        Create(range);

    /// <summary>
    /// Implicitly converts a <see cref="CharClassUnicodeCategory"/> to a <see cref="CharClassElement"/>.
    /// </summary>
    /// <param name="category">The <see cref="CharClassUnicodeCategory"/> to convert.</param>
    /// <returns>
    /// A <see cref="CharClassElement"/> representing the specified Unicode category.
    /// </returns>
    public static implicit operator CharClassElement(CharClassUnicodeCategory category) =>
        Create((GeneralUnicodeCategory)category.Value);

    /// <summary>
    /// Implicitly converts a <see cref="GeneralUnicodeCategory"/> to a <see cref="CharClassElement"/>.
    /// </summary>
    /// <param name="category">The <see cref="GeneralUnicodeCategory"/> to convert.</param>
    /// <returns>
    /// A <see cref="CharClassElement"/> representing the specified Unicode category.
    /// </returns>
    public static implicit operator CharClassElement(GeneralUnicodeCategory category) =>
        Create(category);
}
