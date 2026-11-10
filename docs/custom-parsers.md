# Writing your own parser

[Parser guide](README.md)

Use the built-in combinators for ordinary grammars. When matching needs custom
logic, derive from `Parser<T>` and implement `TryParse(ref ParseContext, out T)`.

## ParseContext

`ParseContext` holds the input and current position shared by the parsers in one parse.
The low-level `TryParse(ref context, out value)` overload does not catch exceptions;
use its boolean return value to decide whether the output is valid.

| Member                                                   | Purpose                                                                                         |
| -------------------------------------------------------- | ----------------------------------------------------------------------------------------------- |
| `Source`, `Remaining`                                    | Read the whole input span or the unread suffix.                                                 |
| `Position`                                               | Read the current zero-based offset.                                                             |
| `MatchedSegment`                                         | Read the most recently marked `Match`.                                                          |
| `Advance(count)`                                         | Consume characters and mark them as the matched segment. Use a count within `Remaining.Length`. |
| `SetMatched(index, length)`                              | Mark a source segment without changing the position.                                            |
| `BookmarkPosition()`, `RestorePosition(bookmark)`        | Save and restore the position. Restoration also clears the matched segment.                     |
| `ReportExpected(text)`                                   | Record an expectation at the current position.                                                  |
| `SuppressDiagnostics()`, `RestoreDiagnosticState(state)` | Temporarily suppress diagnostics and restore their previous state.                              |

Position restoration does not undo diagnostics or callback side effects.
Use the boolean return value to check success: the context can retain expectations
from an earlier branch that failed. `ParseContext` and `Match` are ref structs
and cannot be stored in ordinary heap objects.

## A parser for two ASCII digits

Add this class to your application:

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

        return false;
    }
}
```

Then use it with the same combinators as a built-in parser:

```csharp
var number = new TwoDigitParser().ThenIgnore(Eof);
Console.WriteLine(number.Parse("42").Value);        // 42
Console.WriteLine(number.Parse("4x").ErrorMessage); // (1:1) Expected two ASCII digits
Console.WriteLine(number.Parse("123").Success);     // False
```

On success, set the value, advance the position, and mark the matched segment.
For a single segment, `Advance` does both position and match updates.

On failure, set the value to `default`, leave the position at the starting point,
and clear the matched segment. If nothing was consumed, `Advance(0)` clears it.
If input was consumed, restore a bookmark. Record an expectation before restoring
when it belongs at the position where matching failed.

The base class supplies wrappers for `As` and `Void`, so custom implementations
of naming and void conversion are usually unnecessary. Any optimized void
conversion must preserve the same accepted input and validation as the value
parser.
