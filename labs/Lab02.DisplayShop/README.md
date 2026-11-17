# Lab 2: Formatters, sinks, and Display Shop

C# / .NET 10. Track 1, including the varargs bonus. Lab 1 is preserved in its original project; this project carries its parentheses validator forward using the refactored logger.

## Run from the repository root

```powershell
# Default Monday file at INFO; bundled files are copied beside the executable.
dotnet run --project labs/Lab02.DisplayShop

dotnet run --project labs/Lab02.DisplayShop -- labs/Lab02.DisplayShop/commands/shop-monday.txt TRACE
dotnet run --project labs/Lab02.DisplayShop -- labs/Lab02.DisplayShop/commands/shop-tuesday.txt TRACE
dotnet run --project labs/Lab02.DisplayShop -- labs/Lab02.DisplayShop/commands/shop-edge-cases.txt ERROR

# Standalone formatters, both logger constructors, shared objects, three hard-coded displays.
dotnet run --project labs/Lab02.DisplayShop -- --demo TRACE

# Lab 1 validation with the new logging pipeline (always at TRACE).
dotnet run --project labs/Lab02.DisplayShop -- --parentheses "()" ")((()))" "(" "(())((()())())"

dotnet test tests/Lab02.DisplayShop.Tests/Lab02.DisplayShop.Tests.csproj
```

Normal syntax: `[file] [TRACE|INFO|WARN|ERROR]`. Both arguments are optional; the default is the bundled Monday file at INFO. Paths containing spaces must be quoted. `--help` prints usage. Missing/unreadable files and invalid startup arguments return exit code 1. A readable command file completes with exit code 0 even if individual lines are rejected; ERROR messages identify those lines.

## Design

- `Logger`: a name and the shared static `Level`; checks the threshold and delegates. There is no `Logger.Format`, `SetFormat`, or console print in this class.
- `Formatter`: owns an **instance** `Pattern`, formats a line and returns it. Section 4.6 explicitly allows this alternative to Task A1's static format. Loggers can share one formatter while a special logger uses another.
- `ConsoleSink`: the only logging class that calls `Console.WriteLine`.
- `Logger(name)`: composition, with private formatter/sink constructed inside it.
- `Logger(name, formatter, sink)`: aggregation, using caller-created parts. The application creates shared parts and uses this constructor for all shop loggers.
- `Shop` composes its internal lists and parser logger; it aggregates formatter/sink. A manager owns its list, but aggregates the `Display` objects added to it. Removing a reference from Bob's list does not remove Alice's reference to the same display. The catalog also retains it.
- `Display` compares physical diagonals, not pixel counts. Diagonal inches = `sqrt(width² + height²) / ppi`. Differences below 0.05 inches count as equal physical size; PPI comparisons are exact. `CompareWithMonitor` reuses both comparison methods. A size/sharpness trade-off does not imply an overall winner.

The program uses concrete classes and simple loops for lookups, as requested. No interface/inheritance framework is necessary. C# garbage collection controls actual reclamation: an aggregated display remains reachable while any manager/catalog holds it. Buying it is not object destruction.

## Command policies

Commands and names are case-sensitive. Names/models are single whitespace-delimited tokens. Only whole-line comments beginning with `#` after optional leading whitespace are supported. Tabs and repeated spaces are accepted. Every command has an exact argument count. Numeric parsing uses invariant culture, with a decimal dot. Dimensions must be positive integers; PPI must be positive and finite.

Duplicate managers/models are WARN and leave existing state intact (creation is skipped). Duplicate assignment is WARN, without another list entry. ASSIST with fewer than two displays is WARN. Buying the last assigned display also produces WARN. Unknown commands/names, bad arguments/numbers and buying an unassigned display are ERROR. Parsing continues after each rejected line. Warnings emitted directly by a manager identify the manager/model; parser errors always contain the file line number.

## Formatting and bonus

```csharp
var formatter = new Formatter("[%l] %n: %m (%r)");
var sink = new ConsoleSink();
var logger = new Logger("Demo", formatter, sink);
logger.Info("Pair found at % and %; progress 100%%", 3, 7);
```

The outer pattern supports `%t`, `%l`, `%n`, `%m`, `%r`. Unknown tokens remain literal. Inserted values are never reinterpreted. A message `Ratio is 100%n` without extra arguments stays `Ratio is 100%n`, even with a named logger.

Inside the **message**, bare `%` consumes the next `params object?[]` argument; `%%` is a literal percent. Missing arguments leave `%` unchanged; extra arguments are ignored; null arguments render as `<null>`. These are chosen policies, not errors. To pass one null argument explicitly, use `new object?[] { null }`. Literal `%n` in a parameterized message is written as `%%n`. Substituted argument text remains literal even if it contains percent signs.

The level check happens before formatting arguments. Hidden entries therefore do not call an argument's `ToString`. Repeated `%r` within one entry uses one phrase; later entries may repeat the same random phrase.

## Expected example results

See [EXPECTED_RESULTS.md](EXPECTED_RESULTS.md) for every line in the supplied files. Monday has 0 ERRORs. Tuesday has 9 ERRORs under the documented case-sensitive policy, and warnings for duplicate creation/assignment and an empty manager. The additional edge file has 6 ERRORs and still creates AfterErrors at its end.

Repeat each file with TRACE, INFO and ERROR. At INFO there are no TRACE messages; at ERROR every printed log entry is an ERROR. Filtering never changes shop state or parentheses results.

## Source map

`Logging/` contains the refactored pipeline. `Display.cs` and `DisplayManager.cs` are the shop model. `Shop.cs` parses and executes lines. `CommandReader.cs` handles file I/O. `Application.cs` selects the mode, creates shared logging objects and handles startup errors. `Program.cs` only calls `Application.Run(args)`. The `commands/` folder contains both exact handout examples and one additional file.
