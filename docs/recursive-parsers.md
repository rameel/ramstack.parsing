# Recursive parsers and expressions

[Documentation index](README.md)

A recursive parser refers to itself, directly or through other parsers. This is
useful for nested parentheses, arrays, objects, and statements. Folds handle a
common case: a series of values joined by operators. The examples use the
[standard imports](README.md#reading-the-examples).

## Deferred: define a parser later

`Deferred<T>()` creates a placeholder. You can use it while you build other
parsers, and then assign its `Parser` property once.

```csharp
var value = Deferred<int>();

value.Parser = Choice(
    Literal.Number<int>(),
    value.Between(L('('), L(')')));

var complete = value.ThenIgnore(Eof);

Console.WriteLine(complete.Parse("42").Value);     // 42
Console.WriteLine(complete.Parse("((42))").Value); // 42
Console.WriteLine(complete.Parse("(42").Success);  // False
```

The placeholder holds a reference to the real parser. In the parenthesized
branch, the opening parenthesis is read before the recursive parser is called
again.

Finish the setup before you parse:

- Assign a non-null parser exactly once. A second assignment throws
  `InvalidOperationException`.
- If you parse with a placeholder that has no parser yet, the parse stops with a
  fatal error.
- A placeholder with no parser yet cannot be used where its value is thrown away.
  This includes `Void()`, the first parser in `Then`, and the parsers inside `And`
  or `Not`. Building such a parser throws `InvalidOperationException` at once.
- A placeholder with no parser yet can be used where its value is kept. This
  includes the middle parser of `Between` and a part of `Seq`. The example above
  is built this way.

```csharp
var item = Deferred<int>();

var kept = item.Between(L('('), L(')')); // Works: the value is kept.

try
{
    item.Then(L(';')); // Fails: the value would be thrown away.
}
catch (InvalidOperationException)
{
    Console.WriteLine("Assign item.Parser first."); // Assign item.Parser first.
}
```

For parsers that call each other, create all the placeholders first. Then assign
all their definitions. Build any parser that throws a value away after that.

## Recursive: define a self-reference with a function

`Recursive<T>(factory)` passes a reference to the unfinished parser into the
factory, and uses the parser that the factory returns as its definition.

```csharp
var value = Recursive<int>(self => Choice(
    Literal.Number<int>(),
    self.Between(L('('), L(')'))))
    .ThenIgnore(Eof);

Console.WriteLine(value.Parse("((7))").Value); // 7
```

Use `Recursive` for a single parser, and `Deferred` when several parsers need to
refer to each other. The factory must return a non-null parser. The rules above
for a placeholder with no parser yet also apply inside the factory.

## Avoid left recursion

A parser must read some text before it calls itself on the same path. A grammar
such as `expression = expression '+' number | number` calls itself before it
reads anything. It never stops: the app crashes with a stack overflow, and
`try/catch` cannot catch it. `Deferred` and `Recursive` do not prevent this.

For binary operators, use `FoldL` or `FoldR`. For prefix operators such as unary
minus, use `Prefixed`. For other recursive parsers, rewrite
the grammar so that the recursion comes after some input is read. Very deeply
nested valid input also uses the call stack. The library sets no limit on the
depth.

## FoldL: combine operators from the left

`operand.FoldL(operator, reduce)` reads one operand, then zero or more
operator/operand pairs. The `reduce` function receives `(left, right, operatorValue)`.

```csharp
var subtraction = Literal.Number<int>()
    .FoldL(L('-'), (left, right, _) => left - right)
    .ThenIgnore(Eof);

Console.WriteLine(subtraction.Parse("10-3-2").Value); // 5
```

This groups the input as `(10 - 3) - 2`. Use it for left-associative operators
such as ordinary subtraction and division. `Fold` is an alias for `FoldL`. Prefer
`FoldL`, so that the grouping is clear.

## FoldR: combine operators from the right

`FoldR` reads the same kind of chain, but combines it from the right:

```csharp
var power = Literal.Number<double>()
    .FoldR(L('^'), (left, right, _) => Math.Pow(left, right))
    .ThenIgnore(Eof);

Console.WriteLine(power.Parse("2^3^2").Value); // 512
```

This means `2 ^ (3 ^ 2)`. Use right grouping for operators such as powers or
assignment, when your language defines them that way.

Both folds require the first operand. If an operator is not followed by a valid
operand, the position goes back to before that operator, and the fold returns the
value built so far. Add `Eof` or another outer parser to reject the unfinished
end:

```csharp
var sum = Literal.Number<int>().FoldL(L('+'), (left, right, _) => left + right);

Console.WriteLine(sum.Parse("1+").Value);  // 1
Console.WriteLine(sum.Parse("1+").Length); // 1
Console.WriteLine(sum.ThenIgnore(Eof).Parse("1+").Success); // False
```

If a whole operator/operand pair reads no text, the fold throws that pair away
and stops. It does not call the `reduce` function for that pair.

## Prefixed: read operators before an operand

`operand.Prefixed(operator, apply)` reads zero or more operators, then one operand.
The `apply` function receives `(operand, operatorValue)`. It runs once for each
operator, starting with the operator closest to the operand.

```csharp
var negation = Set('0', '9').Do(c => c - '0')
    .Prefixed(L('-'), (value, _) => -value)
    .ThenIgnore(Eof);

Console.WriteLine(negation.Parse("5").Value);    // 5
Console.WriteLine(negation.Parse("---5").Value); // -5
```

With several kinds of operators, the closest one is applied first. In `!-x`, the
`-` is applied to `x`, and then the `!` is applied to that result:

```csharp
var tree = Set('a', 'z').Map(m => m.ToString())
    .Prefixed(OneOf("-!"), (value, op) => $"({op}{value})");

Console.WriteLine(tree.Parse("!-x").Value); // (!(-x))
```

`Prefixed` reads the operators in a loop, so a very long chain of operators does
not use the call stack. If the operand is missing, `Prefixed` fails and the position
goes back to where it started, including the operators that it read. If an
operator reads no text, `Prefixed` stops looking for operators and does not call
`apply` for it.

Use the parser as the operand of a binary level. The binary `-` and the prefix `-`
do not clash, because each one is tried at a different place:

```csharp
var digit = Set('0', '9').Do(c => c - '0').ThenIgnore(S);
var unary = digit.Prefixed(L('-').ThenIgnore(S), (value, _) => -value);

var sum = unary.FoldL(
    OneOf("+-").ThenIgnore(S),
    (left, right, op) => op == '+' ? left + right : left - right);

Console.WriteLine(sum.Parse("5 - - 3").Value); // 8
```

## Operator precedence

Build one parser for each precedence level. Use the tighter level as the operand
parser of the looser level:

```csharp
var number = Literal.Number<double>().ThenIgnore(S);

var product = number.FoldL(
    OneOf("*/").ThenIgnore(S),
    (left, right, op) => op == '*' ? left * right : left / right);

var sum = product.FoldL(
    OneOf("+-").ThenIgnore(S),
    (left, right, op) => op == '+' ? left + right : left - right);

var expression = sum.Between(S, Eof);
Console.WriteLine(expression.Parse("2 + 3 * 4").Value); // 14
```

Here multiplication and division are handled before addition and subtraction.
For parentheses, the primary parser needs a reference back to the whole
expression. See the [complete calculator](examples.md#calculator).

Next: [Errors](errors.md).
