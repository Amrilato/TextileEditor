using Textile.Common;

namespace Textile.Interfaces;

public interface IObservableTextile<TIndex, TValue> : ITextile<TIndex, TValue>, IReadOnlyObservableTextile<TIndex, TValue>
{
    new event TextileStateChangedEventHandler<TIndex, TValue> TextileStateChanged;
}