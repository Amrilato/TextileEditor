using SkiaSharp;
using Textile.Common;
using Textile.Data;
using Textile.Interfaces;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.EventHandlers;

public class TextileRangeSelectEventHandler : TextileGridEventHandlerBase<TextileIndex, bool>
{
    private ITextile<TextileIndex, bool>? Textile;

    private GridRange Range => new(First, Last);
    private GridIndex First;
    private GridIndex Last;
    private bool IsPointerDown = false;
    static TextileRangeSelectEventHandler() => SKPaint = new() { BlendMode = SKBlendMode.Src, StrokeWidth = 1, Style = SKPaintStyle.Stroke };
    [ThreadStatic]
    private static readonly SKPaint SKPaint;

    private SKColor borderColor;
    public SKColor BorderColor
    {
        get => borderColor;
        set
        {
            if (borderColor == value)
                return;
            borderColor = value;
            if (Textile is not null)
                InvokeRequestSurface();
        }
    }
    public IReadOnlyTextile<TextileIndex, bool>? Clip()
    {
        if (Textile is TextileData data)
            return data.Clip(Range.ToTextileRange());
        else
            return null;
    }

    public override void OnPaintSurface(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo, IReadOnlyTextile<TextileIndex, bool> textileData, GridSize size)
    {
        if (textileData == Textile)
        {
            SKPaint.Color = BorderColor;
            surface.Canvas.DrawRect(size.ToSettings(textileData).GetRangeRect(Range), SKPaint);
        }
    }

    public override void OnPointerLeave(SKPoint point, ITextile<TextileIndex, bool> textileData, GridSize size) => IsPointerDown = false;
    public override void OnPointerDown(SKPoint point, ITextile<TextileIndex, bool> textileData, GridSize size)
    {
        IsPointerDown = true;
        First = size.ToSettings(textileData).GetIndex(point).AsGridIndex();
        Last = First;
        Textile = textileData;
        InvokeRequestSurface();
    }

    public override void OnPointerMove(SKPoint point, ITextile<TextileIndex, bool> textileData, GridSize size)
    {
        if (IsPointerDown)
        {
            Last = size.ToSettings(textileData).GetIndex(point).AsGridIndex();
            Textile = textileData;
            InvokeRequestSurface();
        }
    }
    public override void OnPointerUp(SKPoint point, ITextile<TextileIndex, bool> textileData, GridSize size) => IsPointerDown = false;
}
