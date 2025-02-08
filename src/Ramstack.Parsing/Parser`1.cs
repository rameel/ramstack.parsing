namespace Ramstack.Parsing;

/// <summary>
/// Represents the base class of a parser, providing parsing logic and extension points for derived parsers.
/// </summary>
/// <typeparam name="T">The type of the value produced by the parser.</typeparam>
public abstract class Parser<T>
{
    /// <summary>
    /// Gets the name of the parser.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Parser{T}"/> class.
    /// </summary>
    protected Parser()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Parser{T}"/> class with the specified name.
    /// </summary>
    /// <param name="name">The name to assign to the parser.</param>
    protected Parser(string? name) =>
        Name = name;

    /// <summary>
    /// Attempts to parse the specified source text. Diagnostic messages are suppressed during this operation.
    /// </summary>
    /// <param name="source">The source text to parse.</param>
    /// <param name="value">When this method returns, contains the parsed result if the parsing was successful;
    /// otherwise, <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the parser succeeded; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryParse(ReadOnlySpan<char> source, [NotNullWhen(true)] out T? value)
    {
        try
        {
            var context = new ParseContext(source)
            {
                DiagnosticState = DiagnosticState.Suppressed
            };

            if (TryParse(ref context, out value))
                return true;
        }
        catch
        {
            // Ignore exceptions
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Parses the specified source text.
    /// </summary>
    /// <param name="source">The source text to parse.</param>
    /// <returns>
    /// A <see cref="ParseResult{T}"/> containing the parsed result or error information.
    /// </returns>
    public ParseResult<T> Parse(ReadOnlySpan<char> source)
    {
        var context = new ParseContext(source);
        try
        {
            if (TryParse(ref context, out var value))
            {
                return new ParseResult<T>
                {
                    Value = value,
                    Length = context.Position
                };
            }

            return new ParseResult<T>
            {
                Length = 0,
                ErrorMessage = context.ToString()
            };
        }
        catch (FatalErrorException exception)
        {
            var message = ParseContext.GenerateErrorMessage(context.Source, context.Position, exception.Message);
            return new ParseResult<T>
            {
                Length = 0,
                ErrorMessage = message
            };
        }
        catch (Exception exception)
        {
            var message = ParseContext.GenerateErrorMessage(context.Source, context.Position, exception.ToString());
            return new ParseResult<T>
            {
                Length = 0,
                ErrorMessage = message,
                Exception = exception
            };
        }
    }

    /// <summary>
    /// Attempts to parse the source text using the provided <see cref="ParseContext"/>.
    /// </summary>
    /// <param name="context">The parse context containing the source text and position.</param>
    /// <param name="value">When this method returns, contains the parsed result if the parsing was successful;
    /// otherwise, <see langword="null"/>.</param>
    /// <returns>
    /// <see langword="true"/> if the parser succeeded; otherwise, <see langword="false"/>.
    /// </returns>
    public abstract bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value);

    /// <summary>
    /// Creates a new parser with the specified name.
    /// </summary>
    /// <param name="name">The name to assign to the parser.</param>
    /// <returns>
    /// A newly created <see cref="Parser{T}"/> instance with the specified name.
    /// </returns>
    protected internal virtual Parser<T> ToNamedParser(string? name) =>
        new Parser.NamedParser<T>(this) { Name = name };

    /// <summary>
    /// Creates a parser that matches the input without capturing or storing the parsed result.
    /// </summary>
    /// <returns>
    /// A lightweight <see cref="Parser{T}"/> instance that performs parsing without result allocation.
    /// </returns>
    protected internal virtual Parser<Unit> ToVoidParser() =>
        this as Parser<Unit> ?? new Parser.VoidParser<T>(this) { Name = Name };
}
