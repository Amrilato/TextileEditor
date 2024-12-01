using SkiaSharp;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.EventHandlers;

public class TextileClickEventHandler : TextileGridEventHandlerBase<TextileIndex, bool>
{
    private TextileIndex TextileIndex = new(-1, -1);
    private bool IsPointerDown = false;
    private bool IsPointerMoved = false;
    private bool UpdateTextileIndex(TextileIndex index)
    {
        if (TextileIndex == index)
            return false;
        TextileIndex = index;
        return true;
    }

    public override void OnPointerLeave(SKPoint point, ITextile<TextileIndex, bool> textileData, GridSize size)
    {
        IsPointerDown = false;
        IsPointerMoved = false;
        TextileIndex = new(-1, -1);
    }

    public override void OnPointerDown(SKPoint point, ITextile<TextileIndex, bool> textileData, GridSize size)
    {
        IsPointerDown = true;
    }

    public override void OnPointerMove(SKPoint point, ITextile<TextileIndex, bool> textileData, GridSize size)
    {
        if (IsPointerDown)
        {
            IsPointerMoved = true;
            if (UpdateTextileIndex(size.ToSettings(textileData).GetIndex(point)))
                textileData[TextileIndex] = !textileData[TextileIndex];
        }
    }
    public override void OnPointerUp(SKPoint point, ITextile<TextileIndex, bool> textileData, GridSize size)
    {
        if (IsPointerDown)
        {
            if (!IsPointerMoved)
            {
                UpdateTextileIndex(size.ToSettings(textileData).GetIndex(point));
                textileData[TextileIndex] = !textileData[TextileIndex];
            }
            else
                IsPointerMoved = false;
            IsPointerDown = false;
        }
    }
}
