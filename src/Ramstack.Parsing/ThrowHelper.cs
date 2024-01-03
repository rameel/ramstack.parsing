namespace Ramstack.Parsing;

internal static class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowArgumentOutOfRangeException() =>
        throw new ArgumentOutOfRangeException();

    [DoesNotReturn]
    public static void ThrowNotSupportedException() =>
        throw new NotSupportedException();
}
