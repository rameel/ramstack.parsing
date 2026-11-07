# Parser guide

A `Parser<T>` reads text and produces a value of type `T`. Small parsers match
individual tokens; combinators join them into lists, expressions, and grammars.
The examples use these imports:

```csharp
using Ramstack.Parsing;
using static Ramstack.Parsing.Parser;
```

## Basic parsers

| Parser | Purpose |
| --- | --- |
| `L('a')` | Match one specific character and return it. |
| `L("hello")` | Match a literal string. Comparison is ordinal by default; a `StringComparison` argument can change it. |
| `OneOf("abc")` | Match one character from the supplied string. |
| `OneOf("if", "else")` | Match one of several literal strings. For ordinal comparisons, prefer the longest matching literal. |
| `Set('a', 'z')` | Match a character in an inclusive range. |
| `Set("a-zA-Z0-9_")` | Match a character from a character class. |
| `L(UnicodeCategory...)`, `L(GeneralUnicodeCategory...)` | Match a character by its Unicode category. |
| `Any` | Match any single character. Fail at the end of input. |
| `S` | Consume zero or more whitespace characters, including line breaks. |
| `Eol` | Consume a line break, or succeed at the end of input. |
| `Eof` | Require the end of input without consuming anything. |
| `Regex(regex)` | Match a regular expression at the current position; its pattern must start with `^`. Available on .NET 7+. |

`Character` provides ready-made character parsers: `Letter`, `Uppercase`,
`Lowercase`, `Digit`, `LetterOrDigit`, `HexDigit`, `WhiteSpace`, and other Unicode
groups. These return `char`; `S`, `Eol`, and `Eof` return `Unit`, representing
success without a meaningful value.

```csharp
var identifier = Seq(
    Set("a-zA-Z_"),
    Set("a-zA-Z0-9_").ZeroOrMore()
).Text();
// "item_42" => "item_42"
```

## Literals

`L` matches fixed text. The `Literal` class parses a value written in text form.

| Parser | Purpose |
| --- | --- |
| `Literal.Number<T>()` | Parse a numeric value, such as `int`, `double`, or `decimal`. |
| `Literal.Number<T>(kind)` | Select a numeric format with `NumberKind`: `Auto`, `Integer`, `Float`, `HexNumber`, or `BinaryNumber` (.NET 8+). Supported formats depend on `T`. |
| `Literal.DoubleQuotedString` | Parse a double-quoted string, remove its quotes, and decode escapes. |
| `Literal.SingleQuotedString` | Parse a single-quoted string with the same value handling. |
| `Literal.QuotedCharacter` | Parse one character enclosed in single quotes. |
| `Literal.EscapeSequence` | Decode an escape sequence such as `\n` into a character. |
| `Literal.UnicodeEscapeSequence` | Decode a Unicode escape such as `\u0041`. |

```csharp
var integer = Literal.Number<int>();      // "42" => 42
var text = Literal.DoubleQuotedString;    // "\"hello\"" => "hello"
```

## Combining parsers

| Combinator | Purpose |
| --- | --- |
| `Seq(a, b, ...)` | Run parsers in order and return their values as a tuple. Sequences of `Unit` parsers return `Unit`. |
| `a.Then(b)` | Run both parsers; keep the value from `b`. |
| `a.ThenIgnore(b)` | Run both parsers; keep the value from `a`. |
| `p.Between(open, close)` | Parse an opening delimiter, `p`, and a closing delimiter; keep the value from `p`. |
| `p.Between(around)` | Use the same parser before and after `p`. |
| `Choice(a, b, ...)`, `a.Or(b)` | Try alternatives in order and return the first successful result. Alternatives must have the same result type. |

```csharp
var boolean = Choice(
    L("true").Do(_ => true),
    L("false").Do(_ => false));

var parenthesized = Literal.Number<int>().Between(L('('), L(')'));
// "(42)" => 42
```

Whitespace is handled explicitly: `p.ThenIgnore(S)` accepts whitespace after
`p`, while `p.Between(S, S)` accepts it on both sides. Alternatives are ordered:
`L("a").Or(L("ab"))` accepts the prefix `"a"` of `"ab"`.

## Repetition and lists

| Combinator | Purpose |
| --- | --- |
| `p.ZeroOrMore()`, `p.Many()` | Read zero or more items. An empty result is allowed. |
| `p.OneOrMore()` | Read at least one item. |
| `p.AtLeast(n)` | Read at least `n` items. |
| `p.Repeat(n)` | Read exactly `n` items. |
| `p.Repeat(min, max)` | Read between `min` and `max` items, inclusive. |
| `p.Separated(separator)` | Read items separated by another parser; discard separator values. |
| `p.Until(terminator)` | Read items until the terminator matches. Require the terminator, but leave it unconsumed. |
| `p.Optional()` | Accept an absent item; return `OptionalValue<T>` with `HasValue` and `Value`. |
| `p.DefaultOnFail(value)` | On ordinary failure, succeed with a fallback value. With no argument, use `default(T)`. |

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

## Working with results

| Combinator | Purpose |
| --- | --- |
| `p.Do(value => ...)` | Transform the parsed value. Overloads unpack tuples produced by `Seq`. |
| `p.Map(match => ...)` | Transform the matched source segment using its text, index, and length. |
| `p.Map((match, value) => ...)` | Transform both the matched segment and the parsed value. |
| `p.Text()` | Return the matched source text as a string. |
| `p.Void()` | Discard the value and return `Unit`. |
| `Return(value)` | Succeed with a constant value without consuming input. |
| `p.Where(predicate, expected)` | Accept the parsed value only when a condition holds; otherwise report the expected description. |

```csharp
var pair = Seq(Literal.Number<int>(), L(':'), Literal.Number<int>())
    .Do((left, _, right) => (Left: left, Right: right));
// "12:34" => (Left: 12, Right: 34)
```

## Recursive parsers

Use recursion for nested structures such as parentheses, arrays, or syntax trees.
`Deferred<T>()` creates a reference whose `.Parser` is assigned once the rule is
built. `Recursive<T>(self => ...)` defines a rule using a self-reference in a
factory function.

```csharp
var nested = Recursive<int>(self =>
    Literal.Number<int>().Or(self.Between(L('('), L(')'))));
// "42", "(42)", and "((42))" => 42

var deferred = Deferred<int>();
deferred.Parser = Literal.Number<int>()
    .Or(deferred.Between(L('('), L(')')));
```

Initialize a deferred rule before parsing or using it in operations that discard
its value, such as `Void`, `And`, or `Not`. Recursive calls must follow input
consumption: a rule that immediately calls itself is left-recursive and is not
supported.

## Operators and expressions

| Combinator | Purpose |
| --- | --- |
| `p.FoldL(op, reduce)` | Parse an operand followed by operator/operand pairs; combine from the left. `Fold` is an alias. |
| `p.FoldR(op, reduce)` | Combine operator/operand pairs from the right. |
| `p.Prefixed(op, reduce)` | Parse zero or more prefix operators before an operand; apply the closest operator first. |

```csharp
var number = Literal.Number<int>().ThenIgnore(S);
var subtraction = number.FoldL(L('-').ThenIgnore(S), (l, r, _) => l - r);
// "10 - 3 - 2" => (10 - 3) - 2 => 5
```

Build precedence in layers: for example, use a product parser as the operand
parser for a sum. See the [calculator](../samples/CalcExpr/README.md).

## Lookahead and errors

| Operation | Purpose |
| --- | --- |
| `And(p)` | Require `p` to match without consuming input. Return `Unit`. |
| `Not(p)` | Require `p` to fail without consuming input. Return `Unit`. |
| `p.As("name")` | Give a parser a readable name for diagnostics. |
| `Error<T>("expected")` | Fail normally and report an expectation; alternatives may still be tried. |
| `Fail<T>("message")` | Force parsing to terminate with an error message. |
| `FatalError("message")` | Force termination from a callback. |

`Optional` and `DefaultOnFail` handle ordinary failures; they do not recover from
forced termination.

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

For complete grammars, see [JSON](../samples/Json/README.md) and
[TinyC](../samples/TinyC/README.md). Installation is covered in the
[project README](../README.md).
