namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a new parser with the specified name.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <param name="parser">The parser instance to name.</param>
    /// <param name="name">The name to assign to the parser.</param>
    /// <returns>
    /// A new parser instance with the specified name.
    /// </returns>
    public static Parser<T> As<T>(this Parser<T> parser, string name)
    {
        Argument.ThrowIfNullOrEmpty(name);
        return parser.ToNamedParser(name);
    }

    #region Inner type: NamedParser

    /// <summary>
    /// Represents a parser that has an assigned name.
    /// </summary>
    /// <typeparam name="T">The type of the value produced by the parser.</typeparam>
    /// <param name="parser">The underlying parser to wrap.</param>
    internal sealed class NamedParser<T>(Parser<T> parser) : Parser<T>
    {
        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var state = context.SuppressDiagnostics();

            if (parser.TryParse(ref context, out value))
            {
                context.RestoreDiagnosticState(state);
                return true;
            }

            context.RestoreDiagnosticState(state);
            context.AddError(Name);
            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new NamedParser<T>(parser) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new NamedParser<Unit>(parser.Void()) { Name = Name };
    }

    #endregion
}
