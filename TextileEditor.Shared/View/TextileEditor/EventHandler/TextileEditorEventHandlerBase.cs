using SkiaSharp;
using Textile.Interfaces;

namespace TextileEditor.Shared.View.TextileEditor.EventHandler;

public abstract class TextileEditorEventHandlerBase<TIndex, TValue>
{
    public virtual void OnClick(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
    public virtual void OnPointerMove(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
    public virtual void OnPointerEnter(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
    public virtual void OnPointerLeave(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
    public virtual void OnPointerDown(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
    public virtual void OnPointerUp(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
}
