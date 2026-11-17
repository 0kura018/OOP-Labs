using Lab02.DisplayShop.Logging;
using Lab02.DisplayShop;
using System.Globalization;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Lab02.DisplayShop.Tests;

public sealed class Lab02Tests : IDisposable
{
    private readonly TextWriter previousOutput = Console.Out;
    private readonly LogLevel previousLevel = Logger.Level;
    private readonly StringWriter output = new();

    public Lab02Tests() { Console.SetOut(output); Logger.Level = LogLevel.TRACE; }
    private string Text => output.ToString();
    private static Shop NewShop() => new(new Formatter("[%l] %n: %m"), new ConsoleSink());

    [Fact]
    public void FormatterIsPureAndDoesNotReinterpretInsertedValues()
    {
        var formatter = new Formatter("%l %n %m %m %x %");
        string line = formatter.Format(LogLevel.INFO, "Name %m", "Ratio is 100%n");
        Assert.Equal("INFO Name %m Ratio is 100%n Ratio is 100%n %x %", line);
        Assert.Equal("", Text);
    }

    [Fact]
    public void FormattersHaveIndependentPatterns()
    {
        var first = new Formatter("%n: %m");
        var second = new Formatter("[%l] %m");
        Assert.Equal("A: hello", first.Format(LogLevel.INFO, "A", "hello"));
        Assert.Equal("[INFO] hello", second.Format(LogLevel.INFO, "A", "hello"));
    }

    [Fact]
    public void TimeTokenHasExpectedShape()
    {
        Assert.Matches(@"^\[\d{2}:\d{2}:\d{2}\] hello$",
            new Formatter("[%t] %m").Format(LogLevel.INFO, "A", "hello"));
    }

    [Theory]
    [InlineData(LogLevel.TRACE, "Following the next step", "Looking into the details")]
    [InlineData(LogLevel.INFO, "Operation completed", "Here is your result")]
    [InlineData(LogLevel.WARN, "Something needs attention", "Check this situation")]
    [InlineData(LogLevel.ERROR, "Operation failed", "Input needs correction")]
    public void RandomPhraseMatchesLevelAndIsReused(LogLevel level, string a, string b)
    {
        string[] parts = new Formatter("%r|%r").Format(level, "A", "hello").Split('|');
        Assert.Contains(parts[0], new[] { a, b });
        Assert.Equal(parts[0], parts[1]);
    }

    [Fact]
    public void VarargsEscapePercentAndHandleMismatchesWithoutRecursiveSubstitution()
    {
        Assert.Equal("Pair 3 and 7", Formatter.FormatMessage("Pair % and %", 3, 7));
        Assert.Equal("100% done", Formatter.FormatMessage("100%% done"));
        Assert.Equal("3 and %", Formatter.FormatMessage("% and %", 3));
        Assert.Equal("3", Formatter.FormatMessage("%", 3, 7));
        Assert.Equal("%n", Formatter.FormatMessage("%", "%n"));
        Assert.Equal("<null>", Formatter.FormatMessage("%", new object?[] { null }));
    }

    [Fact]
    public void InjectedPartsAreSharedWhileOtherFormattersRemainIndependent()
    {
        var formatter = new Formatter("%m");
        var sink = new ConsoleSink();
        var a = new Logger("A", formatter, sink);
        var b = new Logger("B", formatter, sink);
        var c = new Logger("C", new Formatter("SPECIAL %m"), sink);
        formatter.Pattern = "%n: %m";
        a.Info("one"); b.Info("two"); c.Info("three");
        Assert.Equal(new[] { "A: one", "B: two", "SPECIAL three" }, Lines());
    }

    [Fact]
    public void CompositionConstructorWorks()
    {
        new Logger("Composed").Info("hello");
        Assert.Contains("[INFO] Composed: hello", Text);
    }

    [Theory]
    [InlineData(LogLevel.TRACE)]
    [InlineData(LogLevel.INFO)]
    [InlineData(LogLevel.WARN)]
    [InlineData(LogLevel.ERROR)]
    public void LevelIsSharedAndFiltersEveryEntry(LogLevel threshold)
    {
        var a = new Logger("A", new Formatter("%n %l"), new ConsoleSink());
        var b = new Logger("B", new Formatter("%n %l"), new ConsoleSink());
        Logger.Level = threshold;
        foreach (LogLevel level in Enum.GetValues<LogLevel>()) { a.Log(level, ""); b.Log(level, ""); }
        Assert.Equal(Enum.GetValues<LogLevel>().Where(l => l >= threshold)
            .SelectMany(l => new[] { $"A {l}", $"B {l}" }), Lines());
    }

    private sealed class ThrowsOnToString
    {
        public override string ToString() => throw new InvalidOperationException("Must not format a hidden entry.");
    }

    [Fact]
    public void HiddenMessageDoesNotFormatArguments()
    {
        Logger.Level = LogLevel.ERROR;
        new Logger("A").Info("%", new ThrowsOnToString());
        Assert.Empty(Text);
    }

    [Theory]
    [InlineData("()", true)]
    [InlineData(")((()))", false)]
    [InlineData("(", false)]
    [InlineData("(())((()())())", true)]
    [InlineData("", true)]
    [InlineData(")(", false)]
    [InlineData("(a)", false)]
    [InlineData(null, false)]
    public void ParenthesesStillWorkAtAllLevels(string? input, bool expected)
    {
        var validator = new ParenthesesValidator();
        foreach (var level in Enum.GetValues<LogLevel>())
        { Logger.Level = level; Assert.Equal(expected, validator.IsValid(input)); }
    }

    [Fact]
    public void PhysicalSizeIsBasedOnPpiAndComparisonReusesBothMethods()
    {
        var small = new Display("Small", 1920, 1080, 200);
        var big = new Display("Big", 1920, 1080, 100);
        Assert.Equal(Math.Sqrt(1920d * 1920 + 1080d * 1080) / 100, big.DiagonalInches, 8);
        Assert.True(big.CompareSize(small) > 0);
        Assert.True(big.CompareSharpness(small) < 0);
        Assert.Contains("trade-off", big.CompareWithMonitor(small));
        Assert.Contains("bigger than", Text);
        Assert.Contains("less sharp than", Text);
    }

    [Theory]
    [InlineData(0, 1080, 100f)]
    [InlineData(1920, -1, 100f)]
    [InlineData(1920, 1080, 0f)]
    [InlineData(1920, 1080, float.NaN)]
    [InlineData(1920, 1080, float.PositiveInfinity)]
    public void InvalidDisplayDataIsRejected(int width, int height, float ppi)
        => Assert.Throws<ArgumentOutOfRangeException>(() => new Display("Bad", width, height, ppi));

    [Fact]
    public void SharedDisplaySurvivesPurchaseFromAnotherManager()
    {
        var display = new Display("Shared", 1920, 1080, 100);
        var bob = new DisplayManager("Bob");
        var alice = new DisplayManager("Alice");
        Assert.True(bob.AssignDisplay(display));
        Assert.False(bob.AssignDisplay(display));
        alice.AssignDisplay(display);
        Assert.Same(display, bob.BuyDisplay(display));
        Assert.Empty(bob.AssignedDisplays);
        Assert.Same(display, Assert.Single(alice.AssignedDisplays));
        Assert.Throws<InvalidOperationException>(() => bob.BuyDisplay(display));
    }

    [Fact]
    public void AssistComparesAdjacentPairsExactlyOnce()
    {
        var manager = new DisplayManager("A");
        for (int i = 0; i < 3; i++) manager.AssignDisplay(new Display($"D{i}", 1920, 1080, 100));
        manager.Assist();
        Assert.Equal(2, Lines().Count(l => l.Contains("Comparison ")));
    }

    [Theory]
    [InlineData("shop-monday.txt", 0)]
    [InlineData("shop-tuesday.txt", 9)]
    [InlineData("shop-edge-cases.txt", 6)]
    public void CommandFilesRunAtEveryLevelWithExpectedErrors(string file, int errors)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "commands", file);
        foreach (var level in new[] { LogLevel.TRACE, LogLevel.INFO, LogLevel.ERROR })
        {
            output.GetStringBuilder().Clear();
            Assert.Equal(0, Application.Run([path, level.ToString()]));
            Assert.Equal(errors, Lines().Count(l => l.StartsWith("[ERROR]")));
            if (level != LogLevel.TRACE) Assert.DoesNotContain("[TRACE]", Text);
            if (level == LogLevel.ERROR)
                Assert.All(Lines(), l => Assert.StartsWith("[ERROR]", l));
            if (file == "shop-tuesday.txt" && level == LogLevel.TRACE)
            {
                foreach (int n in new[] { 4, 5, 7, 8, 11, 12, 13, 15, 17 })
                    Assert.Contains($"Line {n}:", Text);
                Assert.Contains("[WARN]", Text);
            }
        }
    }

    [Fact]
    public void ParserContinuesAfterErrorsAndUsesInvariantNumbers()
    {
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            var shop = NewShop();
            shop.ParseCommand("DISPLAY Bad 1920 1080 NaN", 1);
            shop.ParseCommand("MANAGER Bob extra", 2);
            shop.ParseCommand("\tMANAGER\tBob  ", 3);
            shop.ParseCommand("DISPLAY Good 1920 1080 108.8", 4);
            shop.ParseCommand("ASSIGN Good Bob", 5);
            Assert.Single(shop.Managers);
            Assert.Equal(108.8f, Assert.Single(shop.Displays).Ppi);
            Assert.Single(shop.Managers[0].AssignedDisplays);
            Assert.Contains("Line 1:", Text); Assert.Contains("Line 2:", Text);
        }
        finally { CultureInfo.CurrentCulture = previousCulture; }
    }

    [Fact]
    public void MissingFileAndInvalidCliAreHandled()
    {
        Assert.Equal(1, Application.Run([Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt")]));
        Assert.Contains("Cannot read command file", Text);
        Assert.Equal(1, Application.Run(["ignored.txt", "WRONG"]));
        Assert.Contains("Unknown log level", Text);
    }

    [Fact]
    public void DuplicatesAndCaseSensitivityAreConsistent()
    {
        var shop = NewShop();
        shop.ParseCommand("MANAGER Bob", 1);
        shop.ParseCommand("MANAGER Bob", 2);
        shop.ParseCommand("assist Bob", 3);
        shop.ParseCommand("ASSIST bob", 4);
        shop.ParseCommand("ASSIST Bob", 5);
        Assert.Single(shop.Managers);
        Assert.Contains("[WARN] Parser: Line 2:", Text);
        Assert.Contains("[ERROR] Parser: Line 3:", Text);
        Assert.Contains("[ERROR] Parser: Line 4:", Text);
    }

    private string[] Lines() => Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
    public void Dispose()
    {
        Console.SetOut(previousOutput);
        Logger.Level = previousLevel;
        output.Dispose();
    }
}
