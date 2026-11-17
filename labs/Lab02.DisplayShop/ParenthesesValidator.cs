using Lab02.DisplayShop.Logging;

namespace Lab02.DisplayShop;

public sealed class ParenthesesValidator
{
    private readonly Logger logger;

    public ParenthesesValidator() => logger = new Logger("Parentheses");
    public ParenthesesValidator(Logger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        this.logger = logger;
    }

    public bool IsValid(string? input)
    {
        if (input is null) return Invalid(input, "Input is null.");
        int openCount = 0;
        for (int i = 0; i < input.Length; i++)
        {
            switch (input[i])
            {
                case '(':
                    openCount++;
                    logger.Trace("Opening parenthesis at index %.", i);
                    break;
                case ')':
                    if (openCount == 0)
                    {
                        logger.Warn("Closing parenthesis without an opening at index %.", i);
                        logger.Warn("Remaining characters will not be checked (%).", input.Length - i - 1);
                        return Invalid(input, $"Unmatched closing parenthesis at index {i}.");
                    }
                    openCount--;
                    logger.Info("Matching pair found at closing index %.", i);
                    break;
                default:
                    return Invalid(input, $"Unexpected character '{input[i]}' at index {i}.");
            }
        }
        if (openCount > 0) return Invalid(input, $"{openCount} opening parentheses remain unmatched.");
        logger.Info("Validation result for \"%\": true.", input);
        return true;
    }

    private bool Invalid(string? input, string reason)
    {
        string display = input is null ? "<null>" : $"\"{input}\"";
        logger.Error("Invalid input %: %", display, reason);
        logger.Info("Validation result for %: false.", display);
        return false;
    }
}
