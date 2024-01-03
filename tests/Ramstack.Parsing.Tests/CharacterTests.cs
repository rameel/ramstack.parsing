namespace Ramstack.Parsing;

using static Character;

[TestFixture]
public class CharacterTests
{
    [Test]
    public void DigitTest()
    {
        for (var c = '0'; c <= '9'; c++)
        {
            var s = c.ToString();
            Assert.That(Digit.Parse(s).Success, Is.True);
            Assert.That(Digit.Parse(s).Value, Is.EqualTo(c));
            Assert.That(Digit.Map(m => (m.Index, m.Length)).Parse(s).Value, Is.EqualTo((0, 1)));
        }

        Assert.That(Digit.Parse("a").Success, Is.False);
        Assert.That(Digit.Parse("*").Success, Is.False);
    }

    [Test]
    public void SpaceTest()
    {
        Assert.That(WhiteSpace.Parse(" ").Success, Is.True);
        Assert.That(WhiteSpace.Map(m => (m.Index, m.Length)).Parse(" ").Value, Is.EqualTo((0, 1)));
    }
}
