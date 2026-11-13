# Complete examples

[Documentation index](README.md)

Each example below can be pasted into a console app with the
[standard imports](README.md#reading-the-examples). Each one checks the whole
input. Build the parser once, then reuse it for multiple inputs.

## List of integers

Read a bracketed list with optional spaces. Empty lists are allowed; trailing
commas are rejected.

```csharp
var item = Literal.Number<int>().ThenIgnore(S);
var comma = L(',').ThenIgnore(S);

var list = item
    .Separated(comma)
    .Between(
        L('[').ThenIgnore(S),
        L(']').ThenIgnore(S))
    .Between(S, Eof);

var result = list.Parse(" [1, -2, 3] ");
if (result.Success)
{
    Console.WriteLine(string.Join(", ", result.Value!)); // 1, -2, 3
}

Console.WriteLine(list.Parse("[]").Value!.Count);       // 0
Console.WriteLine(list.Parse("[1,]").Success);          // False
Console.WriteLine(list.Parse("[1] extra").Success);     // False
```

`Separated` handles items and commas. `Between` requires the brackets.
Whitespace is read after tokens and at the start. To allow a trailing comma,
set `allowTrailing: true` on `Separated`.

## A setting with a quoted value

Read one setting such as `name = "Ada"`. The key is an ASCII identifier and the
value uses the library's double-quoted string syntax.

```csharp
var identifier = Seq(
    Set("A-Za-z_"),
    Set("A-Za-z0-9_").ZeroOrMore())
    .Text()
    .As("setting name");

var setting = Seq(
    identifier.ThenIgnore(S),
    L('=').ThenIgnore(S),
    Literal.DoubleQuotedString.ThenIgnore(S))
    .Do((key, _, value) => KeyValuePair.Create(key, value))
    .Between(S, Eof);

var result = setting.Parse(" name = \"Ada\" ");
if (result.Success)
{
    Console.WriteLine(result.Value.Key);   // name
    Console.WriteLine(result.Value.Value); // Ada
}

Console.WriteLine(setting.Parse("name = Ada").Success); // False
Console.WriteLine(setting.Parse("9name = \"Ada\"").Success); // False
```

`Seq(...).Text()` keeps the whole identifier. `Do` turns the sequence into a
key/value pair. This parser reads one setting. It does not define a whole
configuration-file format.

## Calculator

This example reads numbers, parentheses, unary minus, and the four basic
arithmetic operators. Multiplication and division have higher precedence than
addition and subtraction. Operators at the same level group from the left.

```csharp
var calculator = CreateCalculator();

Console.WriteLine(calculator.Parse("2 + 3 * (4 - 1)").Value); // 11
Console.WriteLine(calculator.Parse("-(2 + 3)").Value);       // -5
Console.WriteLine(calculator.Parse("- 5").Value);            // -5
Console.WriteLine(calculator.Parse("10 - 3 - 2").Value);     // 5
Console.WriteLine(calculator.Parse("1 + (").Success);        // False
Console.WriteLine(calculator.Parse("2 + 3 extra").Success);  // False

static Parser<double> CreateCalculator()
{
    var expression = Deferred<double>();

    var number = Literal.Number<double>("number").ThenIgnore(S);

    var parenthesized = expression.Between(
        L('(').ThenIgnore(S),
        L(')').ThenIgnore(S));

    var primary = parenthesized.Or(number);

    var unary = Seq(
        L('-').Optional(),
        S,
        primary)
        .Do((sign, _, value) => sign.HasValue ? -value : value);

    var product = unary.FoldL(
        OneOf("*/").ThenIgnore(S),
        (left, right, op) => op == '*' ? left * right : left / right);

    expression.Parser = product.FoldL(
        OneOf("+-").ThenIgnore(S),
        (left, right, op) => op == '+' ? left + right : left - right);

    return expression.Between(S, Eof);
}
```

The deferred parser lets parentheses refer back to the full expression. `FoldL`
builds each precedence level. `Number<double>` reads its own sign, so `-5` is
already a number. The unary parser is still needed for `-(2 + 3)` and for `- 5`
with a space.

This example uses `double` arithmetic. Dividing by zero gives infinity or `NaN`;
it does not throw. The example has no variables, functions, or power operator.
See [Recursive parsers](recursive-parsers.md) for `FoldR` and a power example.

## Read the project samples

The repository has larger working grammars:

| Sample | Useful for |
| --- | --- |
| [Calculator source](../samples/CalcExpr/ExpressionParser.cs) and [grammar](../samples/CalcExpr/README.md) | Recursive expressions and operator precedence. |
| [JSON source](../samples/Json/JsonParser.cs) and [grammar](../samples/Json/README.md) | Recursive arrays and objects, lists, and values with different .NET types. |
| [TinyC source](../samples/TinyC/TinyCParser.cs) and [grammar](../samples/TinyC/README.md) | Statements, comments, keyword boundaries, and syntax trees. |

The JSON sample uses the library's general number and string parsers. Treat it
as a grammar example, not as a strict JSON validator. Those parsers accept forms
that JSON does not allow, such as a leading `+` on a number and extra string
escapes.

Next: [Common mistakes](common-mistakes.md).
