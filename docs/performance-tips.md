# Performance tips

[Documentation index](README.md)

These tips follow from how the library works. Results depend on your grammar,
your input, and your hardware, so measure before you tune. The project
[benchmarks](https://github.com/rameel/ramstack.parsing/blob/main/benchmarks/README.md)
show how to do that.

## Build parsers once

Building a parser does work, for example it prepares tables for character sets.
Build it once, keep it in a `static readonly` field, and reuse it. Each call to
`Parse` or `TryParse` creates its own position, so one parser can serve many
inputs.

The `Character` properties create a new parser each time you read them. Keep them
in a field too.

## Use TryParse when you do not need an error message

`TryParse` does not build error messages, so it does less work than `Parse`.
Use `Parse` when you need the message, or use `TryParse` first and run `Parse`
again only when it fails.

## Throw values away

A parser that only reads or checks input does not need to build a value.
`Then`, `ThenIgnore`, `Between`, `And`, `Not`, separators, and `Void()` throw
values away, and the library can skip the work of building them. Write the
punctuation of your grammar with these methods.

## Use Text instead of a list of characters

`Set("a-z").OneOrMore()` returns a `List<char>`. If you want the text, add
`.Text()`. It gives you a string and does not build the list:

```csharp
var word = Set("a-z").OneOrMore().Text();
Console.WriteLine(word.Parse("hello world").Value); // hello
```

Repeating a single character set (`Set`, or `OneOf` with a string of characters)
uses a special scan that is made for this case. Prefer it to repeating a more
complex parser when you only need a run of characters.

## Prefer ordinal comparisons

`L` and `OneOf` compare strings with `StringComparison.Ordinal` by default. This
is the simplest and the most predictable choice. A culture-based comparison can
match text of a different length than the literal, and it depends on the culture
that was current when you built the parser.

Next: [Documentation index](README.md).
