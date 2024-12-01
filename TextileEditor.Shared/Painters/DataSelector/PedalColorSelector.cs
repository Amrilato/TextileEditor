using Textile.Colors;
using Textile.Common;
using Textile.Data;
using Textile.Interfaces;

namespace TextileEditor.Shared.Painters.DataSelector;

internal readonly struct PedalColorSelector : ITextileSelector<TextileStructure, int, Color, PedalColorSelector>, IDisposable
{
    public static ITextile<int, Color> SelectTextileData(TextileStructure value) => value.PedalColor;

    public static PedalColorSelector Subscribe(ITextileChangedWatcher<int, Color> watcher, TextileStructure value) => new(watcher, value);

    public PedalColorSelector(ITextileChangedWatcher<int, Color> watcher, TextileStructure structure)
    {
        this.watcher = watcher;
        this.structure = structure;
        structure.PedalColor.TextileStateChanged += TextileStateChanged;
    }

    private void TextileStateChanged(IReadOnlyTextile<int, Color> sender, TextileStateChangedEventArgs<int, Color> eventArgs) => watcher.OnChanged(eventArgs.ChangedIndices);

    private readonly ITextileChangedWatcher<int, Color> watcher;
    private readonly TextileStructure structure;


    public void Dispose() => structure.PedalColor.TextileStateChanged -= TextileStateChanged;
}
