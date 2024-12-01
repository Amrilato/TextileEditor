using Textile.Common;
using Textile.Data;
using Textile.Interfaces;

namespace TextileEditor.Shared.Painters.DataSelector;

internal readonly struct HeddleSelector : ITextileSelector<TextileStructure, TextileIndex, bool, HeddleSelector>, IDisposable
{
    public static ITextile<TextileIndex, bool> SelectTextileData(TextileStructure value) => value.Heddle;

    public static HeddleSelector Subscribe(ITextileChangedWatcher<TextileIndex, bool> watcher, TextileStructure value) => new(watcher, value);

    public HeddleSelector(ITextileChangedWatcher<TextileIndex, bool> watcher, TextileStructure structure)
    {
        this.watcher = watcher;
        this.structure = structure;
        structure.Heddle.TextileStateChanged += TextileStateChanged;
    }

    private void TextileStateChanged(IReadOnlyTextile<TextileIndex, bool> sender, TextileStateChangedEventArgs<TextileIndex, bool> eventArgs) => watcher.OnChanged(eventArgs.ChangedIndices);

    private readonly ITextileChangedWatcher<TextileIndex, bool> watcher;
    private readonly TextileStructure structure;


    public void Dispose() => structure.Heddle.TextileStateChanged -= TextileStateChanged;
}
