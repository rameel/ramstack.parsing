# Basic parsers

[Documentation index](README.md)

These parsers match text at the current position. They do not skip spaces or
require the end of the input unless you add those parsers. See the index for the
[imports used in examples](README.md#reading-the-examples).

## L: one character or one literal string

`L(char)` reads one exact character. `L(string, comparison)` reads an exact
string. Both return the literal they matched.

```csharp
Console.WriteLine(L(':').Parse(":rest").Value);       // :
Console.WriteLine(L("hello").Parse("hello!").Value);  // hello
Console.WriteLine(L("hello").Parse("Hello").Success); // False
```

String matching uses `StringComparison.Ordinal` by default. Pass a comparison
to change it:

```csharp
var word = L("hello", StringComparison.OrdinalIgnoreCase);

Console.WriteLine(word.Parse("HELLO").Value);        // hello
Console.WriteLine(word.Text().Parse("HELLO").Value); // HELLO
```

The returned value is the literal you gave to `L`, even when the source has
different letter case. Use `Text()` to get the source spelling.

Use ordinal comparisons (the default) for programming languages and file formats.
A culture-based comparison reads the current culture when you build the parser.
Changing the culture later does not change that parser. It can also match text
that is shorter or longer than the literal, and in rare cases it matches without
reading any text. Avoid it inside repeated parsers.

`L("")` is not allowed. Use `Return(value)` for a parser that reads nothing.

## OneOf: characters or words

`OneOf("+-")` reads one character from the string. It returns a `char`.
The string is a list of characters, so `OneOf("a-z")` means `a`, `-`, or `z`.
Use `Set("a-z")` for a range.

`OneOf("yes", "no")` or `OneOf(new[] { "yes", "no" })` reads one whole
string and returns a `string`. There are also overloads for three or four
strings, and the string overloads accept a `StringComparison`.

```csharp
Console.WriteLine(OneOf("+-").Parse("-1").Value); // -
Console.WriteLine(OneOf("yes", "no").Parse("yes!").Value); // yes

var comparison = OneOf("<", "<=", ">", ">=");
Console.WriteLine(comparison.Parse("<=").Value); // <=
```

With ordinal comparisons, `OneOf` tries longer words first. So `OneOf("<", "<=")`
reads `<=` when the input has it. The order in which you wrote the words does not
matter. `Choice` is different: it tries alternatives in the order you gave them.

With culture-based comparisons, `OneOf` tries the words in descending sort order.
Words that compare equal are merged, and only the first one is kept. This also
applies to ignore-case comparisons:

```csharp
var ab = OneOf("Ab", "ab", StringComparison.OrdinalIgnoreCase);

Console.WriteLine(ab.Parse("ab").Value);        // Ab
Console.WriteLine(ab.Text().Parse("ab").Value); // ab
```

As with `L`, the result is the word you supplied. Use `Text()` to get the source
text.

The character string, the array of words, and each word must be non-empty.

## Set: a character range or class

`Set(lo, hi)` reads one character in an inclusive range. `Set(pattern)` reads
one character from a class written without square brackets.

```csharp
Console.WriteLine(Set('a', 'z').Parse("cat").Value);     // c
Console.WriteLine(Set("A-Za-z0-9_").Parse("_id").Value); // _
Console.WriteLine(Set(@"\p{L}").Parse("é").Value);       // é
Console.WriteLine(Set(@"\d").Parse("5").Value);          // 5
```

| Pattern | Matches |
| --- | --- |
| `a-z`, `0-9` | An inclusive range. |
| `abc_` | Any of these literal characters. |
| `\p{L}`, `\p{Nd}` | A Unicode general category. |
| `\P{L}` | A character outside that Unicode general category. |
| `\d`, `\D` | A Unicode decimal digit, or a character outside that class. |
| `\w`, `\W` | A word character, or a character outside that class. |
| `\s`, `\S` | Whitespace, or a character outside that class. |
| `\n`, `\r`, `\t`, `\a`, `\b`, `\v`, `\f`, `\e`, `\0` | A character escape. Here `\b` means backspace and `\0` is the NUL character. |
| `\\`, `\-` | A literal backslash or hyphen. |
| `\u0041` | A character given by exactly four hexadecimal digits. |

Use a verbatim C# string, such as `@"\d"`, or double the backslash in a normal
string, such as `"\\d"`.

A hyphen makes a range only when it sits between two characters. At the start or
at the end of the pattern it is an ordinary hyphen, so `Set("+-")` reads `+` or
`-`. In the middle, write `\-`:

```csharp
Console.WriteLine(Set("+-").Parse("-").Success);     // True
Console.WriteLine(Set(@"a\-z").Parse("-").Success);  // True
Console.WriteLine(Set(@"a\-z").Parse("m").Success);  // False
```

`Set` is a character-class parser, not a full regular expression. These
characters have no special meaning in it: `^`, `[`, `]`, and `.`. They match
themselves.

```csharp
Console.WriteLine(Set("^a").Parse("^").Success);    // True
Console.WriteLine(Set("^a").Parse("b").Success);    // False
Console.WriteLine(Set("[a-z]").Parse("[").Success); // True
```

So `Set("^a")` does not mean "any character except `a`". `Set` also has no class
subtraction, no anchors, and no named Unicode blocks such as `\p{IsGreek}`. A bad
pattern, such as `\q`, `\.`, or a backward range like `z-a`, throws
`ArgumentException` when you build the parser. To read a character other than a
comma, use `Not(L(',')).Then(Any)`.

## Unicode categories and Character

`L(UnicodeCategory)` reads a character from one .NET Unicode category.
`L(GeneralUnicodeCategory)` accepts flags, including groups such as `Letter`,
`Number`, and `Punctuation`.

```csharp
var letter = L(UnicodeCategory.LowercaseLetter);
var letterOrDigit = L(
    GeneralUnicodeCategory.Letter | GeneralUnicodeCategory.DecimalDigitNumber);

Console.WriteLine(letter.Parse("é").Value);        // é
Console.WriteLine(letterOrDigit.Parse("7").Value); // 7
```

`GeneralUnicodeCategory.None` is not a valid category for `L`. It throws
`ArgumentException`.

The `Character` class provides ready-made `Parser<char>` properties:

| Property | What it reads |
| --- | --- |
| `Letter` | Any Unicode letter. |
| `Uppercase`, `Lowercase` | An uppercase letter, or a lowercase letter. |
| `Digit` | A decimal digit from any script. |
| `Number` | A character in any Unicode number category. |
| `LetterOrDigit` | A letter or a decimal digit. |
| `HexDigit` | An ASCII hexadecimal digit: `0-9`, `A-F`, or `a-f`. |
| `Symbol`, `Separator`, `Punctuation`, `Control` | A character in the matching Unicode category group. |
| `WhiteSpace` | One whitespace character. |
| `Surrogate`, `HighSurrogate`, `LowSurrogate` | One UTF-16 surrogate code unit. |

```csharp
Console.WriteLine(Character.Letter.Parse("Ж").Value);   // Ж
Console.WriteLine(Character.HexDigit.Parse("F").Value); // F
Console.WriteLine(Character.Digit.Parse("٣").Success);  // True
Console.WriteLine(Set("0-9").Parse("٣").Success);       // False
```

Each time you read a `Character` property, it creates a new parser. Keep it in
a variable or a field if you use it in several places.

These parsers read one C# `char` at a time. Some characters, such as emoji, use
two `char` values. `Any` and the `Character` parsers do not join the pair.

## Any, S, Eol, and Eof

| Parser | Result | What it does |
| --- | --- | --- |
| `Any` | `char` | Reads any one UTF-16 code unit, including a line break. Fails at the end of the input. |
| `S` | `Unit` | Reads zero or more whitespace characters, using `char.IsWhiteSpace`. Always succeeds. |
| `Eol` | `Unit` | Reads a line break. At the end of the input it succeeds without reading anything. |
| `Eof` | `Unit` | Succeeds only at the end of the input, without reading anything. |

`Unit` means that the parser has no useful value to return.

```csharp
Console.WriteLine(Any.Parse("abc").Value);       // a
Console.WriteLine(S.Parse(" \tvalue").Length);   // 2
Console.WriteLine(Eol.Parse("\r\nnext").Length); // 2
Console.WriteLine(Eol.Parse("").Success);        // True
Console.WriteLine(Eof.Parse("x").Success);       // False
```

`Eol` reads CRLF as one line break. It also reads LF, CR, U+0085, U+2028,
and U+2029. Because it can succeed without reading anything at the end of the
input, do not use it alone in a loop that must move forward.

## Regex (.NET 7 or later)

`Regex(System.Text.RegularExpressions.Regex)` turns a .NET regular expression
into a `Parser<string>`. The pattern must start with the character `^`. Without it,
`Regex` throws `ArgumentException`. The match must start at the current position.

```csharp
var code = Regex(new System.Text.RegularExpressions.Regex("^[A-Z]{2}[0-9]{3}"))
    .ThenIgnore(Eof);

Console.WriteLine(code.Parse("AB123").Value);    // AB123
Console.WriteLine(code.Parse("xAB123").Success); // False
```

The parser returns the matched text. It fails when no text is left, even if the
pattern could match an empty string. When some text is left, a pattern such as
`^a*` can match zero characters. Then the parser succeeds and reads nothing. Use
`Eof` when the whole input must match.

The regular expression sees only the text that is left, not the text that was
already read. A lookbehind such as `(?<=a)b` cannot see earlier characters, and
`\b` at the start always sees a word boundary:

```csharp
var lookbehind = L('a').Then(Regex(new System.Text.RegularExpressions.Regex("^(?<=a)b")));
Console.WriteLine(lookbehind.Parse("ab").Success); // False
```

The error message uses the pattern text. Give the parser a name with `As` to
get a message that is easier to read:

```csharp
var code = Regex(new System.Text.RegularExpressions.Regex("^[A-Z]{2}[0-9]{3}"));
Console.WriteLine(code.Parse("1").ErrorMessage); // (1:1) Expected ^[A-Z]{2}[0-9]{3}

var named = code.As("product code");
Console.WriteLine(named.Parse("1").ErrorMessage); // (1:1) Expected product code
```

Next: [Combining parsers](combining-parsers.md).
