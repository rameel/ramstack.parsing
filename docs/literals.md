# Numbers and quoted text

[Documentation index](README.md)

The `Literal` class reads common values and converts their text into .NET
values. It is separate from `Parser.L`, which matches a fixed character or
string. The examples use the [standard imports](README.md#reading-the-examples).

## `Literal.Number<T>`

`Literal.Number<T>(kind)` reads a number and converts it to `T`.
`Literal.Number<T>(name, kind)` also sets the name used in error messages.
The default kind is `NumberKind.Auto`.

```csharp
Console.WriteLine(Literal.Number<int>().Parse("-42").Value);    // -42
Console.WriteLine(Literal.Number<double>().Parse("1.5e2").Value); // 150

var quantity = Literal.Number<int>("quantity").ThenIgnore(Eof);
Console.WriteLine(quantity.Parse("oops").ErrorMessage); // (1:1) Expected quantity
```

Without a name, the error message says `number` for integers, `floating-point number`
for floating-point types, `hexadecimal number` for `HexNumber`, and `binary number`
for `BinaryNumber`.

The parser uses `CultureInfo.InvariantCulture`. Decimal input uses `.` even if
the current culture uses a comma. Whitespace is not read as part of the number.
Add `S` where the grammar allows it.

### Supported types and kinds

| Target type | Supported kinds |
| --- | --- |
| `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `nint`, `nuint` | `Auto`, `Integer`, `HexNumber`; also `BinaryNumber` on .NET 8 or later. |
| `Int128`, `UInt128` (.NET 7 or later) | The same integer kinds. |
| `BigInteger` | `Auto`, `Integer`, `HexNumber`; also `BinaryNumber` in apps that target .NET 9 or later (see below). |
| `float`, `double`, `decimal`, `Half` | `Auto`, `Float`. |

`BigInteger` is in `System.Numerics`. On .NET 7 or later, `T` also has an
`INumber<T>` constraint, but the method still accepts only the types in the table.
An unsupported type, such as `char`, throws `InvalidOperationException` when you
build the parser. A kind that does not fit the type, such as `Float` for `int`,
throws `ArgumentException`.

### Decimal integers and floating-point values

`Integer` reads an optional `+` or `-`, followed by one or more ASCII digits.
`Float` reads the same part, followed by an optional fraction and exponent:

```text
[+-]?[0-9]+(\.[0-9]+)?([eE][+-]?[0-9]+)?
```

Leading zeros are allowed. A fraction needs digits on both sides of the dot.
An exponent needs at least one digit. This grammar does not accept `.5`, `NaN`,
or `Infinity`.

Like other parsers, a number can match a prefix. An incomplete fraction or
exponent is left unread:

```csharp
var number = Literal.Number<double>();

Console.WriteLine(number.Parse("1.").Value);    // 1
Console.WriteLine(number.Parse("1e+").Length);  // 1
Console.WriteLine(number.Parse(".5").Success);  // False
Console.WriteLine(number.ThenIgnore(Eof).Parse("1e+").Success); // False

Console.WriteLine(Literal.Number<byte>().Parse("256").Success); // False
```

Use `Eof`, a delimiter, or a boundary check to reject an invalid suffix.
An integer outside the range of the target type fails normally. A floating-point
value follows the parsing rules of the .NET type. For example, `double` turns
`1e999` into infinity, and `decimal` fails. Use `Where` for any extra range or
finite-value checks that your format needs.

### Hexadecimal and binary integers

`HexNumber` reads one or more hexadecimal digits. `BinaryNumber`, available on
.NET 8 or later, reads one or more `0` or `1` digits. Neither accepts a sign or
a base prefix. Add the prefix as a separate parser:

```csharp
var hex = Literal.Number<int>(NumberKind.HexNumber).ThenIgnore(Eof);
var prefixedHex = L("0x").Then(hex);

Console.WriteLine(hex.Parse("2A").Value);           // 42
Console.WriteLine(hex.Parse("0x2A").Success);       // False
Console.WriteLine(prefixedHex.Parse("0x2A").Value); // 42
```

```csharp
// .NET 8 or later
var binary = Literal.Number<int>(NumberKind.BinaryNumber).ThenIgnore(Eof);
Console.WriteLine(binary.Parse("101010").Value); // 42
Console.WriteLine(binary.Parse("0b1010").Success); // False
```

The conversion follows the .NET rules for hexadecimal and binary numbers. For a
signed type of fixed size, the top bit of the full width is the sign bit. For
example, `Number<int>(HexNumber)` reads `FFFFFFFF` as -1.

`BigInteger` has no fixed size, so the first digit decides the sign. A hexadecimal
digit from `8` to `F`, or a binary `1`, at the start makes the number negative.
Add a leading `0` to get a positive number:

```csharp
using System.Numerics;

var big = Literal.Number<BigInteger>(NumberKind.HexNumber);
Console.WriteLine(big.Parse("FF").Value);  // -1
Console.WriteLine(big.Parse("0FF").Value); // 255
```

```csharp
// .NET 9 or later
using System.Numerics;

var bigBinary = Literal.Number<BigInteger>(NumberKind.BinaryNumber);
Console.WriteLine(bigBinary.Parse("101010").Value);  // -22
Console.WriteLine(bigBinary.Parse("0101010").Value); // 42
```

`BigInteger` can read binary numbers only in apps that target .NET 9 or later.
.NET 8 cannot parse binary `BigInteger` values. In an app that targets .NET 8,
`Literal.Number<BigInteger>(NumberKind.BinaryNumber)` throws `ArgumentException`
when you build the parser. This also happens when the app runs on a newer
runtime. Use another type, or read the digits yourself, if you must support
.NET 8.

## Quoted strings

`Literal.DoubleQuotedString` and `Literal.SingleQuotedString` read a quoted
string and return its decoded contents. The quotes are required, but they are not
part of the returned value. Empty strings are allowed.

```csharp
Console.WriteLine(Literal.DoubleQuotedString.Parse("\"hello\"").Value); // hello
Console.WriteLine(Literal.SingleQuotedString.Parse("'hello'").Value);   // hello
Console.WriteLine(Literal.DoubleQuotedString.Parse("\"\"").Value!.Length); // 0

var escaped = Literal.DoubleQuotedString.Parse("\"A\\tB\"");
Console.WriteLine(escaped.Value == "A\tB"); // True

var raw = Literal.DoubleQuotedString.Text();
Console.WriteLine(raw.Parse("\"A\\tB\"").Value); // "A\tB"
```

The decoded value contains a tab. `Text()` returns the original spelling,
including the quotes and the backslash escape. The length read includes the
quotes and all source characters used for escape sequences.

A quoted string fails when it has an unescaped CR or LF, a missing closing quote,
or an unknown escape. These are general-purpose string parsers. Their escapes and
control characters do not follow a strict JSON string grammar.

## QuotedCharacter

`Literal.QuotedCharacter` reads exactly one character inside single quotes,
including the supported escape forms, and returns a `char`.

```csharp
Console.WriteLine(Literal.QuotedCharacter.Parse("'A'").Value); // A
Console.WriteLine(Literal.QuotedCharacter.Parse("'\\u0041'").Value); // A
Console.WriteLine(Literal.QuotedCharacter.Parse("'ab'").Success); // False
Console.WriteLine(Literal.QuotedCharacter.Parse("''").Success);   // False
```

## EscapeSequence and UnicodeEscapeSequence

These parsers read an escape without surrounding quotes:

```csharp
Console.WriteLine(Literal.EscapeSequence.Parse(@"\n").Value == '\n'); // True
Console.WriteLine(Literal.UnicodeEscapeSequence.Parse(@"\u0041").Value); // A
Console.WriteLine(Literal.EscapeSequence.Parse(@"\q").Success); // False
```

`EscapeSequence` accepts these two-character forms. Quoted strings and quoted
characters use the same table:

| Source escape | Decoded character |
| --- | --- |
| `\0` | NUL, U+0000. |
| `\a` | Bell, U+0007. |
| `\b` | Backspace, U+0008. |
| `\t` | Tab, U+0009. |
| `\n` | Line feed, U+000A. |
| `\v` | Vertical tab, U+000B. |
| `\f` | Form feed, U+000C. |
| `\r` | Carriage return, U+000D. |
| `\e` | Escape, U+001B. |
| `\"`, `\'`, `\/`, `\\` | A literal double quote, single quote, slash, or backslash. |

`UnicodeEscapeSequence` accepts `\u` followed by exactly four hexadecimal
digits. It reads six source characters and returns one UTF-16 code unit.
It does not support `\xNN` or `\UNNNNNNNN`. It does not check surrogate pairs,
so it can return a lone surrogate as a `char`.

To accept either escape form without quotes, use a choice:

```csharp
var escape = Literal.EscapeSequence.Or(Literal.UnicodeEscapeSequence);
Console.WriteLine(escape.Parse(@"\u0042").Value); // B
```

Next: [Recursive parsers and expressions](recursive-parsers.md).
