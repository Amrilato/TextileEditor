using Textile.Interfaces;

namespace TextileEditor.Shared.Painters.DataSelector;

internal interface ITextileSelector<TTextile, TIndex, TValue, TSelf>
    where TSelf : ITextileSelector<TTextile, TIndex, TValue, TSelf>, IDisposable
{
    static abstract TSelf Subscribe(ITextileChangedWatcher<TIndex, TValue> watcher, TTextile value);
    static abstract ITextile<TIndex, TValue> SelectTextileData(TTextile value);
}
