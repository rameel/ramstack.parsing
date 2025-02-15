using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class Literal
{
    private static Parser<char>? _quotedCharacter;

    /// <summary>
    /// Gets a parser that matches a character enclosed in single quotes.
    /// </summary>
    public static Parser<char> QuotedCharacter => _quotedCharacter ??=
        Choice(
            EscapeSequence,
            UnicodeEscapeSequence,
            Not(
                Choice(
                    L('\''),
                    L('\n'),
                    L('\r'))
                ).Then(Any))
        .Between(L('\''), L('\''))
        .As("quoted-character");
}
