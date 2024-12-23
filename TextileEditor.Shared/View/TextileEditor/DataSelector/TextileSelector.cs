using Textile.Common;
using Textile.Data;
using Textile.Interfaces;

namespace TextileEditor.Shared.View.TextileEditor.DataSelector;

internal readonly struct TextileSelector : ITextileSelector<TextileIndex, bool, TextileSelector>, IDisposable
{
    public IReadOnlyTextile<TextileIndex, bool> Textile => structure.Textile;
    public static ITextile<TextileIndex, bool> Select(TextileStructure structure) => structure.Textile;
    public static TextileSelector Subscribe(ITextileChangedWatcher<TextileIndex, bool> watcher, IReadOnlyTextileStructure value) => new(watcher, value);

    public TextileSelector(ITextileChangedWatcher<TextileIndex, bool> watcher, IReadOnlyTextileStructure structure)
    {
        this.watcher = watcher;
        this.structure = structure;
        structure.Textile.TextileStateChanged += TextileStateChanged;
        structure.HeddleColor.TextileStateChanged += HeddleColor_TextileStateChanged;
        structure.PedalColor.TextileStateChanged += PedalColor_TextileStateChanged;
    }

    private void PedalColor_TextileStateChanged(IReadOnlyTextile<int, Textile.Colors.Color> sender, TextileStateChangedEventArgs<int, Textile.Colors.Color> eventArgs)
    {
        int index = 0;
        Span<ChangedValue<TextileIndex, bool>> buffer = stackalloc ChangedValue<TextileIndex, bool>[eventArgs.ChangedIndices.Length * structure.Textile.Height];
        foreach (var item in eventArgs.ChangedIndices)
            for (int i = 0; i < structure.Textile.Height; i++)
                buffer[index++] = new(new(i, item.Index), structure.Textile[new(i, item.Index)], false);
        watcher.OnChanged(buffer);
    }

    private void HeddleColor_TextileStateChanged(IReadOnlyTextile<int, Textile.Colors.Color> sender, TextileStateChangedEventArgs<int, Textile.Colors.Color> eventArgs)
    {
        int index = 0;
        Span<ChangedValue<TextileIndex, bool>> buffer = stackalloc ChangedValue<TextileIndex, bool>[eventArgs.ChangedIndices.Length * structure.Textile.Width];
        foreach (var item in eventArgs.ChangedIndices)
            for (int i = 0; i < structure.Textile.Width; i++)
                buffer[index++] = new(new(item.Index, i), structure.Textile[new(item.Index, i)], false);
        watcher.OnChanged(buffer);
    }

    private void TextileStateChanged(IReadOnlyTextile<TextileIndex, bool> sender, TextileStateChangedEventArgs<TextileIndex, bool> eventArgs) => watcher.OnChanged(eventArgs.ChangedIndices);

    private readonly ITextileChangedWatcher<TextileIndex, bool> watcher;
    private readonly IReadOnlyTextileStructure structure;


    public void Dispose()
    {
        structure.Textile.TextileStateChanged -= TextileStateChanged;
        structure.HeddleColor.TextileStateChanged -= HeddleColor_TextileStateChanged;
        structure.PedalColor.TextileStateChanged -= PedalColor_TextileStateChanged;
    }
}
