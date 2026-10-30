using Lab01.Logging;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Lab01.Logging.Tests;

public sealed class LoggingTests : IDisposable
{
    private readonly TextWriter originalOutput = Console.Out;
    private readonly LogLevel originalLevel = Logger.Level;
    private readonly string originalFormat = Logger.Format;
    private readonly StringWriter output = new();

    public LoggingTests()
    {
        Console.SetOut(output);
        Logger.Level = LogLevel.TRACE;
        Logger.Format = "[%l] %n: %m";
    }

    [Theory]
    [InlineData("()", true)]
    [InlineData(")((()))", false)]
    [InlineData("(", false)]
    [InlineData("(())((()())())", true)]
    [InlineData("", true)]
    [InlineData(")(", false)]
    [InlineData("(()", false)]
    [InlineData("())", false)]
    [InlineData("(a)", false)]
    [InlineData(null, false)]
    public void ValidatorReturnsExpectedResultAtEveryLevel(string? input, bool expected)
    {
        var validator = new ParenthesesValidator();
        foreach (LogLevel level in Enum.GetValues<LogLevel>())
        {
            Logger.Level = level;
            Assert.Equal(expected, validator.IsValid(input));
        }
    }

    [Theory]
    [InlineData(LogLevel.TRACE)]
    [InlineData(LogLevel.INFO)]
    [InlineData(LogLevel.WARN)]
    [InlineData(LogLevel.ERROR)]
    public void ThresholdAppliesToExistingLoggerInstances(LogLevel threshold)
    {
        var first = new Logger("First");
        var second = new Logger("Second");
        Logger.Level = threshold;
        Logger.Format = "%n|%l|%m";
        foreach (LogLevel level in Enum.GetValues<LogLevel>())
        {
            first.Log(level, "hello");
            second.Log(level, "hello");
        }
        var expected = Enum.GetValues<LogLevel>()
            .Where(level => level >= threshold)
            .SelectMany(level => new[] { $"First|{level}|hello", $"Second|{level}|hello" });
        Assert.Equal(expected, Lines());
    }

    [Fact]
    public void FormatPreservesUnknownTokensAndDoesNotReformatValues()
    {
        Logger.Format = "%l %n %m %m %x %";
        new Logger("Name %m").Info("Message %l %n %r");
        Assert.Equal("INFO Name %m Message %l %n %r Message %l %n %r %x %", Assert.Single(Lines()));
    }

    [Fact]
    public void TimeAndPartialFormatsAreSupported()
    {
        Logger.Format = "[%t] %m";
        new Logger("Test").Info("hello");
        Assert.Matches(@"^\[\d{2}:\d{2}:\d{2}\] hello$", Assert.Single(Lines()));
    }

    [Fact]
    public void ValidatorLogsOpeningsPairsAndBothFinalResults()
    {
        var validator = new ParenthesesValidator();
        validator.IsValid("()");
        validator.IsValid("(");
        Assert.Contains("[TRACE] Parentheses: Opening parenthesis at index 0.", Lines());
        Assert.Contains("[INFO] Parentheses: Matching pair found at closing index 1.", Lines());
        Assert.Contains("[INFO] Parentheses: Validation result for \"()\": true.", Lines());
        Assert.Contains("[INFO] Parentheses: Validation result for \"(\": false.", Lines());
        Assert.Contains(Lines(), line => line.StartsWith("[ERROR] Parentheses: Invalid input \"(\":") && line.Contains("unmatched"));
    }

    [Theory]
    [InlineData(LogLevel.TRACE, "Following the next step", "Looking into the details")]
    [InlineData(LogLevel.INFO, "Operation completed", "Here is your result")]
    [InlineData(LogLevel.WARN, "Something needs attention", "Check this situation")]
    [InlineData(LogLevel.ERROR, "Operation failed", "Input needs correction")]
    public void RandomPhraseBelongsToTheEntryLevel(LogLevel level, string first, string second)
    {
        Logger.Format = "%r|%r";
        var logger = new Logger("Test");
        for (int i = 0; i < 20; i++)
            logger.Log(level, "hello");
        foreach (string line in Lines())
        {
            string[] phrases = line.Split('|');
            Assert.Contains(phrases[0], new[] { first, second });
            Assert.Equal(phrases[0], phrases[1]);
        }
    }

    private string[] Lines() => output.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

    public void Dispose()
    {
        Console.SetOut(originalOutput);
        Logger.Level = originalLevel;
        Logger.Format = originalFormat;
        output.Dispose();
    }
}
