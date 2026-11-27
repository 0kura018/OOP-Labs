# Lab 3 The Car Station

C# and .NET 10. Part A adds configuration and interface dispatch to the logging design from Lab 2. Track 1 runs independently of the previous projects.

Run from the solution directory:

```powershell
dotnet run --project labs/Lab03.CarStation -- labs/Lab03.CarStation/scenarios/stations-good.conf
dotnet run --project labs/Lab03.CarStation -- labs/Lab03.CarStation/scenarios/stations-broken.conf
dotnet run --project labs/Lab03.CarStation -- labs/Lab03.CarStation/scenarios/stations-json.conf
dotnet run --project labs/Lab03.CarStation -- labs/Lab03.CarStation/scenarios/stations-good.conf --demo
```

The demo uses three hard-coded cars and exercises all three queues with integer values. The JSON scenario writes JSON Lines to `structured.log` using the asynchronous file sink. The summary is also printed to the console. Log files are appended, so they may contain multiple runs.

## Configuration and routing

- Keys use `key = value`. Input and log paths are relative to the config directory. Missing logging keys use defaults, with warnings subject to the configured threshold. A file sink defaults to `stations.log`. Unknown log options or a missing config stop with a short error and exit code 1.
- Station values use `fuel restaurant queue [capacity]`. Fuel is `ELECTRIC` or `GAS`. Dining is `PEOPLE`, `ROBOTS` or `NONE`. Queues are `circular` with positive capacity, `linked` or `stacks` with no capacity (or 0).
- The dispatcher picks the first compatible station with space in config order. It drains the queues after every `serve.every` input records, including rejected records, and again at EOF. An invalid car is rejected. A valid car without a compatible free station is unserved. Invalid station definitions are logged and skipped, and zero usable stations is fatal.
- All three queues are FIFO. Circular storage wraps around and throws `QueueFullException` on overflow. Empty dequeue throws `InvalidOperationException` for every implementation. The third queue uses two stacks, transferring items to the outgoing stack only when it is empty.
- Counts of fuel type, passengers, dinners and consumption describe cars actually served. `TOTAL = SERVED + REJECTED + UNSERVED`. When using the course generator, point `input` to the generated file and set optional `input.seed` to include the generator's seed in the summary. Compare the common fields with its `stats.json`. The two extra fields are `SERVED` and `UNSERVED`.

The bundled cars are a small local fixture with five invalid records. In both input formats the good configuration matches `scenarios/stats.json`. The broken configuration skips three station definitions and produces 2 served, 5 rejected and 7 unserved. The official course generator is separate from this fixture.

The `Logger` depends on `IFormatter` and `ISink`. `LoggerFactory` selects the formatter and destinations. `StationFactory` selects the queue, fueling service and dining service for each station. `NoDining` supplies an object implementing the dining contract even when a station has no restaurant. For the bonus, set `log.async = true` with a file sink. The worker drains buffered lines on normal shutdown. An abrupt termination can lose the messages still pending.
