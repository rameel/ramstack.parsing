namespace Ramstack.Parsing;

/// <summary>
/// Specifies the types of numeric string representations.
/// </summary>
public enum NumberKind
{
    /// <summary>
    /// Automatically determines the numeric type that can be parsed.
    /// </summary>
    Auto,

    /// <summary>
    /// Indicates that the input represents a floating-point numeric string,
    /// optionally prefixed with a "+" or "-" sign.
    /// </summary>
    Float,

    /// <summary>
    /// Indicates that the input represents an integer numeric string,
    /// optionally prefixed with a "+" or "-" sign.
    /// </summary>
    Integer,

    /// <summary>
    /// Indicates that the input represents a hexadecimal numeric string.
    /// Leading "+" or "-" signs are not allowed.
    /// </summary>
    HexNumber,

    #if NET8_0_OR_GREATER
    /// <summary>
    /// Indicates that the input represents a binary numeric string.
    /// Leading "+" or "-" signs are not allowed.
    /// </summary>
    BinaryNumber
    #endif
}
