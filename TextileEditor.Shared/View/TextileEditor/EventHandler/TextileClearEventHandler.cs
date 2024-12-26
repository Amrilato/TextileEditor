using SkiaSharp;
using Textile.Common;
using Textile.Interfaces;

namespace TextileEditor.Shared.View.TextileEditor.EventHandler;

public class TextileClearEventHandler : TextileEditorEventHandlerBase<TextileIndex, bool>, IStatelessTextileEditorEventHandler<TextileIndex, bool>
{
    public override void OnClick(SKPoint point, ITextile<TextileIndex, bool> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) => textileData.Clear();
}
