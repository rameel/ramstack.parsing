namespace Ramstack.Parsing;

/// <summary>
/// Represents a parser that can be defined later.
/// </summary>
public sealed class DeferredParser<T> : Parser<T>
{
    private static readonly Parser<T> s_sentinel =
        Parsing.Parser.Fail<T>("The deferred parser has not been initialized.");

    private Parser<T> _parser = s_sentinel;

    // Non-null only while ToVoidParser is running; breaks recursive conversion.
    private DeferredParser<Unit>? _voidParser;

    /// <summary>
    /// Gets or sets the underlying parser.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   The underlying parser can be assigned only once.
    /// </para>
    /// <para>
    ///   Assign the parser before composing the deferred parser with combinators that require
    ///   a void parser, such as <c>Then</c>, <c>And</c>, <c>Not</c>, or <c>Void</c>;
    ///   otherwise an <see cref="InvalidOperationException"/> is thrown.
    /// </para>
    /// </remarks>
    public Parser<T> Parser
    {
        get => _parser;
        set
        {
            Argument.ThrowIfNull(value);
            if (!ReferenceEquals(_parser, s_sentinel))
                throw new InvalidOperationException(
                    "The deferred parser has already been initialized.");

            _parser = value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeferredParser{T}"/> class with an empty parser.
    /// </summary>
    internal DeferredParser()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DeferredParser{T}"/> class
    /// using the specified function to define the parser.
    /// </summary>
    /// <param name="parser">A function that accepts a reference to this deferred parser and returns the resulting parser.</param>
    internal DeferredParser(Func<Parser<T>, Parser<T>> parser)
    {
        Parser = parser(this) ?? throw new InvalidOperationException("The parser factory returned null.");
        EnsureInitialized();
    }

    /// <inheritdoc />
    public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value) =>
        _parser.TryParse(ref context, out value);

    /// <inheritdoc />
    protected internal override Parser<Unit> ToVoidParser()
    {
        EnsureInitialized();

        if (_voidParser is not null)
            return _voidParser;

        // Register the placeholder before converting the inner parser, otherwise recursive
        // references convert indefinitely. Do not replace this with an object initializer.
        var self = new DeferredParser<Unit>();
        _voidParser = self;

        try
        {
            self.Parser = _parser.Void();
            return self;
        }
        finally
        {
            _voidParser = null;
        }
    }

    private void EnsureInitialized()
    {
        if (ReferenceEquals(_parser, s_sentinel))
            throw new InvalidOperationException("The deferred parser has not been initialized.");
    }
}
