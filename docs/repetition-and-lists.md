# Repetition and lists

[Documentation index](README.md)

These methods run a parser more than once. They normally return `List<T>`, with
the values in source order. See the [standard imports](README.md#reading-the-examples).

## ZeroOrMore, Many, and OneOrMore

`ZeroOrMore()` reads as many matches as it can, including none. `Many()` is an
alias for the same method. `OneOrMore()` requires at least one match.

```csharp
var digits = Set("0-9");

Console.WriteLine(digits.ZeroOrMore().Parse("").Value!.Count); // 0
Console.WriteLine(digits.Many().Text().Parse("123x").Value);   // 123
Console.WriteLine(digits.OneOrMore().Parse("").Success);      // False
Console.WriteLine(digits.OneOrMore().Text().Parse("123x").Value); // 123
```

Use `.Text()` when you want the matched text instead of a list of characters.
Use `.Void()` when you only need to check or skip the matches.

For a `Parser<Unit>`, these repetition methods have overloads that return
`Parser<Unit>` directly. This avoids a list of empty values:

```csharp
Parser<Unit> spaces = L(' ').Void().ZeroOrMore();
Console.WriteLine(spaces.Parse("   x").Length); // 3
```

## Repeat and AtLeast

| Method | Accepted number of matches |
| --- | --- |
| `Repeat(count)` | Exactly `count`. |
| `Repeat(min, max)` | From `min` to `max`, inclusive. |
| `AtLeast(count)` | At least `count`, with no practical upper limit. |

```csharp
var threeDigits = Set("0-9").Repeat(3).Text().ThenIgnore(Eof);
Console.WriteLine(threeDigits.Parse("123").Value);   // 123
Console.WriteLine(threeDigits.Parse("12").Success);  // False
Console.WriteLine(threeDigits.Parse("1234").Success); // False

var shortWord = Set("a-z").Repeat(2, 4).Text();
Console.WriteLine(shortWord.Parse("abcdef").Value); // abcd

var atLeastTwo = L('x').AtLeast(2).Text();
Console.WriteLine(atLeastTwo.Parse("xxxx!").Value); // xxxx
```

The same methods have `Parser<Unit>` overloads. The counts must obey `min >= 0`,
`max > 0`, and `min <= max`. `Repeat(0)` and `Repeat(0, 0)` are not allowed.
`AtLeast(0)` is allowed and works like `ZeroOrMore()`. A wrong count throws
`ArgumentOutOfRangeException` when you build the parser.

Repetition is greedy: it reads until the parser fails or the maximum is reached.
It never gives matches back to help a later parser succeed. For example,
`Set("a-z").OneOrMore().Then(L('z'))` cannot match `"abz"`: the repeated parser
has already read the `z`.

If fewer than the minimum number of items match, the repetition fails and moves
the input position back to the start.

## Separated: lists with delimiters

`item.Separated(separator, allowTrailing: false, min: 0, max: int.MaxValue)`
reads items with a separator between them. It returns the items and throws the
separators away. By default, an empty list is allowed and a trailing separator is
left unread.

```csharp
var item = Literal.Number<int>().ThenIgnore(S);
var comma = L(',').ThenIgnore(S);

var list = item.Separated(comma).Between(S, Eof);
var result = list.Parse("1, 2, 3");

Console.WriteLine(string.Join(", ", result.Value!)); // 1, 2, 3
Console.WriteLine(list.Parse("").Value!.Count);     // 0
Console.WriteLine(list.Parse("1,").Success);       // False

var withTrailingComma = item
    .Separated(comma, allowTrailing: true, min: 1, max: 3)
    .Between(S, Eof);

Console.WriteLine(withTrailingComma.Parse("1, 2,").Success); // True
Console.WriteLine(withTrailingComma.Parse("").Success);     // False
Console.WriteLine(withTrailingComma.Parse("1,2,3,4").Success); // False
```

`min` and `max` count items, not separators. The limits follow the same rules
as `Repeat`. Suppose a separator was read but no next item matches. With
`allowTrailing: false`, the position goes back to before that separator. With
`allowTrailing: true`, the separator stays read, even after the last allowed item.
Without `Eof` or a closing delimiter, a list can succeed and leave more input.

To read a bracketed list, add `Between` with opening and closing parsers. See the
[complete integer list example](examples.md#list-of-integers).

## Until: stop before a terminator

`item.Until(terminator)` checks the terminator before each item. It succeeds as
soon as the terminator matches, and returns a list of items. The terminator is
required, but it is not read. Its value is thrown away.

```csharp
var body = Any.Until(L("*/")).Text();
var prefix = body.Parse("hello*/tail");

Console.WriteLine(prefix.Value);  // hello
Console.WriteLine(prefix.Length); // 5

var comment = body.Between(L("/*"), L("*/")).ThenIgnore(Eof);
Console.WriteLine(comment.Parse("/*hello*/").Value);   // hello
Console.WriteLine(comment.Parse("/**/").Value!.Length); // 0
Console.WriteLine(comment.Parse("/*hello").Success);   // False
```

Read the terminator separately with `ThenIgnore`, `Seq`, or `Between`. Reaching
the end of the input is a failure, unless the terminator itself accepts it. When
`Until` fails, it moves the input position back to the start. This simple
comment parser does not support nested comments.

For a line comment, `Eol` also accepts the end of the input:

```csharp
var comment = L("//")
    .Then(Any.Until(Eol).Text())
    .ThenIgnore(Eol);

Console.WriteLine(comment.Parse("// hello\nnext").Value); //  hello
Console.WriteLine(comment.Parse("// last line").Value);   //  last line
```

## Parsers that can match no text

`S`, `Eof`, `Return`, lookahead, and optional parsers can succeed without moving
the position. The repetition methods protect against endless loops, but the
results can be surprising:

- `Repeat` stops after a match that reads no text, once the minimum is met. That
  empty match can count as an item.
- `Separated` stops once its minimum is met, if an item and the separator after it
  together read no text. That item is included in the result.
- `Until` fails if its item reads no text before the terminator has matched.
- A fold throws away an operator/operand pair that reads no text, and stops.

Prefer an item parser that reads some text every time it succeeds. If part of the
item is optional, put `Optional` inside the item. Do not make the whole item
optional. See [Common mistakes](common-mistakes.md#making-the-whole-item-optional).

Next: [Working with values](working-with-values.md).
