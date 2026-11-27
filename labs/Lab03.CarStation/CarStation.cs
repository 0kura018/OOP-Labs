using Lab03.CarStation.Logging;

namespace Lab03.CarStation;

public sealed class CarStation(string name, IQueue<Car> queue, IRefuelable fuel, IDineable dining, Logger logger)
{
    public string Name => name;
    public bool IsFull => queue.IsFull;

    public bool CanServe(Car car) =>
        fuel.Accepts(car.Type) && (!car.IsDining || dining.Accepts(car.Passengers));

    public void AddCar(Car car)
    {
        queue.Enqueue(car);
        logger.Trace($"Car {car.Id} queued at {Name} ({queue.Count} waiting).");
    }

    public void ServeCars()
    {
        int count = queue.Count;
        if (count == 0)
            return;

        while (!queue.IsEmpty)
        {
            Car car = queue.Dequeue();
            if (car.IsDining)
                dining.ServeDinner(car);
            fuel.Refuel(car);
        }
        logger.Info($"Station {Name} served {count} car(s).");
    }
}
