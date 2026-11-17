using System.Globalization;
using Lab02.DisplayShop.Logging;

namespace Lab02.DisplayShop;

public sealed class Shop
{
    private readonly List<Display> displays = [];
    private readonly List<DisplayManager> managers = [];
    private readonly Formatter formatter;
    private readonly ConsoleSink sink;
    private readonly Logger logger;
    public IReadOnlyList<Display> Displays => displays.AsReadOnly();
    public IReadOnlyList<DisplayManager> Managers => managers.AsReadOnly();

    public Shop(Formatter formatter, ConsoleSink sink)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        ArgumentNullException.ThrowIfNull(sink);
        this.formatter = formatter;
        this.sink = sink;
        logger = new Logger("Parser", formatter, sink);
    }

    public void ParseCommand(string line, int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(line);
        string trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.StartsWith('#')) return;
        string[] words = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        try
        {
            switch (words[0])
            {
                case "MANAGER":
                    RequireWords(words, 2);
                    foreach (var existing in managers)
                        if (existing.Name == words[1])
                        {
                            logger.Warn("Line %: manager % already exists; creation ignored.", lineNumber, words[1]);
                            return;
                        }
                    managers.Add(new DisplayManager(words[1], new Logger("Manager", formatter, sink)));
                    break;
                case "DISPLAY":
                    RequireWords(words, 5);
                    foreach (var existing in displays)
                        if (existing.Model == words[1])
                        {
                            logger.Warn("Line %: display % already exists; creation ignored.", lineNumber, words[1]);
                            return;
                        }
                    if (!int.TryParse(words[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int width) ||
                        !int.TryParse(words[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int height) ||
                        !float.TryParse(words[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float ppi))
                        throw new ArgumentException("DISPLAY requires integer width/height and numeric PPI (decimal dot).");
                    displays.Add(new Display(words[1], width, height, ppi, new Logger("Display", formatter, sink)));
                    break;
                case "ASSIGN":
                    RequireWords(words, 3);
                    Display assigned = FindDisplay(words[1]);
                    DisplayManager manager = FindManager(words[2]);
                    if (!manager.AssignDisplay(assigned))
                        logger.Warn("Line %: duplicate ASSIGN ignored.", lineNumber);
                    break;
                case "COMPARE":
                    RequireWords(words, 3);
                    FindDisplay(words[1]).CompareWithMonitor(FindDisplay(words[2]));
                    break;
                case "ASSIST":
                    RequireWords(words, 2);
                    FindManager(words[1]).Assist();
                    break;
                case "BUY":
                    RequireWords(words, 3);
                    FindManager(words[2]).BuyDisplay(FindDisplay(words[1]));
                    break;
                default:
                    throw new ArgumentException($"Unknown command '{words[0]}'. Commands and names are case-sensitive.");
            }
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            logger.Error("Line %: %", lineNumber, ex.Message);
        }
    }

    private static void RequireWords(string[] words, int count)
    {
        if (words.Length != count)
            throw new ArgumentException($"{words[0]} expects {count - 1} argument(s), received {words.Length - 1}.");
    }

    private Display FindDisplay(string model)
    {
        foreach (var display in displays) if (display.Model == model) return display;
        throw new InvalidOperationException($"Display '{model}' does not exist.");
    }

    private DisplayManager FindManager(string name)
    {
        foreach (var manager in managers) if (manager.Name == name) return manager;
        throw new InvalidOperationException($"Manager '{name}' does not exist.");
    }
}
