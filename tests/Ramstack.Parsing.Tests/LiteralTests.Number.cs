using System.Globalization;
using System.Numerics;

namespace Ramstack.Parsing;

partial class LiteralTests
{
    [Test]
    public void Number_HexNumber()
    {
        Assert.That(
            Literal.Number<long>(NumberKind.HexNumber).Parse(long.MaxValue.ToString("x8")).Value,
            Is.EqualTo(long.MaxValue));
    }

    [Test]
    public void Number_BinaryNumber()
    {
        #if NET8_0_OR_GREATER
        Assert.That(
            Literal.Number<long>(NumberKind.BinaryNumber).Parse(long.MaxValue.ToString("b8")).Value,
            Is.EqualTo(long.MaxValue));
        #endif
    }

    [Test]
    public void Number_Byte()
    {
        Assert.That(
            Literal.Number<byte>().Parse(byte.MaxValue.ToString(CultureInfo.InvariantCulture)).Value,
            Is.EqualTo(byte.MaxValue));
    }

    [Test]
    public void Number_Short()
    {
        Assert.That(
            Literal.Number<short>().Parse(short.MinValue.ToString(CultureInfo.InvariantCulture)).Value,
            Is.EqualTo(short.MinValue));
    }

    [Test]
    public void Number_Long()
    {
        Assert.That(
            Literal.Number<long>().Parse(long.MinValue.ToString(CultureInfo.InvariantCulture)).Value,
            Is.EqualTo(long.MinValue));
    }

    [Test]
    public void Number_Int128()
    {
        #if NET7_0_OR_GREATER
        Assert.That(
            Literal.Number<Int128>().Parse(Int128.MaxValue.ToString(CultureInfo.InvariantCulture)).Value,
            Is.EqualTo(Int128.MaxValue));
        #endif
    }

    [Test]
    public void Number_BigInteger()
    {
        const string Number = "1701411834604692317316873037158841057271701411834604692317316873037158841057271701411834604692311701411834604692";
        Assert.That(
            Literal.Number<BigInteger>().Parse(Number).Value,
            Is.EqualTo(BigInteger.Parse(Number)));
    }

    [Test]
    public void Number_DecimalNumber()
    {
        Assert.That(
            Literal.Number<decimal>().Parse(decimal.MaxValue.ToString(CultureInfo.InvariantCulture)).Value,
            Is.EqualTo(decimal.MaxValue));
    }

    [Test]
    public void Number_HalfNumber()
    {
        Assert.That(
            Literal.Number<Half>().Parse(Half.MaxValue.ToString(CultureInfo.InvariantCulture)).Value,
            Is.EqualTo(Half.MaxValue));
    }

    [Test]
    public void Number_FloatNumber()
    {
        Assert.That(
            Literal.Number<float>().Parse(float.MaxValue.ToString(CultureInfo.InvariantCulture)).Value,
            Is.EqualTo(float.MaxValue));
    }

    [Test]
    public void Number_DoubleNumber()
    {
        Assert.That(
            Literal.Number<double>().Parse(double.MaxValue.ToString(CultureInfo.InvariantCulture)).Value,
            Is.EqualTo(double.MaxValue));
    }

    [Test]
    public void Number_UnsignedNumber()
    {
        Assert.That(Literal.Number<byte>(NumberKind.Integer).Parse("+123").Value, Is.EqualTo((byte)123));
        Assert.That(Literal.Number<ushort>(NumberKind.Integer).Parse("+123").Value, Is.EqualTo((ushort)123));
        Assert.That(Literal.Number<uint>(NumberKind.Integer).Parse("+123").Value, Is.EqualTo((uint)123));
        Assert.That(Literal.Number<ulong>(NumberKind.Integer).Parse("+123").Value, Is.EqualTo((ulong)123));
        Assert.That(Literal.Number<nuint>(NumberKind.Integer).Parse("+123").Value, Is.EqualTo((nuint)123));

        Assert.That(Literal.Number<byte>(NumberKind.Integer).Parse("-123").Success, Is.False);
        Assert.That(Literal.Number<ushort>(NumberKind.Integer).Parse("-123").Success, Is.False);
        Assert.That(Literal.Number<uint>(NumberKind.Integer).Parse("-123").Success, Is.False);
        Assert.That(Literal.Number<ulong>(NumberKind.Integer).Parse("-123").Success, Is.False);
        Assert.That(Literal.Number<nuint>(NumberKind.Integer).Parse("-123").Success, Is.False);

        #if NET7_0_OR_GREATER
        Assert.That(Literal.Number<UInt128>(NumberKind.Integer).Parse("+123").Value, Is.EqualTo((UInt128)123));
        Assert.That(Literal.Number<UInt128>(NumberKind.Integer).Parse("-123").Success, Is.False);
        #endif
    }

    [Test]
    public void Number_Sign()
    {
        Assert.That(
            Literal.Number<int>(NumberKind.HexNumber).Parse("-FF").Success,
            Is.False);

        Assert.That(
            Literal.Number<int>(NumberKind.HexNumber).Parse("+FF").Success,
            Is.False);

        #if NET8_0_OR_GREATER
        Assert.That(
            Literal.Number<int>(NumberKind.BinaryNumber).Parse("-11101").Success,
            Is.False);

        Assert.That(
            Literal.Number<int>(NumberKind.BinaryNumber).Parse("+11101").Success,
            Is.False);
        #endif
    }
}
