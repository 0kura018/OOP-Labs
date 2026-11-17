using System.Globalization;
using System.Text;

namespace Lab02.DisplayShop.Logging;

public sealed class Formatter
{
    private string pattern;

    public Formatter(string pattern = "[%t] [%l] %n: %m")
    {
        ArgumentNullException.ThrowIfNull(pattern);
        this.pattern = pattern;
    }

    // Instance configuration is explicitly allowed by section 4.6 of the handout.
    public string Pattern
    {
        get => pattern;
        set { ArgumentNullException.ThrowIfNull(value); pattern = value; }
    }

    public string Format(LogLevel level, string name, string message, params object?[] args)
    {
        if (!Enum.IsDefined(level)) throw new ArgumentOutOfRangeException(nameof(level));
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(args);
        string body = FormatMessage(message, args);
        string template = Pattern;
        string time = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        string? random = null;
        var result = new StringBuilder();
        for (int i = 0; i < template.Length; i++)
        {
            if (template[i] == '%' && i + 1 < template.Length)
            {
                string? replacement = template[i + 1] switch
                {
                    't' => time,
                    'l' => level.ToString(),
                    'n' => name,
                    'm' => body,
                    'r' => random ??= RandomPhrase(level),
                    _ => null
                };
                if (replacement is not null)
                {
                    result.Append(replacement);
                    i++;
                    continue;
                }
            }
            result.Append(template[i]);
        }
        return result.ToString();
    }

    public static string FormatMessage(string message, params object?[] args)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(args);
        var result = new StringBuilder();
        int argument = 0;
        for (int i = 0; i < message.Length; i++)
        {
            if (message[i] != '%') { result.Append(message[i]); continue; }
            if (i + 1 < message.Length && message[i + 1] == '%')
            {
                result.Append('%');
                i++;
            }
            else if (argument < args.Length)
            {
                object? value = args[argument++];
                result.Append(value is null ? "<null>" : Convert.ToString(value, CultureInfo.InvariantCulture));
            }
            else
                result.Append('%'); // Missing arguments preserve the placeholder; extras are ignored.
        }
        return result.ToString();
    }

    private static string RandomPhrase(LogLevel level)
    {
        string[] phrases = level switch
        {
            LogLevel.TRACE => ["Following the next step", "Looking into the details"],
            LogLevel.INFO => ["Operation completed", "Here is your result"],
            LogLevel.WARN => ["Something needs attention", "Check this situation"],
            LogLevel.ERROR => ["Operation failed", "Input needs correction"],
            _ => throw new ArgumentOutOfRangeException(nameof(level))
        };
        return phrases[Random.Shared.Next(phrases.Length)];
    }
}
