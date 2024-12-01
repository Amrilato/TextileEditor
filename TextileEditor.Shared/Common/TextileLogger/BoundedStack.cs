using System.Diagnostics.CodeAnalysis;

namespace TextileEditor.Shared.Common;

public class BoundedStack<T>
{
    private readonly T?[] buffer;
    private int head;
    private int tail;
    private int count;

    public BoundedStack(int size)
    {
        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than zero.");
        buffer = new T[size];
        head = 0;
        tail = 0;
        count = 0;
    }

    public int Count => count;
    public int Capacity => buffer.Length;

    public void Push(T item)
    {
        if (count == Capacity)
        {
            // Buffer is full, overwrite the oldest item
            head = (head + 1) % Capacity;
            count--;
        }

        buffer[tail] = item;
        tail = (tail + 1) % Capacity;
        count++;
    }

    public T Pop()
    {
        if (count == 0)
            throw new InvalidOperationException("Stack is empty.");

        tail = (tail - 1 + Capacity) % Capacity;
        T? value = buffer[tail];
        buffer[tail] = default; // Clear the element (optional)
        count--;
        return value ?? throw new InvalidOperationException();
    }

    public bool TryPop([MaybeNullWhen(false)] out T value)
    {
        if (count == 0)
        {
            value = default;
            return false;
        }

        tail = (tail - 1 + Capacity) % Capacity;
        value = buffer[tail];
        buffer[tail] = default; // Clear the element (optional)
        count--;
        return value is not null;
    }

    public T Peek()
    {
        if (count == 0)
            throw new InvalidOperationException("Stack is empty.");

        int peekIndex = (tail - 1 + Capacity) % Capacity;
        return buffer[peekIndex] ?? throw new InvalidOperationException();
    }

    public bool TryPeek([MaybeNullWhen(false)] out T value)
    {
        value = default;
        if (count == 0)
            return false;

        int peekIndex = (tail - 1 + Capacity) % Capacity;
        value = buffer[peekIndex];
        return value is not null;
    }

    public void Clear()
    {
        head = 0;
        tail = 0;
        count = 0;
    }
}