# Writing your own parser

[Documentation index](README.md)

Most grammars need only the built-in methods. This page is for the rare case when
you need your own matching logic. It also shows `ParseContext`, the object that
holds the input and the current position. The examples use the
[standard imports](README.md#reading-the-examples).

## ParseContext: share a position between parsers

The low-level `TryParse(ref ParseContext, out value)` overload uses a context that
you supply. Unlike the top-level methods, it does not catch exceptions, and it
does not reset the output on every kind of failure. Use its Boolean result to
decide whether the output is valid.

```csharp
var context = new ParseContext("12:34");
var number = Literal.Number<int>();

if (number.TryParse(ref context, out var first)
    && L(':').TryParse(ref context, out _)
    && number.TryParse(ref context, out var second))
{
    Console.WriteLine($"{first}, {second}"); // 12, 34
    Console.WriteLine(context.Position);    // 5
}
```

| Member                                              | Use                                                                                                                         |
| --------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| `Source`                                            | Read the whole input span.                                                                                                  |
| `Remaining`                                         | Read the span from the current position to the end.                                                                         |
| `Position`                                          | Read the current zero-based offset.                                                                                         |
| `MatchedSegment`                                    | Read the segment that a parser marked most recently.                                                                        |
| `Advance(count)`                                    | Read `count` characters and mark them as the matched segment. `count` must be from 0 to `Remaining.Length`.                 |
| `SetMatched(index, length)`                         | Mark the segment that `Map` and `Text` receive, without changing the position. The segment must lie inside `Source`.        |
| `BookmarkPosition()`                                | Save a `PositionBookmark` so that you can go back.                                                                          |
| `RestorePosition(bookmark)`                         | Go back to the saved position and make the matched segment empty. It does not undo error messages or the work of callbacks. |
| `ReportExpected(text)` / `ReportExpected(string[])` | Record what was expected at the current position, when error messages are on.                                               |
| `DiagnosticState`                                   | Get or set `Normal` or `Suppressed`. Error messages are recorded only in the `Normal` state.                                |
| `SuppressDiagnostics()`                             | Turn error messages off and return the previous state.                                                                      |
| `SuppressDiagnosticsIfNamed(name)`                  | Turn error messages off when the name is not `null`, and return the previous state.                                         |
| `RestoreDiagnosticState(state)`                     | Restore a saved state.                                                                                                      |
| `ToString()`                                        | Format the recorded expectations with a line and column. An empty string means that nothing was recorded.                   |

Error messages may remain from an attempt that failed, even when a later parser
succeeds. Do not use `context.ToString()` to decide whether parsing succeeded. Use
the Boolean result from `TryParse`.

`ParseContext` and `Match` are ref structs that hold spans. Use them only inside
the current synchronous parse. You cannot store them in normal heap objects.

## Write a custom parser

To write your own matching logic, derive from `Parser<T>` and implement
`TryParse`.

This parser reads exactly two ASCII digits and returns their integer value:

```csharp
public sealed class TwoDigitParser : Parser<int>
{
    public TwoDigitParser() : base("two ASCII digits")
    {
    }

    public override bool TryParse(ref ParseContext context, out int value)
    {
        var source = context.Remaining;

        if (source.Length >= 2
            && source[0] >= '0' && source[0] <= '9'
            && source[1] >= '0' && source[1] <= '9')
        {
            value = (source[0] - '0') * 10 + source[1] - '0';
            context.Advance(2);
            return true;
        }

        value = default;
        context.ReportExpected(Name);
        context.Advance(0);
        return false;
    }
}
```

After you add that class to your app, use it like any other parser:

```csharp
var number = new TwoDigitParser().ThenIgnore(Eof);

Console.WriteLine(number.Parse("42").Value);      // 42
Console.WriteLine(number.Parse("4x").ErrorMessage); // (1:1) Expected two ASCII digits
Console.WriteLine(number.Parse("123").Success);   // False
```

The rules for `TryParse`:

- **On success**, move the context forward by the length you read, and set the
  value. Mark the segment that `Map` and `Text` should see. `Advance` does both
  for a single segment.
- **On failure**, leave the position where the parser started, set the output to
  `default`, and return `false`. If you did not move, call `context.Advance(0)`.
  This makes the matched segment empty, so a failed attempt does not leave an older
  match behind. If you already moved, go back with a bookmark: `RestorePosition`
  also makes the segment empty. If the expectation belongs at the failed position,
  record it with `ReportExpected` before you go back.
- **Fatal error**: call `FatalError(message)` only when the parse must stop
  completely. A mismatch is a failure, not a fatal error.

## Call other parsers and go back on failure

A custom parser can call other parsers with the same context. Each successful
call moves the position. So when a later step fails, the parser must go back to
where it started. Save a bookmark before the first step.

This parser reads a 24-hour time such as `09:30`. It reuses `TwoDigitParser`
from the previous section and checks the range of the hours and minutes:

```csharp
public sealed class ClockTimeParser : Parser<TimeSpan>
{
    private static readonly Parser<int> TwoDigits = new TwoDigitParser();
    private static readonly Parser<char> Colon = L(':');

    public ClockTimeParser() : base("time")
    {
    }

    public override bool TryParse(ref ParseContext context, out TimeSpan value)
    {
        var start = context.BookmarkPosition();

        if (TwoDigits.TryParse(ref context, out var hours)
            && Colon.TryParse(ref context, out _)
            && TwoDigits.TryParse(ref context, out var minutes)
            && hours < 24
            && minutes < 60)
        {
            value = new TimeSpan(hours, minutes, 0);
            context.SetMatched(start.Position, context.Position - start.Position);
            return true;
        }

        // Go back first, so that "time" is reported at the start of the input.
        context.RestorePosition(start);
        context.ReportExpected(Name);
        value = default;
        return false;
    }
}
```

```csharp
var time = new ClockTimeParser().ThenIgnore(Eof);

Console.WriteLine(time.Parse("09:30").Value);        // 09:30:00
Console.WriteLine(time.Parse("25:00").ErrorMessage); // (1:1) Expected time
Console.WriteLine(time.Parse("12-30").ErrorMessage); // (1:3) Expected ':'

var text = new ClockTimeParser().Text();
Console.WriteLine(text.Parse("09:30").Value);        // 09:30
```

Three details matter here:

- Each inner parser marks its own segment. After the last call, the matched
  segment holds only the minutes. `SetMatched` marks the whole time, so `Text()`
  returns `09:30`.
- For `25:00`, the parser has already read all five characters when the range
  check fails. `RestorePosition` moves back to the start and makes the matched
  segment empty.
- The inner parsers record their own expectations, such as `':'` for `12-30`.
  The message uses the furthest position, so that precise expectation wins over
  `time` at the start.

## Naming and void conversion

The base class has two methods that other methods call: `ToNamedParser(string? name)`
is used by `As`, and `ToVoidParser()` is used by `Void` and by many combinators.
The default versions wrap your parser, which is enough for most custom parsers.

You can override them (as `protected override`) to make your parser faster, for
example to skip building a value that is not needed. If you do, keep the same
matching behavior and the same checks as the normal version. A void parser must
accept and reject exactly the same input.

Next: [Complete examples](examples.md).
