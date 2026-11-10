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

// "(42)" => 42
var parenthesized = Literal
    .Number<int>()
    .Between(L('('), L(')'));
```

Whitespace is handled explicitly: `p.ThenIgnore(S)` accepts whitespace after `p`,
while `p.Between(S, S)` accepts it on both sides.

Alternatives are ordered: `L("a").Or(L("ab"))` accepts the prefix `"a"` of `"ab"`.
Once an alternative succeeds, a later failure outside the choice does not make
it try another alternative. Put longer literals first, or use the string
overloads of `OneOf` for the longest ordinal match.

`Seq`, `Then`, `ThenIgnore`, and `Between` restore their starting position on
ordinary failure, so another branch can try the same input. Restoring the
position does not undo changes made by callbacks.

The choice of sequence combinator determines which values survive:

```csharp
var number = Literal.Number<int>();
var labeled = L("count=").Then(number);
var percent = number.ThenIgnore(L('%'));

Console.WriteLine(labeled.Parse("count=12").Value); // 12
Console.WriteLine(percent.Parse("12%").Value);      // 12

Console.WriteLine(
    Seq(L('a'), L('b')).Parse("ab").Value); // (a, b)
```

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
// "12:34" => (Left: 12, Right: 34)
var pair = Seq(
    Literal.Number<int>(),
    L(':'),
    Literal.Number<int>()
    ).Do((left, _, right) => (Left: left, Right: right));
```

Use `Map` when the matched text matters, and `Where` to validate a value:

```csharp
var digits = Set("0-9").OneOrMore().Map(match => match.Length);
Console.WriteLine(digits.Parse("1234").Value); // 4

var port = Literal.Number<int>()
    .Where(
        value => value >= 1 && value <= 65535,
        "port from 1 to 65535")
    .ThenIgnore(Eof);

Console.WriteLine(port.Parse("8080").Value); // 8080
Console.WriteLine(port.Parse("0").Success);  // False
```

`Do(n => n < 10)` produces a successful boolean result, even when the value is `false`.
Use `Where` to reject a value. Its predicate also runs when the parsed value is discarded.

### Matched text and source locations

The matched segment used by `Text` and `Map` can be smaller than the consumed input
reported by `ParseResult.Length`:

| Parser                          | Matched segment                           |
| ------------------------------- | ----------------------------------------- |
| `Seq`, repetition, lists, folds | The whole successful sequence.            |
| `a.Then(b)`                     | The segment from `b`.                     |
| `a.ThenIgnore(b)`               | The segment from `a`.                     |
| `p.Between(open, close)`        | The segment from `p`, without delimiters. |

```csharp
var token = L("hello").ThenIgnore(S);
Console.WriteLine(token.Text().Parse("hello   ").Value); // hello
Console.WriteLine(token.Parse("hello   ").Length);       // 8
Console.WriteLine(Seq(L("hello"), S).Text().Parse("hello   ").Value!.Length); // 8
```

`Map` receives a `Match` with `Index`, `Length`, and a `ReadOnlySpan<char>` in `Value`.
The two-argument form can attach a source location to a parsed value:

```csharp
var located = L("id=")
    .Then(Literal.Number<int>()
    .Map((match, value) =>
        (Value: value, Start: match.Index, Width: match.Length)));

Console.WriteLine(located.Parse("id=042").Value); // (42, 3, 3)
```

`Match` is a ref struct and cannot be stored in an ordinary heap object.
Keep its offsets or copy the text with `ToString()` if the result needs to outlive the parse.

### Callbacks and discarded values

`Void`, `Then`, `ThenIgnore`, `Between`, lookahead, and separators discard values
that are not needed. `Text` and the one-argument `Map` also discard the inner
value. This can avoid creating intermediate lists or other result objects.

The library may skip `Do`, `Map`, or fold callbacks when their result is discarded.
A callback that runs can belong to an attempt that later fails. Keep callbacks
free of external side effects; put validation in `Where` and perform actions
after the top-level parse succeeds.
