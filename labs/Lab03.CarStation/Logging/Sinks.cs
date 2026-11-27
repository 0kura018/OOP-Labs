using System.Collections.Concurrent;

namespace Lab03.CarStation.Logging;

public sealed class ConsoleSink : ISink
{
    public void Write(string line) => Console.WriteLine(line);
}

public sealed class FileSink(string path) : ISink
{
    public void Write(string line) => File.AppendAllText(path, line + Environment.NewLine);
}

public sealed class MultiSink(IReadOnlyList<ISink> sinks) : ISink
{
    public void Write(string line)
    {
        foreach (ISink sink in sinks)
            sink.Write(line);
    }
}

public sealed class AsyncSink : ISink, IDisposable
{
    private readonly BlockingCollection<string> pending = new();
    private readonly Task worker;
    private bool disposed;

    public AsyncSink(ISink destination)
    {
        worker = Task.Factory.StartNew(() =>
        {
            foreach (string line in pending.GetConsumingEnumerable())
                destination.Write(line);
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    public void Write(string line)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (worker.IsFaulted)
            worker.GetAwaiter().GetResult();
        pending.Add(line);
    }

    public void Dispose()
    {
        if (disposed)
            return;

        disposed = true;
        pending.CompleteAdding();
        try
        {
            worker.GetAwaiter().GetResult();
        }
        finally
        {
            pending.Dispose();
        }
    }
}
