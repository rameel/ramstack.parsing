using Samples.Utilities;

namespace Samples.TinyC;

public abstract record Node
{
    public static Node Empty() => new BlockNode([]);
    public static Node Number(int value) => new NumberNode(value);
    public static Node Variable(string name) => new VariableNode(name);
    public static Node If(Node test, Node ifTrue, Node ifFalse) => new IfNode(test, Wrap(ifTrue), Wrap(ifFalse));
    public static Node Ternary(Node test, Node ifTrue, Node ifFalse) => new TernaryNode(test, ifTrue, ifFalse);
    public static Node WhileLoop(Node test, Node body) => new WhileLoopNode(test, Wrap(body));
    public static Node DoWhileLoop(Node test, Node body) => new DoWhileLoopNode(test, Wrap(body));
    public static Node Unary(char op, Node operand) => new UnaryNode(op, operand);
    public static Node Binary(string op, Node left, Node right) => new BinaryNode(op, left, right);
    public static Node Assign(Node variable, Node value) => Binary("=", variable, value);
    public static Node Block(IReadOnlyList<Node> statements) => new BlockNode(statements);
    private static Node Wrap(Node statement) => statement is BlockNode ? statement : new BlockNode([statement]);

    public sealed override string ToString()
    {
        var sb = new IndentedStringBuilder();
        Print(this, sb);
        return sb.ToString();

        static void Print(Node node, IndentedStringBuilder sb)
        {
            switch (node)
            {
                case VariableNode(Name: var name):
                    sb.Append(name);
                    break;

                case NumberNode(Value: var number):
                    sb.Append(number);
                    break;

                case IfNode c:
                    sb.Append("if (");
                    Print(c.Test, sb);
                    sb.Append(')');
                    sb.AppendLine();

                    Print(c.IfTrue, sb);

                    if (c.IfFalse is BlockNode { Statements: [IfNode ifElse] })
                    {
                        sb.Append("else ");
                        Print(ifElse, sb);
                    }
                    else if (c.IfFalse is not BlockNode([]))
                    {
                        sb.Append("else");
                        sb.AppendLine();
                        Print(c.IfFalse, sb);
                    }
                    break;

                case TernaryNode c:
                    Print(c.Test, sb);
                    sb.IncrementIndent();
                    sb
                        .AppendLine()
                        .Append("? ");
                    Print(c.IfTrue, sb);

                    sb
                        .AppendLine()
                        .Append(": ");
                    Print(c.IfFalse, sb);
                    sb.DecrementIndent();
                    break;

                case WhileLoopNode w:
                    sb.Append("while (");
                    Print(w.Test, sb);
                    sb.Append(')');
                    sb.AppendLine();
                    Print(w.Body, sb);
                    break;

                case DoWhileLoopNode d:
                    sb.AppendLine("do");
                    Print(d.Body, sb);
                    sb.Append("while (");
                    Print(d.Test, sb);
                    sb.AppendLine(");");
                    break;

                case UnaryNode u:
                    sb.Append(u.Operator);
                    sb.Append('(');
                    Print(u.Operand, sb);
                    sb.Append(')');
                    break;

                case BinaryNode(Operator: "=", var variable, var expr):
                    Print(variable, sb);
                    sb.Append(" = (");
                    Print(expr, sb);
                    sb.Append(')');
                    break;

                case BinaryNode b:
                    sb.Append('(');
                    Print(b.Left, sb);
                    sb.Append($" {b.Operator} ");
                    Print(b.Right, sb);
                    sb.Append(')');
                    break;

                case BlockNode block:
                    sb.AppendLine("{");
                    sb.IncrementIndent();

                    foreach (var stmt in block.Statements)
                    {
                        Print(stmt, sb);
                        if (stmt is NumberNode or VariableNode or UnaryNode or BinaryNode)
                            sb.AppendLine(";");
                    }

                    sb.DecrementIndent();
                    sb.AppendLine("}");
                    break;
            }
        }
    }

    public sealed record VariableNode(string Name) : Node;
    public sealed record NumberNode(int Value) : Node;
    public sealed record IfNode(Node Test, Node IfTrue, Node IfFalse) : Node;
    public sealed record TernaryNode(Node Test, Node IfTrue, Node IfFalse) : Node;
    public sealed record WhileLoopNode(Node Test, Node Body) : Node;
    public sealed record DoWhileLoopNode(Node Test, Node Body) : Node;
    public sealed record UnaryNode(char Operator, Node Operand) : Node;
    public sealed record BinaryNode(string Operator, Node Left, Node Right) : Node;
    public sealed record BlockNode(IReadOnlyList<Node> Statements) : Node;
}
