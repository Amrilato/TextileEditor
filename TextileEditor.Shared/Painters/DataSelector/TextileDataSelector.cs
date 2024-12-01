using Textile.Common;
using Textile.Data;
using Textile.Interfaces;

namespace TextileEditor.Shared.Painters.DataSelector;

internal readonly struct TextileDataSelector : ITextileSelector<TextileData, TextileIndex, bool, TextileDataSelector>, IDisposable
{
    public static ITextile<TextileIndex, bool> SelectTextileData(TextileData value) => value;

    public static TextileDataSelector Subscribe(ITextileChangedWatcher<TextileIndex, bool> watcher, TextileData value) => new(watcher, value);

    public TextileDataSelector(ITextileChangedWatcher<TextileIndex, bool> watcher, TextileData textileData)
    {
        this.watcher = watcher;
        this.textileData = textileData;
        textileData.TextileStateChanged += TextileStateChanged;
    }
    private void TextileStateChanged(IReadOnlyTextile<TextileIndex, bool> sender, TextileStateChangedEventArgs<TextileIndex, bool> eventArgs) => watcher.OnChanged(eventArgs.ChangedIndices);

    private readonly ITextileChangedWatcher<TextileIndex, bool> watcher;
    private readonly TextileData textileData;

    public void Dispose() => textileData.TextileStateChanged -= TextileStateChanged;
}