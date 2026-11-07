# Combining parsers and transforming values

[Parser guide](README.md)

Build sequences and alternatives, then transform or discard their results.

## Combining parsers

| Combinator                     | Purpose                                                                                                        |
| ------------------------------ | -------------------------------------------------------------------------------------------------------------- |
| `Seq(a, b, ...)`               | Run parsers in order and return their values as a tuple. Sequences of `Unit` parsers return `Unit`.            |
| `a.Then(b)`                    | Run both parsers; keep the value from `b`.                                                                     |
| `a.ThenIgnore(b)`              | Run both parsers; keep the value from `a`.                                                                     |
| `p.Between(open, close)`       | Parse an opening delimiter, `p`, and a closing delimiter; keep the value from `p`.                             |
| `p.Between(around)`            | Use the same parser before and after `p`.                                                                      |
| `Choice(a, b, ...)`, `a.Or(b)` | Try alternatives in order and return the first successful result. Alternatives must have the same result type. |

```csharp
var boolean = Choice(
    L("true").Do(_ => true),
    L("false").Do(_ => false));

var parenthesized = Literal
    .Number<int>()
    .Between(L('('), L(')'));
// "(42)" => 42
```

Whitespace is handled explicitly: `p.ThenIgnore(S)` accepts whitespace after
`p`, while `p.Between(S, S)` accepts it on both sides. Alternatives are ordered:
`L("a").Or(L("ab"))` accepts the prefix `"a"` of `"ab"`.

## Working with results

| Combinator                     | Purpose                                                                                         |
| ------------------------------ | ----------------------------------------------------------------------------------------------- |
| `p.Do(value => ...)`           | Transform the parsed value. Overloads unpack tuples produced by `Seq`.                          |
| `p.Map(match => ...)`          | Transform the matched source segment using its text, index, and length.                         |
| `p.Map((match, value) => ...)` | Transform both the matched segment and the parsed value.                                        |
| `p.Text()`                     | Return the matched source text as a string.                                                     |
| `p.Void()`                     | Discard the value and return `Unit`.                                                            |
| `Return(value)`                | Succeed with a constant value without consuming input.                                          |
| `p.Where(predicate, expected)` | Accept the parsed value only when a condition holds; otherwise report the expected description. |

```csharp
var pair = Seq(
    Literal.Number<int>(),
    L(':'),
    Literal.Number<int>()
    ).Do((left, _, right) => (Left: left, Right: right));
// "12:34" => (Left: 12, Right: 34)
```
