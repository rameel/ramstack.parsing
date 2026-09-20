namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that matches a character in the specified Unicode category.
    /// </summary>
    /// <param name="category">The <see cref="UnicodeCategory"/> of the character to match.</param>
    /// <returns>
    /// A parser that matches a character in the specified Unicode category.
    /// </returns>
    public static Parser<char> L(UnicodeCategory category) =>
        L((GeneralUnicodeCategory)(1 << (int)category));

    /// <summary>
    /// Creates a parser that matches a character in any of the specified Unicode categories.
    /// </summary>
    /// <param name="categories">The Unicode categories to match.</param>
    /// <returns>
    /// A parser that matches a character in any of the specified Unicode categories.
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
