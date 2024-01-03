using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void StringTest()
    {
        Assert.That(L("Apple").Parse("Apple").Success, Is.True);
        Assert.That(L("Apple").Parse("Apple").Value, Is.EqualTo("Apple"));
        Assert.That(L("Apple").Parse("Apples").Value, Is.EqualTo("Apple"));
        Assert.That(L("Apple").Map(m => (m.Index, m.Length)).Parse("Apple").Value, Is.EqualTo((0, 5)));
        Assert.That(L("Apple").Map(m => m.ToString()).Parse("Apple").Value, Is.EqualTo("Apple"));

        Assert.That(L("Apple", StringComparison.OrdinalIgnoreCase).Parse("apple").Value, Is.EqualTo("Apple"));

        Assert.That(L("Apple").Parse("apple").Success, Is.False);
        Assert.That(L("Apple").Parse("apple").ErrorMessage, Is.EqualTo("(1:1) Expected 'Apple'"));
        Assert.That(L("Apple").Parse("Fruit").Success, Is.False);
        Assert.That(L("Apple").Parse("").Success, Is.False);
    }

    [Test]
    public void OneOf_String_OverlappingWords()
    {
        var literals = new[]
        {
            "Sun",
            "Sunset",
            "Light",
            "Lighthouse",
            "Star",
            "Starlight",
            "Book",
            "Bookstore",
            "Home",
            "Homeland",
            "Sea",
            "Seashore",
            "1234",
            "12345",
            "Практика",
            "Практикант"
        };

        for (var count = 2; count <= literals.Length; count++)
        {
            var values = literals[..count];

            foreach (var value in values)
            {
                foreach (var comparison in Enum.GetValues<StringComparison>())
                {
                    var lower = ((int)comparison & 1) == 1
                        ? value.ToLower()
                        : value;

                    var upper = ((int)comparison & 1) == 1
                        ? value.ToUpper()
                        : value;

                    var parser = OneOf(values, comparison);

                    Assert.That(parser.Parse(value).Success, Is.True);
                    Assert.That(parser.Parse(lower).Success, Is.True);
                    Assert.That(parser.Parse(upper).Success, Is.True);
                    Assert.That(parser.Parse(value).Value, Is.EqualTo(value));
                    Assert.That(parser.Parse(lower).Value, Is.EqualTo(value));
                    Assert.That(parser.Parse(upper).Value, Is.EqualTo(value));

                    Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(value).Value, Is.EqualTo((0, value.Length)));
                    Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(lower).Value, Is.EqualTo((0, value.Length)));
                    Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(upper).Value, Is.EqualTo((0, value.Length)));
                }
            }
        }
    }

    [Test]
    public void OneOf_String()
    {
        var literals = new[]
        {
            "Sun",
            "Rain",
            "Lighthouse",
            "Starlight",
            "Bookstore",
            "Home",
            "Seashore",
            "12345",
            "Практика"
        };

        for (var count = 2; count <= literals.Length; count++)
        {
            var values = literals[..count];

            foreach (var value in values)
            {
                foreach (var comparison in Enum.GetValues<StringComparison>())
                {
                    var lower = ((int)comparison & 1) == 1
                        ? value.ToLower()
                        : value;

                    var upper = ((int)comparison & 1) == 1
                        ? value.ToLower()
                        : value;

                    var parser = OneOf(values, comparison);

                    Assert.That(parser.Parse(value).Success, Is.True);
                    Assert.That(parser.Parse(lower).Success, Is.True);
                    Assert.That(parser.Parse(upper).Success, Is.True);
                    Assert.That(parser.Parse(value).Value, Is.EqualTo(value));
                    Assert.That(parser.Parse(lower).Value, Is.EqualTo(value));
                    Assert.That(parser.Parse(upper).Value, Is.EqualTo(value));

                    Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(value).Value, Is.EqualTo((0, value.Length)));
                    Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(lower).Value, Is.EqualTo((0, value.Length)));
                    Assert.That(parser.Map(m => (m.Index, m.Length)).Parse(upper).Value, Is.EqualTo((0, value.Length)));
                }
            }
        }

        foreach (var text in new[] { "None", "Light", "123", "Практик" })
        {
            Assert.That(
                OneOf(literals).Parse(text).ErrorMessage,
                Is.EqualTo("(1:1) Expected 'Sun', 'Rain', 'Lighthouse', 'Starlight', 'Bookstore', 'Home', 'Seashore', '12345', or 'Практика'"));
        }
    }
}
