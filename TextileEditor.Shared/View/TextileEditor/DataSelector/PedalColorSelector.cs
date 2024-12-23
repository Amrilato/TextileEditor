using Textile.Colors;
using Textile.Common;
using Textile.Data;
using Textile.Interfaces;

namespace TextileEditor.Shared.View.TextileEditor.DataSelector;

internal readonly struct PedalColorSelector : ITextileSelector<int, Color, PedalColorSelector>, IDisposable
{
    public IReadOnlyTextile<int, Color> Textile=> structure.PedalColor;
    public static ITextile<int, Color> Select(TextileStructure structure) => structure.PedalColor;
    public static PedalColorSelector Subscribe(ITextileChangedWatcher<int, Color> watcher, IReadOnlyTextileStructure value) => new(watcher, value);

    public PedalColorSelector(ITextileChangedWatcher<int, Color> watcher, IReadOnlyTextileStructure structure)
    {
        this.watcher = watcher;
        this.structure = structure;
        structure.PedalColor.TextileStateChanged += TextileStateChanged;
    }

    private void TextileStateChanged(IReadOnlyTextile<int, Color> sender, TextileStateChangedEventArgs<int, Color> eventArgs) => watcher.OnChanged(eventArgs.ChangedIndices);

    private readonly ITextileChangedWatcher<int, Color> watcher;
    private readonly IReadOnlyTextileStructure structure;


    public void Dispose() => structure.PedalColor.TextileStateChanged -= TextileStateChanged;
}
