# OOP Labs

Laboratory assignments in C# (.NET 10).

## Lab 02: Formatters, Sinks, and Display Shop

Track 1 with shared logging objects, command-file parsing, continued processing after bad lines, a migrated parentheses validator and the `params` formatting bonus.

```powershell
dotnet run --project labs/Lab02.DisplayShop
dotnet run --project labs/Lab02.DisplayShop -- labs/Lab02.DisplayShop/commands/shop-tuesday.txt TRACE
dotnet run --project labs/Lab02.DisplayShop -- --demo TRACE
dotnet run --project labs/Lab02.DisplayShop -- --parentheses "()" ")((()))" "(" "(())((()())())"
dotnet test tests/Lab02.DisplayShop.Tests/Lab02.DisplayShop.Tests.csproj
```

See [Lab 2 instructions and design decisions](labs/Lab02.DisplayShop/README.md) and [expected command outcomes](labs/Lab02.DisplayShop/EXPECTED_RESULTS.md).

## Lab 01: Logging

A configurable logger and a parentheses validator.

```powershell
dotnet run --project labs/Lab01.Logging
dotnet run --project labs/Lab01.Logging -- "(())" ")(" ""
dotnet test
```

`Logger.Level` and `Logger.Format` are shared across logger instances; each instance has its own name. Levels are ordered `TRACE < INFO < WARN < ERROR`. Supported placeholders: `%t` (local time, HH:mm:ss), `%m` (message), `%n` (name), `%l` (level), and `%r` (random phrase for the entry's level). Unknown placeholders remain unchanged. Substituted values are never interpreted as format strings.

`ParenthesesValidator.IsValid` uses a counter: an unmatched closing parenthesis fails immediately, and any openings remaining at the end also fail. Time: O(n); extra space: O(1). The empty string is valid; null and characters other than `(` and `)` are invalid. Validation results do not depend on the configured log level.

The default demonstration runs all four required handout examples at TRACE, shows shared configuration using two loggers, and demonstrates `%r`.
