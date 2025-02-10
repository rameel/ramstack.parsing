namespace Ramstack.Parsing;

/// <summary>
/// Represents a parser that can be defined later.
/// </summary>
public sealed class DeferredParser<T> : Parser<T>
{
    /// <summary>
    /// Gets or sets the underlying parser.
    /// </summary>
    public Parser<T> Parser { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeferredParser{T}"/> class with an empty parser.
    /// </summary>
    internal DeferredParser() =>
        Parser = Parsing.Parser.Fail<T>("The deferred parser has not been initialized.");

    /// <summary>
    /// Initializes a new instance of the <see cref="DeferredParser{T}"/> class
    /// using the specified function to define the parser.
    /// </summary>
    /// <param name="parser">A function that accepts a reference to this deferred parser and returns the resulting parser.</param>
    internal DeferredParser(Func<Parser<T>, Parser<T>> parser) =>
        Parser = parser(this) ?? throw new InvalidOperationException("The deferred parser has not been initialized.");

    /// <inheritdoc />
    public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value) =>
        Parser.TryParse(ref context, out value);

    /// <inheritdoc />
    protected internal override Parser<T> ToNamedParser(string? name)
    {
        return new DeferredParser<T>
        {
            Name = name,
            Parser = Parser.ToNamedParser(name)
        };
    }

    /// <inheritdoc />
    protected internal override Parser<Unit> ToVoidParser()
    {
        return new DeferredParser<Unit>
        {
            Name = Name,
            Parser = Parser.Void()
        };
    }
}
