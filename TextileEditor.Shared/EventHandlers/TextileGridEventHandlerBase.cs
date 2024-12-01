using SkiaSharp;
using Textile.Interfaces;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.EventHandlers;

public abstract class TextileGridEventHandlerBase<TIndex, TValue> : ITextileGridEventHandler<TIndex, TValue>
{
    public event Action? RequestSurface;
    protected void InvokeRequestSurface() => RequestSurface?.Invoke();
    public virtual void OnPaintSurface(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo, IReadOnlyTextile<TIndex, TValue> textileData, GridSize size) { }
    public virtual void OnClick(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size) { }
    public virtual void OnPointerDown(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size) { }
    public virtual void OnPointerEnter(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size) { }
    public virtual void OnPointerLeave(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size) { }
    public virtual void OnPointerMove(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size) { }
    public virtual void OnPointerUp(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size) { }
}
