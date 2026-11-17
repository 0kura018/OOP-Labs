using Lab02.DisplayShop.Logging;

namespace Lab02.DisplayShop;

public sealed class CommandReader
{
    private readonly Logger logger;
    public CommandReader(Logger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        this.logger = logger;
    }

    public string[]? Read(string path)
    {
        try { return File.ReadAllLines(path); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            logger.Error("Cannot read command file '%': %", path, ex.Message);
            return null;
        }
    }
}
