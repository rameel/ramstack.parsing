# Working with values

[Documentation index](README.md)

A parser can return a value, keep the source text, or only check that the input
matches. These methods let you choose. The examples use the
[standard imports](README.md#reading-the-examples).

| Method | Callback receives | Result |
| --- | --- | --- |
| `Do(value => ...)` | The parsed value. | The callback's return value. |
| `Where(value => ..., expected)` | The parsed value. | The same value, or a normal failure when the callback returns `false`. |
| `Map(match => ...)` | The matched segment. | The callback's return value. The original parsed value is thrown away. |
| `Map((match, value) => ...)` | Both the matched segment and the parsed value. | The callback's return value. |
| `Text()` | No callback needed. | The matched segment as a string. |
| `Void()` | No callback needed. | `Unit`. The value is thrown away. |

The *matched segment* is the part of the input that a parser matched.

## Do: change the parsed value

`Do` changes a value after the parser succeeds. Despite its name, it takes a
function that returns a value, not an `Action`.

```csharp
var doubled = Literal.Number<int>().Do(n => n * 2);
Console.WriteLine(doubled.Parse("21").Value); // 42

var pair = Seq(Literal.Number<int>(), L(':'), Literal.Number<int>())
    .Do((hours, _, minutes) => new TimeSpan(hours, minutes, 0));

Console.WriteLine(pair.Parse("2:30").Value); // 02:30:00
```

The tuple overloads let a callback take 2 through 10 values as separate
parameters. They are useful after `Seq`. If you return `false` from `Do`,
`false` just becomes the successful result. To reject a value, use `Where`.

## Where: check a parsed value

`parser.Where(predicate, expected)` accepts the result only if the predicate
returns `true`. If it returns `false`, the parser moves the input position back
to the start and reports `expected` there. The description must be non-empty.

```csharp
var port = Literal.Number<int>()
    .Where(n => n >= 1 && n <= 65535, "valid port")
    .ThenIgnore(Eof);

Console.WriteLine(port.Parse("8080").Success); // True
Console.WriteLine(port.Parse("0").Success);    // False
Console.WriteLine(port.Parse("0").ErrorMessage); // (1:1) Expected valid port
```

Use `Where` for ranges, allowed values, and other checks on a parsed value. A
rejection is a normal failure, so `Or` can try another parser. An exception from
the predicate is an exception, not a rejection. Unlike `Do`, the predicate runs
even when the value is thrown away; see
[below](#callbacks-and-thrown-away-values).

## Map: use the matched segment

The one-argument form maps the matched text and throws away the old value. For
example, you can convert digits directly from their span:

```csharp
var number = Set("0-9").OneOrMore()
    .Map(m => int.Parse(m.Value, NumberStyles.None, CultureInfo.InvariantCulture));

Console.WriteLine(number.Parse("123").Value); // 123
```

For ordinary numbers, prefer `Literal.Number<T>()`. It treats a conversion
problem as a normal failure. The `int.Parse` call above throws an exception when
the number is too large.

The two-argument form keeps the old value too. It is useful for adding source
locations or keeping the original spelling:

```csharp
var number = Literal.Number<int>()
    .Map((m, value) => (Value: value, Start: m.Index, Width: m.Length));

var located = L("id=").Then(number);
var result = located.Parse("id=042");

Console.WriteLine(result.Value.Value); // 42
Console.WriteLine(result.Value.Start); // 3
Console.WriteLine(result.Value.Width); // 3
```

## Match: a view of the source

`Match` is a `readonly ref struct`. It gives a view of a part of the source
without first making a string:

| Member | Meaning |
| --- | --- |
| `Index` | Zero-based start offset in the source passed to this parse. |
| `Length` | Number of UTF-16 code units in the segment. |
| `Value` | A `ReadOnlySpan<char>` over the segment. |
| `ToString()` | Copies the segment into a string. |

`Match` can also be converted to `ReadOnlySpan<char>` or deconstructed into
`(index, length)`. You cannot store it in a normal heap object. If you need the
text after the callback ends, copy the text, or store the index and length.

## Text: return source text

`Text()` is a shortcut for mapping the matched segment to a string. It keeps
the exact source spelling and does not decode escape sequences.

```csharp
var identifier = Set("A-Za-z_")
    .ThenIgnore(Set("A-Za-z0-9_").ZeroOrMore());

Console.WriteLine(identifier.Parse("user42").Value); // u

// Seq marks the whole sequence as its matched segment.
var fullIdentifier = Seq(
    Set("A-Za-z_"),
    Set("A-Za-z0-9_").ZeroOrMore()).Text();

Console.WriteLine(fullIdentifier.Parse("user42").Value); // user42
```

The matched segment and the amount of input read are different things:

- `Seq`, repetition, lists, and folds mark their whole successful span.
- `Then` keeps the segment from its second parser.
- `ThenIgnore` keeps the segment from its first parser.
- `Between` keeps the segment from the middle parser.

For example, a token can read spaces but leave them out of `Text()`:

```csharp
var token = L("hello").ThenIgnore(S);
Console.WriteLine(token.Text().Parse("hello   ").Value); // hello
Console.WriteLine(token.Parse("hello   ").Length);      // 8

var withSpaces = Seq(L("hello"), S).Text();
Console.WriteLine(withSpaces.Parse("hello   ").Value!.Length); // 8
```

Put `Text` or `Map` at the point in the grammar whose segment you want.
`ParseResult.Length` always counts the input read by the whole parse.

## Void and Unit: throw a value away

`Void()` returns a `Parser<Unit>`. `Unit.Value` is an empty value. It is used when
a parser only needs to read or check input.

```csharp
var punctuation = Seq(L('('), L(')')).Void();
Console.WriteLine(punctuation.Parse("()").Success); // True
Console.WriteLine(punctuation.Parse("()").Length);  // 2
```

## Callbacks and thrown-away values

`Void` is not the only method that throws a value away. Others do it too:

- `Then` throws away the first value, and `ThenIgnore` the second.
- `Between` throws away the surrounding values.
- `And` and `Not` throw away the value of the parser they check.
- `Separated` throws away the separators, and `Until` the terminator.
- `Text` and the one-argument `Map` throw away the inner value.

This avoids creating strings, lists, tuples, and other result objects that
nobody reads. It also means that the library may skip a callback in `Do`, `Map`,
a fold, or `Prefixed` when its result is thrown away. For example, the callback
below is skipped:

```csharp
var calls = 0;
var token = L('x').Do(c =>
{
    calls++;
    return c;
});

Console.WriteLine(token.Void().Parse("x").Success); // True
Console.WriteLine(calls);                            // 0
```

`Where` is different. Its predicate decides whether the parser matches, so it
runs even when the value is thrown away:

```csharp
var nonZero = Set("0-9").Where(c => c != '0', "non-zero digit");
Console.WriteLine(nonZero.Void().Parse("0").Success); // False
```

A callback that does run may run during an attempt that later fails. Moving the
input position back does not undo changes to external state. So keep callbacks
focused on building values, put checks in `Where`, and do external actions, such
as logging or counting, after a successful top-level parse.

Next: [Recursive parsers and expressions](recursive-parsers.md).
