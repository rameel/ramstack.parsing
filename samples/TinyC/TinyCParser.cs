using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Ramstack.Parsing;

using static Ramstack.Parsing.Parser;

namespace Samples.TinyC;

public static class TinyCParser
{
    public static readonly Parser<Node> Parser = CreateParser();

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static Parser<Node> CreateParser()
    {
        var identifier_part =
            Set("a-zA-Z_0-9");

        var number =
            Set("0-9")
                .OneOrMore()
                .Map(Node (m) => Node.Number(int.Parse(m, NumberStyles.Integer, CultureInfo.InvariantCulture)))
                .As("number");

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
            Keyword("if").ThenIgnore(ws).Void();

        var else_keyword =
            Keyword("else").ThenIgnore(ws).Void();

        var while_keyword =
            Keyword("while").ThenIgnore(ws).Void();

        var do_keyword =
            Keyword("do").ThenIgnore(ws).Void();

        var keyword =
            Choice(
                while_keyword,
                do_keyword,
                if_keyword,
                else_keyword);

        var variable =
            Seq(
                Not(keyword),
                Set("a-z"),
                identifier_part.ZeroOrMore()
                ).Map(Identifier).As("variable");

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

        var mul_expr = unary_expr.FoldL(
            OneOf("*/%").ThenIgnore(ws),
            CreateBinary);

        var sum_expr =
            mul_expr.FoldL(
                OneOf("+-").ThenIgnore(ws),
                CreateBinary);

        var shift_expr =
            sum_expr.FoldL(
                OneOf("<<", ">>").ThenIgnore(ws),
                (l, r, o) => Node.Binary(o, l, r));

        var relational_expr =
            shift_expr.FoldL(
                OneOf("<", "<=", ">", ">=").ThenIgnore(ws),
                (l, r, o) => Node.Binary(o, l, r));

        var eq_expr =
            relational_expr.FoldL(
                OneOf("==", "!=").ThenIgnore(ws),
                (l, r, o) => Node.Binary(o, l, r));

        var bitwise_and_expr =
            eq_expr.FoldL(
                L('&').ThenIgnore(ws),
                (l, r, _) => Node.Binary("&", l, r));

        var bitwise_xor_expr =
            bitwise_and_expr.FoldL(
                L('^').ThenIgnore(ws),
                (l, r, _) => Node.Binary("^", l, r));

        var bitwise_or_expr =
            bitwise_xor_expr.FoldL(
                L('|').ThenIgnore(ws),
                (l, r, _) => Node.Binary("|", l, r));

        var logical_and_expr =
            bitwise_or_expr.FoldL(
                L("&&").ThenIgnore(ws),
                (l, r, _) => Node.Binary("&&", l, r));

        var logical_or_expr =
            logical_and_expr.FoldL(
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
            expr.ThenIgnore(semicolon).Do(Node.ExpressionStatement);

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

        Parser<string> Keyword(string word) =>
            L(word).ThenIgnore(Not(identifier_part)).As($"'{word}'");

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
