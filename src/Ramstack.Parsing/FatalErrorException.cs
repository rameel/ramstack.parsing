namespace Ramstack.Parsing;

/// <summary>
/// Represents a user-defined error that occurred during parsing.
/// </summary>
/// <param name="message">The message describing the error.</param>
internal sealed class FatalErrorException(string message) : Exception(message);
