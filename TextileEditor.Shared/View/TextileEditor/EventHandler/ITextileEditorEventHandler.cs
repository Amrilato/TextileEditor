using SkiaSharp;
using Textile.Interfaces;

namespace TextileEditor.Shared.View.TextileEditor.EventHandler;

public interface IStatefulTextileEditorEventHandler<TIndex, TValue> : ITextileEditorEventHandler<TIndex, TValue>;
public interface IStatelessTextileEditorEventHandler<TIndex, TValue> : ITextileEditorEventHandler<TIndex, TValue>;
public interface ITextileEditorEventHandler<TIndex, TValue>
{
    void OnClick(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure);
    void OnPointerMove(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure);
    void OnPointerEnter(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure);
    void OnPointerLeave(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure);
    void OnPointerDown(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure);
    void OnPointerUp(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure);

    static ITextileEditorEventHandler<TIndex, TValue> Empty => EmptyTextileEditorEventHandler<TIndex, TValue>.Instance;
}

file class EmptyTextileEditorEventHandler<TIndex, TValue> : ITextileEditorEventHandler<TIndex, TValue>
{
    public static EmptyTextileEditorEventHandler<TIndex, TValue> Instance = new();

    public void OnClick(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
    public void OnPointerDown(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
    public void OnPointerEnter(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
    public void OnPointerLeave(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
    public void OnPointerMove(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
    public void OnPointerUp(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
}
