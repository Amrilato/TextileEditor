using SkiaSharp;
using Textile.Colors;
using Textile.Interfaces;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Painters.Renderers;

public class TextileColorIntersectionRenderer(IReadOnlyTextileColor textileColor) : TextileIntersectionRenderer<int, Color, IReadOnlyTextileColor>
{
    public override void Receive(IReadOnlyTextileColor data) => textileColor = data;

    protected override void RenderIntersection(SKSurface surface, IReadOnlyTextile<int, Color> textile, GridSettings settings, int index)
    {
        SKPaint.Color = textile[index].AsSKColor();
        surface.Canvas.DrawRect(settings.GetCellOffset(textileColor.ToIndex(index, 0)), SKPaint);
    }
}