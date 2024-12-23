using Textile.Common;
using Textile.Data;
using Textile.Interfaces;

namespace TextileEditor.Shared.View.TextileEditor.DataSelector;

internal interface ITextileSelector<TIndex, TValue, TSelf>
    where TSelf : ITextileSelector<TIndex, TValue, TSelf>, IDisposable
{
    static abstract ITextile<TIndex, TValue> Select(TextileStructure structure);
    static abstract TSelf Subscribe(ITextileChangedWatcher<TIndex, TValue> watcher, IReadOnlyTextileStructure value);
    IReadOnlyTextile<TIndex, TValue> Textile { get; }
}

internal interface ITextileChangedWatcher<TIndex, TValue>
{
    void OnChanged(ReadOnlySpan<ChangedValue<TIndex, TValue>> changedValues);
}
