namespace Lab03.CarStation.Logging;

public enum LogLevel { TRACE, INFO, WARN, ERROR }

public interface ISink
{
    void Write(string line);
}

public interface IFormatter
{
    string Format(DateTimeOffset time, LogLevel level, string name, string message);
}

public sealed class Logger(string name, LogLevel threshold, IFormatter formatter, ISink sink)
{
    public void Log(LogLevel level, string message)
    {
        if (level >= threshold)
            sink.Write(formatter.Format(DateTimeOffset.Now, level, name, message));
    }

    public void Trace(string message) => Log(LogLevel.TRACE, message);
    public void Info(string message) => Log(LogLevel.INFO, message);
    public void Warn(string message) => Log(LogLevel.WARN, message);
    public void Error(string message) => Log(LogLevel.ERROR, message);
}
