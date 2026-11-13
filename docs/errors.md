# Errors

[Documentation index](README.md)

There are three kinds of problems when you parse:

- A **failure** is a normal part of parsing. A choice may try another parser, or a
  repetition may stop.
- A **fatal error** stops the whole parse. You create it with `Fail` or
  `FatalError`. Nothing recovers from it.
- An **exception** from your own function is a third case.

The examples use the [standard imports](README.md#reading-the-examples).

## How error messages look

When `Parse` fails, it builds an error message from what the parsers expected at
the furthest position they reached. The message has the form
`(line:column) Expected ...`. Line and column numbers start at 1. A column counts
`char` values, and a tab counts as one column. If several things are expected,
the message joins them:

```csharp
var abc = Choice(L("a"), L("b"), L("c"));
Console.WriteLine(abc.Parse("x").ErrorMessage); // (1:1) Expected 'a', 'b', or 'c'

var ab = Seq(L("a"), L("b"));
Console.WriteLine(ab.Parse("ax").ErrorMessage); // (1:2) Expected 'b'
```

The message points to the furthest position that any parser reached. It is not
always the place where the whole attempt started:

```csharp
var sum = Literal.Number<int>().FoldL(L('+'), (a, b, _) => a + b).ThenIgnore(Eof);

Console.WriteLine(sum.Parse("1+").ErrorMessage); // (1:3) Expected number
```

The fold read `1+` and then looked for a number at column 3. That message is more
useful than "expected end of input" at column 2.

Every built-in parser has a default name for its messages:

| Parser | Name in the message |
| --- | --- |
| `L('a')`, `L("hello")` | `'a'`, `'hello'` |
| `OneOf("yes", "no")` | `'yes'` and `'no'`, as two separate items. |
| `OneOf("+-")`, `Set("a-z")` | The sorted set: `[+-]`, `[a-z]`. A pattern such as `A-Za-z0-9_` shows as `[0-9A-Z_a-z]`. |
| `Literal.Number<int>()` | `number` |
| `Literal.Number<double>()` | `floating-point number` |
| `NumberKind.HexNumber`, `NumberKind.BinaryNumber` | `hexadecimal number`, `binary number` |
| `Literal.DoubleQuotedString`, `Literal.SingleQuotedString` | `double-quoted string`, `single-quoted string` |
| `Literal.QuotedCharacter` | `quoted-character` |
| `Literal.EscapeSequence`, `Literal.UnicodeEscapeSequence` | `escape sequence`, `unicode escape` |
| `Eof`, `Eol`, `Any` | `end of input`, `end of line`, `any character` |
| `Character.Letter`, `Character.Digit`, `Character.WhiteSpace` | `letter`, `digit`, `whitespace` |
| `Regex(...)` | The text of the pattern. |

```csharp
Console.WriteLine(L("hello").Parse("x").ErrorMessage);                // (1:1) Expected 'hello'
Console.WriteLine(Set("A-Za-z0-9_").Parse("!").ErrorMessage);         // (1:1) Expected [0-9A-Z_a-z]
Console.WriteLine(Literal.Number<double>().Parse("x").ErrorMessage);  // (1:1) Expected floating-point number
Console.WriteLine(L("hello").ThenIgnore(Eof).Parse("hello world").ErrorMessage); // (1:6) Expected end of input
```

Use `As` (below) to replace a default name with a better one.

If a parser fails and no parser recorded what it expected, `Parse` returns
`Parsing failed.`. This happens, for example, when `Not` fails, because `Not` and
`And` do not record error messages:

```csharp
Console.WriteLine(Not(L("a")).Parse("a").ErrorMessage); // Parsing failed.
```

## As: give a parser a useful name

`parser.As(name)` returns a parser with a name for error messages. The name must
be non-empty. `As` leaves the original parser unchanged, and the result type
stays the same.

```csharp
var identifier = Seq(
    Set("A-Za-z_"),
    Set("A-Za-z0-9_").ZeroOrMore())
    .Text()
    .As("identifier");

Console.WriteLine(identifier.Parse("123").ErrorMessage); // (1:1) Expected identifier
```

A named parser hides the error messages of the parsers inside it. When it fails,
it reports its own name. Name the useful units of your grammar, such as
`identifier` or `number`, to make errors easier to read. Naming a very large
parser can hide helpful details about the exact token that was missing.

## Error: report a failure

`Error<T>(expected)` always fails normally and records what was expected. It reads
no text, and an alternative can still succeed after it.

```csharp
var letter = Error<char>("letter");
Console.WriteLine(letter.Parse("1").ErrorMessage); // (1:1) Expected letter

var fallback = Error<char>("letter").Or(L('1'));
Console.WriteLine(fallback.Parse("1").Value); // 1
```

It is useful for adding an explicit "missing" message to one branch of a
grammar. `Error` does not make a whole repetition or optional parser fail.

## Fail and FatalError: stop parsing

`Fail<T>(message)` creates a parser that stops the parse when it is reached.
`FatalError(message)` does the same from inside your own function. The message must
be non-empty. A choice, an optional parser, or a repetition does not recover from a
fatal error.

```csharp
var stopped = Fail<char>("Stopped here.").Or(L('x'));

Console.WriteLine(stopped.Parse("x").ErrorMessage); // (1:1) Stopped here.
Console.WriteLine(stopped.TryParse("x", out _));    // False
```

```csharp
var number = Literal.Number<int>().Do(n =>
{
    if (n == 0)
        FatalError("Zero is not allowed.");

    return n;
});

Console.WriteLine(number.Parse("0").ErrorMessage); // (1:2) Zero is not allowed.
```

The position in a fatal error is the current position. In the second example,
the number has already been read, so the column is 2. A callback can be skipped
when its value is thrown away, so use `Where` for checks that must always run.

## When your own code throws

`Parse` and `TryParse` treat exceptions differently:

| Situation | `Parse(source)` | `TryParse(source, out value)` |
| --- | --- | --- |
| Failure | Failed result with `ErrorMessage`. `Exception` is `null`. | Returns `false`. |
| `Fail` or `FatalError` | Failed result with your message and the current location. `Exception` is `null`. | Returns `false`. |
| Any other exception thrown while parsing | Failed result with `ErrorMessage` and the caught `Exception`. | The exception reaches the caller. |

A failed top-level result has `Length == 0` and a default value. A failed
top-level `TryParse` call also sets the output to `default(T)`.

```csharp
var parser = L('x').Do<char, int>(
    _ => throw new InvalidOperationException("Cannot build the value."));

var result = parser.Parse("x");
Console.WriteLine(result.Success); // False
Console.WriteLine(result.Exception is InvalidOperationException); // True

var startsLikeThis = result.ErrorMessage!.StartsWith(
    "(1:2) System.InvalidOperationException: Cannot build the value.");
Console.WriteLine(startsLikeThis); // True

try
{
    parser.TryParse("x", out _);
}
catch (InvalidOperationException)
{
    Console.WriteLine("Caught by the caller."); // Caught by the caller.
}
```

When `Parse` catches an exception, `ErrorMessage` contains the location, the
exception type, its message, and the stack trace (see `startsLikeThis` above). Do
not show this text to end users. Show your own message, and log `result.Exception`
instead.

## Mistakes while building a parser

These rules apply while a parser runs. Mistakes in how you build a parser throw an
exception right away, and the exception reaches the caller:

| Call | Exception |
| --- | --- |
| `Repeat(0)`, `Repeat(0, 0)`, `Separated(..., max: 0)` | `ArgumentOutOfRangeException` |
| `Repeat(3, 2)` or `Separated(..., min: 3, max: 2)` | `ArgumentOutOfRangeException` |
| `Set('z', 'a')` | `ArgumentOutOfRangeException` |
| `Set("")`, `Set("z-a")`, `Set(@"\q")`, `Set(@"\p{IsGreek}")` | `ArgumentException` |
| `L("")`, `OneOf("")`, `OneOf("yes", "")`, `Choice<T>()` with no parsers | `ArgumentException` |
| `As("")`, `Where(..., "")`, `Fail<T>("")` | `ArgumentException` |
| `Regex(...)` with a pattern that does not start with `^` | `ArgumentException` |
| `Literal.Number<T>` with a kind that does not fit `T`, or `BinaryNumber` for `BigInteger` in an app that targets .NET 8 | `ArgumentException` |
| `Literal.Number<char>()`, or another unsupported type | `InvalidOperationException` |
| Assigning `Deferred.Parser` twice | `InvalidOperationException` |
| Assigning `null` to `Deferred.Parser` | `ArgumentNullException` |
| Using a `Deferred` without a parser where its value is thrown away | `InvalidOperationException` |

Next: [Writing your own parser](custom-parsers.md).
