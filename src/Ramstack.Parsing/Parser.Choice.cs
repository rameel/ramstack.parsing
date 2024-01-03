namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that attempts to parse using each of the specified parsers in sequence,
    /// returning the result of the first parser that succeeds.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by each parser.</typeparam>
    /// <param name="parsers">An array of parsers that are tried in sequence.</param>
    /// <returns>
    /// A <see cref="Parser{T}"/> that tries each parser in <paramref name="parsers"/> sequentially
    /// and returns the result of the first parser that successfully parses the input.
    /// </returns>
    public static Parser<T> Choice<T>(params Parser<T>[] parsers)
    {
        Argument.ThrowIfNullOrEmpty(parsers);

        var list = new ArrayList<Parser<T>>();

        while (true)
        {
            foreach (var parser in parsers)
            {
                switch (parser)
                {
                    case ChoiceParser<T> p:
                        list.AddRange(p.Parsers);
                        break;

                    default:
                        list.Add(parser);
                        break;
                }
            }

            if (list.Count == parsers.Length)
                break;
        }

        if (list.Count == 1)
            return list[0];

        if (typeof(T) == typeof(char) || typeof(T) == typeof(Unit))
        {
            var count = 0;
            foreach (var parser in list)
                if (parser is ICharClassSupport)
                    count++;

            if (count > 1)
            {
                var @class = new CharClass(CharClassUnicodeCategory.Create(0));
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i] is ICharClassSupport s)
                    {
                        @class = @class.MergeClasses(s.GetCharClass());
                        list.RemoveAt(i);
                    }
                }

                var p = Set(@class);
                list.Insert(0,
                    (Parser<T>)(object)(
                        typeof(T) == typeof(Unit) ? p.Void() : p)
                        );
            }

            parsers = list.ToArray();
        }

        if (parsers.Length == 1)
            return parsers[0];

        foreach (var parser in parsers)
            if (parser.Name is null)
                return new ChoiceParser<T>(parsers);

        return new DeferredDiagnosticChoiceParser<T>(parsers);
    }

    #region Inner type: ChoiceParser

    /// <summary>
    /// Represents a parser that attempts to parse using each of the specified parsers in sequence,
    /// returning the result of the first parser that succeeds.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by each parser.</typeparam>
    /// <param name="parsers">An array of sub-parsers that are tried in sequence.</param>
    private sealed class ChoiceParser<T>(Parser<T>[] parsers) : Parser<T>
    {
        /// <summary>
        /// Gets the array of parsers that are tried in sequence.
        /// </summary>
        public Parser<T>[] Parsers => parsers;

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var state = context.SuppressDiagnosticsIfNamed(Name);

            foreach (var parser in parsers)
            {
                if (parser.TryParse(ref context, out value))
                {
                    context.RestoreDiagnosticState(state);
                    return true;
                }
            }

            context.RestoreDiagnosticState(state);
            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new ChoiceParser<T>(parsers) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new ChoiceParser<Unit>([..parsers.Select(p => p.Void())]) { Name = Name };
    }

    #endregion

    #region Inner type: DeferredDiagnosticChoiceParser

    /// <summary>
    /// Represents a parser that attempts to parse the input by sequentially
    /// using each of the specified sub-parsers. This variant is designed
    /// for scenarios where all sub-parsers are named.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="DeferredDiagnosticChoiceParser{T}"/> is an optimization
    /// that takes effect when <em>all</em> sub-parsers have names.
    /// </para>
    /// <para>
    /// Instead of reporting a diagnostic message every time a sub-parser fails,
    /// this parser silently attempts the next one, suppressing diagnostics in the process.
    /// Only if <em>all</em> sub-parsers ultimately fail does it produce an error message
    /// listing all parser names (i.e., every expected token). As a result,
    /// if any sub-parser succeeds, no diagnostics are reported.
    /// </para>
    /// <para>
    /// This approach enhances performance by avoiding multiple "failed parse" errors,
    /// reducing unnecessary memory allocations, and keeping the error log
    /// free of clutter when parsing is successful.
    /// </para>
    /// </remarks>
    /// <typeparam name="T">The type of the result produced by each parser.</typeparam>
    private sealed class DeferredDiagnosticChoiceParser<T> : Parser<T>
    {
        private readonly Parser<T>[] _parsers;
        private readonly string[] _expected;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredDiagnosticChoiceParser{T}"/> class.
        /// </summary>
        /// <param name="parsers">An array of sub-parsers that are tried in sequence.</param>
        /// <param name="expected">An array of expected tokens for error reporting.</param>
        private DeferredDiagnosticChoiceParser(Parser<T>[] parsers, string[] expected) =>
            (_parsers, _expected) = (parsers, expected);

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferredDiagnosticChoiceParser{T}"/> class.
        /// </summary>
        /// <param name="parsers">An array of sub-parsers that are tried in sequence.</param>
        public DeferredDiagnosticChoiceParser(Parser<T>[] parsers)
        {
            _parsers = parsers;

            var expected = new string[parsers.Length];
            if (expected.Length == parsers.Length)
                for (var i = 0; i < parsers.Length; i++)
                    expected[i] = parsers[i].Name!;

            _expected = expected;
        }

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var state = context.SuppressDiagnostics();

            foreach (var parser in _parsers)
            {
                if (parser.TryParse(ref context, out value))
                {
                    context.RestoreDiagnosticState(state);
                    return true;
                }
            }

            context.RestoreDiagnosticState(state);

            switch (Name)
            {
                case not null:
                    context.AddError(Name);
                    break;

                default:
                    context.AddErrors(_expected);
                    break;
            }

            value = default;
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new DeferredDiagnosticChoiceParser<T>(_parsers, _expected) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new DeferredDiagnosticChoiceParser<Unit>([.._parsers.Select(p => p.Void())], _expected) { Name = Name };
    }

    #endregion
}
