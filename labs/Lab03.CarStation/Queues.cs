namespace Lab03.CarStation;

public interface IQueue<T>
{
    void Enqueue(T item);
    T Dequeue();
    int Count { get; }
    bool IsEmpty => Count == 0;
    bool IsFull => false;
}

public sealed class QueueFullException(string message) : Exception(message);

public sealed class CircularQueue<T> : IQueue<T>
{
    private readonly T[] items;
    private int head;
    private int tail;

    public CircularQueue(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        items = new T[capacity];
    }

    public int Count { get; private set; }
    public bool IsFull => Count == items.Length;

    public void Enqueue(T item)
    {
        if (IsFull)
            throw new QueueFullException($"Queue capacity is {items.Length}.");

        items[tail] = item;
        tail = (tail + 1) % items.Length;
        Count++;
    }

    public T Dequeue()
    {
        if (Count == 0)
            throw new InvalidOperationException("The queue is empty.");

        T item = items[head];
        items[head] = default!;
        head = (head + 1) % items.Length;
        Count--;
        return item;
    }
}

public sealed class LinkedQueue<T> : IQueue<T>
{
    private sealed class Node(T item)
    {
        public T Item { get; } = item;
        public Node? Next { get; set; }
    }

    private Node? head;
    private Node? tail;
    public int Count { get; private set; }

    public void Enqueue(T item)
    {
        var node = new Node(item);
        if (tail is null)
            head = node;
        else
            tail.Next = node;

        tail = node;
        Count++;
    }

    public T Dequeue()
    {
        Node node = head ?? throw new InvalidOperationException("The queue is empty.");
        head = node.Next;
        if (head is null)
            tail = null;
        Count--;
        return node.Item;
    }
}

public sealed class TwoStackQueue<T> : IQueue<T>
{
    private readonly Stack<T> incoming = new();
    private readonly Stack<T> outgoing = new();
    public int Count => incoming.Count + outgoing.Count;

    public void Enqueue(T item) => incoming.Push(item);

    public T Dequeue()
    {
        if (outgoing.Count == 0)
        {
            while (incoming.TryPop(out T? item))
                outgoing.Push(item);
        }

        if (!outgoing.TryPop(out T? result))
            throw new InvalidOperationException("The queue is empty.");

        return result;
    }
}
