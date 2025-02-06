namespace Ramstack.Parsing.Collections;

[TestFixture]
public class ArrayListTests
{
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(9)]
    public void Ctor_Capacity(int capacity)
    {
        var list = new ArrayList<int>(capacity);

        foreach (var _ in list)
            Assert.Fail();

        foreach (var _ in list.AsSpan())
            Assert.Fail();

        Assert.That(list.Count, Is.Zero);
        Assert.That(list.ToArray(), Is.SameAs(Array.Empty<int>()));
        Assert.That(list.Clear, Throws.Nothing);
    }

    [TestCase(-1)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    public void Ctor_InvalidCapacity_ShouldThrow(int capacity)
    {
        Assert.That(
            () => new ArrayList<int>(capacity),
            Throws.TypeOf<OverflowException>().Or.TypeOf<OutOfMemoryException>());
    }

    [Test]
    public void Indexer_ReturnsReference()
    {
        var list = new ArrayList<int>();
        for (var i = 1; i <= 5; i++)
            list.Add(i);

        list[1] = 10;

        Assert.That(list[3], Is.EqualTo(4));

        ref var value = ref list[3];
        value = 20;

        Assert.That(list[3], Is.EqualTo(20));
        Assert.That(
            list.ToArray(),
            Is.EquivalentTo([1, 10, 3, 20, 5]));
    }

    [Test]
    public void Indexer_ReturnsValue()
    {
        var list = new ArrayList<int>();
        for (var i = 0; i < 5; i++)
            list.Add(i);

        for (var i = 0; i < list.Count; i++)
            Assert.That(list[i], Is.EqualTo(i));
    }

    [Test]
    public void Indexer_InvalidIndex_ShouldThrow()
    {
        var list = new ArrayList<int>();
        Assert.That(
            () => list[0],
            Throws.TypeOf<ArgumentOutOfRangeException>());

        for (var i = 0; i < 5; i++)
            list.Add(i);

        Assert.That(
            () => list[6],
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Add_ValueAdded()
    {
        var list = new ArrayList<int> { 10 };

        Assert.That(list.Count, Is.EqualTo(1));
        Assert.That(list.ToArray(), Is.EquivalentTo([10]));
    }

    [Test]
    public void Add_MultipleValues_AllValuesAdded()
    {
        const int Count = 999;

        var list = new ArrayList<object>(4);
        for (var i = 0; i < Count; i++)
            list.Add(i);

        Assert.That(list.Count, Is.EqualTo(Count));
        Assert.That(list.ToArray().Length, Is.EqualTo(Count));
    }

    [Test]
    public void Insert_ValuesAdded()
    {
        var list = new ArrayList<object>();
        for (var i = 0; i < 10; i++)
            list.Insert(i, i);

        Assert.That(list.Count, Is.EqualTo(10));
        Assert.That(list.ToArray(), Is.EquivalentTo(Enumerable.Range(0, 10)));
    }

    [Test]
    public void Insert_FromStart_ValuesAdded()
    {
        var list = new ArrayList<int>(4);
        for (var i = 0; i < 10; i++)
            list.Insert(0, i);

        Assert.That(list.Count, Is.EqualTo(10));
        Assert.That(list.ToArray(), Is.EquivalentTo(Enumerable.Range(0, 10).Reverse()));
    }

    [Test]
    public void Insert_OutOfRange_ShouldThrow()
    {
        var list = new ArrayList<int>();
        for (var i = 0; i < 10; i++)
            list.Insert(0, i);

        Assert.Throws<ArgumentOutOfRangeException>(() => list.Insert(11, 0));
        Assert.That(list.Count, Is.EqualTo(10));
    }

    [Test]
    public void RemoveAt_EmptyList_ShouldThrow()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        var list = new ArrayList<int>();

        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(5));
        Assert.That(list.Count, Is.EqualTo(0));
    }

    [Test]
    public void RemoveAt_OutOfRange_ShouldThrow()
    {
        var list = new ArrayList<int>();
        for (var i = 0; i < 5; i++)
            list.Add(i);

        Assert.Throws<ArgumentOutOfRangeException>(() => list.RemoveAt(5));
        Assert.That(list.Count, Is.EqualTo(5));
    }

    [Test]
    public void RemoveAt_FromStart_ValueRemoved()
    {
        const int Count = 100;

        var reference = new List<string?>();
        var list = new ArrayList<string?>();

        for (var i = 0; i < Count; i++)
        {
            list.Add(i.ToString());
            reference.Add(i.ToString());
        }

        for (var i = 0; i < Count; i++)
        {
            list.RemoveAt(0);
            reference.RemoveAt(0);

            Assert.That(list.Count, Is.EqualTo(reference.Count));
            Assert.That(list, Is.EquivalentTo(reference));
            Assert.That(list.ToArray(), Is.EquivalentTo(reference));
        }

        Assert.That(list.Count, Is.Zero);
        Assert.That(list.Any(), Is.False);
    }

    [Test]
    public void RemoveAt_FromEnd_ValueRemoved()
    {
        const int Count = 100;

        var reference = new List<string?>();
        var list = new ArrayList<string?>();

        for (var i = 0; i < Count; i++)
        {
            list.Add(i.ToString());
            reference.Add(i.ToString());
        }

        for (var i = Count - 1; i >= 0; i--)
        {
            list.RemoveAt(i);
            reference.RemoveAt(i);

            Assert.That(list.Count, Is.EqualTo(reference.Count));
            Assert.That(list, Is.EquivalentTo(reference));
            Assert.That(list.ToArray(), Is.EquivalentTo(reference));
        }

        Assert.That(list.Count, Is.Zero);
        Assert.That(list.Any(), Is.False);
    }

    [Test]
    public void RemoveAt_RemoveAll_AllValuesRemoved()
    {
        const int Count = 100;

        var list = new ArrayList<int>();
        for (var i = 0; i < Count; i++)
            list.Add(i);

        for (var i = 0; i < Count; i++)
            list.RemoveAt(0);

        Assert.That(list.Count, Is.EqualTo(0));
        Assert.That(list, Is.EquivalentTo(Array.Empty<int>()));
        Assert.That(list.ToArray(), Is.EquivalentTo(Array.Empty<int>()));
    }

    [Test]
    public void Clear_AllValuesRemoved()
    {
        var list = new ArrayList<string?>();
        for (var i = 0; i < 10; i++)
            list.Add(i.ToString());

        list.Clear();

        Assert.That(list.Count, Is.Zero);
        Assert.That(list, Is.EquivalentTo(Array.Empty<int>()));
        Assert.That(list.ToArray(), Is.EquivalentTo(Array.Empty<int>()));
    }

    [Test]
    public void RemoveAt_RemoveReferenceValue_ArrayElementNullified()
    {
        var list = new ArrayList<string>();
        for (var i = 0; i < 4; i++)
            list.Add(i.ToString());

        ref var r2 = ref list[2];
        ref var r3 = ref list[3];

        Assert.That(list, Is.EquivalentTo(["0", "1", "2", "3"]));

        list.RemoveAt(3);

        Assert.That(list, Is.EquivalentTo(["0", "1", "2"]));
        Assert.That(r3, Is.Null);

        list.RemoveAt(0);
        Assert.That(list, Is.EquivalentTo(["1", "2"]));
        Assert.That(r2, Is.Null);
    }

    [Test]
    public void Clear_ReferenceValues_ArrayElementNullified()
    {
        var list = new ArrayList<string>();
        for (var i = 0; i < 4; i++)
            list.Add(i.ToString());

        ref var r0 = ref list[0];
        ref var r1 = ref list[1];
        ref var r2 = ref list[2];
        ref var r3 = ref list[3];

        list.Clear();

        Assert.That(list, Is.Empty);
        Assert.That(r0, Is.Null);
        Assert.That(r1, Is.Null);
        Assert.That(r2, Is.Null);
        Assert.That(r3, Is.Null);
    }

    [TestCase(0)]
    [TestCase(2)]
    [TestCase(4)]
    [TestCase(5)]
    public void AsSpan_Empty(int capacity)
    {
        var list = new ArrayList<int>(capacity);

        Assert.That(list.Count, Is.Zero);
        Assert.That(list.AsSpan().Length, Is.Zero);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(7)]
    [TestCase(9)]
    public void ToArray(int count)
    {
        var list = new ArrayList<int>(count);
        for (var i = 0; i < count; i++)
            list.Add(i);

        Assert.That(list.ToArray(), Is.EquivalentTo(Enumerable.Range(0, count)));
        Assert.That(list.Count, Is.EqualTo(count));
    }

    [Test]
    public void ToArray_Empty()
    {
        Assert.That(new ArrayList<string>().ToArray(), Is.Empty);
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(4)]
    [TestCase(7)]
    [TestCase(9)]
    public void AsSpan(int count)
    {
        var list = new ArrayList<int>(count);
        for (var i = 0; i < count; i++)
            list.Add(i);

        Assert.That(
            list.AsSpan().ToArray(),
            Is.EquivalentTo(Enumerable.Range(0, count)));
    }

    [Test]
    public void AsSpan_Empty()
    {
        var view = new ArrayList<string>().AsSpan();
        Assert.That(view.Length, Is.Zero);
    }
}
