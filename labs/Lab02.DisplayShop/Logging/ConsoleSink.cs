namespace Lab02.DisplayShop.Logging;

public sealed class ConsoleSink
{
    public void Write(string line)
    {
        ArgumentNullException.ThrowIfNull(line);
        Console.WriteLine(line);
    }
}
