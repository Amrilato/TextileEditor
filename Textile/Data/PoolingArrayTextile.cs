using System.Buffers;
using Textile.Common;
using Textile.Interfaces;

namespace Textile.Data;

internal struct PoolingArrayTextile : ITextile<TextileIndex, bool>, IReadOnlyTextile<TextileIndex, bool>, IDisposable
{
    public int Width { get; }
    public int Height { get; }
    private uint[] Value { get; }

    public readonly IEnumerable<TextileIndex> Indices
    {
        get
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    yield return new(x, y);
        }
    }

    private bool disposed = false;

    public PoolingArrayTextile(int width, int height)
    {
        Width = width;
        Height = height;
        Value = ArrayPool<uint>.Shared.Rent(width.GetArraySize() * height);
        Value.AsSpan().Clear();
    }

    public readonly bool this[TextileIndex index]
    {
        get
        {
            this.ThrowIfIndexWasOutOfBound(index);
            return ArrayConsecutiveLine(index.Y).Span.IsBitSet(index.X);
        }
        set
        {
            this.ThrowIfIndexWasOutOfBound(index);
            ArrayConsecutiveLine(index.Y).Span.SetBit(index.X, value);
        }
    }

    public void Write(IEnumerable<KeyValuePair<TextileIndex, bool>> values)
    {
        foreach (var (index, value) in values)
            this[index] = value;
    }

    public readonly void Clear() => Value.AsSpan().Clear();

    private readonly Memory<uint> ArrayConsecutiveLine(int index)
    {
        var size = Width.GetArraySize();
        return Value.AsMemory()[(index * size)..(index * size + size)];
    }

    public void Dispose()
    {
        if (!disposed)
        {
            ArrayPool<uint>.Shared.Return(Value);
            disposed = true;
        }
    }
}
