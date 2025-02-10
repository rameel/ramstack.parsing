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
            .Separated(Seq(S, L(',')))
            .Between(
                L('['),
                Seq(S, L(']')))
            .Do(object? (list) => list);

        var member = Seq(
            S, DoubleQuotedString, S, L(':'), value
            ).Do((_, name, _, _, v) => KeyValuePair.Create(name, v));

        var @object = member
            .Separated(Seq(S, L(',')))
            .Between(
                L('{'),
                Seq(S, L('}')))
            .Do(object? (members) => new Dictionary<string, object?>(members));

        value.Parser =
            S.Then(
                Choice(
                    @string,
                    number,
                    primitive,
                    array,
                    @object));

        return value;
    }
}
