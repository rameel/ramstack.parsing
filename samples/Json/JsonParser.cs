using Ramstack.Parsing;

using static Ramstack.Parsing.Literal;
using static Ramstack.Parsing.Parser;

namespace Samples.Json;

public static class JsonParser
{
    public static readonly Parser<object?> Parser = CreateJsonParser();

    private static Parser<object?> CreateJsonParser()
    {
        var value =
            Deferred<object?>();

        var @string =
            DoubleQuotedString.Do(object? (s) => s);

        var number =
            Number<double>().Do(object? (n) => n);

        var primitive = OneOf(["true", "false", "null"]).Do(s =>
        {
            object? r = null;
            if (s.Length != 0 && s[0] != 'n')
                r = s[0] == 't';
            return r;
        });

        var array = value
            .Separated(Seq(L(','), S))
            .Between(
                Seq(L('['), S),
                Seq(L(']'), S))
            .Do(object? (list) => list);

        var member = Seq(
            DoubleQuotedString, S, L(':'), S, value
            ).Do((name, _, _, _, v) => KeyValuePair.Create(name, v));

        var @object = member
            .Separated(Seq(L(','), S))
            .Between(
                Seq(L('{'), S),
                Seq(L('}'), S))
            .Do(object? (members) => new Dictionary<string, object?>(members));

        value.Parser =
            Choice(
                @string,
                number,
                primitive,
                array,
                @object
                ).ThenIgnore(S);

        return S.Then(value);
    }
}
