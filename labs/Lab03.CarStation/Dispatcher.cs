using Lab03.CarStation.Logging;

namespace Lab03.CarStation;

public sealed class NoStationException(string message) : Exception(message);

public sealed class Dispatcher(IReadOnlyList<CarStation> stations, int serveEvery, Logger logger)
{
    private int sinceLastService;

    public void Dispatch(Car car)
    {
        foreach (CarStation station in stations)
        {
            if (!station.CanServe(car))
                continue;

            if (station.IsFull)
            {
                logger.Warn($"Station {station.Name} is full for car {car.Id}. Trying the next station.");
                continue;
            }

            station.AddCar(car);
            return;
        }

        throw new NoStationException($"No station available for car {car.Id} ({car.Type}, {car.Passengers}, dining={car.IsDining}).");
    }

    public void Arrived()
    {
        if (++sinceLastService == serveEvery)
            Finish();
    }

    public void Finish()
    {
        foreach (CarStation station in stations)
            station.ServeCars();
        sinceLastService = 0;
    }
}
