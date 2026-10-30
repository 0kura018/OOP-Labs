# OOP Labs

Laboratory assignments in C# (.NET 10).

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
