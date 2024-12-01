using Textile.Common;
using Textile.Data;
using Textile.Interfaces;

namespace TextileEditor.Shared.Painters.DataSelector;

internal readonly struct PedalSelector : ITextileSelector<TextileStructure, TextileIndex, bool, PedalSelector>, IDisposable
{
    public static ITextile<TextileIndex, bool> SelectTextileData(TextileStructure value) => value.Pedal;

    public static PedalSelector Subscribe(ITextileChangedWatcher<TextileIndex, bool> watcher, TextileStructure value) => new(watcher, value);

    public PedalSelector(ITextileChangedWatcher<TextileIndex, bool> watcher, TextileStructure structure)
    {
        this.watcher = watcher;
        this.structure = structure;
        structure.Pedal.TextileStateChanged += TextileStateChanged;
    }

    private void TextileStateChanged(IReadOnlyTextile<TextileIndex, bool> sender, TextileStateChangedEventArgs<TextileIndex, bool> eventArgs) => watcher.OnChanged(eventArgs.ChangedIndices);

    private readonly ITextileChangedWatcher<TextileIndex, bool> watcher;
    private readonly TextileStructure structure;


    public void Dispose() => structure.Pedal.TextileStateChanged -= TextileStateChanged;
}
