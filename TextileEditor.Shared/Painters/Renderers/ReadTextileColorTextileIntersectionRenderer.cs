using SkiaSharp;
using Textile.Common;
using Textile.Data;
using Textile.Interfaces;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Painters.Renderers;

public class ReadTextileColorTextileIntersectionRenderer(TextileStructure textileStructure) : TextileIntersectionRenderer<TextileIndex, bool, TextileStructure>
{
    public override void Receive(TextileStructure data) => textileStructure = data;

    protected override void RenderIntersection(SKSurface surface, IReadOnlyTextile<TextileIndex, bool> textile, GridSettings settings, TextileIndex index)
    {
        SKPaint.Color = (textile[index] ? textileStructure.HeddleColor[index.X] : textileStructure.PedalColor[index.Y]).AsSKColor();
        surface.Canvas.DrawRect(settings.GetCellOffset(index), SKPaint);
    }
}
