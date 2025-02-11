using System.Text;

namespace Samples.Utilities;

internal sealed class IndentedStringBuilder
{
    private int _indent;
    private bool _indentPending = true;

    private readonly StringBuilder _sb = new StringBuilder();

    public IndentedStringBuilder Append(char value)
    {
        DoIndent();
        _sb.Append(value);
        return this;
    }

    public IndentedStringBuilder Append(int value)
    {
        DoIndent();
        _sb.Append(value);
        return this;
    }

    public IndentedStringBuilder Append(string value)
    {
        DoIndent();
        _sb.Append(value);
        return this;
    }

    public IndentedStringBuilder Append(FormattableString value)
    {
        DoIndent();
        _sb.Append(value);
        return this;
    }

    public IndentedStringBuilder AppendLine()
    {
        AppendLine(string.Empty);
        return this;
    }

    public IndentedStringBuilder AppendLine(string value)
    {
        DoIndent();
        _sb.AppendLine(value);
        _indentPending = true;
        return this;
    }

    public IndentedStringBuilder IncrementIndent()
    {
        _indent++;
        return this;
    }

    public IndentedStringBuilder DecrementIndent()
    {
        _indent = Math.Max(0, _indent - 1);
        return this;
    }

    public override string ToString() =>
        _sb.ToString();

    private void DoIndent()
    {
        if (_indentPending && _indent != 0)
            _sb.Append(' ', _indent * 4);

        _indentPending = false;
    }
}
