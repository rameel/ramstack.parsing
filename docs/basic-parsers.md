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

```csharp
var identifier = Seq(
    Set("a-zA-Z_"),
    Set("a-zA-Z0-9_").ZeroOrMore()
).Text();
// "item_42" => "item_42"
```

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
