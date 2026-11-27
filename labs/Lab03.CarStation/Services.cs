using Lab03.CarStation.Logging;

namespace Lab03.CarStation;

public interface IRefuelable
{
    bool Accepts(CarType type);
    void Refuel(Car car);
}

public interface IDineable
{
    bool Accepts(Passengers passengers);
    void ServeDinner(Car car);
}

public sealed class ElectricStation(StationStats stats, Logger logger) : IRefuelable
{
    public bool Accepts(CarType type) => type == CarType.ELECTRIC;
    public void Refuel(Car car)
    {
        stats.RecordFuel(car);
        logger.Info($"Charging electric car {car.Id}: {car.Consumption} units.");
    }
}

public sealed class GasStation(StationStats stats, Logger logger) : IRefuelable
{
    public bool Accepts(CarType type) => type == CarType.GAS;
    public void Refuel(Car car)
    {
        stats.RecordFuel(car);
        logger.Info($"Refueling gas car {car.Id}: {car.Consumption} units.");
    }
}

public sealed class PeopleDinner(StationStats stats, Logger logger) : IDineable
{
    public bool Accepts(Passengers passengers) => passengers == Passengers.PEOPLE;
    public void ServeDinner(Car car)
    {
        stats.RecordDinner(car);
        logger.Info($"Serving dinner to people in car {car.Id}.");
    }
}

public sealed class RobotDinner(StationStats stats, Logger logger) : IDineable
{
    public bool Accepts(Passengers passengers) => passengers == Passengers.ROBOTS;
    public void ServeDinner(Car car)
    {
        stats.RecordDinner(car);
        logger.Info($"Serving dinner to robots in car {car.Id}.");
    }
}

public sealed class NoDining : IDineable
{
    public bool Accepts(Passengers passengers) => false;
    public void ServeDinner(Car car) { }
}
