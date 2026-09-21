using static Ramstack.Parsing.Parser;

namespace Ramstack.Parsing;

partial class ParsersTests
{
    [Test]
    public void Void_RecursiveDeferredParser_DoesNotRecurseInfinitely()
    {
        // value = 'x' | '(' value ')'

        var value = Deferred<char>();
        value.Parser = Choice(L('x'), value.Between(L('('), L(')')));

        var parser = value.Void();
        var result = parser.Parse("((x))tail");

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Length, Is.EqualTo(5));

        Assert.That(parser.Parse("(y)").Success, Is.False);
    }

    [Test]
    public void As_RecursiveDeferredParser_ReportsName()
    {
        // value = 'x' | '(' value ')'

        var value = Deferred<char>();
        value.Parser = Choice(L('x'), value.Between(L('('), L(')')));

        var parser = value.As("value");
        var result = parser.Parse("((x))");

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Value, Is.EqualTo('x'));
        Assert.That(result.Length, Is.EqualTo(5));

        Assert.That(parser.Parse("?").ErrorMessage, Is.EqualTo("(1:1) Expected value"));
    }

    [Test]
    public void Void_CyclicDeferredParsers_DoesNotRecurseInfinitely()
    {
        // a = 'x' | b;
        // b = '(' c ')';
        // c = '[' a ']'

        var a = Deferred<char>();
        var b = Deferred<char>();
        var c = Deferred<char>();

        a.Parser = Choice(L('x'), b);
        b.Parser = c.Between(L('('), L(')'));
        c.Parser = a.Between(L('['), L(']'));

        var parser = a.Void();
        var result = parser.Parse("([x])tail");

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Length, Is.EqualTo(5));

        Assert.That(parser.Parse("([y])").Success, Is.False);
    }

    [Test]
    public void Recursive_SelfReferencingParser_DoesNotRecurseInfinitely()
    {
        // value = 'x' | '(' value ')'

        var value = Recursive<char>(self => Choice(L('x'), self.Between(L('('), L(')'))));

        var void1Parser = value.Void();
        var namedParser = value.As("value");
        var void2Parser = value.Void();

        Assert.That(void1Parser.Parse("((x))").Success, Is.True);
        Assert.That(void2Parser.Parse("((x))").Success, Is.True);
        Assert.That(namedParser.Parse("((x))").Value, Is.EqualTo('x'));
    }

    [Test]
    public void Recursive_ParserFactoryReturnsNull_ThrowsInvalidOperationException()
    {
        Assert.That(
            () => Recursive<char>(_ => null!),
            Throws
                .TypeOf<InvalidOperationException>()
                .With.Message.EqualTo("The parser factory returned null."));
    }

    [Test]
    public void As_UninitializedDeferredParser_UsesLaterDefinition()
    {
        var parser = Deferred<char>();
        var namedParser = parser.As("value");

        // value = 'x'
        parser.Parser = L('x');

        var result = namedParser.Parse("x");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo('x'));
        Assert.That(result.Length, Is.EqualTo(1));

        Assert.That(namedParser.Parse("y").ErrorMessage, Is.EqualTo("(1:1) Expected value"));
    }

    [Test]
    public void As_MutuallyRecursiveDeferredParsers_PreservesParsing()
    {
        // a = 'x' | '(' b ')';
        // b = '[' a ']'

        var a = Deferred<char>();
        var b = Deferred<char>();
        var namedA = a.As("A");
        var namedB = b.As("B");

        a.Parser = Choice(L('x'), b.Between(L('('), L(')')));
        b.Parser = a.Between(L('['), L(']'));

        var resultA = namedA.Parse("([([x])])");
        var resultB = namedB.Parse("[([x])]");

        Assert.That(resultA.Success, Is.True);
        Assert.That(resultA.Value, Is.EqualTo('x'));
        Assert.That(resultA.Length, Is.EqualTo(9));

        Assert.That(resultB.Success, Is.True);
        Assert.That(resultB.Value, Is.EqualTo('x'));
        Assert.That(resultB.Length, Is.EqualTo(7));

        Assert.That(namedA.Parse("?").ErrorMessage, Is.EqualTo("(1:1) Expected A"));
        Assert.That(namedB.Parse("?").ErrorMessage, Is.EqualTo("(1:1) Expected B"));
    }

    [Test]
    public void Void_MutuallyRecursiveDeferredParsers_PreservesParsing()
    {
        // a = 'x' | '(' b ')';
        // b = '[' a ']'

        var a = Deferred<char>();
        var b = Deferred<char>();

        a.Parser = Choice(L('x'), b.Between(L('('), L(')')));
        b.Parser = a.Between(L('['), L(']'));

        var voidA = a.Void();
        var voidB = b.Void();

        var resultA = voidA.Parse("([([x])])");
        var resultB = voidB.Parse("[([x])]");

        Assert.That(resultA.Success, Is.True);
        Assert.That(resultA.Length, Is.EqualTo(9));

        Assert.That(resultB.Success, Is.True);
        Assert.That(resultB.Length, Is.EqualTo(7));
    }

    [Test]
    public void As_RecursiveDeferredParser_KeepsNamesIndependent()
    {
        // value = '(' ('x' | value) ')'

        var value = Deferred<char>();
        value.Parser = Choice(L('x'), value).Between(L('('), L(')'));

        var lower = value.As("value");
        var upper = value.As("VALUE");

        Assert.That(value.Name, Is.Null);

        var lowerResult = lower.Parse("((x))");
        var upperResult = upper.Parse("((x))");

        Assert.That(lowerResult.Success, Is.True);
        Assert.That(lowerResult.Value, Is.EqualTo('x'));
        Assert.That(lowerResult.Length, Is.EqualTo(5));

        Assert.That(upperResult.Success, Is.True);
        Assert.That(upperResult.Value, Is.EqualTo('x'));
        Assert.That(upperResult.Length, Is.EqualTo(5));

        Assert.That(lower.Parse("?").ErrorMessage, Is.EqualTo("(1:1) Expected value"));
        Assert.That(upper.Parse("?").ErrorMessage, Is.EqualTo("(1:1) Expected VALUE"));
    }

    [Test]
    public void DeferredParser_ReassignParser_ThrowsException()
    {
        var value = Deferred<char>();

        // value = 'x'
        value.Parser = L('x');

        Assert.That(
            () => value.Parser = L('y'),
            Throws
                .TypeOf<InvalidOperationException>()
                .With.Message.EqualTo("The deferred parser has already been initialized."));

        Assert.That(value.Parse("x").Success, Is.True);
        Assert.That(value.Parse("y").Success, Is.False);
    }

    [Test]
    public void DeferredParser_AssignedNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => Deferred<char>().Parser = null!);
    }

    [Test]
    public void Void_UninitializedDeferredParser_ThrowsInvalidOperationException()
    {
        var value = Deferred<char>();

        Assert.That(
            value.Void,
            Throws
                .TypeOf<InvalidOperationException>()
                .With.Message.EqualTo("The deferred parser has not been initialized."));

        value.Parser = L('x');

        Assert.That(value.Void().Parse("x").Success, Is.True);
    }

    [Test]
    public void Void_NamedRecursiveDeferredParser_PreservesName()
    {
        // value = '(' ('x' | value) ')'

        var value = Deferred<char>();
        value.Parser = Choice(L('x'), value).Between(L('('), L(')'));

        var lower = value.As("value").Void();
        var upper = value.As("VALUE").Void();

        var lowerResult = lower.Parse("((x))");
        var upperResult = upper.Parse("((x))");

        Assert.That(lowerResult.Success, Is.True);
        Assert.That(lowerResult.Length, Is.EqualTo(5));

        Assert.That(upperResult.Success, Is.True);
        Assert.That(upperResult.Length, Is.EqualTo(5));

        Assert.That(lower.Parse("?").ErrorMessage, Is.EqualTo("(1:1) Expected value"));
        Assert.That(upper.Parse("?").ErrorMessage, Is.EqualTo("(1:1) Expected VALUE"));
    }

    [TestCase("x", true, 1)]
    [TestCase("(x)", true, 3)]
    [TestCase("((x))", true, 5)]
    [TestCase("((x))tail", true, 5)]
    [TestCase("", false, 0)]
    [TestCase("(x", false, 0)]
    [TestCase("(y)", false, 0)]
    public void Void_RecursiveDeferredParser_PreservesParsing(string source, bool expectedSuccess, int expectedPosition)
    {
        // value = 'x' | '(' value ')'

        // var value = Recursive<char>(v => Choice(L('x'), v.Between(L('('), L(')'))));
        var value = Deferred<char>();
        value.Parser = Choice(L('x'), value.Between(L('('), L(')')));

        var parser = value.Void();
        var context = new ParseContext(source);

        var success = parser.TryParse(ref context, out _);

        Assert.That(success, Is.EqualTo(expectedSuccess));
        Assert.That(context.Position, Is.EqualTo(expectedPosition));
    }

    [Test]
    public void Void_RecursiveDeferredParser_SkipsValueFactories()
    {
        var doCalls = 0;
        var mapCalls = 0;

        // value = 'x' | '(' value ')'

        var value = Deferred<char>();
        value.Parser = Choice(L('x'), value.Between(L('('), L(')')))
            .Do(c =>
            {
                doCalls++;
                return c;
            })
            .Map((_, c) =>
            {
                mapCalls++;
                return c;
            });

        var parser = value.Void();
        var context = new ParseContext("((x))tail");

        var success = parser.TryParse(ref context, out _);

        Assert.That(success, Is.True);
        Assert.That(context.Position, Is.EqualTo(5));
        Assert.That(doCalls, Is.Zero);
        Assert.That(mapCalls, Is.Zero);

        var result = value.Parse("((x))tail");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Value, Is.EqualTo('x'));
        Assert.That(result.Length, Is.EqualTo(5));
        Assert.That(doCalls, Is.EqualTo(3));
        Assert.That(mapCalls, Is.EqualTo(3));
    }

    [Test]
    public void Void_ConversionThrows_AllowsRetry()
    {
        var inner = new ThrowOnceOnConversionParser(L('x'));

        var value = Deferred<char>();
        value.Parser = inner;

        Assert.That(
            value.Void,
            Throws
                .TypeOf<InvalidOperationException>()
                .With.Message.EqualTo("Conversion failed."));

        var parser = value.Void();
        var result = parser.Parse("x");

        Assert.That(result.Success, Is.True);
        Assert.That(result.Length, Is.EqualTo(1));

        Assert.That(parser.Parse("y").Success, Is.False);
    }

    [Test]
    public void Void_PartialRecursiveConversionThrows_AllowsRetry()
    {
        var inner = new ThrowOnceOnConversionParser(L('y'));

        // a = 'x' | b | 'y';
        // b = '(' a ')'

        var a = Deferred<char>();
        var b = Deferred<char>();

        // Convert second before inner throws,
        // so second already references first's placeholder.
        a.Parser = Choice(L('x'), b, inner);
        b.Parser = a.Between(L('('), L(')'));

        Assert.That(
            a.Void,
            Throws
                .TypeOf<InvalidOperationException>()
                .With.Message.EqualTo("Conversion failed."));

        var parser = a.Void();
        var result = parser.Parse("((x))tail");

        Assert.That(result.Success, Is.True, result.ErrorMessage);
        Assert.That(result.Length, Is.EqualTo(5));

        Assert.That(parser.Parse("(z)").Success, Is.False);
    }

    #region Inner type: ThrowOnceOnConversionParser

    private sealed class ThrowOnceOnConversionParser(Parser<char> parser) : Parser<char>
    {
        private int _attempts;

        public override bool TryParse(ref ParseContext context, out char value) =>
            parser.TryParse(ref context, out value);

        #if TEST_INTERNALS
        protected internal override Parser<Unit> ToVoidParser()
        #else
        protected override Parser<Unit> ToVoidParser()
        #endif
        {
            if (++_attempts == 1)
                throw new InvalidOperationException("Conversion failed.");

            return parser.Void();
        }
    }

    #endregion
}
