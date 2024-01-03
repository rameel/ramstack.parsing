namespace Ramstack.Parsing;

/// <summary>
/// Represents the diagnostic state for controlling the recording of parsing errors.
/// </summary>
public enum DiagnosticState
{
    /// <summary>
    /// Diagnostic messages are recorded during parsing.
    /// </summary>
    Normal,

    /// <summary>
    /// Diagnostic messages are suppressed and not recorded during parsing.
    /// </summary>
    Suppressed
}
