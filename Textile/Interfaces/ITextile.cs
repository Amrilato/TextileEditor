namespace Textile.Interfaces;

public interface ITextile<TIndex, TValue> : IReadOnlyTextile<TIndex, TValue>
{
    new TValue this[TIndex index] { get; set; }
    void Write(IEnumerable<KeyValuePair<TIndex, TValue>> values);
    void Clear();
}
