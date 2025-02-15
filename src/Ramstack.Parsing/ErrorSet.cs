namespace Ramstack.Parsing;

/// <summary>
/// Represents a collection of parsing errors in the source text.
/// </summary>
internal struct ErrorSet
{
    private ArrayBuilder<string> _expectations;
    private int _index;

    /// <summary>
    /// Gets the position in the source text where a parsing error occurred.
    /// </summary>
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWithPrivateSetter")]
    public readonly int Index => _index;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorSet"/> structure.
    /// </summary>
    public ErrorSet() =>
        _expectations = new ArrayBuilder<string>();

    /// <summary>
    /// Reports a missing expected sequence or rule at the specified position.
    /// </summary>
    /// <param name="index">The position where the error occurred.</param>
    /// <param name="expected">The expected sequence or rule.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void ReportExpected(int index, string expected)
    {
        if (index >= _index)
        {
            if (index > _index)
            {
                _index = index;
                _expectations.Clear();
            }

            _expectations.Add(expected);
        }
    }

    /// <summary>
    /// Reports multiple missing expected sequences or rules at the specified position.
    /// </summary>
    /// <param name="index">The position in the source where the errors occurred.</param>
    /// <param name="expected">An array of expected sequences or rules.</param>
    public void ReportExpected(int index, string[] expected)
    {
        if (index >= _index)
        {
            if (index > _index)
            {
                _index = index;
                _expectations.Clear();
            }

            _expectations.AddRange(expected);
        }
    }

    /// <summary>
    /// Returns a formatted string describing the expected sequences that were not found.
    /// </summary>
    /// <returns>
    /// A formatted string listing the expected sequences.
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
