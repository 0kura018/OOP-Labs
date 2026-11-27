namespace Lab03.CarStation;

public sealed class ConfigException(string message) : Exception(message);

public sealed class Config
{
    private readonly Dictionary<string, string> values = new(StringComparer.Ordinal);
    private readonly List<string> keys = [];

    public string? this[string key] => values.GetValueOrDefault(key);
    public string this[string key, string fallback] => this[key] ?? fallback;

    public IEnumerable<KeyValuePair<string, string>> Stations =>
        keys.Where(key => key.StartsWith("station.", StringComparison.Ordinal))
            .Select(key => KeyValuePair.Create(key, values[key]));

    public string ResolvePath(string path) => Path.GetFullPath(path, Directory);
    private string Directory { get; }

    private Config(string path) => Directory = Path.GetDirectoryName(Path.GetFullPath(path))!;

    public static Config Load(string path)
    {
        var config = new Config(path);
        int number = 0;
        foreach (string raw in File.ReadLines(path))
        {
            number++;
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            int separator = line.IndexOf('=');
            if (separator < 1)
                throw new ConfigException($"Line {number}: expected key = value.");

            string key = line[..separator].Trim();
            if (!config.values.TryAdd(key, line[(separator + 1)..].Trim()))
                throw new ConfigException($"Line {number}: duplicate key '{key}'.");

            config.keys.Add(key);
        }

        return config;
    }
}
