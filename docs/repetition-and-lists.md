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

```csharp
var item = Literal.Number<int>().ThenIgnore(S);
var comma = L(',').ThenIgnore(S);
var list = item.Separated(comma)
    .Between(L('[').ThenIgnore(S), L(']'))
    .ThenIgnore(Eof);
// "[1, 2, 3]" => List<int> { 1, 2, 3 }; "[]" => an empty list

var comment = Any.Until(L("*/")).Text()
    .Between(L("/*"), L("*/"));
// "/*note*/" => "note"; Between consumes the terminator left by Until
```
