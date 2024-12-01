using SkiaSharp;
using Textile.Interfaces;
using TextileEditor.Shared.Painters;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.EventHandlers;

public interface ITextileGridEventHandler<TIndex, TValue> : ITextileSKSurfacePainter<TIndex, TValue>
{
    void OnClick(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size);
    void OnPointerMove(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size);
    void OnPointerEnter(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size);
    void OnPointerLeave(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size);
    void OnPointerDown(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size);
    void OnPointerUp(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size);

    static ITextileGridEventHandler<TIndex, TValue> Empty => EmptyTextileGridEventHandler<TIndex, TValue>.Instance;
}

file class EmptyTextileGridEventHandler<TIndex, TValue> : ITextileGridEventHandler<TIndex, TValue>
{
    public static EmptyTextileGridEventHandler<TIndex, TValue> Instance = new();

    public event Action RequestSurface
    {
        add { }
        remove { }
    }
    public void OnPaintSurface(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo, IReadOnlyTextile<TIndex, TValue> textileData, GridSize size) { }
    public void OnClick(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size) { }
    public void OnPointerDown(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size) { }
    public void OnPointerEnter(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size) { }
    public void OnPointerLeave(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size) { }
    public void OnPointerMove(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size) { }
    public void OnPointerUp(SKPoint point, ITextile<TIndex, TValue> textileData, GridSize size) { }
}
