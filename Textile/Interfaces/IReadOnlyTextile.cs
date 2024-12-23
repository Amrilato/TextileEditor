namespace Textile.Interfaces;

public interface IReadOnlyTextile<TIndex, TValue> : ITextileSize
{
    TValue this[TIndex index] { get; }
    IEnumerable<TIndex> Indices { get; }
}
