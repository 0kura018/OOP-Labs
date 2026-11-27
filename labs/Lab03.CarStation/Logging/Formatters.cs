using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Lab03.CarStation.Logging;

public sealed class PatternFormatter(string pattern) : IFormatter
{
    private static readonly Regex Tokens = new("%[tlnm]");

    public string Format(DateTimeOffset time, LogLevel level, string name, string message) =>
        Tokens.Replace(pattern, match => match.Value[1] switch
        {
            't' => time.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            'l' => level.ToString(),
            'n' => name,
            _ => message
        });
}

public sealed class JsonFormatter : IFormatter
{
    public string Format(DateTimeOffset time, LogLevel level, string name, string message) =>
        JsonSerializer.Serialize(new
        {
            time = time.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            level = level.ToString(),
            logger = name,
            message
        });
}
