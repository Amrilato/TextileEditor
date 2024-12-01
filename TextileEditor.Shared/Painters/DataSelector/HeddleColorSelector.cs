using Textile.Colors;
using Textile.Common;
using Textile.Data;
using Textile.Interfaces;

namespace TextileEditor.Shared.Painters.DataSelector;

internal readonly struct HeddleColorSelector : ITextileSelector<TextileStructure, int, Color, HeddleColorSelector>, IDisposable
{
    public static ITextile<int, Color> SelectTextileData(TextileStructure value) => value.HeddleColor;

    public static HeddleColorSelector Subscribe(ITextileChangedWatcher<int, Color> watcher, TextileStructure value) => new(watcher, value);

    public HeddleColorSelector(ITextileChangedWatcher<int, Color> watcher, TextileStructure structure)
    {
        this.watcher = watcher;
        this.structure = structure;
        structure.HeddleColor.TextileStateChanged += TextileStateChanged;
    }

    private void TextileStateChanged(IReadOnlyTextile<int, Color> sender, TextileStateChangedEventArgs<int, Color> eventArgs) => watcher.OnChanged(eventArgs.ChangedIndices);

    private readonly ITextileChangedWatcher<int, Color> watcher;
    private readonly TextileStructure structure;


    public void Dispose() => structure.HeddleColor.TextileStateChanged -= TextileStateChanged;
}
