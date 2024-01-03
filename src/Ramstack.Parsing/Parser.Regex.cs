#if NET7_0_OR_GREATER

using System.Text.RegularExpressions;

namespace Ramstack.Parsing;

partial class Parser
{
    /// <summary>
    /// Creates a parser that matches the specified regular expression pattern.
    /// </summary>
    /// <remarks>
    /// The regular expression pattern must start with the <c>"^"</c> anchor character.
    /// </remarks>
    /// <param name="regex">The regular expression.</param>
    /// <returns>
    /// A parser that parses the specified regular expression pattern.
    /// </returns>
    public static Parser<string> Regex(Regex regex)
    {
        if (!regex.ToString().StartsWith('^'))
            throw new ArgumentException("The pattern must start with the '^' character.");

        return new RegexParser<string>(regex);
    }

    /// <summary>
    /// Represents a parser that matches the specified regular expression pattern.
    /// </summary>
    private sealed class RegexParser<T> : Parser<T>
    {
        /// <summary>
        /// Gets the regular expression.
        /// </summary>
        public Regex Expression { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexParser{T}"/> class.
        /// </summary>
        /// <param name="regex">The regular expression.</param>
        public RegexParser(Regex regex) =>
            (Expression, Name) = (regex, regex.ToString());

        /// <inheritdoc />
        public override bool TryParse(ref ParseContext context, [NotNullWhen(true)] out T? value)
        {
            var source = context.Remaining;
            if (source.Length != 0)
            {
                var e = Expression.EnumerateMatches(source);
                if (e.MoveNext() && e.Current.Index == 0)
                {
                    context.Advance(e.Current.Length);

                    if (typeof(T) != typeof(Unit))
                        value = ((T)(object)context.MatchedSegment.ToString())!;
                    else
                        value = default!;

                    return true;
                }
            }

            value = default;
            context.AddError(Name);

            return false;
        }

        /// <inheritdoc />
        protected internal override Parser<T> ToNamedParser(string? name) =>
            new RegexParser<T>(Expression) { Name = name };

        /// <inheritdoc />
        protected internal override Parser<Unit> ToVoidParser() =>
            new RegexParser<Unit>(Expression) { Name = Name };
    }
}

#endif
