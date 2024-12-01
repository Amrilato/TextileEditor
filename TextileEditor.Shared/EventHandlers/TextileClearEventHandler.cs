using SkiaSharp;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.EventHandlers;

public class TextileClearEventHandler : TextileGridEventHandlerBase<TextileIndex, bool>
{
    public override void OnClick(SKPoint point, ITextile<TextileIndex, bool> textileData, GridSize size) => textileData.Clear();
}
