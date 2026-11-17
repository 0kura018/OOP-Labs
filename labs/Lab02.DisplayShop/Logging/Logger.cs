namespace Lab02.DisplayShop.Logging;

public sealed class Logger
{
    private readonly string name;
    private readonly Formatter formatter;
    private readonly ConsoleSink sink;
    private static LogLevel level = LogLevel.INFO;

    // Composition: construct private parts owned by this logger.
    public Logger(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        this.name = name;
        formatter = new Formatter();
        sink = new ConsoleSink();
    }

    // Aggregation: use parts that the caller may also give to other loggers.
    public Logger(string name, Formatter formatter, ConsoleSink sink)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(sink);
        this.name = name;
        this.formatter = formatter;
        this.sink = sink;
    }

    public static LogLevel Level
    {
        get => level;
        set
        {
            if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
            level = value;
        }
    }

    public static void SetLevel(LogLevel newLevel) => Level = newLevel;

    public void Log(LogLevel messageLevel, string message, params object?[] args)
    {
        if (!Enum.IsDefined(messageLevel)) throw new ArgumentOutOfRangeException(nameof(messageLevel));
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(args);
        if (messageLevel < Level) return;
        sink.Write(formatter.Format(messageLevel, name, message, args));
    }

    public void Trace(string message, params object?[] args) => Log(LogLevel.TRACE, message, args);
    public void Info(string message, params object?[] args) => Log(LogLevel.INFO, message, args);
    public void Warn(string message, params object?[] args) => Log(LogLevel.WARN, message, args);
    public void Error(string message, params object?[] args) => Log(LogLevel.ERROR, message, args);
}
