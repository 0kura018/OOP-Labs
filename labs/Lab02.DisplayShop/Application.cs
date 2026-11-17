using Lab02.DisplayShop.Logging;

namespace Lab02.DisplayShop;

public static class Application
{
    public static int Run(string[] args)
    {
        var formatter = new Formatter("[%l] %n: %m");
        var sink = new ConsoleSink();
        var logger = new Logger("Application", formatter, sink);
        try
        {
            if (args.Length > 0 && args[0] == "--help")
            {
                sink.Write("Usage: Lab02.DisplayShop [command-file] [TRACE|INFO|WARN|ERROR]");
                sink.Write("       Lab02.DisplayShop --demo [level]");
                sink.Write("       Lab02.DisplayShop --parentheses [strings...]");
                return 0;
            }
            if (args.Length > 0 && args[0] == "--parentheses")
            {
                Logger.Level = LogLevel.TRACE;
                var validator = new ParenthesesValidator(new Logger("Parentheses", formatter, sink));
                string[] inputs = args.Length > 1 ? args[1..] : ["()", ")((()))", "(", "(())((()())())"];
                foreach (string input in inputs) validator.IsValid(input);
                return 0;
            }
            if (args.Length > 2) throw new ArgumentException("Expected at most a file/mode and a log level. Use --help.");
            Logger.Level = LogLevel.INFO;
            if (args.Length == 2)
            {
                if (!Enum.TryParse(args[1], true, out LogLevel level) || !Enum.IsDefined(level) || int.TryParse(args[1], out _))
                    throw new ArgumentException($"Unknown log level '{args[1]}'. Use TRACE, INFO, WARN or ERROR.");
                Logger.Level = level;
            }
            if (args.Length > 0 && args[0] == "--demo")
            {
                Demo(formatter, sink);
                return 0;
            }
            string path = args.Length > 0 ? args[0] : Path.Combine(AppContext.BaseDirectory, "commands", "shop-monday.txt");
            string[]? lines = new CommandReader(new Logger("Reader", formatter, sink)).Read(path);
            if (lines is null) return 1;
            var shop = new Shop(formatter, sink);
            for (int i = 0; i < lines.Length; i++) shop.ParseCommand(lines[i], i + 1);
            return 0; // Malformed lines are reported and skipped; completing the file is successful.
        }
        catch (ArgumentException ex)
        {
            logger.Error("Invalid startup arguments: %", ex.Message);
            return 1;
        }
    }

    private static void Demo(Formatter formatter, ConsoleSink sink)
    {
        sink.Write("Formatter alone (the sink prints its returned value):");
        sink.Write(formatter.Format(LogLevel.INFO, "Standalone", "Ratio is 100%n"));
        sink.Write(new Formatter("%n >> %m").Format(LogLevel.INFO, "Standalone", "Second format"));
        var composed = new Logger("Composed");
        composed.Info("Default parts were created by this logger.");
        var first = new Logger("First", formatter, sink);
        var second = new Logger("Second", formatter, sink);
        formatter.Pattern = "[%l] %n: %m (%r)";
        first.Info("Pair found at % and %; progress 100%%", 3, 7);
        second.Info("Both loggers see the shared formatter change.");
        new Logger("Special", new Formatter("%l >> %m"), sink).Info("An independent formatter.");
        formatter.Pattern = "[%l] %n: %m";

        var dell = new Display("Dell", 3840, 2160, 163, new Logger("Display", formatter, sink));
        var lg = new Display("LG", 2560, 1440, 108.8f, new Logger("Display", formatter, sink));
        var samsung = new Display("Samsung", 2560, 1440, 93, new Logger("Display", formatter, sink));
        var bob = new DisplayManager("Bob", new Logger("Manager", formatter, sink));
        var alice = new DisplayManager("Alice", new Logger("Manager", formatter, sink));
        bob.AssignDisplay(dell); bob.AssignDisplay(lg); bob.AssignDisplay(samsung);
        alice.AssignDisplay(dell); alice.AssignDisplay(samsung);
        bob.Assist();
        Display bought = bob.BuyDisplay(dell);
        first.Info("Bought object is the original Dell: %.", ReferenceEquals(bought, dell));
        alice.Assist();
        first.Info("Alice still holds the same Dell object: %.", alice.AssignedDisplays.Contains(dell));
    }
}
