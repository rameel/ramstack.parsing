# Basic parsers and literals

[Parser guide](README.md)

Match characters, fixed text, and values such as numbers and quoted strings.

## Basic parsers

| Parser                                                  | Purpose                                                                                                    |
| ------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- |
| `L('a')`                                                | Match one specific character and return it.                                                                |
| `L("hello")`                                            | Match a literal string. Comparison is ordinal by default; a `StringComparison` argument can change it.     |
| `OneOf("abc")`                                          | Match one character from the supplied string.                                                              |
| `OneOf("if", "else")`                                   | Match one of several literal strings. For ordinal comparisons, prefer the longest matching literal.        |
| `Set('a', 'z')`                                         | Match a character in an inclusive range.                                                                   |
| `Set("a-zA-Z0-9_")`                                     | Match a character from a character class.                                                                  |
| `L(UnicodeCategory...)`, `L(GeneralUnicodeCategory...)` | Match a character by its Unicode category.                                                                 |
| `Any`                                                   | Match any single character. Fail at the end of input.                                                      |
| `S`                                                     | Consume zero or more whitespace characters, including line breaks.                                         |
| `Eol`                                                   | Consume a line break, or succeed at the end of input.                                                      |
| `Eof`                                                   | Require the end of input without consuming anything.                                                       |
| `Regex(regex)`                                          | Match a regular expression at the current position; its pattern must start with `^`. Available on .NET 7+. |

`Character` provides ready-made character parsers: `Letter`, `Uppercase`,
`Lowercase`, `Digit`, `LetterOrDigit`, `HexDigit`, `WhiteSpace`, and other Unicode
groups. These return `char`; `S`, `Eol`, and `Eof` return `Unit`, representing
success without a meaningful value.

`OneOf` distinguishes a set of characters from a set of strings:

```csharp
var sign = OneOf("+-");
var comparison = OneOf("=", "==");

Console.WriteLine(sign.Parse("+").Value);        // +
Console.WriteLine(comparison.Parse("==").Value); // ==
```

`L` and the string overloads of `OneOf` return the supplied literal,
even if a case-insensitive match has different spelling in the source.
Use `Text` for the original spelling:

```csharp
var word = L("hello", StringComparison.OrdinalIgnoreCase);
Console.WriteLine(word.Parse("HELLO").Value);        // hello
Console.WriteLine(word.Text().Parse("HELLO").Value); // HELLO
```

Combine character classes to describe an identifier:

```csharp
var identifier = Seq(
    Set("a-zA-Z_"),
    Set("a-zA-Z0-9_").ZeroOrMore()
).Text();

Console.WriteLine(identifier.Parse("item_42").Value); // item_42
Console.WriteLine(identifier.Parse("42item").Success); // False
```

### Character classes and Unicode

Write `Set` patterns without surrounding square brackets. Use verbatim C# strings
for patterns with backslashes:

| Pattern                    | Meaning                                                                          |
| -------------------------- | -------------------------------------------------------------------------------- |
| `a-z`, `0-9`               | An inclusive character range.                                                    |
| `abc_`                     | Any of the listed characters.                                                    |
| `\p{L}`, `\p{Nd}`, `\P{L}` | A Unicode letter, a decimal digit, or a character outside the letter categories. |
| `\d`, `\w`, `\s`           | A Unicode decimal digit, word character, or whitespace character.                |
| `\n`, `\r`, `\t`, `\u0041` | A character escape or a four-digit Unicode escape.                               |
| `\-`, `\\`                 | A literal hyphen or backslash.                                                   |

`OneOf("a-z")` means `a`, `-`, or `z`; `Set("a-z")` means a range.
`Set` is a character-class parser: `^`, `[`, `]`, and `.` match themselves.
For example, `Set("^a")` accepts `^` or `a`. It has no regex anchors, class subtraction,
or named Unicode blocks such as `\p{IsGreek}`.

```csharp
Console.WriteLine(Set(@"\p{L}").Parse("Ж").Value);     // Ж
Console.WriteLine(Character.Digit.Parse("٣").Success); // True
Console.WriteLine(Set("0-9").Parse("٣").Success);      // False
```

Character parsers read one UTF-16 code unit (`char`), so an emoji can require two matches.
`S` includes line breaks; use `OneOf(" \t").ZeroOrMore().Void()` when only horizontal spaces are allowed.

## Literals

`L` matches fixed text. The `Literal` class parses a value written in text form.

| Parser                          | Purpose                                                                                                                                           |
| ------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Literal.Number<T>()`           | Parse a numeric value, such as `int`, `double`, or `decimal`.                                                                                     |
| `Literal.Number<T>(kind)`       | Select a numeric format with `NumberKind`: `Auto`, `Integer`, `Float`, `HexNumber`, or `BinaryNumber` (.NET 8+). Supported formats depend on `T`. |
| `Literal.DoubleQuotedString`    | Parse a double-quoted string, remove its quotes, and decode escapes.                                                                              |
| `Literal.SingleQuotedString`    | Parse a single-quoted string with the same value handling.                                                                                        |
| `Literal.QuotedCharacter`       | Parse one character enclosed in single quotes.                                                                                                    |
| `Literal.EscapeSequence`        | Decode an escape sequence such as `\n` into a character.                                                                                          |
| `Literal.UnicodeEscapeSequence` | Decode a Unicode escape such as `\u0041`.                                                                                                         |

```csharp
var integer = Literal.Number<int>();      // "42" => 42
var text = Literal.DoubleQuotedString;    // "\"hello\"" => "hello"
```

Numeric formats and escape sequences produce values directly:

```csharp
var hex = Literal.Number<int>(NumberKind.HexNumber);
Console.WriteLine(hex.Parse("2A").Value); // 42

var quoted = Literal.DoubleQuotedString.Parse("\"A\\u0042\"");
Console.WriteLine(quoted.Value); // AB
```

Numbers use invariant culture: a decimal point is `.`.
Integer input permits an optional sign and ASCII digits;
floating-point input also permits a fraction and exponent, such as `1.5e2`. 
fraction requires digits on both sides of the point. An incomplete suffix
such as `1e+` stays unread, so use `Eof` or a delimiter to reject it.
An integer outside the target type's range fails normally.

`HexNumber` and `BinaryNumber` read digits without a sign or a base prefix.
Parse a prefix separately, for example `L("0x").Then(hex)`.
Binary `BigInteger` requires a target of .NET 9 or later.

Quoted strings return decoded contents; `Text()` returns the original spelling,
including quotes and escapes:

```csharp
var text = Literal.DoubleQuotedString;
Console.WriteLine(text.Parse("\"A\\u0042\"").Value); // AB
Console.WriteLine(text.Text().Parse("\"A\\u0042\"").Value); // "A\u0042"
Console.WriteLine(Literal.QuotedCharacter.Parse("'ab'").Success); // False
```

Quoted strings require a closing quote and reject unescaped CR/LF and unknown escapes.
Unicode escapes use `\u` plus exactly four hexadecimal digits; `\x` and `\U` forms are unsupported.
These are general-purpose literals: their number and string syntax does not enforce a strict JSON grammar.
