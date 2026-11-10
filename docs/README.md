# Parser guide

> Draft: a brief overview of the available parsers and combinators.

A `Parser<T>` reads text and produces a value of type `T`. Small parsers match
individual tokens; combinators join them into lists, expressions, and grammars.
The examples use these imports:

```csharp
using System;
using Ramstack.Parsing;
using static Ramstack.Parsing.Parser;
```

Each snippet is independent unless stated otherwise.
Comments after `Console.WriteLine` show the expected output.
Small examples may parse only a prefix; complete grammars use `Eof`.
Check `Success` before using `Value` in application code;
examples read known successful results directly.

## Contents

- [Basic parsers and literals](basic-parsers.md) — Match characters, fixed text, and values such as numbers and quoted strings.
- [Combining parsers and transforming values](combinators.md) — Build sequences and alternatives, then transform or discard their results.
- [Repetition and lists](repetition-and-lists.md) — Read repeated items, separated lists, optional values, and text up to a terminator.
- [Recursive parsers and expressions](recursive-parsers.md) — Describe nested structures and expressions with associativity and precedence.
- [Running parsers, lookahead, and errors](parsing-and-errors.md) — Run a parser, inspect its result, check upcoming input, and report failures.
- [Writing your own parser](custom-parsers.md) — Implement matching logic with `Parser<T>` and `ParseContext`.

## Find a method by task

| Task                                                           | Use                                                   |
| -------------------------------------------------------------- | ----------------------------------------------------- |
| Match fixed text or a character class                          | `L`, `OneOf`, `Set`, `Character`                      |
| Read a number or quoted text                                   | `Literal`                                             |
| Keep every value, the next value, or the first value           | `Seq`, `Then`, `ThenIgnore`                           |
| Try alternatives or accept an absent value                     | `Choice`, `Or`, `Optional`, `DefaultOnFail`           |
| Read repeated items, a separated list, or a terminated section | `Repeat`, `OneOrMore`, `Separated`, `Until`           |
| Transform a value, capture source text, or validate            | `Do`, `Map`, `Text`, `Where`                          |
| Describe nesting or operators                                  | `Deferred`, `Recursive`, `FoldL`, `FoldR`, `Prefixed` |
| Check upcoming input or improve an error message               | `And`, `Not`, `As`                                    |

Build parsers once and reuse them. Each top-level call creates its own parse context.
Keep callbacks focused on building values; do external work after a successful parse.
See [callbacks and discarded values](combinators.md#callbacks-and-discarded-values).

## Complete examples

- [Calculator](../samples/CalcExpr/README.md): arithmetic expressions and precedence.
- [JSON](../samples/Json/README.md): nested objects and arrays.
- [TinyC](../samples/TinyC/README.md): a small language grammar.

Installation is covered in the [project README](../README.md).
