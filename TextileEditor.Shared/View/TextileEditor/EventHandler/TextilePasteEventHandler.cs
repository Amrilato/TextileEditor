using R3;
using SkiaSharp;
using Textile.Common;
using Textile.Data;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;
using TextileEditor.Shared.View.TextileEditor.Renderer;

namespace TextileEditor.Shared.View.TextileEditor.EventHandler;

public class TextilePasteEventHandler : TextileEditorEventHandlerBase<TextileIndex, bool>, ISynchronizationReactiveTextileEditorViewRenderer<TextileIndex, bool>, IStatefulTextileEditorEventHandler<TextileIndex, bool>
{
    public IReadOnlyTextile<TextileIndex, bool>? Clipboard { get; set; }

    private readonly Subject<Unit> renderStateChanged = new();
    public Observable<Unit> RenderStateChanged => renderStateChanged;

    private TextileIndex ReferencePoint = new(-1, -1);
    private ITextile<TextileIndex, bool>? RenderTarget;
    private void InvokeRequestSurface() => renderStateChanged.OnNext(Unit.Default);


    static TextilePasteEventHandler() => SKPaint = new() { BlendMode = SKBlendMode.Src };
    [ThreadStatic]
    private static readonly SKPaint SKPaint;
    private bool UpdateTextileIndex(TextileIndex index)
    {
        if (ReferencePoint == index)
            return false;
        ReferencePoint = index;
        return true;
    }
    public override void OnPointerLeave(SKPoint point, ITextile<TextileIndex, bool> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure)
    {
        RenderTarget = null;
        InvokeRequestSurface();
    }

    public override void OnPointerDown(SKPoint point, ITextile<TextileIndex, bool> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure)
    {
        if (Clipboard is not null && ReferencePoint != new TextileIndex(-1, -1) && textileData is TextileBase data)
            data.CopyFrom(Clipboard, destinationOffset: ReferencePoint);
    }
    public override void OnPointerMove(SKPoint point, ITextile<TextileIndex, bool> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure)
    {
        RenderTarget = textileData;
        if (UpdateTextileIndex(configure.GridSize.ToSettings(textileData).GetIndex(point)) && Clipboard is not null)
            InvokeRequestSurface();
    }

    public RenderProgress Render(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TextileIndex, bool> textile, ITextileEditorViewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress)
    {
        if (Clipboard is not null && ReferencePoint != new TextileIndex(-1, -1) && RenderTarget == textile)
        {
            currentProgress = currentProgress with { Step = 0, MaxStep = Clipboard.TotalElement() };
            var setting = configure.GridSize.ToSettings(textile);
            SKPaint.Color = configure.PastPreviewIntersectionColor;
            foreach (var index in Clipboard.Indices)
            {
                progress.Report(currentProgress = currentProgress with { Step = currentProgress.Step + 1 });
                if (Clipboard[index])
                    surface.Canvas.DrawRect(setting.GetCellOffset((ReferencePoint + index).AsGridIndex()), SKPaint);
            }
        }
        return currentProgress;
    }

    public RenderProgress UpdateDifferences(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TextileIndex, bool> textile, ReadOnlyMemory<ChangedValue<TextileIndex, bool>> changedValues, ITextileEditorViewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress) => Render(surface, info, structure, textile, configure, progress, currentProgress);
}
