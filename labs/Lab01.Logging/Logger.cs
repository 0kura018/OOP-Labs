using System.Text;

namespace Lab01.Logging;

public sealed class Logger
{
    private readonly string name;
    private static LogLevel level = LogLevel.INFO;
    private static string format = "[%t] [%l] %n: %m";

    public Logger(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        this.name = name;
    }

    public static LogLevel Level
    {
        get => level;
        set
        {
            if (!Enum.IsDefined(value))
                throw new ArgumentOutOfRangeException(nameof(value));
            level = value;
        }
    }

    public static string Format
    {
        get => format;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            format = value;
        }
    }

    public static void SetLevel(LogLevel newLevel) => Level = newLevel;
    public static void SetFormat(string newFormat) => Format = newFormat;

    public void Log(LogLevel messageLevel, string message)
    {
        if (!Enum.IsDefined(messageLevel))
            throw new ArgumentOutOfRangeException(nameof(messageLevel));
        ArgumentNullException.ThrowIfNull(message);
        if (messageLevel < Level)
            return;

        // Scan only the template; substituted values are never formatted again.
        string template = Format;
        string time = DateTime.Now.ToString("HH:mm:ss");
        string? randomPhrase = null;
        var output = new StringBuilder();
        for (int i = 0; i < template.Length; i++)
        {
            if (template[i] == '%' && i + 1 < template.Length)
            {
                string? replacement = template[i + 1] switch
                {
                    't' => time,
                    'm' => message,
                    'n' => name,
                    'l' => messageLevel.ToString(),
                    'r' => randomPhrase ??= GetRandomPhrase(messageLevel),
                    _ => null
                };
                if (replacement is not null)
                {
                    output.Append(replacement);
                    i++;
                    continue;
                }
            }
            output.Append(template[i]);
        }
        Console.WriteLine(output.ToString());
    }

    public void Trace(string message) => Log(LogLevel.TRACE, message);
    public void Info(string message) => Log(LogLevel.INFO, message);
    public void Warn(string message) => Log(LogLevel.WARN, message);
    public void Error(string message) => Log(LogLevel.ERROR, message);

    private static string GetRandomPhrase(LogLevel messageLevel)
    {
        string[] phrases = messageLevel switch
        {
            LogLevel.TRACE => ["Following the next step", "Looking into the details"],
            LogLevel.INFO => ["Operation completed", "Here is your result"],
            LogLevel.WARN => ["Something needs attention", "Check this situation"],
            LogLevel.ERROR => ["Operation failed", "Input needs correction"],
            _ => throw new ArgumentOutOfRangeException(nameof(messageLevel))
        };
        return phrases[Random.Shared.Next(phrases.Length)];
    }
}
