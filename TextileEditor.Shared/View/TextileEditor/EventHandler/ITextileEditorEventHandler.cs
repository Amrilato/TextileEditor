using SkiaSharp;
using Textile.Interfaces;

namespace TextileEditor.Shared.View.TextileEditor.EventHandler;

public interface ITextileEditorEventHandler<TIndex, TValue>
{
    void OnClick(SKPoint point, ITextile<TIndex, TValue> textileData, TextileEditorViewContext context);
    void OnPointerMove(SKPoint point, ITextile<TIndex, TValue> textileData, TextileEditorViewContext context);
    void OnPointerEnter(SKPoint point, ITextile<TIndex, TValue> textileData, TextileEditorViewContext context);
    void OnPointerLeave(SKPoint point, ITextile<TIndex, TValue> textileData, TextileEditorViewContext context);
    void OnPointerDown(SKPoint point, ITextile<TIndex, TValue> textileData, TextileEditorViewContext context);
    void OnPointerUp(SKPoint point, ITextile<TIndex, TValue> textileData, TextileEditorViewContext context);

    static ITextileEditorEventHandler<TIndex, TValue> Empty => EmptyTextileEditorEventHandler<TIndex, TValue>.Instance;
}

file class EmptyTextileEditorEventHandler<TIndex, TValue> : ITextileEditorEventHandler<TIndex, TValue>
{
    public static EmptyTextileEditorEventHandler<TIndex, TValue> Instance = new();

    public void OnClick(SKPoint point, ITextile<TIndex, TValue> textileData, TextileEditorViewContext context) { }
    public void OnPointerDown(SKPoint point, ITextile<TIndex, TValue> textileData, TextileEditorViewContext context) { }
    public void OnPointerEnter(SKPoint point, ITextile<TIndex, TValue> textileData, TextileEditorViewContext context) { }
    public void OnPointerLeave(SKPoint point, ITextile<TIndex, TValue> textileData, TextileEditorViewContext context) { }
    public void OnPointerMove(SKPoint point, ITextile<TIndex, TValue> textileData, TextileEditorViewContext context) { }
    public void OnPointerUp(SKPoint point, ITextile<TIndex, TValue> textileData, TextileEditorViewContext context) { }
}
