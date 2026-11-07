# Running parsers, lookahead, and errors

[Parser guide](README.md)

Run a parser, inspect its result, check upcoming input, and report failures.

## Running a parser

`p.Parse(source)` returns `ParseResult<T>`: check `Success` before reading `Value`.
`Length` reports the consumed length on success; `ErrorMessage` describes a
failure. Forced termination produces an error message; other callback exceptions
are also captured in `Exception`.

`p.TryParse(source, out value)` returns a boolean and suppresses diagnostics.
Forced termination returns `false`, while other callback exceptions propagate.
Both entry points accept `ReadOnlySpan<char>`, including strings via conversion.

```csharp
var parser = Literal.Number<int>().Between(S, S).ThenIgnore(Eof);
var result = parser.Parse(" 42 "); // Success: true, Value: 42
var invalid = parser.Parse("42!"); // Success: false
```

A successful parse can match only a prefix. Add `ThenIgnore(Eof)` when the entire
input must match. Build parsers once and reuse them.

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

`Optional` and `DefaultOnFail` handle ordinary failures; they do not recover from
forced termination.

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
