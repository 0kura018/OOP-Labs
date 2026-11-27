namespace Lab03.CarStation.Logging;

public sealed class LoggerFactory : IDisposable
{
    private readonly IFormatter formatter;
    private readonly ISink sink;
    private readonly LogLevel level;
    private readonly AsyncSink? asyncFile;

    public LoggerFactory(Config config)
    {
        var warnings = new List<string>();
        string Read(string key, string fallback)
        {
            string? value = config[key];
            if (value is not null)
                return value;

            warnings.Add($"Missing {key}. Using '{fallback}'.");
            return fallback;
        }

        string levelName = Read("log.level", "INFO");
        if (!Enum.GetNames<LogLevel>().Contains(levelName, StringComparer.OrdinalIgnoreCase))
            throw new ConfigException($"Unknown log.level '{levelName}'.");
        level = Enum.Parse<LogLevel>(levelName, ignoreCase: true);

        formatter = Read("log.format", "pattern").ToLowerInvariant() switch
        {
            "pattern" => new PatternFormatter(Read("log.pattern", "[%l] %n: %m")),
            "json" => new JsonFormatter(),
            string unknown => throw new ConfigException($"Unknown log.format '{unknown}'.")
        };

        string[] names = Read("log.sinks", "console").ToLowerInvariant()
            .Split(',', StringSplitOptions.TrimEntries);
        foreach (string name in names)
        {
            if (name is not ("console" or "file"))
                throw new ConfigException($"Unknown log sink '{name}'.");
        }

        if (!bool.TryParse(config["log.async", "false"], out bool asyncEnabled))
            throw new ConfigException("log.async must be true or false.");

        var sinks = new List<ISink>();
        foreach (string name in names.Distinct())
        {
            if (name == "console")
            {
                sinks.Add(new ConsoleSink());
                continue;
            }

            string path = Read("log.file", "stations.log");
            if (path.Length == 0)
                throw new ConfigException("log.file must contain a path.");

            ISink file = new FileSink(config.ResolvePath(path));
            if (asyncEnabled)
                file = asyncFile = new AsyncSink(file);
            sinks.Add(file);
        }

        sink = sinks.Count == 1 ? sinks[0] : new MultiSink(sinks);
        try
        {
            var logger = Create("Config");
            foreach (string warning in warnings)
                logger.Warn(warning);
        }
        catch
        {
            asyncFile?.Dispose();
            throw;
        }
    }

    public Logger Create(string name) => new(name, level, formatter, sink);
    public void Dispose() => asyncFile?.Dispose();
}
