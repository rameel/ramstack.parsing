using System.Diagnostics.CodeAnalysis;

using Ramstack.Parsing;

using static Ramstack.Parsing.Parser;

namespace Samples.TinyC;

public static class TinyCParser
{
    public static readonly Parser<Node> Parser = CreateParser();

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static Parser<Node> CreateParser()
    {
        var keyword =
            Seq(
                OneOf("while", "do", "if", "else"),
                Not(Set("\\w")));

        var number =
            Set("0-9")
                .OneOrMore()
                .Map(Node (m) => Node.Number(int.Parse(m)))
                .As("number");

        var variable =
            Seq(
                Not(keyword),
                Set("a-z"),
                Set("a-zA-Z_0-9").ZeroOrMore()
                ).Map(Identifier).As("variable");

        var multiline_comment =
            Seq(
                L("/*"),
                Choice(
                    Any,
                    Eof.Then(Error<char>("'*/'"))
                ).Until(L("*/")),
                L("*/")
            ).Void();

        var single_comment =
            Seq(
                L("//"),
                Any.Until(Eol),
                Eol
            ).Void();

        var ws =
            Choice(
                single_comment,
                multiline_comment,
                S
            ).ZeroOrMore().Void();

        var semicolon =
            Seq(L(';'), ws).Void();

        var eq =
            Seq(L('='), ws).Void();

        var if_keyword =
            Seq(L("if"), ws).Void();

        var else_keyword =
            Seq(L("else"), ws).Void();

        var while_keyword =
            Seq(L("while"), ws).Void();

        var do_keyword =
            Seq(L("do"), ws).Void();

        var expr =
            Deferred<Node>();

        var number_expr =
            number.ThenIgnore(ws);

        var var_expr =
            variable.ThenIgnore(ws);

        var parenthesis =
            expr.Between(
                Seq(L('('), ws),
                Seq(L(')'), ws));

        var primary_expr =
            Choice(
                parenthesis,
                number_expr,
                var_expr);

        var unary_expr =
            Seq(
                OneOf("-+~!").Optional(),
                ws,
                primary_expr
            ).Do(CreateUnary);

        var mul_expr = unary_expr.Fold(
            OneOf("*/%").ThenIgnore(ws),
            CreateBinary);

        var sum_expr =
            mul_expr.Fold(
                OneOf("+-").ThenIgnore(ws),
                CreateBinary);

        var shift_expr =
            sum_expr.Fold(
                OneOf("<<", ">>").ThenIgnore(ws),
                (l, r, o) => Node.Binary(o, l, r));

        var relational_expr =
            shift_expr.Fold(
                OneOf("<", "<=", ">", ">=").ThenIgnore(ws),
                (l, r, o) => Node.Binary(o, l, r));

        var eq_expr =
            relational_expr.Fold(
                OneOf("==", "!=").ThenIgnore(ws),
                (l, r, o) => Node.Binary(o, l, r));

        var bitwise_and_expr =
            eq_expr.Fold(
                L('&').ThenIgnore(ws),
                (l, r, _) => Node.Binary("&", l, r));

        var bitwise_xor_expr =
            bitwise_and_expr.Fold(
                L('^').ThenIgnore(ws),
                (l, r, _) => Node.Binary("^", l, r));

        var bitwise_or_expr =
            bitwise_xor_expr.Fold(
                L('|').ThenIgnore(ws),
                (l, r, _) => Node.Binary("|", l, r));

        var logical_and_expr =
            bitwise_or_expr.Fold(
                L("&&").ThenIgnore(ws),
                (l, r, _) => Node.Binary("&&", l, r));

        var logical_or_expr =
            logical_and_expr.Fold(
                L("||").ThenIgnore(ws),
                (l, r, _) => Node.Binary("||", l, r));

        var ternary_expr = Deferred<Node>();
        ternary_expr.Parser =
            Seq(
                logical_or_expr,
                Seq(
                    L('?'), ws, expr,
                    L(':'), ws, ternary_expr).Optional()
            ).Do(CreateTernary);

        var assignment_expr =
            Seq(var_expr, eq, expr).Do(CreateAssign);

        expr.Parser =
            Choice(
                assignment_expr,
                ternary_expr);

        var statement =
            Deferred<Node>();

        var else_clause =
            Seq(
                else_keyword,
                statement
            ).Do((_, s) => s).DefaultOnFail(Node.Empty());

        var if_statement =
            Seq(
                if_keyword,
                parenthesis,
                statement,
                else_clause
            ).Do(CreateIf);

        var block_statement =
            statement
                .ZeroOrMore()
                .Between(
                    Seq(L('{'), ws),
                    Seq(L('}'), ws))
                .Do(CreateBlock);

        var empty_statement =
            Seq(L(';'), ws
            ).Map(_ => Node.Empty());

        var while_statement =
            Seq(
                while_keyword,
                parenthesis,
                statement
            ).Do(CreateWhile);

        var do_while_statement =
            Seq(
                do_keyword,
                statement,
                while_keyword,
                parenthesis,
                semicolon
            ).Do(CreateDoWhile);

        var expr_statement =
            expr.ThenIgnore(semicolon);

        statement.Parser =
            Choice(
                if_statement,
                while_statement,
                do_while_statement,
                block_statement,
                expr_statement,
                empty_statement);

        return statement
            .Between(ws, Eof);

        static Node Identifier(Match m) =>
            Node.Variable(m.ToString());

        static Node CreateUnary(OptionalValue<char> u, Unit _, Node operand) =>
            u.HasValue ? Node.Unary(u.Value, operand) : operand;

        static Node CreateBinary(Node l, Node r, char o) =>
            Node.Binary(new string(o, 1), l, r);

        static Node CreateAssign(Node variable, Unit _, Node value) =>
            Node.Assign(variable, value);

        static Node CreateTernary(Node expression, OptionalValue<(char _1, Unit _2, Node @true, char _4, Unit _5, Node @false)> optional) =>
            optional.HasValue
                ? Node.Ternary(expression, optional.Value.@true, optional.Value.@false)
                : expression;

        static Node CreateIf(Unit _, Node expression, Node @true, Node @false) =>
            Node.If(expression, @true, @false);

        static Node CreateWhile(Unit _, Node expression, Node body) =>
            Node.WhileLoop(expression, body);

        static Node CreateDoWhile(Unit _0, Node body, Unit _2, Node expression, Unit _4) =>
            Node.DoWhileLoop(expression, body);

        static Node CreateBlock(IReadOnlyList<Node> statements) =>
            Node.Block(statements);
    }
}
