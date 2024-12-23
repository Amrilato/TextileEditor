using SkiaSharp;

namespace TextileEditor.Shared.View.Common;

public interface ICanvasEventHandler
{
    void OnClick(SKPoint point);
    void OnPointerMove(SKPoint point);
    void OnPointerEnter(SKPoint point);
    void OnPointerLeave(SKPoint point);
    void OnPointerDown(SKPoint point);
    void OnPointerUp(SKPoint point);
}
