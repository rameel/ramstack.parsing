#if TEST_INTERNALS
namespace Ramstack.Parsing.Collections;

[TestFixture]
public class ArrayBuilderTests
{
    [Test]
    public void Ctor_ZeroCapacity()
    {
        var builder = new ArrayBuilder<int>();

        foreach (var _ in builder.AsSpan())
            Assert.Fail();

        Assert.That(builder.Count, Is.Zero);
        Assert.That(builder.InnerBuffer.Length, Is.EqualTo(0));
        Assert.That(builder.Clear, Throws.Nothing);
    }

    [Test]
    public void Add_ValueAdded()
    {
        var builder = new ArrayBuilder<int>();
        builder.Add(10);

        Assert.That(builder.Count, Is.EqualTo(1));
        Assert.That(builder.InnerBuffer.Length > 1, Is.True);
        Assert.That(builder.AsSpan().Length, Is.EqualTo(1));
        Assert.That(builder.AsSpan()[0], Is.EqualTo(10));
    }

    [Test]
    public void Add_MultipleValues_AllValuesAdded()
    {
        const int Count = 999;

        var builder = new ArrayBuilder<object>();
        for (var i = 0; i < Count; i++)
            builder.Add(i);

        Assert.That(builder.Count, Is.EqualTo(Count));
        Assert.That(builder.InnerBuffer.Length, Is.GreaterThan(Count));
        Assert.That(builder.AsSpan().Length, Is.EqualTo(Count));
    }

    [Test]
    public void AddRange_AllValuesAdded()
    {
        var builder = new ArrayBuilder<int>();
        builder.Add(1);
        builder.AddRange([2, 3, 4, 5, 6, 7, 8, 9, 10]);

        Assert.That(builder.Count, Is.EqualTo(10));
        Assert.That(builder.AsSpan().Length, Is.EqualTo(10));
        Assert.That(builder.InnerBuffer.Length, Is.GreaterThan(10));
        Assert.That(builder.ToArray(), Is.EquivalentTo(Enumerable.Range(1, 10)));
    }

    [Test]
    public void Clear_ReferenceValues_Keep()
    {
        var builder = new ArrayBuilder<string?>();
        for (var i = 0; i < 10; i++)
            builder.Add(i.ToString());

        builder.Clear();

        Assert.That(builder.Count, Is.Zero);
        Assert.That(builder.InnerBuffer.Take(10).All(v => v is not null), Is.True);
    }

    [Test]
    public void Clear_BufferUnchanged()
    {
        var builder = new ArrayBuilder<int>();
        for (var i = 0; i < 10; i++)
            builder.Add(i);

        var buffer = builder.InnerBuffer;
        builder.Clear();

        Assert.That(builder.InnerBuffer, Is.SameAs(buffer));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(7)]
    [TestCase(9)]
    public void AsSpan(int count)
    {
        var builder = new ArrayBuilder<int>();
        for (var i = 0; i < count; i++)
            builder.Add(i);

        Assert.That(
            builder.ToArray(),
            Is.EquivalentTo(Enumerable.Range(0, count)));
    }

    [Test]
    public void AsSpan_Empty()
    {
        var view = new ArrayBuilder<string>().AsSpan();
        Assert.That(view.Length, Is.Zero);
    }
}
#endif
