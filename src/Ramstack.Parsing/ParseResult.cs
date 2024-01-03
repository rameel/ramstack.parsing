namespace Ramstack.Parsing;

/// <summary>
/// Represents the result of a parsing operation.
/// </summary>
/// <typeparam name="T">The type of the parsed value.</typeparam>
public sealed class ParseResult<T>
{
    /// <summary>
    /// Gets a value indicating whether the parsing was successful.
    /// </summary>
    public bool Success => string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Gets the parsed value if parsing succeeded;
    /// otherwise, the <see langword="default"/> value of <typeparamref name="T"/>.
    /// </summary>
    public T? Value { get; init; }

    /// <summary>
    /// Gets the number of characters that were successfully matched during parsing.
    /// </summary>
    public required int Length { get; init; }

    /// <summary>
    /// Gets the error message that occurred during parsing, if any.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Success))]
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the exception that was thrown during parsing, if any.
    /// </summary>
    public Exception? Exception { get; init; }

    /// <inheritdoc />
    public override string ToString() =>
        (Success ? Value?.ToString() : ErrorMessage) ?? "";
}
