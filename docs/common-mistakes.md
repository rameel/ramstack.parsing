# Common mistakes

[Documentation index](README.md)

Each section shows a mistake, what happens, and how to fix it. The examples use
the [standard imports](README.md#reading-the-examples).

## Forgetting Eof

A parser needs to match only the start of the input. It succeeds and ignores the
rest.

```csharp
var number = Literal.Number<int>();
var wrong = number.Parse("12abc");

Console.WriteLine(wrong.Success); // True
Console.WriteLine(wrong.Length);  // 2

var right = number.ThenIgnore(Eof);
Console.WriteLine(right.Parse("12abc").Success); // False
```

Add `ThenIgnore(Eof)` to the outermost parser.

## Forgetting spaces

The library never skips spaces by itself.

```csharp
var wrong = Seq(
    Literal.Number<int>(),
    L(','),
    Literal.Number<int>()
    ).ThenIgnore(Eof);

Console.WriteLine(wrong.Parse("1, 2").Success); // False

var right = Seq(
    Literal.Number<int>().ThenIgnore(S),
    L(',').ThenIgnore(S),
    Literal.Number<int>()
    ).ThenIgnore(Eof);

Console.WriteLine(right.Parse("1, 2").Success); // True
```

Read the spaces after each token with `ThenIgnore(S)`. See
[Handle whitespace](getting-started.md#handle-whitespace).

## Putting a short alternative first

A choice uses the first alternative that succeeds. It does not look for the
longest one.

```csharp
var wrong = Choice(L("<"), L("<=")).ThenIgnore(Eof);
Console.WriteLine(wrong.Parse("<=").Success); // False

var right = OneOf("<", "<=").ThenIgnore(Eof);
Console.WriteLine(right.Parse("<=").Success); // True
```

`OneOf` tries longer words first. With `Choice`, put `L("<=")` before `L("<")`.

## Matching a keyword that is only the start of a word

```csharp
var wrong = L("if");
Console.WriteLine(wrong.Parse("iffy").Success); // True

var right = L("if").ThenIgnore(Not(Set("A-Za-z0-9_")));
Console.WriteLine(right.Parse("iffy").Success); // False
```

Add a check that no identifier character follows the keyword.

## Expecting a repetition to give text back

Repetition is greedy. It reads as much as it can, and it never gives text back.

```csharp
var wrong = Any.OneOrMore().Text().ThenIgnore(L(".txt"));
Console.WriteLine(wrong.Parse("notes.txt").Success); // False

var right = Any.Until(L(".txt")).Text().ThenIgnore(L(".txt"));
Console.WriteLine(right.Parse("notes.txt").Value); // notes
```

`Any.OneOrMore()` reads the whole input, so nothing is left for `.txt`. `Until`
stops before the terminator.

## Putting Text in the wrong place

`Text()` returns the matched segment of the parser it is attached to. Some
methods keep the segment of only one of their parts.

```csharp
var wrong = Set("A-Za-z_")
    .ThenIgnore(Set("A-Za-z0-9_").ZeroOrMore());

Console.WriteLine(wrong.Parse("user42").Value); // u

var right = Seq(
    Set("A-Za-z_"),
    Set("A-Za-z0-9_").ZeroOrMore())
    .Text();

Console.WriteLine(right.Parse("user42").Value); // user42
```

`ThenIgnore` keeps the value and the segment of its first parser. `Seq` marks the
whole sequence. See [Text: return source text](working-with-values.md#text-return-source-text).

## Making the whole item optional

A repetition stops when its item reads no text. The empty match still counts as
an item.

```csharp
var wrong = L('x').Optional().ZeroOrMore();
Console.WriteLine(wrong.Parse("xxb").Value!.Count); // 3

var right = L('x').ZeroOrMore();
Console.WriteLine(right.Parse("xxb").Value!.Count); // 2
```

The `wrong` parser returns two items for the two `x` characters and a third, empty
item at `b`. Use an item that reads text every time it succeeds. If only a part of
the item is optional, put `Optional` inside the item.

## Checking a value with Do

`Do` changes a value. It does not reject one.

```csharp
var wrong = Literal.Number<int>().Do(n => n < 10);

Console.WriteLine(wrong.Parse("50").Success); // True
Console.WriteLine(wrong.Parse("50").Value);   // False

var right = Literal.Number<int>().Where(n => n < 10, "number below 10");
Console.WriteLine(right.Parse("50").Success); // False
```

Use `Where` to check a value.

## Using Value without checking Success

On failure, `Value` is `default(T)`. For `int` that is `0`, which looks like a
real result.

```csharp
var result = Literal.Number<int>().Parse("abc");

Console.WriteLine(result.Value);   // 0
Console.WriteLine(result.Success); // False
```

Always check `Success` first, or use `TryParse`.

## Counting or logging inside a callback

The functions that you pass to `Do`, `Map`, and folds can run during an attempt
that later fails. They can also be skipped when the value is thrown away. So a
counter or a log line inside a callback can be wrong. Build values in callbacks,
and do external work after a successful parse. See
[Void and Unit](working-with-values.md#void-and-unit-throw-a-value-away).

## Left recursion

A parser that calls itself before it reads any text never stops. The app crashes
with a stack overflow, and `try/catch` cannot catch it. Use `FoldL` or `FoldR`
for operators, or rewrite the grammar. See
[Avoid left recursion](recursive-parsers.md#avoid-left-recursion).

Next: [Performance tips](performance-tips.md).
