# Ramstack.Parsing documentation

Ramstack.Parsing is a parser combinator library for .NET. A parser reads text and
returns a value. You build larger parsers by joining small ones: a character,
a word, a number, a list, or a whole expression.

Start with [Getting started](getting-started.md). Then use the other pages as a
guide to the methods you need.

| Page                                                      | What it covers                                                                                                   |
| --------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| [Getting started](getting-started.md)                     | Install the library, run a parser, read results, handle spaces, and require the whole input.                     |
| [Basic parsers](basic-parsers.md)                         | `L`, `OneOf`, `Set`, Unicode categories, `Character`, `Any`, `S`, `Eol`, `Eof`, and `Regex`.                     |
| [Numbers and quoted text](literals.md)                    | `Literal.Number<T>`, `NumberKind`, quoted strings, quoted characters, and escape sequences.                      |
| [Combining parsers](combining-parsers.md)                 | `Seq`, `Then`, `ThenIgnore`, `Between`, `Choice`, `Or`, `Optional`, `DefaultOnFail`, `Return`, `And`, and `Not`. |
| [Repetition and lists](repetition-and-lists.md)           | `ZeroOrMore`, `Many`, `OneOrMore`, `AtLeast`, `Repeat`, `Separated`, and `Until`.                                |
| [Working with values](working-with-values.md)             | `Do`, `Where`, `Map`, `Text`, `Void`, `Match`, `Unit`, and when callbacks run.                                   |
| [Recursive parsers and expressions](recursive-parsers.md) | `Deferred`, `Recursive`, `FoldL`, `FoldR`, `Prefixed`, and operator precedence.                                  |
| [Errors](errors.md)                                       | Error messages, `As`, `Error`, `Fail`, `FatalError`, and exceptions.                                             |
| [Writing your own parser](custom-parsers.md)              | `ParseContext` and a custom `Parser<T>` class.                                                                   |
| [Complete examples](examples.md)                          | A list of integers, a setting, a calculator, and links to the project samples.                                   |
| [Common mistakes](common-mistakes.md)                     | Problems that new users often meet, and how to fix them.                                                         |
| [Performance tips](performance-tips.md)                   | How to keep parsers fast.                                                                                        |

## Find a method by task

| I want to...                             | Use                                                        |
| ---------------------------------------- | ---------------------------------------------------------- |
| Read an exact character or word          | `L`                                                        |
| Read one of several words                | `OneOf`                                                    |
| Read one character from a range or a set | `Set`, `Character`                                         |
| Read a number                            | `Literal.Number<T>`                                        |
| Read a quoted string                     | `Literal.DoubleQuotedString`, `Literal.SingleQuotedString` |
| Skip spaces                              | `S`                                                        |
| Require the end of the input             | `Eof`                                                      |
| Run parsers one after another            | `Seq`, `Then`, `ThenIgnore`, `Between`                     |
| Try alternatives                         | `Choice`, `Or`                                             |
| Make a part optional                     | `Optional`, `DefaultOnFail`                                |
| Repeat a parser                          | `ZeroOrMore`, `OneOrMore`, `Repeat`, `AtLeast`             |
| Read a list with separators              | `Separated`                                                |
| Read until an end marker                 | `Until`                                                    |
| Change the value                         | `Do`                                                       |
| Get the source text                      | `Text`                                                     |
| Check a value                            | `Where`                                                    |
| Look ahead without reading               | `And`, `Not`                                               |
| Read operators such as `+` or `^`        | `FoldL`, `FoldR`                                           |
| Read prefix operators such as `-` or `!` | `Prefixed`                                                 |
| Let a parser use itself                  | `Deferred`, `Recursive`                                    |
| Give a parser a better error name        | `As`                                                       |

## Words used in these docs

- **Failure**: a parser does not match the text. This is normal. A choice tries
  the next alternative, and a repetition stops.
- **Fatal error**: `Fail` or `FatalError` stops the whole parse. Nothing recovers from it.
- **Callback**: a function that you pass to a method, such as the one in `Do`.
- **Matched segment**: the part of the input that a parser matched.
  `Text` and `Map` read it.
- **Void parser**: a parser that throws its value away. `Unit` is the empty value
  that it returns.
- **Prefix**: the text at the start of the input.

## Reading the examples

The examples use these imports. Add them to a console app before you copy a
snippet. Each snippet is separate unless the page says otherwise.

```csharp
using System;
using System.Collections.Generic;
using System.Globalization;
using Ramstack.Parsing;
using static Ramstack.Parsing.Parser;
```

Comments after `Console.WriteLine` show the expected output. Small examples
often read only the start of the input, to show how a method works. The
complete examples use `Eof` to check the whole input.

`ParseResult<T>.Value` has a nullable type. When an example knows that the parse
succeeded, it writes `result.Value!`.

The library targets .NET 6, 7, 8, and 9. It also works in later .NET versions.
`Parser.Regex` needs .NET 7 or later. `NumberKind.BinaryNumber` needs .NET 8 or
later (.NET 9 for `BigInteger`). All other examples compile with C# 10, the
default for .NET 6.

## Common rules

- Parsing starts at the current position. A parser does not search ahead for a match.
- Spaces are part of the input. Add `S` or your own whitespace parser where needed.
- A successful parse can leave unread text. Add `ThenIgnore(Eof)` to reject it.
- A choice uses the first alternative that succeeds. Put longer and more specific
  alternatives first.
- Repetition is greedy. It reads as much as it can and never gives text back.
- Keep the functions that build values free of side effects. They may run during
  an attempt that later fails, or be skipped when the value is thrown away. See
  [Callbacks and thrown-away values](working-with-values.md#callbacks-and-thrown-away-values).
- Build a parser once and reuse it. Finish setting up recursive parsers before use.

See [Common mistakes](common-mistakes.md) for examples of what goes wrong when you
forget these rules. See the [project README](../README.md) for a short overview
and benchmark links.
