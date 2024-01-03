namespace Ramstack.Parsing.Utilities;

/// <summary>
/// Represents a Unicode category mask used for character classification.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
internal readonly struct CharClassUnicodeCategory : IEquatable<CharClassUnicodeCategory>
{
    /// <summary>
    /// The raw integer value representing the Unicode category mask.
    /// </summary>
    public readonly int Value;

    /// <summary>
    /// Gets a value indicating whether the category mask is empty.
    /// </summary>
    public bool IsEmpty => Value == 0;

    /// <summary>
    /// Gets a value indicating whether the category mask is specified (non-empty).
    /// </summary>
    public bool IsSpecified => Value != 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="CharClassUnicodeCategory"/> struct with the specified category mask.
    /// </summary>
    /// <param name="categories">The category mask to initialize with.</param>
    private CharClassUnicodeCategory(int categories) =>
        Value = categories & (int)CharClassExtensions.NormalizedCategoryMask;

    /// <summary>
    /// Determines whether the specified <see cref="UnicodeCategory"/> is included in this category mask.
    /// </summary>
    /// <param name="category">The <see cref="UnicodeCategory"/> to check.</param>
    /// <returns>
    /// <see langword="true"/> if the category is included; otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInclude(UnicodeCategory category) =>
        (Value & (1 << (int)category)) != 0;

    /// <summary>
    /// Determines whether the category of the specified character is included in this category mask.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <returns>
    /// <see langword="true"/> if the character's category is included;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInclude(char c)
    {
        return Value != 0 && IsIncludeImpl(Value, c);

        static bool IsIncludeImpl(int value, char c)
        {
            var category = char.GetUnicodeCategory(c);
            return (value & (1 << (int)category)) != 0;
        }
    }

    /// <summary>
    /// Combines this category mask with the specified <see cref="UnicodeCategory"/>.
    /// </summary>
    /// <param name="category">The <see cref="UnicodeCategory"/> to combine with.</param>
    /// <returns>
    /// A new <see cref="CharClassUnicodeCategory"/> representing the combined category mask.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CharClassUnicodeCategory Combine(UnicodeCategory category) =>
        new CharClassUnicodeCategory(Value | (1 << (int)category));

    /// <summary>
    /// Combines this category mask with the specified <see cref="GeneralUnicodeCategory"/>.
    /// </summary>
    /// <param name="category">The <see cref="GeneralUnicodeCategory"/> to combine with.</param>
    /// <returns>
    /// A new <see cref="CharClassUnicodeCategory"/> representing the combined category mask.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CharClassUnicodeCategory Combine(GeneralUnicodeCategory category) =>
        new CharClassUnicodeCategory(Value | (int)category);

    /// <summary>
    /// Combines this category mask with another <see cref="CharClassUnicodeCategory"/>.
    /// </summary>
    /// <param name="category">The other <see cref="CharClassUnicodeCategory"/> to combine with.</param>
    /// <returns>
    /// A new <see cref="CharClassUnicodeCategory"/> representing the combined category mask.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CharClassUnicodeCategory Combine(CharClassUnicodeCategory category) =>
        new CharClassUnicodeCategory(Value | category.Value);

    /// <summary>
    /// Creates a new <see cref="CharClassUnicodeCategory"/> from a <see cref="GeneralUnicodeCategory"/>.
    /// </summary>
    /// <param name="category">The <see cref="GeneralUnicodeCategory"/> to represent.</param>
    /// <returns>
    /// A new <see cref="CharClassUnicodeCategory"/> representing the specified Unicode categories.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CharClassUnicodeCategory Create(GeneralUnicodeCategory category) =>
        new CharClassUnicodeCategory((int)category);

    /// <summary>
    /// Creates a new <see cref="CharClassUnicodeCategory"/> from a <see cref="UnicodeCategory"/>.
    /// </summary>
    /// <param name="category">The <see cref="UnicodeCategory"/> to represent.</param>
    /// <returns>
    /// A new <see cref="CharClassUnicodeCategory"/> representing the specified Unicode category.
    /// </returns>
    public static CharClassUnicodeCategory Create(UnicodeCategory category) =>
        new CharClassUnicodeCategory(1 << (int)category);

    /// <summary>
    /// Creates a new <see cref="CharClassUnicodeCategory"/> from a raw mask value.
    /// </summary>
    /// <param name="mask">The integer mask representing the Unicode categories.</param>
    /// <returns>
    /// A new <see cref="CharClassUnicodeCategory"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CharClassUnicodeCategory Create(int mask) =>
        new CharClassUnicodeCategory(mask);

    /// <inheritdoc />
    public override string ToString() =>
        ((GeneralUnicodeCategory)Value).ToPrintable();

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(CharClassUnicodeCategory other) =>
        Value == other.Value;

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is CharClassUnicodeCategory other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() =>
        Value;

    /// <summary>
    /// Implicitly converts a <see cref="GeneralUnicodeCategory"/> to a <see cref="CharClassUnicodeCategory"/>.
    /// </summary>
    /// <param name="category">The <see cref="GeneralUnicodeCategory"/> to convert.</param>
    /// <returns>
    /// A <see cref="CharClassUnicodeCategory"/> representing the specified Unicode category.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator CharClassUnicodeCategory(GeneralUnicodeCategory category) =>
        new CharClassUnicodeCategory((int)category);

    /// <summary>
    /// Compares two <see cref="CharClassUnicodeCategory"/> instances for equality.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(CharClassUnicodeCategory left, CharClassUnicodeCategory right) =>
        left.Value == right.Value;

    /// <summary>
    /// Compares two <see cref="CharClassUnicodeCategory"/> instances for inequality.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(CharClassUnicodeCategory left, CharClassUnicodeCategory right) =>
        left.Value != right.Value;
}
