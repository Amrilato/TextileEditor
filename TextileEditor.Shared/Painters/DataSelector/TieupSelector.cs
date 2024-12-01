using Textile.Common;
using Textile.Data;
using Textile.Interfaces;

namespace TextileEditor.Shared.Painters.DataSelector;

internal readonly struct TieupSelector : ITextileSelector<TextileStructure, TextileIndex, bool, TieupSelector>, IDisposable
{
    public static ITextile<TextileIndex, bool> SelectTextileData(TextileStructure value) => value.Tieup;

    public static TieupSelector Subscribe(ITextileChangedWatcher<TextileIndex, bool> watcher, TextileStructure value) => new(watcher, value);

    public TieupSelector(ITextileChangedWatcher<TextileIndex, bool> watcher, TextileStructure structure)
    {
        this.watcher = watcher;
        this.structure = structure;
        structure.Tieup.TextileStateChanged += TextileStateChanged;
    }

    private void TextileStateChanged(IReadOnlyTextile<TextileIndex, bool> sender, TextileStateChangedEventArgs<TextileIndex, bool> eventArgs) => watcher.OnChanged(eventArgs.ChangedIndices);

    private readonly ITextileChangedWatcher<TextileIndex, bool> watcher;
    private readonly TextileStructure structure;


    public void Dispose() => structure.Tieup.TextileStateChanged -= TextileStateChanged;
}
