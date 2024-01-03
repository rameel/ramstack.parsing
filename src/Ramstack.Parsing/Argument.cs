namespace Ramstack.Parsing;

/// <summary>
/// Provides helper methods to validate arguments and throw exceptions if the arguments do not meet specified conditions.
/// </summary>
internal static class Argument
{
    #if NET8_0_OR_GREATER
    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than <paramref name="other"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="other">The reference value to compare against.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfLessThan<T>(T value, T other, [CallerArgumentExpression("value")] string? paramName = null) where T : struct, System.Numerics.IComparisonOperators<T, T, bool>
    {
        if (value < other)
            Error_ArgumentLess(value, other, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than or equal to <paramref name="other"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="other">The reference value to compare against.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfLessThanOrEqual<T>(T value, T other, [CallerArgumentExpression("value")] string? paramName = null) where T : struct, System.Numerics.IComparisonOperators<T, T, bool>
    {
        if (value <= other)
            Error_ArgumentLessOrEqual(value, other, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than <paramref name="other"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="other">The reference value to compare against.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfGreaterThan<T>(T value, T other, [CallerArgumentExpression("value")] string? paramName = null) where T : struct, System.Numerics.IComparisonOperators<T, T, bool>
    {
        if (value > other)
            Error_ArgumentGreater(value, other, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than or equal to <paramref name="other"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="other">The reference value to compare against.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfGreaterThanOrEqual<T>(T value, T other, [CallerArgumentExpression("value")] string? paramName = null) where T : struct, System.Numerics.IComparisonOperators<T, T, bool>
    {
        if (value >= other)
            Error_ArgumentGreaterOrEqual(value, other, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.
    /// </summary>
    /// <typeparam name="T">The type of the value being checked, which implements <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfNegative<T>(T value, [CallerArgumentExpression("value")] string? paramName = null) where T : struct, System.Numerics.INumberBase<T>
    {
        if (T.IsNegative(value))
            Error_ArgumentNegative(value, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative or zero.
    /// </summary>
    /// <typeparam name="T">The type of the value being checked, which implements <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfNegativeOrZero<T>(T value, [CallerArgumentExpression("value")] string? paramName = null) where T : struct, System.Numerics.INumberBase<T>
    {
        if (T.IsNegative(value) || T.IsZero(value))
            Error_ArgumentNegativeOrZero(value, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is equal to <paramref name="other"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared, which implements <see cref="IEquatable{T}"/>.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="other">The reference value to compare against.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfEqual<T>(T value, T other, [CallerArgumentExpression("value")] string? paramName = null) where T : struct, System.Numerics.IEqualityOperators<T, T, bool>
    {
        if (value == other)
            Error_ArgumentEqual(value, other, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is not equal to <paramref name="other"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared, which implements <see cref="IEquatable{T}"/>.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="other">The reference value to compare against.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfNotEqual<T>(T value, T other, [CallerArgumentExpression("value")] string? paramName = null) where T : struct, System.Numerics.IEqualityOperators<T, T, bool>
    {
        if (value != other)
            Error_ArgumentNotEqual(value, other, paramName);
    }
    #else
    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than <paramref name="other"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="other">The reference value to compare against.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfLessThan<T>(T value, T other, [CallerArgumentExpression("value")] string? paramName = null) where T : struct, IComparable<T>
    {
        if (value.CompareTo(other) < 0)
            Error_ArgumentLess(value, other, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than or equal to <paramref name="other"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="other">The reference value to compare against.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfLessThanOrEqual<T>(T value, T other, [CallerArgumentExpression("value")] string? paramName = null) where T : struct, IComparable<T>
    {
        if (value.CompareTo(other) <= 0)
            Error_ArgumentLessOrEqual(value, other, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than <paramref name="other"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="other">The reference value to compare against.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfGreaterThan<T>(T value, T other, [CallerArgumentExpression("value")] string? paramName = null) where T : struct, IComparable<T>
    {
        if (value.CompareTo(other) > 0)
            Error_ArgumentGreater(value, other, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than or equal to <paramref name="other"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="other">The reference value to compare against.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfGreaterThanOrEqual<T>(T value, T other, [CallerArgumentExpression("value")] string? paramName = null) where T : struct, IComparable<T>
    {
        if (value.CompareTo(other) >= 0)
            Error_ArgumentGreaterOrEqual(value, other, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.
    /// </summary>
    /// <typeparam name="T">The type of the value being checked, which implements <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfNegative<T>(T value, [CallerArgumentExpression("value")] string? paramName = null) where T : struct, IComparable<T>
    {
        if (value.CompareTo(default) < 0)
            Error_ArgumentNegative(value, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative or zero.
    /// </summary>
    /// <typeparam name="T">The type of the value being checked, which implements <see cref="IComparable{T}"/>.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfNegativeOrZero<T>(T value, [CallerArgumentExpression("value")] string? paramName = null) where T : struct, IComparable<T>
    {
        if (value.CompareTo(default) <= 0)
            Error_ArgumentNegativeOrZero(value, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is equal to <paramref name="other"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared, which implements <see cref="IEquatable{T}"/>.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="other">The reference value to compare against.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfEqual<T>(T value, T other, [CallerArgumentExpression("value")] string? paramName = null) where T : IEquatable<T>?
    {
        if (EqualityComparer<T>.Default.Equals(value, other))
            Error_ArgumentEqual(value, other, paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is not equal to <paramref name="other"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values being compared, which implements <see cref="IEquatable{T}"/>.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="other">The reference value to compare against.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfNotEqual<T>(T value, T other, [CallerArgumentExpression("value")] string? paramName = null) where T : IEquatable<T>?
    {
        if (EqualityComparer<T>.Default.Equals(value, other))
            Error_ArgumentNotEqual(value, other, paramName);
    }
    #endif

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the specified object is <see langword="null"/>.
    /// </summary>
    /// <param name="value">The object to check.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfNull([NotNull] object? value, [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (value is null)
            Error_ArgumentNull(paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the specified span is empty.
    /// </summary>
    /// <param name="value">The span to check.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfEmpty<T>(ReadOnlySpan<T> value, [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (value.IsEmpty)
            Error_ArgumentEmpty(paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the specified string is <see langword="null"/> or empty.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfNullOrEmpty([NotNull] string? value, [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (string.IsNullOrEmpty(value))
            Error_ArgumentNullOrEmpty(paramName);
    }

    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the specified array is <see langword="null"/> or empty.
    /// </summary>
    /// <typeparam name="T">The type of the array elements.</typeparam>
    /// <param name="value">The array to check.</param>
    /// <param name="paramName">The name of the parameter being checked. Automatically supplied by the compiler.</param>
    public static void ThrowIfNullOrEmpty<T>([NotNull] T[]? value, [CallerArgumentExpression("value")] string? paramName = null)
    {
        if (value is null || value.Length == 0)
            Error_ArgumentNullOrEmpty(paramName);
    }

    [DoesNotReturn]
    private static void Error_ArgumentNull(string? paramName) =>
        throw new ArgumentNullException(paramName);

    [DoesNotReturn]
    private static void Error_ArgumentEmpty(string? paramName) =>
        throw new ArgumentException("Argument should not be empty.", paramName);

    [DoesNotReturn]
    private static void Error_ArgumentNullOrEmpty(string? paramName) =>
        throw new ArgumentException("Argument should not be null or empty.", paramName);

    [DoesNotReturn]
    private static void Error_ArgumentEqual<T>(T value, T other, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must not be equal to '{other}'.");

    [DoesNotReturn]
    private static void Error_ArgumentNotEqual<T>(T value, T other, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be equal to '{other}'.");

    [DoesNotReturn]
    private static void Error_ArgumentLess<T>(T value, T other, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be greater than or equal to '{other}'.");

    [DoesNotReturn]
    private static void Error_ArgumentLessOrEqual<T>(T value, T other, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be greater than '{other}'.");

    [DoesNotReturn]
    private static void Error_ArgumentGreater<T>(T value, T other, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be less than or equal to '{other}'.");

    [DoesNotReturn]
    private static void Error_ArgumentGreaterOrEqual<T>(T value, T other, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be less than '{other}'.");

    [DoesNotReturn]
    private static void Error_ArgumentNegative<T>(T value, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be greater than or equal to zero.");

    [DoesNotReturn]
    private static void Error_ArgumentNegativeOrZero<T>(T value, string? paramName) =>
        throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}') must be greater than zero.");
}
