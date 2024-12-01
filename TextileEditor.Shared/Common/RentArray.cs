using System.Buffers;

namespace TextileEditor.Shared.Shared.Common;

public readonly struct RentArray<T>(int minimumLength, ArrayPool<T> pool) : IDisposable
{
    private readonly T[] values = pool.Rent(minimumLength);
    public Memory<T> Values => values.AsMemory()[..minimumLength];
    public void Dispose() => pool.Return(values);
}
