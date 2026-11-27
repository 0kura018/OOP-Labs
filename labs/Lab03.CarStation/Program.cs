using System.Globalization;
using System.Text.Json;
using Lab03.CarStation;
using Lab03.CarStation.Logging;

if (args.Length == 1 && args[0] == "--help")
{
    Console.WriteLine("Usage: Lab03.CarStation <stations.conf> [--demo]");
    return 0;
}

if (args.Length is < 1 or > 2 || (args.Length == 2 && args[1] != "--demo"))
{
    Console.Error.WriteLine("Usage: Lab03.CarStation <stations.conf> [--demo]");
    return 1;
}

try
{
    Config config = Config.Load(args[0]);
    using var loggers = new LoggerFactory(config);
    Logger logger = loggers.Create("Dispatcher");
    var stats = new StationStats();
    var factory = new StationFactory(stats, loggers);
    bool demo = args.Length == 2;
    var stations = demo
        ? new List<CarStation>
        {
            factory.Create("demo-electric", "ELECTRIC PEOPLE circular 2"),
            factory.Create("demo-gas", "GAS ROBOTS linked"),
            factory.Create("demo-express", "GAS NONE stacks")
        }
        : factory.Build(config);

    if (!int.TryParse(config["serve.every", "5"], NumberStyles.None, CultureInfo.InvariantCulture, out int serveEvery)
        || serveEvery < 1)
        throw new ConfigException("serve.every must be a positive integer.");

    int? seed = null;
    if (config["input.seed"] is string seedText)
    {
        if (!int.TryParse(seedText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedSeed))
            throw new ConfigException("input.seed must be an integer.");
        seed = parsedSeed;
    }

    var dispatcher = new Dispatcher(stations, serveEvery, logger);
    foreach (Car? car in demo ? DemoCars() : ReadCars(config, loggers))
    {
        stats.RecordArrival();
        if (car is null)
        {
            stats.RecordRejected();
        }
        else
        {
            try
            {
                dispatcher.Dispatch(car);
            }
            catch (NoStationException ex)
            {
                logger.Error(ex.Message);
                stats.RecordUnserved();
            }
        }
        dispatcher.Arrived();
    }

    dispatcher.Finish();
    loggers.Create("Stats").Info(stats.ToJson(demo ? null : seed));
    Console.WriteLine(stats.ToJson(demo ? null : seed, indented: true));
    return 0;
}
catch (Exception ex) when (ex is ConfigException or IOException or UnauthorizedAccessException or JsonException
    or ArgumentException or NotSupportedException)
{
    Console.Error.WriteLine($"[ERROR] {ex.Message}");
    return 1;
}

static IEnumerable<Car?> ReadCars(Config config, LoggerFactory loggers)
{
    string path = config.ResolvePath(config["input", "cars.txt"]);
    ICarReader reader = Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".txt" => new LinesCarReader(),
        ".json" => new JsonCarReader(),
        _ => throw new ConfigException("input must refer to a .txt or .json file.")
    };
    return reader.Read(path, loggers.Create("Reader"));
}

static IEnumerable<Car?> DemoCars()
{
    IQueue<int>[] queues = [new CircularQueue<int>(2), new LinkedQueue<int>(), new TwoStackQueue<int>()];
    foreach (IQueue<int> queue in queues)
    {
        queue.Enqueue(1);
        queue.Enqueue(2);
        int first = queue.Dequeue();
        queue.Enqueue(3);
        Console.WriteLine($"{queue.GetType().Name}: {first}, {queue.Dequeue()}, {queue.Dequeue()}");
    }

    yield return new Car(1, CarType.ELECTRIC, Passengers.PEOPLE, true, 25);
    yield return new Car(2, CarType.GAS, Passengers.ROBOTS, true, 30);
    yield return new Car(3, CarType.GAS, Passengers.PEOPLE, false, 10);
}
