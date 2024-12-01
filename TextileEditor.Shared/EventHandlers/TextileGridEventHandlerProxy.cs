using SkiaSharp;
using Textile.Interfaces;
using TextileEditor.Shared.Painters;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.EventHandlers;

internal class TextileGridEventHandlerProxy<TIndex, TValue>(ITextileGridEventHandler<TIndex, TValue> eventHandler, ITextile<TIndex, TValue> textileData, GridSize gridSize) : ICanvasEventHandler, ISKSurfacePainter, IDisposable
{
    public ITextile<TIndex, TValue> TextileData { get; set; } = textileData;
    public GridSize GridSize { get; set; } = gridSize;
    public ITextileGridEventHandler<TIndex, TValue> EventHandler
    {
        get => eventHandler;
        set
        {
            if (eventHandler == value)
                return;
            eventHandler = value;
            eventHandler.RequestSurface -= InvokeRequestSurface;
            value.RequestSurface += InvokeRequestSurface;
            InvokeRequestSurface();
        }
    }

    public event Action? RequestSurface;
    private void InvokeRequestSurface() => RequestSurface?.Invoke();
    public void OnPaintSurface(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo) => EventHandler.OnPaintSurface(surface, info, rawInfo, TextileData, GridSize);

    public void OnClick(SKPoint point) => EventHandler?.OnClick(point, TextileData, GridSize);
    public void OnPointerDown(SKPoint point) => EventHandler?.OnPointerDown(point, TextileData, GridSize);
    public void OnPointerEnter(SKPoint point) => EventHandler?.OnPointerEnter(point, TextileData, GridSize);
    public void OnPointerLeave(SKPoint point) => EventHandler?.OnPointerLeave(point, TextileData, GridSize);
    public void OnPointerMove(SKPoint point) => EventHandler?.OnPointerMove(point, TextileData, GridSize);
    public void OnPointerUp(SKPoint point) => EventHandler?.OnPointerUp(point, TextileData, GridSize);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        eventHandler.RequestSurface -= InvokeRequestSurface;
    }
}