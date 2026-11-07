# Parser guide

A `Parser<T>` reads text and produces a value of type `T`. Small parsers match
individual tokens; combinators join them into lists, expressions, and grammars.
The examples use these imports:

```csharp
using Ramstack.Parsing;
using static Ramstack.Parsing.Parser;
```

## Contents

- [Basic parsers and literals](basic-parsers.md) — Match characters, fixed text, and values such as numbers and quoted strings.
- [Combining parsers and transforming values](combinators.md) — Build sequences and alternatives, then transform or discard their results.
- [Repetition and lists](repetition-and-lists.md) — Read repeated items, separated lists, optional values, and text up to a terminator.
- [Recursive parsers and expressions](recursive-parsers.md) — Describe nested structures and expressions with associativity and precedence.
- [Running parsers, lookahead, and errors](parsing-and-errors.md) — Run a parser, inspect its result, check upcoming input, and report failures.

## Complete examples

- [Calculator](../samples/CalcExpr/README.md): arithmetic expressions and precedence.
- [JSON](../samples/Json/README.md): nested objects and arrays.
- [TinyC](../samples/TinyC/README.md): a small language grammar.

Installation is covered in the [project README](../README.md).
