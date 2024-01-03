using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

/// <summary>
/// Provides parsers for character-based parsing operations.
/// </summary>
public static class Character
{
    /// <summary>
    /// Gets a parser that matches a single letter character.
    /// </summary>
    public static Parser<char> Letter => L(GeneralUnicodeCategory.Letter).As("letter");

    /// <summary>
    /// Gets a parser that matches a single upper letter character.
    /// </summary>
    public static Parser<char> Uppercase => L(GeneralUnicodeCategory.UppercaseLetter).As("uppercase letter");

    /// <summary>
    /// Gets a parser that matches a single lower letter character.
    /// </summary>
    public static Parser<char> Lowercase => L(GeneralUnicodeCategory.LowercaseLetter).As("lowercase letter");

    /// <summary>
    /// Gets a parser that matches a single digit character.
    /// </summary>
    public static Parser<char> Digit => L(GeneralUnicodeCategory.DecimalDigitNumber).As("digit");

    /// <summary>
    /// Gets a parser that matches a single letter or digit character.
    /// </summary>
    public static Parser<char> LetterOrDigit => L(GeneralUnicodeCategory.Letter | GeneralUnicodeCategory.DecimalDigitNumber);

    /// <summary>
    /// Gets a parser that matches a single number character.
    /// </summary>
    public static Parser<char> Number => L(GeneralUnicodeCategory.Number);

    /// <summary>
    /// Gets a parser that matches a single symbol character.
    /// </summary>
    public static Parser<char> Symbol => L(GeneralUnicodeCategory.Symbol);

    /// <summary>
    /// Gets a parser that matches a single separator character.
    /// </summary>
    public static Parser<char> Separator => L(GeneralUnicodeCategory.Separator);

    /// <summary>
    /// Gets a parser that matches a single punctuation character.
    /// </summary>
    public static Parser<char> Punctuation => L(GeneralUnicodeCategory.Punctuation);

    /// <summary>
    /// Gets a parser that matches a single control character.
    /// </summary>
    public static Parser<char> Control => L(GeneralUnicodeCategory.Control);

    /// <summary>
    /// Gets a parser that matches any surrogate character (high or low).
    /// </summary>
    public static Parser<char> Surrogate => Set('\uD800', '\uDFFF').As("surrogate");

    /// <summary>
    /// Gets a parser that matches a high surrogate character (U+D800 to U+DBFF).
    /// </summary>
    public static Parser<char> HighSurrogate => Set('\uD800', '\uDBFF').As("high-surrogate");

    /// <summary>
    /// Gets a parser that matches a low surrogate character (U+DC00 to U+DFFF).
    /// </summary>
    public static Parser<char> LowSurrogate => Set('\uDC00', '\uDFFF').As("low-surrogate");

    /// <summary>
    /// Gets a parser that matches a single whitespace character.
    /// </summary>
    public static Parser<char> WhiteSpace => Choice(
        OneOf("\t\n\v\f\r\u0085 "),
        L(GeneralUnicodeCategory.Separator)
        ).As("whitespace");

    /// <summary>
    /// Gets a parser that matches a single hexadecimal digit (0..9, A..F, a..f).
    /// </summary>
    public static Parser<char> HexDigit => Set("0-9A-Fa-f").As("hexadecimal");
}
