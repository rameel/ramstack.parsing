# Repetition and lists

[Parser guide](README.md)

Read repeated items, separated lists, optional values, and text up to a terminator.

| Combinator                   | Purpose                                                                                   |
| ---------------------------- | ----------------------------------------------------------------------------------------- |
| `p.ZeroOrMore()`, `p.Many()` | Read zero or more items. An empty result is allowed.                                      |
| `p.OneOrMore()`              | Read at least one item.                                                                   |
| `p.AtLeast(n)`               | Read at least `n` items.                                                                  |
| `p.Repeat(n)`                | Read exactly `n` items.                                                                   |
| `p.Repeat(min, max)`         | Read between `min` and `max` items, inclusive.                                            |
| `p.Separated(separator)`     | Read items separated by another parser; discard separator values.                         |
| `p.Until(terminator)`        | Read items until the terminator matches. Require the terminator, but leave it unconsumed. |
| `p.Optional()`               | Accept an absent item; return `OptionalValue<T>` with `HasValue` and `Value`.             |
| `p.DefaultOnFail(value)`     | On ordinary failure, succeed with a fallback value. With no argument, use `default(T)`.   |

Repetition normally returns `List<T>`. Repeating a `Parser<Unit>` with methods
such as `ZeroOrMore` or `Repeat` returns `Unit` instead.

`Separated` allows an empty list by default. Use `min` and `max` to constrain its
size and `allowTrailing: true` to accept a final separator.

## Repeated items

Use `Text` to turn repeated character matches into a single string:

```csharp
var code = Set("0-9").Repeat(4).Text().ThenIgnore(Eof);

Console.WriteLine(code.Parse("1234").Value);  // 1234
Console.WriteLine(code.Parse("123").Success); // False
Console.WriteLine(code.Parse("12345").Success); // False
```

Repetition is greedy: it reads as much as possible and does not give matches
back to a later parser. Use `Until` when a following marker must remain unread:

```csharp
var greedy = Any.OneOrMore().ThenIgnore(L(".txt"));
var filename = Any.Until(L(".txt")).Text().ThenIgnore(L(".txt"));

Console.WriteLine(greedy.Parse("notes.txt").Success); // False
Console.WriteLine(filename.Parse("notes.txt").Value); // notes
```

Repetition bounds must satisfy `min >= 0`, `max > 0`, and `min <= max`.
`Repeat(0)` is invalid; `AtLeast(0)` works like `ZeroOrMore`. Invalid bounds throw
when the parser is built. If the minimum is not met, repetition fails and restores
its starting position.

## Lists and terminators

```csharp
// "[1, 2, 3]" => List<int> { 1, 2, 3 }; "[]" => an empty list
var item = Literal.Number<int>().ThenIgnore(S);
var comma = L(',').ThenIgnore(S);
var list = item.Separated(comma)
    .Between(L('[').ThenIgnore(S), L(']'))
    .ThenIgnore(Eof);

// "/*note*/" => "note"; Between consumes the terminator left by Until
var comment = Any.Until(L("*/")).Text().Between(L("/*"), L("*/"));
```

`Separated` counts items, not separators. With `allowTrailing: false`, a separator
without a following item is left unread. A closing delimiter or `Eof` then rejects
it. Without either check, the list can still succeed with a shorter prefix.

```csharp
var list = Literal.Number<int>()
    .Separated(L(','), allowTrailing: true, min: 1, max: 3)
    .ThenIgnore(Eof);

Console.WriteLine(list.Parse("1,2,").Success);    // True
Console.WriteLine(list.Parse("").Success);        // False
Console.WriteLine(list.Parse("1,2,3,4").Success); // False
```

`Until` checks the terminator before each item. Missing the terminator is a failure,
even at the end of input, unless the terminator accepts that position.
For a line comment, `Any.Until(Eol).Text().ThenIgnore(Eol)` accepts both a line
break and the end of the input. The block-comment example above does not handle nested comments.

## Optional values

`Optional` preserves whether a value was present. `DefaultOnFail` supplies a replacement instead:

```csharp
var marker = L('!').Optional().ThenIgnore(Eof);
Console.WriteLine(marker.Parse("!").Value.HasValue); // True
Console.WriteLine(marker.Parse("").Value.HasValue);  // False

var count = Literal.Number<int>().DefaultOnFail(10).ThenIgnore(Eof);
Console.WriteLine(count.Parse("").Value);   // 10
Console.WriteLine(count.Parse("12").Value); // 12
```

Check `HasValue` before reading an optional value. `DefaultOnFail` leaves invalid
text unread: the `count` parser above still rejects `"missing"` because of `Eof`.
Neither method catches forced termination or callback exceptions.

## Empty matches in repetition

Prefer an item parser that consumes text whenever it succeeds.
Making the whole item optional can introduce an extra empty item:

```csharp
var items = L('x').Optional().ZeroOrMore();
Console.WriteLine(items.Parse("xxb").Value!.Count); // 3
Console.WriteLine(L('x').ZeroOrMore().Parse("xxb").Value!.Count); // 2
```

`Repeat` stops after an empty match once its minimum is met; that match can count
as an item. `Separated` stops when an item and its following separator together
consume nothing, once the minimum is met. `Until` fails if its item consumes
nothing before the terminator matches. These checks prevent endless loops,
but an optional part usually belongs inside an item that otherwise consumes text.
