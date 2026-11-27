using System.Globalization;
using Lab03.CarStation.Logging;

namespace Lab03.CarStation;

public sealed class StationFactory(StationStats stats, LoggerFactory loggers)
{
    public List<CarStation> Build(Config config)
    {
        Logger logger = loggers.Create("StationFactory");
        var stations = new List<CarStation>();
        foreach (var entry in config.Stations)
        {
            try
            {
                stations.Add(Create(entry.Key["station.".Length..], entry.Value));
            }
            catch (ConfigException ex)
            {
                logger.Error($"{entry.Key}: {ex.Message} Station skipped.");
            }
        }

        if (stations.Count == 0)
            throw new ConfigException("The config did not define any usable stations.");
        return stations;
    }

    public CarStation Create(string name, string definition)
    {
        string[] words = definition.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (name.Length == 0 || words.Length is < 3 or > 4)
            throw new ConfigException("Expected a station name and fuel restaurant queue [capacity].");

        Logger logger = loggers.Create(name);

        IRefuelable fuel = words[0] switch
        {
            "ELECTRIC" => new ElectricStation(stats, logger),
            "GAS" => new GasStation(stats, logger),
            _ => throw new ConfigException($"Unknown fuel '{words[0]}'.")
        };

        IDineable dining = words[1] switch
        {
            "PEOPLE" => new PeopleDinner(stats, logger),
            "ROBOTS" => new RobotDinner(stats, logger),
            "NONE" => new NoDining(),
            _ => throw new ConfigException($"Unknown restaurant '{words[1]}'.")
        };

        IQueue<Car> queue = words[2] switch
        {
            "circular" => new CircularQueue<Car>(ReadCapacity(words)),
            "linked" when words.Length == 3 || words[3] == "0" => new LinkedQueue<Car>(),
            "stacks" when words.Length == 3 || words[3] == "0" => new TwoStackQueue<Car>(),
            "linked" or "stacks" => throw new ConfigException($"Queue '{words[2]}' is unbounded. Omit capacity or use 0."),
            _ => throw new ConfigException($"Unknown queue '{words[2]}'.")
        };

        logger.Info($"Built station {name}: fuel={words[0]}, restaurant={words[1]}, queue={string.Join(' ', words[2..])}.");
        return new CarStation(name, queue, fuel, dining, logger);
    }

    private static int ReadCapacity(string[] words)
    {
        if (words.Length != 4 ||
            !int.TryParse(words[3], NumberStyles.None, CultureInfo.InvariantCulture, out int capacity) ||
            capacity < 1 || capacity > Array.MaxLength)
            throw new ConfigException("Circular queue requires a positive integer capacity within the array limit.");
        return capacity;
    }
}
