# Combining parsers

[Documentation index](README.md)

A combinator takes existing parsers and returns a new parser. The examples
below use the [standard imports](README.md#reading-the-examples).

## Seq: keep every result

`Seq(p1, p2, ...)` runs parsers in order. All must succeed. It returns a tuple
with their values, in the same order. There are overloads for 2 through 10
parsers. If all the parsers return `Unit`, the result is `Unit` too.

```csharp
var pair = Seq(
    Literal.Number<int>(),
    L(':'),
    Literal.Number<int>());

var result = pair.Parse("12:34");

if (result.Success)
{
    var (left, separator, right) = result.Value;
    Console.WriteLine($"{left} {separator} {right}"); // 12 : 34
}
```

Use `.Do((a, b, ...) => ...)` to turn a tuple into the value you need:

```csharp
var pair = Seq(Literal.Number<int>(), L(':'), Literal.Number<int>())
    .Do((left, _, right) => left + right)
    .ThenIgnore(Eof);

Console.WriteLine(pair.Parse("12:34").Value); // 46
```

If one part fails, `Seq` moves the input position back to where the sequence
began. An alternative can then try the same text.

## Then and ThenIgnore: keep one result

Both methods require two parsers to succeed in order:

| Method                     | Value kept        |
| -------------------------- | ----------------- |
| `first.Then(second)`       | The second value. |
| `first.ThenIgnore(second)` | The first value.  |

```csharp
var number = L("id=").Then(Literal.Number<int>()).ThenIgnore(L(';'));

Console.WriteLine(number.Parse("id=42;").Value); // 42

var word = L("hello").ThenIgnore(S);
Console.WriteLine(word.Parse("hello   ").Value);  // hello
Console.WriteLine(word.Parse("hello   ").Length); // 8
```

Use these methods for punctuation, prefixes, whitespace, and `Eof`. If either
part fails, they move the input position back to the start.

## Between: surround a value

`parser.Between(before, after)` reads an opening parser, the value, and a closing
parser. Only the middle value is returned. `parser.Between(around)` uses the same
parser on both sides.

```csharp
var bracketed = Literal.Number<int>().Between(L('['), L(']'));
var padded = Literal.Number<int>().Between(S);

Console.WriteLine(bracketed.Parse("[42]").Value); // 42
Console.WriteLine(padded.Parse(" 42 ").Value);   // 42
Console.WriteLine(bracketed.Parse("[42").Success); // False
```

`Between` requires both surrounding parsers. If any part fails, it moves the
input position back to the start. The surrounding values are thrown away.

## Choice and Or: try alternatives

`Choice(p1, p2, ...)` tries alternatives in the given order and keeps the first
one that succeeds. `p1.Or(p2)` is the two-parser form. All alternatives must
produce the same type. A choice needs at least one alternative.

```csharp
var color = Choice(L("red"), L("green"), L("blue"));
Console.WriteLine(color.Parse("green").Value); // green

var answer = L("yes").Or(L("no"));
Console.WriteLine(answer.Parse("no").Value); // no

var shortFirst = Choice(L("<"), L("<="));
Console.WriteLine(shortFirst.Parse("<=").Value); // <
Console.WriteLine(shortFirst.ThenIgnore(Eof).Parse("<=").Success); // False
```

Once an alternative succeeds, a later failure outside the choice does not make
it try the other alternatives. Put `L("<=")` before `L("<")`, or use
`OneOf("<", "<=")`, which tries longer words first.

A normal failure lets the choice try the next alternative. A
[fatal error](errors.md#fail-and-fatalerror-stop-parsing) stops the whole parse,
and the choice does not try the remaining alternatives.

## Optional: keep track of whether a value was present

`parser.Optional()` returns `Parser<OptionalValue<T>>`. When the parser fails
normally, `Optional` succeeds without reading anything and returns
`HasValue == false`. When the parser matches, `HasValue` is `true` and `Value`
holds the result.

```csharp
var sign = L('-').Optional();

var present = sign.Parse("-42").Value;
var absent = sign.Parse("42").Value;

Console.WriteLine(present.HasValue); // True
Console.WriteLine(present.Value);    // -
Console.WriteLine(absent.HasValue);  // False
```

Check `HasValue` before you use `Value`. Absence is different from successfully
parsing `default(T)` or `null`. If you call `Optional()` on a parser that already
returns `OptionalValue<T>`, you get the same parser back. It does not add a second
wrapper.

## DefaultOnFail: supply a fallback value

`parser.DefaultOnFail(value)` returns the parsed value on success and the
fallback when the parser fails normally. The overload without an argument uses
`default(T)`.

```csharp
var count = Literal.Number<int>().DefaultOnFail(10);

Console.WriteLine(count.Parse("3").Value);        // 3
Console.WriteLine(count.Parse("").Value);         // 10
Console.WriteLine(count.Parse("missing").Length); // 0

var zero = Literal.Number<int>().DefaultOnFail();
Console.WriteLine(zero.Parse("").Value); // 0
```

This method does not skip invalid text. When the parser fails, the text stays
unread. For example, if you add `ThenIgnore(Eof)`, then `"missing"` fails. Use
`Optional` when you need to tell absence from an actual fallback value.

Neither `Optional` nor `DefaultOnFail` catches fatal errors or exceptions from
your functions.

## Return: succeed without reading text

`Return(value)` always succeeds and returns that value without moving the
input position. It is useful as an explicit fallback in a choice.

```csharp
var enabled = L("on").Do(_ => true).Or(Return(false));

Console.WriteLine(enabled.Parse("on").Value); // True
Console.WriteLine(enabled.Parse("").Value);   // False
Console.WriteLine(Return(42).Parse("").Length); // 0
```

A `Return` alternative hides all later alternatives, because it always succeeds.
Usually it belongs last. It also reads no text, which matters in repeated and
recursive parsers.

## And and Not: check what comes next

These are lookahead parsers: they test a parser and leave the input position
where it was. Both return `Unit`. They do not record error messages for the
parser that they check.

| Method        | Succeeds when                       |
| ------------- | ----------------------------------- |
| `And(parser)` | The supplied parser succeeds.       |
| `Not(parser)` | The supplied parser fails normally. |

```csharp
var beforeColon = Set("a-z").OneOrMore().Text().ThenIgnore(And(L(':')));
var result = beforeColon.Parse("name:value");

Console.WriteLine(result.Value);  // name
Console.WriteLine(result.Length); // 4

var keyword = L("if").ThenIgnore(Not(Set("A-Za-z0-9_"))).As("'if'");
Console.WriteLine(keyword.Parse("if(").Success);   // True
Console.WriteLine(keyword.Parse("iffy").Success);  // False
```

`L("if")` alone also matches the start of `"iffy"`. The negative check adds a
keyword boundary. A failed lookahead records no error message of its own. Without
a name, the result is only `Parsing failed.`. Use `As` to give the parser a useful
error name:

```csharp
var unnamed = L("if").ThenIgnore(Not(Set("A-Za-z0-9_")));
var named = unnamed.As("'if'");

Console.WriteLine(unnamed.Parse("iffy").ErrorMessage); // Parsing failed.
Console.WriteLine(named.Parse("iffy").ErrorMessage);   // (1:1) Expected 'if'
```

Next: [Repetition and lists](repetition-and-lists.md).
