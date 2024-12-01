using SkiaSharp;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Painters.Renderers;

public class TextileDataIntersectionRenderer(SKColor intersectionColor) : TextileIntersectionRenderer<TextileIndex, bool, SKColor>
{
    public override void Receive(SKColor data) => intersectionColor = data;

    protected override void RenderIntersection(SKSurface surface, IReadOnlyTextile<TextileIndex, bool> textile, GridSettings settings, TextileIndex index)
    {
        SKPaint.Color = textile[index] ? intersectionColor : SKColors.Transparent;
        surface.Canvas.DrawRect(settings.GetCellOffset(index), SKPaint);
    }
}
