namespace Ramstack.Parsing;

/// <summary>
/// Represents an optional value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The underlying type of the optional value.</typeparam>
public readonly struct OptionalValue<T>
{
    /// <summary>
    /// The underlying value.
    /// If <see cref="HasValue"/> is <see langword="false"/>, this contains an uninitialized value.
    /// </summary>
    public readonly T? Value;

    /// <summary>
    /// A value indicating whether the current <see cref="OptionalValue{T}"/> object
    /// has a valid value of its underlying type.
    /// </summary>
    public readonly bool HasValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionalValue{T}"/> structure with the specified value.
    /// </summary>
    /// <param name="value">The value to store.</param>
    public OptionalValue(T value)
    {
        Value = value;
        HasValue = true;
    }
}
