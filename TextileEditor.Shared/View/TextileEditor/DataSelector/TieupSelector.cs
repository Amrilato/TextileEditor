using Textile.Common;
using Textile.Data;
using Textile.Interfaces;

namespace TextileEditor.Shared.View.TextileEditor.DataSelector;

internal readonly struct TieupSelector : ITextileSelector<TextileIndex, bool, TieupSelector>, IDisposable
{
    public IReadOnlyTextile<TextileIndex, bool> Textile => structure.Tieup;
    public static ITextile<TextileIndex, bool> Select(TextileStructure structure) => structure.Tieup;
    public static TieupSelector Subscribe(ITextileChangedWatcher<TextileIndex, bool> watcher, IReadOnlyTextileStructure value) => new(watcher, value);

    public TieupSelector(ITextileChangedWatcher<TextileIndex, bool> watcher, IReadOnlyTextileStructure structure)
    {
        this.watcher = watcher;
        this.structure = structure;
        structure.Tieup.TextileStateChanged += TextileStateChanged;
    }

    private void TextileStateChanged(IReadOnlyTextile<TextileIndex, bool> sender, TextileStateChangedEventArgs<TextileIndex, bool> eventArgs) => watcher.OnChanged(eventArgs.ChangedIndices);

    private readonly ITextileChangedWatcher<TextileIndex, bool> watcher;
    private readonly IReadOnlyTextileStructure structure;


    public void Dispose() => structure.Tieup.TextileStateChanged -= TextileStateChanged;
}
