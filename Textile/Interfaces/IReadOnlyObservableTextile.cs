using Textile.Common;

namespace Textile.Interfaces;

public interface IReadOnlyObservableTextile<TIndex, TValue> : IReadOnlyTextile<TIndex, TValue>
{
    event TextileStateChangedEventHandler<TIndex, TValue> TextileStateChanged;
}
