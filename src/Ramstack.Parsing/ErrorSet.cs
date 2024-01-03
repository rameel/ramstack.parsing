namespace Ramstack.Parsing;

/// <summary>
/// Represents a set of parsing errors and the positions at which they occurred in the source text.
/// </summary>
internal struct ErrorSet
{
    private ArrayBuilder<string> _expectations;
    private int _index;

    /// <summary>
    /// Gets the position in the source where the parsing failed or an unmatched sequence was encountered.
    /// </summary>
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWithPrivateSetter")]
    public readonly int Index => _index;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorSet"/> structure.
    /// </summary>
    public ErrorSet() =>
        _expectations = new ArrayBuilder<string>();

    /// <summary>
    /// Adds an error message describing an expected sequence at the specified source position.
    /// </summary>
    /// <param name="index">The position in the source where the error occurred.</param>
    /// <param name="error">A string describing the expected sequence.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void AddError(int index, string error)
    {
        if (index >= _index)
        {
            if (index > _index)
            {
                _index = index;
                _expectations.Clear();
            }

            _expectations.Add(error);
        }
    }

    /// <summary>
    /// Adds multiple error messages describing expected sequences at the specified source position.
    /// </summary>
    /// <param name="index">The position in the source where the errors occurred.</param>
    /// <param name="errors">An array of strings describing the expected sequences.</param>
    public void AddErrors(int index, string[] errors)
    {
        if (index >= _index)
        {
            if (index > _index)
            {
                _index = index;
                _expectations.Clear();
            }

            _expectations.AddRange(errors);
        }
    }

    /// <summary>
    /// Returns a formatted string representing the list of expected sequences that were not matched.
    /// </summary>
    /// <returns>
    /// A string describing the expected sequences, formatted appropriately.
    /// </returns>
    public readonly override string ToString()
    {
        if (_expectations.Count == 0)
            return "";

        var list = _expectations
            .InnerBuffer
            .Take(_expectations.Count)
            .Distinct()
            .ToArray();

        if (list.Length == 0)
            return "";

        return "Expected " + list.Length switch
        {
            1 => list[0],
            2 => list[0] + " or " + list[1],
            _ => string.Join(", ", list, 0, list.Length - 1) + ", or " + list[^1]
        };
    }
}
