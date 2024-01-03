#if !NET7_0_OR_GREATER

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Specifies that a type has required members or that a member is required.
    /// </summary>
    [DebuggerNonUserCode]
    [AttributeUsage(AttributeTargets.Class
        | AttributeTargets.Struct
        | AttributeTargets.Field
        | AttributeTargets.Property,
        Inherited = false)]
    internal sealed class RequiredMemberAttribute : Attribute;

    /// <summary>
    /// Indicates that compiler support for a particular feature is required for the location where this attribute is applied.
    /// </summary>
    /// <param name="featureName">The name of the required compiler feature.</param>
    [DebuggerNonUserCode]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    internal sealed class CompilerFeatureRequiredAttribute(string featureName) : Attribute
    {
        /// <summary>
        /// Gets the name of the compiler feature.
        /// </summary>
        public string FeatureName => featureName;

        /// <summary>
        /// Gets a value that indicates whether the compiler can choose to allow access
        /// to the location where this attribute is applied if it does not understand <see cref="FeatureName"/>.
        /// </summary>
        public bool IsOptional { get; init; }
    }
}

namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Specifies that this constructor sets all required members for the current type,
    /// and callers do not need to set any required members themselves.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor)]
    internal sealed class SetsRequiredMembersAttribute : Attribute;
}

namespace System.Linq
{
    /// <summary>
    /// Provides extension methods for the <see cref="IEnumerable{T}"/>.
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Sorts the elements of a sequence in ascending order.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="comparer">An <see cref="IComparer{T}"/> to compare keys.</param>
        /// <returns>
        /// An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted.
        /// </returns>
        public static IOrderedEnumerable<T> Order<T>(this IEnumerable<T> source, IComparer<T>? comparer = null) =>
            source.OrderBy(OrderHelper<T>.IdentityFunc, comparer);

        /// <summary>
        /// Sorts the elements of a sequence in descending order.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="source">A sequence of values to order.</param>
        /// <param name="comparer">An <see cref="IComparer{T}"/> to compare keys.</param>
        /// <returns>
        /// An <see cref="IOrderedEnumerable{TElement}"/> whose elements are sorted.
        /// </returns>
        public static IOrderedEnumerable<T> OrderDescending<T>(this IEnumerable<T> source, IComparer<T>? comparer = null) =>
            source.OrderByDescending(OrderHelper<T>.IdentityFunc, comparer);

        private static class OrderHelper<T>
        {
            /// <summary>
            /// The function that returns the same object which was passed as parameter.
            /// </summary>
            public static readonly Func<T, T> IdentityFunc = v => v;
        }
    }
}

#endif
