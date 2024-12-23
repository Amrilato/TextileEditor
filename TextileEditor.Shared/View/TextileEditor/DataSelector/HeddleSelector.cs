using Textile.Common;
using Textile.Data;
using Textile.Interfaces;

namespace TextileEditor.Shared.View.TextileEditor.DataSelector;

internal readonly struct HeddleSelector : ITextileSelector<TextileIndex, bool, HeddleSelector>, IDisposable
{
    public IReadOnlyTextile<TextileIndex, bool> Textile => structure.Heddle;
    public static ITextile<TextileIndex, bool> Select(TextileStructure structure) => structure.Heddle;
    public static HeddleSelector Subscribe(ITextileChangedWatcher<TextileIndex, bool> watcher, IReadOnlyTextileStructure value) => new(watcher, value);

    public HeddleSelector(ITextileChangedWatcher<TextileIndex, bool> watcher, IReadOnlyTextileStructure structure)
    {
        this.watcher = watcher;
        this.structure = structure;
        structure.Heddle.TextileStateChanged += TextileStateChanged;
    }

    private void TextileStateChanged(IReadOnlyTextile<TextileIndex, bool> sender, TextileStateChangedEventArgs<TextileIndex, bool> eventArgs) => watcher.OnChanged(eventArgs.ChangedIndices);

    private readonly ITextileChangedWatcher<TextileIndex, bool> watcher;
    private readonly IReadOnlyTextileStructure structure;


    public void Dispose() => structure.Heddle.TextileStateChanged -= TextileStateChanged;
}