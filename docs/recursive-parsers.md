# Recursive parsers and expressions

[Parser guide](README.md)

Describe nested structures and expressions with associativity and precedence.

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

| Combinator               | Purpose                                                                                         |
| ------------------------ | ----------------------------------------------------------------------------------------------- |
| `p.FoldL(op, reduce)`    | Parse an operand followed by operator/operand pairs; combine from the left. `Fold` is an alias. |
| `p.FoldR(op, reduce)`    | Combine operator/operand pairs from the right.                                                  |
| `p.Prefixed(op, reduce)` | Parse zero or more prefix operators before an operand; apply the closest operator first.        |

```csharp
var number = Literal.Number<int>().ThenIgnore(S);
var subtraction = number.FoldL(L('-').ThenIgnore(S), (l, r, _) => l - r);
// "10 - 3 - 2" => (10 - 3) - 2 => 5
```

Build precedence in layers: for example, use a product parser as the operand
parser for a sum. See the [calculator](../samples/CalcExpr/README.md).
