using SkiaSharp;
using Textile.Common;
using Textile.Data;
using Textile.Interfaces;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.EventHandlers;

public class TextilePasteEventHandler : TextileGridEventHandlerBase<TextileIndex, bool>
{
    public SKColor FillColor { get; set; }
    public IReadOnlyTextile<TextileIndex, bool>? Textile
    {
        get => textile;
        set
        {
            textile = value;
            InvokeRequestSurface();
        }
    }
    private TextileIndex TextileIndex = new(-1, -1);
    private IReadOnlyTextile<TextileIndex, bool>? RenderTarget;
    private IReadOnlyTextile<TextileIndex, bool>? textile;

    static TextilePasteEventHandler() => SKPaint = new() { BlendMode = SKBlendMode.Src };
    [ThreadStatic]
    private static readonly SKPaint SKPaint;
    private bool UpdateTextileIndex(TextileIndex index)
    {
        if (TextileIndex == index)
            return false;
        TextileIndex = index;
        return true;
    }

    public override void OnPaintSurface(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo, IReadOnlyTextile<TextileIndex, bool> textileData, GridSize size)
    {
        if (Textile is not null && RenderTarget == textileData && TextileIndex != new TextileIndex(-1, -1))
        {
            var setting = size.ToSettings(textileData);
            SKPaint.Color = FillColor;
            foreach (var index in Textile.Indices)
                if (Textile[index])
                    surface.Canvas.DrawRect(setting.GetCellOffset((TextileIndex + index).AsGridIndex()), SKPaint);
        }
    }

    public override void OnPointerLeave(SKPoint point, ITextile<TextileIndex, bool> textileData, GridSize size)
    {
        RenderTarget = null;
        InvokeRequestSurface();
    }

    public override void OnPointerDown(SKPoint point, ITextile<TextileIndex, bool> textileData, GridSize size)
    {
        if(Textile is not null && TextileIndex != new TextileIndex(-1, -1) && textileData is TextileData data)
            data.CopyFrom(Textile, destinationOffset: TextileIndex);
    }

    public override void OnPointerMove(SKPoint point, ITextile<TextileIndex, bool> textileData, GridSize size)
    {
        RenderTarget = textileData;
        if (UpdateTextileIndex(size.ToSettings(textileData).GetIndex(point)) && Textile is not null)
            InvokeRequestSurface();
    }
}
