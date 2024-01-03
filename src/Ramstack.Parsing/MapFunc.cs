namespace Ramstack.Parsing;

/// <summary>
/// Encapsulates a method that receives a <see cref="Match"/> representing a matched
/// segment from the parsed source and returns a <typeparamref name="TResult"/>.
/// </summary>
/// <param name="match">The matched segment from the parsed source.</param>
/// <typeparam name="TResult">The type of the returned value.</typeparam>
/// <returns>
/// A <typeparamref name="TResult"/> that is the result of processing or transforming
/// the matched segment.
/// </returns>
public delegate TResult MapFunc<out TResult>(Match match);

/// <summary>
/// Encapsulates a method that receives a <see cref="Match"/> representing a matched
/// segment from the parsed source, along with a parsed value of type <typeparamref name="T"/>,
/// and returns a <typeparamref name="TResult"/>.
/// </summary>
/// <typeparam name="T">The type of the parsed value.</typeparam>
/// <typeparam name="TResult">The type of the returned value.</typeparam>
/// <param name="match">The matched segment from the parsed source.</param>
/// <param name="value">The parsed value of type <typeparamref name="T"/>.</param>
/// <returns>
/// A <typeparamref name="TResult"/> that is the result of processing or transforming
/// both the matched segment and the parsed value.
/// </returns>
public delegate TResult MapFunc<in T, out TResult>(Match match, T value);
