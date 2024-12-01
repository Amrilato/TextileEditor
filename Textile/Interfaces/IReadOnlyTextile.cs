namespace Textile.Interfaces;

public interface IReadOnlyTextile<TIndex, TValue> : ITextileSize
{
    TValue this[TIndex index] { get; }
    //todo: change to IEnumerable<TIndex> to struct IndexEnumerable. less allocation better performance
    IEnumerable<TIndex> Indices { get; }
}
