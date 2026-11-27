using System.Text.Json;

namespace Lab03.CarStation;

public sealed class StationStats
{
    public int Total { get; private set; }
    public int Rejected { get; private set; }
    public int Unserved { get; private set; }
    private int electric;
    private int gas;
    private int people;
    private int robots;
    private int peopleDining;
    private int robotsDining;
    private long electricConsumption;
    private long gasConsumption;

    public void RecordArrival() => Total++;
    public void RecordRejected() => Rejected++;
    public void RecordUnserved() => Unserved++;

    public void RecordFuel(Car car)
    {
        if (car.Type == CarType.ELECTRIC)
        {
            electric++;
            electricConsumption += car.Consumption;
        }
        else
        {
            gas++;
            gasConsumption += car.Consumption;
        }

        if (car.Passengers == Passengers.PEOPLE)
            people++;
        else
            robots++;
    }

    public void RecordDinner(Car car)
    {
        if (car.Passengers == Passengers.PEOPLE)
            peopleDining++;
        else
            robotsDining++;
    }

    public string ToJson(int? seed, bool indented = false)
    {
        var values = new Dictionary<string, object>();
        if (seed is int chosenSeed)
            values["SEED"] = chosenSeed;
        values["TOTAL"] = Total;
        values["REJECTED"] = Rejected;
        values["SERVED"] = electric + gas;
        values["UNSERVED"] = Unserved;
        values["ELECTRIC"] = electric;
        values["GAS"] = gas;
        values["PEOPLE"] = people;
        values["ROBOTS"] = robots;
        values["DINING"] = peopleDining + robotsDining;
        values["NOT_DINING"] = electric + gas - peopleDining - robotsDining;
        values["DINING_BY"] = new { PEOPLE = peopleDining, ROBOTS = robotsDining };
        values["CONSUMPTION"] = new { ELECTRIC = electricConsumption, GAS = gasConsumption };
        return JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = indented });
    }
}
