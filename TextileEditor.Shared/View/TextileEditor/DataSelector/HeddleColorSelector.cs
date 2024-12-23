using Textile.Colors;
using Textile.Common;
using Textile.Data;
using Textile.Interfaces;

namespace TextileEditor.Shared.View.TextileEditor.DataSelector;

internal readonly struct HeddleColorSelector : ITextileSelector<int, Color, HeddleColorSelector>, IDisposable
{
    public IReadOnlyTextile<int, Color> Textile => structure.HeddleColor;
    public static ITextile<int, Color> Select(TextileStructure structure) => structure.HeddleColor;
    public static HeddleColorSelector Subscribe(ITextileChangedWatcher<int, Color> watcher, IReadOnlyTextileStructure value) => new(watcher, value);

    public HeddleColorSelector(ITextileChangedWatcher<int, Color> watcher, IReadOnlyTextileStructure structure)
    {
        this.watcher = watcher;
        this.structure = structure;
        structure.HeddleColor.TextileStateChanged += TextileStateChanged;
    }

    private void TextileStateChanged(IReadOnlyTextile<int, Color> sender, TextileStateChangedEventArgs<int, Color> eventArgs) => watcher.OnChanged(eventArgs.ChangedIndices);

    private readonly ITextileChangedWatcher<int, Color> watcher;
    private readonly IReadOnlyTextileStructure structure;


    public void Dispose() => structure.HeddleColor.TextileStateChanged -= TextileStateChanged;
}
