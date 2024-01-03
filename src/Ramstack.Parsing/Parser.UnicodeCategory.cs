namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that matches the specified Unicode category of the character.
    /// </summary>
    /// <param name="category">The <see cref="UnicodeCategory"/> of the character to match.</param>
    /// <returns>
    /// A parser that parses the specified Unicode category of the character.
    /// </returns>
    public static Parser<char> L(UnicodeCategory category) =>
        L((GeneralUnicodeCategory)(1 << (int)category));

    /// <summary>
    /// Creates a parser that matches the specified Unicode categories of the character.
    /// </summary>
    /// <param name="categories">The <see cref="GeneralUnicodeCategory"/> of the character to match.</param>
    /// <returns>
    /// A parser that parses the specified Unicode categories of the character.
    /// </returns>
    public static Parser<char> L(GeneralUnicodeCategory categories)
    {
        var value = categories & CharClassExtensions.NormalizedCategoryMask;
        if (value == GeneralUnicodeCategory.None)
            Error_InvalidArgument();

        return Set(new CharClass(value));

        [DoesNotReturn]
        static void Error_InvalidArgument() =>
            throw new ArgumentException(
                $"'{nameof(GeneralUnicodeCategory.None)}' is not a valid Unicode category value.",
                nameof(categories));
    }
}
