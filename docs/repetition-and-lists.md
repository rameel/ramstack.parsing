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
