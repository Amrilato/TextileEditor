using Textile.Common;

namespace TextileEditor.Shared.Painters.DataSelector;

internal interface ITextileChangedWatcher<TIndex,TValue>
{
    void OnChanged(ReadOnlySpan<ChangedValue<TIndex, TValue>> changedValues);
}