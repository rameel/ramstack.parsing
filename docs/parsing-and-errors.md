# Running parsers, lookahead, and errors

[Parser guide](README.md)

Run a parser, inspect its result, check upcoming input, and report failures.

## Running a parser

`p.Parse(source)` returns `ParseResult<T>`: check `Success` before reading `Value`.
`Length` reports the consumed length on success; `ErrorMessage` describes a
failure. Forced termination produces an error message; other callback exceptions
are also captured in `Exception`.

| Property       | Meaning                                                                                                    |
| -------------- | ---------------------------------------------------------------------------------------------------------- |
| `Success`      | Whether parsing succeeded.                                                                                 |
| `Value`        | The parsed value on success; `default(T)` on failure.                                                      |
| `Length`       | Consumed UTF-16 code units, including discarded spaces and delimiters; zero on failure.                    |
| `ErrorMessage` | An error with a line and column, or `Parsing failed.` when no expectation was recorded; `null` on success. |
| `Exception`    | The caught exception; `null` for ordinary failures and forced parse errors.                                |

`p.TryParse(source, out value)` returns a boolean and suppresses diagnostics.
Forced termination returns `false`, while other callback exceptions propagate.
Both entry points accept `ReadOnlySpan<char>`, including strings via conversion.

```csharp
var parser = Literal.Number<int>().Between(S, S).ThenIgnore(Eof);
var result = parser.Parse(" 42 "); // Success: true, Value: 42
var invalid = parser.Parse("42!"); // Success: false
```

A successful parse can match only a prefix. Add `ThenIgnore(Eof)` when the entire input must match.
Build parsers once and reuse them.

```csharp
var number = Literal.Number<int>();
var prefix = number.Parse("123abc");
Console.WriteLine(prefix.Value);  // 123
Console.WriteLine(prefix.Length); // 3
Console.WriteLine(number.ThenIgnore(Eof).Parse("123abc").Success); // False
```

Use the boolean API when diagnostics are unnecessary:

```csharp
var integer = Literal.Number<int>().ThenIgnore(Eof);
if (integer.TryParse("42", out var value))
{
    Console.WriteLine(value); // 42
}
```

## Lookahead and errors

| Operation               | Purpose                                                                   |
| ----------------------- | ------------------------------------------------------------------------- |
| `And(p)`                | Require `p` to match without consuming input. Return `Unit`.              |
| `Not(p)`                | Require `p` to fail without consuming input. Return `Unit`.               |
| `p.As("name")`          | Give a parser a readable name for diagnostics.                            |
| `Error<T>("expected")`  | Fail normally and report an expectation; alternatives may still be tried. |
| `Fail<T>("message")`    | Force parsing to terminate with an error message.                         |
| `FatalError("message")` | Force termination from a callback.                                        |

`Optional` and `DefaultOnFail` handle ordinary failures; they do not recover from forced termination.

Lookahead can prevent a keyword from matching the beginning of an identifier:

```csharp
var identifierPart = Character.LetterOrDigit.Or(L('_'));
var keyword = L("if").ThenIgnore(Not(identifierPart));

Console.WriteLine(keyword.Parse("if(").Value);       // if
Console.WriteLine(keyword.Parse("iffy").Success);    // False
Console.WriteLine(keyword.Parse("if_else").Success); // False
```

Provide a custom error when a required rule has no match:

```csharp
var required = Literal
    .Number<int>()
    .Or(Fail<int>("An integer is required."));

var result = required.Parse("oops");

Console.WriteLine(result.Success); // False
Console.WriteLine(result.ErrorMessage); // (1:1) An integer is required.
Console.WriteLine(required.TryParse("oops", out _)); // False
```

### Names and error positions

Ordinary failures report expectations at the furthest recorded position.
Line and column numbers start at 1; columns count UTF-16 code units, with a tab counting
as one column. `As` replaces inner expectations with a name for the whole rule:

```csharp
var name = Seq(
    Set("A-Za-z_"),
    Set("A-Za-z0-9_").ZeroOrMore()
    ).Text().As("identifier");

Console.WriteLine(name.Parse("123").ErrorMessage); // (1:1) Expected identifier

var pair = Seq(L('a'), L('b'));
Console.WriteLine(pair.Parse("ax").ErrorMessage);  // (1:2) Expected 'b'
```

Name useful grammar units; naming an entire document can hide which inner token was missing.
Lookahead records no expectations of its own; use `As` to give it a useful failure message.
A fatal error reports the current position instead.

### Ordinary failures and exceptions

| Situation                      | `Parse(source)`                                    | `TryParse(source, out value)`                       |
| ------------------------------ | -------------------------------------------------- | --------------------------------------------------- |
| Ordinary mismatch              | Failed result with diagnostics.                    | Returns `false` and sets the value to `default(T)`. |
| `Fail` or `FatalError`         | Failed result with the supplied message.           | Returns `false` and sets the value to `default(T)`. |
| Other exception during parsing | Failed result with `ErrorMessage` and `Exception`. | The exception propagates to the caller.             |

`Error<T>` is an ordinary failure, so a choice can continue after it:

```csharp
var fallback = Error<char>("letter").Or(L('1'));
Console.WriteLine(fallback.Parse("1").Value); // 1
Console.WriteLine(fallback.Parse("?").ErrorMessage); // (1:1) Expected letter or '1'
```

`ErrorMessage` includes the exception text and stack trace when `Parse` catches a callback exception.
Parser construction errors, such as invalid repetition bounds or assigning a deferred rule twice,
occur before parsing and propagate directly to the caller.
