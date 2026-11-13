# Getting started

[Documentation index](README.md)

## Install

Add the package to a .NET 6 or later app:

```shell
dotnet add package Ramstack.Parsing
```

## Build and run a parser

`Parser<T>` reads text and produces a value of type `T`. For example,
`Parser<int>` produces an integer, and `Parser<string>` produces a string.
A method that combines parsers returns another parser, so you can build the
grammar one step at a time.

This complete console example reads a port number:

```csharp
using System;
using Ramstack.Parsing;
using static Ramstack.Parsing.Parser;

var port = Literal.Number<int>()
    .Where(n => n >= 1 && n <= 65535, "port from 1 to 65535")
    .Between(S, S)
    .ThenIgnore(Eof);

var result = port.Parse(" 8080 ");

if (result.Success)
{
    Console.WriteLine(result.Value); // 8080
}
else
{
    Console.WriteLine(result.ErrorMessage);
}

Console.WriteLine(port.Parse("70000").Success); // False
Console.WriteLine(port.Parse("8080abc").Success); // False
```

`Number<int>` reads the number. `Where` checks its range. `Between(S, S)`
allows spaces on either side. `ThenIgnore(Eof)` checks that nothing is left.

## Read a result

`Parse` returns `ParseResult<T>`:

| Property | Meaning |
| --- | --- |
| `Success` | Whether the parser succeeded. Check this before you use `Value`. |
| `Value` | The result on success; otherwise `default(T)`. Its type is nullable, so the examples write `result.Value!` when they know the parse succeeded. A successful result can also be `null`, if your grammar returns `null`. |
| `Length` | The number of UTF-16 characters read on success, including spaces and delimiters that the parser threw away. It is zero on failure. |
| `ErrorMessage` | The error text with a line and column, or `Parsing failed.` when nothing more is known. It is `null` on success. |
| `Exception` | The exception caught during parsing, such as one thrown by a function you passed to `Do`. It is `null` for ordinary failures. |

`result.ToString()` returns the value's text on success and the error message on
failure. Use the individual properties when your code needs to tell them apart.

If a function that you pass throws an exception, `ErrorMessage` contains the full
exception text, including the stack trace. Do not show it to end users. See
[Errors](errors.md#when-your-own-code-throws).

## Parse a prefix or the whole input

By default, a parser only needs to match a prefix: the text at the start of the
input. It can succeed and leave unread text.

```csharp
var number = Literal.Number<int>();
var prefix = number.Parse("123abc");

Console.WriteLine(prefix.Success); // True
Console.WriteLine(prefix.Value);   // 123
Console.WriteLine(prefix.Length);  // 3

var wholeNumber = number.ThenIgnore(Eof);
Console.WriteLine(wholeNumber.Parse("123abc").Success); // False
```

Use prefix parsing when you read one part of a larger grammar. Use `Eof` in the
outermost parser when you read a whole value, expression, or document.

## Handle whitespace

The library does not skip spaces for you. `S` reads zero or more whitespace
characters, including tabs and line breaks, and returns `Unit`.

```csharp
var strict = L("hello").ThenIgnore(Eof);
var withSpaces = L("hello").Between(S, S).ThenIgnore(Eof);

Console.WriteLine(strict.Parse(" hello ").Success);     // False
Console.WriteLine(withSpaces.Parse(" hello ").Success); // True
```

A common pattern is `token.ThenIgnore(S)`. It reads a token and any spaces after
it, and keeps the token's value. Use `Between(S, Eof)` for a complete grammar
whose tokens already read their trailing spaces.

If line breaks matter, use a narrower parser such as `OneOf(" \t").ZeroOrMore()`
instead of `S`.

## Use TryParse when you do not need an error message

`TryParse` returns a Boolean and writes the value through an `out` parameter.
It does not build error messages, so it does less work.

```csharp
var number = Literal.Number<int>().ThenIgnore(Eof);

if (number.TryParse("42", out var value))
{
    Console.WriteLine(value); // 42
}

Console.WriteLine(number.TryParse("bad", out _)); // False
```

Both `Parse` and `TryParse` take `ReadOnlySpan<char>`. You can pass a string
directly, or pass a slice without first making a new string:

```csharp
var number = Literal.Number<int>().ThenIgnore(Eof);
ReadOnlySpan<char> source = "id=42;".AsSpan();

Console.WriteLine(number.Parse(source.Slice(3, 2)).Value); // 42
```

`TryParse` returns `false` for ordinary failures and for fatal errors (`Fail` and
`FatalError`). Exceptions thrown by your own functions reach the caller. `Parse`
catches those exceptions and stores them in the result. See
[Errors](errors.md#when-your-own-code-throws).

## Reuse the parser

Build parsers outside the code that handles each input. A finished parser can
be kept in a `static readonly` field and used many times. Each call to `Parse`
creates its own position and error messages. If your functions use shared data,
make that data safe for several threads.

Next: [Basic parsers](basic-parsers.md).
