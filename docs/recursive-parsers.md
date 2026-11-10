# Recursive parsers and expressions

[Parser guide](README.md)

Describe nested structures and expressions with associativity and precedence.

## Recursive parsers

Use recursion for nested structures such as parentheses, arrays, or syntax trees.
`Deferred<T>()` creates a reference whose `.Parser` is assigned once the rule is built.
`Recursive<T>(self => ...)` defines a rule using a self-reference in a factory function.

```csharp
// "42", "(42)", and "((42))" => 42
var nested = Recursive<int>(self =>
    Literal
        .Number<int>()
        .Or(self.Between(L('('), L(')'))));

Console.WriteLine(nested.Parse("((42))").Value);  // 42
Console.WriteLine(nested.Parse("((42)").Success); // False

var deferred = Deferred<int>();
deferred.Parser = Literal
    .Number<int>()
    .Or(deferred.Between(L('('), L(')')));
```

Initialize a deferred rule before parsing or using it in operations that discard its value,
such as `Void`, `And`, or `Not`. Recursive calls must follow input consumption:
a rule that immediately calls itself is left-recursive and is not supported.

The same initialization rule applies when a combinator discards the placeholder
internally: `deferred.Then(other)` needs the definition first. Keeping the value,
as in `deferred.Between(open, close)`, is allowed while building its definition.
Assign `.Parser` exactly once; a second assignment throws `InvalidOperationException`.
For mutually recursive rules, create all placeholders first, assign their definitions,
then build combinations that discard their values.

`Recursive` follows the same rules inside its factory. Use it for a single self-reference
and `Deferred` for several rules that refer to each other. For operators, use folds
to avoid left recursion. Deeply nested valid input still uses the call stack;
the library does not impose a nesting limit.

## Operators and expressions

| Combinator               | Purpose                                                                                         |
| ------------------------ | ----------------------------------------------------------------------------------------------- |
| `p.FoldL(op, reduce)`    | Parse an operand followed by operator/operand pairs; combine from the left. `Fold` is an alias. |
| `p.FoldR(op, reduce)`    | Combine operator/operand pairs from the right.                                                  |
| `p.Prefixed(op, reduce)` | Parse zero or more prefix operators before an operand; apply the closest operator first.        |

```csharp
// "10 - 3 - 2" => (10 - 3) - 2 => 5
var number = Literal.Number<int>().ThenIgnore(S);
var subtraction = number.FoldL(L('-').ThenIgnore(S), (l, r, _) => l - r);
```

Build precedence in layers: for example, use a product parser as the operand parser for a sum.
See the [calculator](../samples/CalcExpr/README.md).

Right-associative folding groups exponentiation from the right.
Prefix operators are applied before the resulting operand is returned:

```csharp
var power = Literal
    .Number<double>()
    .FoldR(L('^'), (left, right, _) => Math.Pow(left, right));

Console.WriteLine(power.Parse("2^3^2").Value); // 512: 2^(3^2)

var unary = Literal.Number<int>().Prefixed(L('-'), (value, _) => -value);
Console.WriteLine(unary.Parse("--5").Value); // 5
```

Both folds require an initial operand. If an operator has no valid operand after it,
the fold restores the position to before that operator and returns the value built so far.
An outer `Eof` rejects the unfinished suffix:

```csharp
var sum = Literal.Number<int>().FoldL(L('+'), (left, right, _) => left + right);
Console.WriteLine(sum.Parse("1+").Value);  // 1
Console.WriteLine(sum.Parse("1+").Length); // 1
Console.WriteLine(sum.ThenIgnore(Eof).Parse("1+").Success); // False
```

Build precedence with one fold per level:

```csharp
var number = Literal.Number<int>().ThenIgnore(S);
var product = number.FoldL(L('*').ThenIgnore(S), (left, right, _) => left * right);
var sum = product.FoldL(L('+').ThenIgnore(S), (left, right, _) => left + right);
var expression = sum.Between(S, Eof);

Console.WriteLine(expression.Parse("2 + 3 * 4").Value); // 14
```

Folds discard an operator/operand pair that consumes no input, without calling the reduction function.
`Prefixed` similarly discards an empty operator match. If its operand is missing,
`Prefixed` fails and restores the starting position, including any prefix operators it read.
