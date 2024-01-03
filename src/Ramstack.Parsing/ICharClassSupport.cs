namespace Ramstack.Parsing;

/// <summary>
/// Provides support for working with character classes.
/// </summary>
internal interface ICharClassSupport
{
    /// <summary>
    /// Generates a <see cref="CharClass"/> object representing the current instance.
    /// </summary>
    /// <returns>
    /// A <see cref="CharClass"/> object representing the current instance.
    /// </returns>
    CharClass GetCharClass();
}
