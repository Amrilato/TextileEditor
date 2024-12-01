using SkiaSharp;
using Textile.Colors;
using Textile.Interfaces;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.EventHandlers;

public class TextileColorClickEventHandler : TextileGridEventHandlerBase<int, Color>
{
    private Color _color;
    public SKColor Color
    {
        get => _color.AsSKColor();
        set => _color = value.AsColor();
    }

    private int Index = -1;
    private bool IsPointerDown = false;
    private bool UpdateTextileIndex(int index)
    {
        if (Index == index)
            return false;
        Index = index;
        return true;
    }

    private void SetColor(SKPoint point, ITextile<int, Color> textileData, GridSize size)
    {
        if (textileData is IReadOnlyTextileColor color && UpdateTextileIndex(color.ToIndex(size.ToSettings(textileData).GetIndex(point))))
            textileData[Index] = _color;
    }

    public override void OnClick(SKPoint point, ITextile<int, Color> textileData, GridSize size)
    {
        SetColor(point, textileData, size);
    }

    public override void OnPointerLeave(SKPoint point, ITextile<int, Color> textileData, GridSize size)
    {
        IsPointerDown = false;
        Index = -1;
    }

    public override void OnPointerDown(SKPoint point, ITextile<int, Color> textileData, GridSize size)
    {
        IsPointerDown = true;
    }

    public override void OnPointerMove(SKPoint point, ITextile<int, Color> textileData, GridSize size)
    {
        if (IsPointerDown)
            SetColor(point, textileData, size);
    }
    public override void OnPointerUp(SKPoint point, ITextile<int, Color> textileData, GridSize size)
    {
        if (IsPointerDown)
            SetColor(point, textileData, size);
        IsPointerDown = false;
    }
}
