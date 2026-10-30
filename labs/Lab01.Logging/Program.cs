using Lab01.Logging;

Logger.SetLevel(LogLevel.TRACE);
var validator = new ParenthesesValidator();
string[] inputs = args.Length > 0 ? args : ["()", ")((()))", "(", "(())((()())())"];
foreach (string input in inputs)
{
    Console.WriteLine($"\nChecking: \"{input}\"");
    bool valid = validator.IsValid(input);
    Console.WriteLine($"Result: {valid}");
}

if (args.Length == 0)
{
    Console.WriteLine("\nShared settings demonstration:");
    var first = new Logger("First");
    var second = new Logger("Second");
    Logger.SetLevel(LogLevel.WARN);
    Logger.SetFormat("[%l] %n: %m");
    first.Info("This INFO message is hidden.");
    first.Warn("WARN is visible with the shared threshold.");
    second.Error("ERROR is visible with the same shared threshold.");
    Console.WriteLine("\nBonus %r demonstration:");
    Logger.SetLevel(LogLevel.INFO);
    Logger.SetFormat("[%l] %n: %m (%r)");
    first.Info("Random phrase matches the INFO level.");
}
