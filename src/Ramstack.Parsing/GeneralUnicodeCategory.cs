namespace Ramstack.Parsing;

/// <summary>
/// Defines the Unicode category of a character.
/// </summary>
/// <remarks>
/// Unlike the standard <see cref="UnicodeCategory"/>, this enumeration is designed
/// to work conveniently with flag-based combinations.<br/>
/// </remarks>
[Flags]
public enum GeneralUnicodeCategory
{
    /// <summary>
    /// Represents the absence of any Unicode category.
    /// Indicates that no specific category has been assigned.
    /// </summary>
    None = 0,

    /// <summary>
    /// Uppercase letter.
    /// Signified by the Unicode designation "Lu" (letter, uppercase).
    /// </summary>
    UppercaseLetter = 1 << UnicodeCategory.UppercaseLetter,

    /// <summary>
    /// Lowercase letter.
    /// Signified by the Unicode designation "Ll" (letter, lowercase).
    /// </summary>
    LowercaseLetter = 1 << UnicodeCategory.LowercaseLetter,

    /// <summary>
    /// Titlecase letter.
    /// Signified by the Unicode designation "Lt" (letter, titlecase).
    /// </summary>
    TitlecaseLetter = 1 << UnicodeCategory.TitlecaseLetter,

    /// <summary>
    /// Modifier letter character, which is free-standing spacing character
    /// that indicates modifications of a preceding letter.
    /// Signified by the Unicode designation "Lm" (letter, modifier).
    /// </summary>
    ModifierLetter = 1 << UnicodeCategory.ModifierLetter,

    /// <summary>
    /// Letter that is not an uppercase letter, a lowercase letter,
    /// a titlecase letter, or a modifier letter.
    /// Signified by the Unicode designation "Lo" (letter, other).
    /// </summary>
    OtherLetter = 1 << UnicodeCategory.OtherLetter,

    /// <summary>
    /// Nonspacing character that indicates modifications of a base character.
    /// Signified by the Unicode designation "Mn" (mark, nonspacing).
    /// </summary>
    NonSpacingMark = 1 << UnicodeCategory.NonSpacingMark,

    /// <summary>
    /// Spacing character that indicates modifications of a base character
    /// and affects the width of the glyph for that base character.
    /// Signified by the Unicode designation "Mc" (mark, spacing combining).
    /// </summary>
    SpacingCombiningMark = 1 << UnicodeCategory.SpacingCombiningMark,

    /// <summary>
    /// Enclosing mark character, which is a nonspacing combining character
    /// that surrounds all previous characters up to and including a base character.
    /// Signified by the Unicode designation "Me" (mark, enclosing).
    /// </summary>
    EnclosingMark = 1 << UnicodeCategory.EnclosingMark,

    /// <summary>
    /// Decimal digit character, that is, a character in the range 0 through 9.
    /// Signified by the Unicode designation "Nd" (number, decimal digit).
    /// </summary>
    DecimalDigitNumber = 1 << UnicodeCategory.DecimalDigitNumber,

    /// <summary>
    /// Number represented by a letter, instead of a decimal digit,
    /// for example, the Roman numeral for five, which is "V".
    /// The indicator is signified by the Unicode designation "Nl" (number, letter).
    /// </summary>
    LetterNumber = 1 << UnicodeCategory.LetterNumber,

    /// <summary>
    /// Number that is neither a decimal digit nor a letter number, for example, the fraction 1/2.
    /// The indicator is signified by the Unicode designation "No" (number, other).
    /// </summary>
    OtherNumber = 1 << UnicodeCategory.OtherNumber,

    /// <summary>
    /// Space character, which has no glyph but is not a control or format character.
    /// Signified by the Unicode designation "Zs" (separator, space).
    /// </summary>
    SpaceSeparator = 1 << UnicodeCategory.SpaceSeparator,

    /// <summary>
    /// Character that is used to separate lines of text.
    /// Signified by the Unicode designation "Zl" (separator, line).</summary>
    LineSeparator = 1 << UnicodeCategory.LineSeparator,

    /// <summary>
    /// Character used to separate paragraphs.
    /// Signified by the Unicode designation "Zp" (separator, paragraph).
    /// </summary>
    ParagraphSeparator = 1 << UnicodeCategory.ParagraphSeparator,

    /// <summary>
    /// Control code character, with a Unicode value of U+007F
    /// or in the range U+0000 through U+001F or U+0080 through U+009F.
    /// Signified by the Unicode designation "Cc" (other, control).
    /// </summary>
    Control = 1 << UnicodeCategory.Control,

    /// <summary>
    /// Format character that affects the layout of text
    /// or the operation of text processes, but is not normally rendered.
    /// Signified by the Unicode designation "Cf" (other, format).
    /// </summary>
    Format = 1 << UnicodeCategory.Format,

    /// <summary>
    /// High surrogate or a low surrogate character.
    /// Surrogate code values are in the range U+D800 through U+DFFF.
    /// Signified by the Unicode designation "Cs" (other, surrogate).
    /// </summary>
    Surrogate = 1 << UnicodeCategory.Surrogate,

    /// <summary>
    /// Private-use character, with a Unicode value in the range U+E000 through U+F8FF.
    /// Signified by the Unicode designation "Co" (other, private use).
    /// </summary>
    PrivateUse = 1 << UnicodeCategory.PrivateUse,

    /// <summary>
    /// Connector punctuation character that connects two characters.
    /// Signified by the Unicode designation "Pc" (punctuation, connector).
    /// </summary>
    ConnectorPunctuation = 1 << UnicodeCategory.ConnectorPunctuation,

    /// <summary>
    /// Dash or hyphen character.
    /// Signified by the Unicode designation "Pd" (punctuation, dash).
    /// </summary>
    DashPunctuation = 1 << UnicodeCategory.DashPunctuation,

    /// <summary>
    /// Opening character of one of the paired punctuation marks,
    /// such as parentheses, square brackets, and braces.
    /// Signified by the Unicode designation "Ps" (punctuation, open).
    /// </summary>
    OpenPunctuation = 1 << UnicodeCategory.OpenPunctuation,

    /// <summary>
    /// Closing character of one of the paired punctuation marks,
    /// such as parentheses, square brackets, and braces.
    /// Signified by the Unicode designation "Pe" (punctuation, close).
    /// </summary>
    ClosePunctuation = 1 << UnicodeCategory.ClosePunctuation,

    /// <summary>
    /// Opening or initial quotation mark character.
    /// Signified by the Unicode designation "Pi" (punctuation, initial quote).
    /// </summary>
    InitialQuotePunctuation = 1 << UnicodeCategory.InitialQuotePunctuation,

    /// <summary>
    /// Closing or final quotation mark character.
    /// Signified by the Unicode designation "Pf" (punctuation, final quote).
    /// </summary>
    FinalQuotePunctuation = 1 << UnicodeCategory.FinalQuotePunctuation,

    /// <summary>
    /// Punctuation character that is not a connector, a dash, open punctuation,
    /// close punctuation, an initial quote, or a final quote.
    /// Signified by the Unicode designation "Po" (punctuation, other).
    /// </summary>
    OtherPunctuation = 1 << UnicodeCategory.OtherPunctuation,

    /// <summary>
    /// Mathematical symbol character, such as "+" or "= ".
    /// Signified by the Unicode designation "Sm" (symbol, math).
    /// </summary>
    MathSymbol = 1 << UnicodeCategory.MathSymbol,

    /// <summary>
    /// Currency symbol character.
    /// Signified by the Unicode designation "Sc" (symbol, currency).
    /// </summary>
    CurrencySymbol = 1 << UnicodeCategory.CurrencySymbol,

    /// <summary>
    /// Modifier symbol character, which indicates modifications of surrounding characters.
    /// For example, the fraction slash indicates that the number to the left is the numerator
    /// and the number to the right is the denominator.
    /// The indicator is signified by the Unicode designation "Sk" (symbol, modifier).
    /// </summary>
    ModifierSymbol = 1 << UnicodeCategory.ModifierSymbol,

    /// <summary>
    /// Symbol character that is not a mathematical symbol, a currency symbol or a modifier symbol.
    /// Signified by the Unicode designation "So" (symbol, other).
    /// </summary>
    OtherSymbol = 1 << UnicodeCategory.OtherSymbol,

    /// <summary>
    /// Character that is not assigned to any Unicode category.
    /// Signified by the Unicode designation "Cn" (other, not assigned).
    /// </summary>
    OtherNotAssigned = 1 << UnicodeCategory.OtherNotAssigned,

    /// <summary>
    /// Represents any letter category, including uppercase, lowercase,
    /// titlecase, modifier, and other letters.
    /// Signified by the Unicode designation "L" ("Lu", "Ll", "Lt", "Lm", "Lo").
    /// </summary>
    Letter =
        UppercaseLetter
        | LowercaseLetter
        | TitlecaseLetter
        | ModifierLetter
        | OtherLetter,

    /// <summary>
    /// Represents any mark category, including non-spacing, spacing combining, and enclosing marks.
    /// Signified by the Unicode designation "M" ("Mn", "Mc", "Me").
    /// </summary>
    Mark =
        NonSpacingMark
        | SpacingCombiningMark
        | EnclosingMark,

    /// <summary>
    /// Represents any number category, including decimal digit, letter, and other numbers.
    /// Signified by the Unicode designation "N" ("Nd", "Nl", "No").
    /// </summary>
    Number =
        DecimalDigitNumber
        | LetterNumber
        | OtherNumber,

    /// <summary>
    /// Represents any separator category, including space, line, and paragraph separators.
    /// Signified by the Unicode designation "Z" ("Zs", "Zl", "Zp").
    /// </summary>
    Separator =
        SpaceSeparator
        | LineSeparator
        | ParagraphSeparator,

    /// <summary>
    /// Represents any punctuation category, including connector, dash, open, close, initial quote, final quote, and other punctuation.
    /// Signified by the Unicode designation "P" ("Pc", "Pd", "Ps", "Pe", "Pi", "Pf", "Po").
    /// </summary>
    Punctuation =
        ConnectorPunctuation
        | DashPunctuation
        | OpenPunctuation
        | ClosePunctuation
        | InitialQuotePunctuation
        | FinalQuotePunctuation
        | OtherPunctuation,

    /// <summary>
    /// Represents any symbol category, including math, currency, modifier, and other symbols.
    /// Signified by the Unicode designation "S" ("Sm", "Sc", "Sk", "So", ).
    /// </summary>
    Symbol =
        MathSymbol
        | CurrencySymbol
        | ModifierSymbol
        | OtherSymbol,

    /// <summary>
    /// Represents any control category, including control, format, surrogate, private use, and other not assigned characters.
    /// Signified by the Unicode designation "C" ("Cc", "Cf", "Cs", "Co", "Cn").
    /// </summary>
    Other =
        Control
        | Format
        | Surrogate
        | PrivateUse
        | OtherNotAssigned,
}
