using Textile.Common;
using Textile.Data;
using Textile.Interfaces;

namespace TextileEditor.Shared.View.TextileEditor.DataSelector;

internal readonly struct PedalSelector : ITextileSelector<TextileIndex, bool, PedalSelector>, IDisposable
{
    public IReadOnlyTextile<TextileIndex, bool> Textile => structure.Pedal;
    public static ITextile<TextileIndex, bool> Select(TextileStructure structure) => structure.Pedal;
    public static PedalSelector Subscribe(ITextileChangedWatcher<TextileIndex, bool> watcher, IReadOnlyTextileStructure value) => new(watcher, value);

    public PedalSelector(ITextileChangedWatcher<TextileIndex, bool> watcher, IReadOnlyTextileStructure structure)
    {
        this.watcher = watcher;
        this.structure = structure;
        structure.Pedal.TextileStateChanged += TextileStateChanged;
    }

    private void TextileStateChanged(IReadOnlyTextile<TextileIndex, bool> sender, TextileStateChangedEventArgs<TextileIndex, bool> eventArgs) => watcher.OnChanged(eventArgs.ChangedIndices);

    private readonly ITextileChangedWatcher<TextileIndex, bool> watcher;
    private readonly IReadOnlyTextileStructure structure;


    public void Dispose() => structure.Pedal.TextileStateChanged -= TextileStateChanged;
}
