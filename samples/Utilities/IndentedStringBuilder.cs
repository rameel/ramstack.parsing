using System.Text;

namespace Samples.Utilities;

internal sealed class IndentedStringBuilder
{
    private int _indent;
    private bool _indentPending = true;

    private readonly StringBuilder _sb = new StringBuilder();

    public int Length => _sb.Length;

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

    public IndentedStringBuilder Append(char value)
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
        if (value.Length != 0)
            DoIndent();

        _sb.AppendLine(value);
        _indentPending = true;
        return this;
    }

    public IndentedStringBuilder AppendLine(FormattableString value)
    {
        DoIndent();
        _sb.Append(value);
        _indentPending = true;
        return this;
    }

    public IndentedStringBuilder Clear()
    {
        _sb.Clear();
        _indentPending = true;
        _indent = 0;

        return this;
    }

    public IndentedStringBuilder IncrementIndent()
    {
        _indent++;
        return this;
    }

    public IndentedStringBuilder DecrementIndent()
    {
        if (_indent > 0)
            _indent--;

        return this;
    }

    public IDisposable Indent() =>
        new Indenter(this);

    public override string ToString() =>
        _sb.ToString();

    private void DoIndent()
    {
        if (_indentPending && _indent > 0)
            _sb.Append(' ', _indent * 4);

        _indentPending = false;
    }

    private sealed class Indenter : IDisposable
    {
        private readonly IndentedStringBuilder _sb;

        public Indenter(IndentedStringBuilder sb)
        {
            _sb = sb;
            _sb.IncrementIndent();
        }

        public void Dispose() =>
            _sb.DecrementIndent();
    }
}
