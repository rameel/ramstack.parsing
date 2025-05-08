namespace Ramstack.Parsing.Utilities;

[TestFixture]
public class StringBufferTests
{
    [Test]
    public void Ctor()
    {
        var sb = new StringBuffer();

        Assert.That(sb.ToString(), Is.EqualTo(""));
        Assert.That(sb.Length, Is.EqualTo(0));
    }

    [Test]
    public void Ctor_Capacity()
    {
        var sb = new StringBuffer(10);

        Assert.That(sb.Length, Is.EqualTo(0));
        Assert.That(sb.ToString(), Is.EqualTo(""));
    }

    [TestCase(-1)]
    [TestCase(-10)]
    [TestCase(int.MinValue)]
    [TestCase(-int.MaxValue)]
    public void Ctor_InvalidCapacity(int capacity) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new StringBuffer(capacity));

    [Test]
    public void Append_Char()
    {
        for (var count = 1; count <= 256; count++)
        {
            var sb = new StringBuffer();
            var expected = "";

            for (var n = 1; n <= count; n++)
            {
                sb.Append('a');
                expected += 'a';

                Assert.That(sb.Length, Is.EqualTo(n));
            }

            Assert.That(sb.ToString(), Is.EqualTo(expected));
        }
    }

    [TestCase("")]
    [TestCase("1")]
    [TestCase("12")]
    [TestCase("123456789")]
    public void Append_ReadOnlySpan(string text)
    {
        var sb = new StringBuffer();
        sb.Append(text);

        Assert.That(sb.Length, Is.EqualTo(text.Length));
        Assert.That(sb.ToString(), Is.EqualTo(text));
    }

    [Test]
    public void Append_LargeValue()
    {
        var a1 = new string('a', 50);
        var a2 = new string('b', 1000);

        var sb = new StringBuffer();

        sb.Append(a1);
        Assert.That(sb.Length, Is.EqualTo(a1.Length));

        sb.Append(a2);
        Assert.That(sb.Length, Is.EqualTo(a1.Length + a2.Length));

        Assert.That(sb.ToString(), Is.EqualTo(a1 + a2));
    }

    [Test]
    public void FailOnLargeString()
    {
        Assert.Throws<OverflowException>(() =>
        {
            var sb = new StringBuffer();
            sb.Append(new char[2_000_000_000]);
            sb.Append(new char[500_000_000]);
        });
    }
}
