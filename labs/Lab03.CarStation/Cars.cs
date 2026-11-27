using System.Globalization;
using System.Text.Json;
using Lab03.CarStation.Logging;

namespace Lab03.CarStation;

public enum CarType { ELECTRIC, GAS }
public enum Passengers { PEOPLE, ROBOTS }

public sealed record Car(int Id, CarType Type, Passengers Passengers, bool IsDining, int Consumption);

public sealed class InvalidCarException(string message) : Exception(message);

public interface ICarReader
{
    IEnumerable<Car?> Read(string path, Logger logger);
}

public abstract class CarReader : ICarReader
{
    public IEnumerable<Car?> Read(string path, Logger logger)
    {
        foreach (Func<Car> parse in ReadRows(path))
        {
            Car? car = null;
            try
            {
                car = parse();
                logger.Trace($"Read car {car.Id}: {car.Type}, {car.Passengers}, dining={car.IsDining}, consumption={car.Consumption}.");
            }
            catch (InvalidCarException ex)
            {
                logger.Error($"Car rejected: {ex.Message}");
            }
            yield return car;
        }
    }

    protected abstract IEnumerable<Func<Car>> ReadRows(string path);

    protected static Car Parse(string label, string? id, string? type, string? passengers,
        string? dining, string? consumption)
    {
        if (!int.TryParse(id, NumberStyles.Integer, CultureInfo.InvariantCulture, out int carId) || carId <= 0)
            throw new InvalidCarException($"{label}: ID must be a positive integer.");
        if (!Enum.GetNames<CarType>().Contains(type, StringComparer.Ordinal))
            throw new InvalidCarException($"{label}: unknown type '{type ?? "null"}'.");
        if (!Enum.GetNames<Passengers>().Contains(passengers, StringComparer.Ordinal))
            throw new InvalidCarException($"{label}: unknown passengers '{passengers ?? "null"}'.");
        if (!bool.TryParse(dining, out bool isDining))
            throw new InvalidCarException($"{label}: isDining must be true or false.");
        if (!int.TryParse(consumption, NumberStyles.Integer, CultureInfo.InvariantCulture, out int amount) || amount < 0)
            throw new InvalidCarException($"{label}: consumption must be a nonnegative integer.");

        return new Car(carId, Enum.Parse<CarType>(type!), Enum.Parse<Passengers>(passengers!), isDining, amount);
    }
}

public sealed class LinesCarReader : CarReader
{
    protected override IEnumerable<Func<Car>> ReadRows(string path)
    {
        int lineNumber = 0;
        foreach (string raw in File.ReadLines(path))
        {
            lineNumber++;
            string line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            int number = lineNumber;
            yield return () =>
            {
                string[] words = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                string label = $"line {number}, car {words[0]}";
                if (words.Length != 5)
                    throw new InvalidCarException($"{label}: expected 5 fields, got {words.Length}.");
                return Parse(label, words[0], words[1], words[2], words[3], words[4]);
            };
        }
    }
}

public sealed class JsonCarReader : CarReader
{
    protected override IEnumerable<Func<Car>> ReadRows(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new ConfigException("The JSON car file must contain an array.");

        int index = 0;
        foreach (JsonElement item in document.RootElement.EnumerateArray())
        {
            int row = ++index;
            yield return () =>
            {
                if (item.ValueKind != JsonValueKind.Object)
                    throw new InvalidCarException($"item {row}: expected a car object.");

                string? Field(string name, JsonValueKind kind)
                {
                    if (!item.TryGetProperty(name, out JsonElement value) || value.ValueKind != kind)
                        return null;
                    return kind == JsonValueKind.String ? value.GetString() : value.GetRawText();
                }

                string? id = Field("id", JsonValueKind.Number);
                return Parse($"item {row}, car {id ?? "?"}", id, Field("type", JsonValueKind.String),
                    Field("passengers", JsonValueKind.String),
                    Field("isDining", JsonValueKind.True) ?? Field("isDining", JsonValueKind.False),
                    Field("consumption", JsonValueKind.Number));
            };
        }
    }
}
