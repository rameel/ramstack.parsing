namespace Ramstack.Parsing;

[TestFixture]
public partial class ParsersTests
{
    // Bound parser invocations so a missing progress check fails instead of hanging the test run.
    private sealed class BoundedParser<T>(Parser<T> parser) : Parser<T>
    {
        public int Attempts { get; private set; }

        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            if (++Attempts > 16)
                throw new InvalidOperationException(
                    "The repetition did not stop after matching empty input.");

            return parser.TryParse(ref context, out value);
        }
    }
}
