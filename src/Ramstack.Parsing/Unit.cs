namespace Ramstack.Parsing;

/// <summary>
/// Specifies a type that does not have a value, serving as a void-like type for generic contexts.
/// Since <see cref="Void"/> cannot be used as a generic type parameter, <see cref="Unit"/> provides
/// an alternative for scenarios requiring a "void" type in generic code.
/// </summary>
public readonly struct Unit
{
    #pragma warning disable 169
    // Before .NET 9, the JIT generates inefficient code for structures without fields.
    private readonly int _;
    #pragma warning restore 169

    /// <summary>
    /// Gets the value of the <see cref="Unit"/> type.
    /// </summary>
    public static Unit Value => default;
}
