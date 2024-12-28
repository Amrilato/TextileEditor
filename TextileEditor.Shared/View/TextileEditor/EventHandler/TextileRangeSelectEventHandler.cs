using R3;
using SkiaSharp;
using Textile.Common;
using Textile.Data;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;
using TextileEditor.Shared.View.TextileEditor.Renderer;

namespace TextileEditor.Shared.View.TextileEditor.EventHandler;

public class TextileRangeSelectEventHandler : TextileEditorEventHandlerBase<TextileIndex, bool>, ISynchronizationReactiveTextileEditorViewRenderer<TextileIndex, bool>, IStatefulTextileEditorEventHandler<TextileIndex, bool>
{
    private ITextile<TextileIndex, bool>? Textile;

    private GridRange Range => new(First, Last);
    private GridIndex First;
    private GridIndex Last;
    private bool IsPointerDown = false;
    static TextileRangeSelectEventHandler() => SKPaint = new() { BlendMode = SKBlendMode.Src, StrokeWidth = 1, Style = SKPaintStyle.Stroke };
    [ThreadStatic]
    private static readonly SKPaint SKPaint;

    private readonly Subject<Unit> renderStateChanged = new();
    public Observable<Unit> RenderStateChanged => renderStateChanged;

    public IReadOnlyTextile<TextileIndex, bool>? Clip()
    {
        if (Textile is TextileBase data)
            return data.Clip(Range.ToTextileRange());
        else
            return null;
    }

    public override void OnPointerLeave(SKPoint point, ITextile<TextileIndex, bool> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) => IsPointerDown = false;
    public override void OnPointerDown(SKPoint point, ITextile<TextileIndex, bool> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure)
    {
        IsPointerDown = true;
        First = configure.GridSize.ToSettings(textileData).GetIndex(point).AsGridIndex();
        Last = First;
        Textile = textileData;
        renderStateChanged.OnNext(Unit.Default);
    }
    public override void OnPointerMove(SKPoint point, ITextile<TextileIndex, bool> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure)
    {
        if (IsPointerDown)
        {
            Last = configure.GridSize.ToSettings(textileData).GetIndex(point).AsGridIndex();
            Textile = textileData;
            renderStateChanged.OnNext(Unit.Default);
        }
    }
    public override void OnPointerUp(SKPoint point, ITextile<TextileIndex, bool> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) => IsPointerDown = false;

    public Progress Render(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TextileIndex, bool> textile, ITextileEditorViewConfigure configure, IProgress<Progress> progress, Progress currentProgress)
    {
        if (textile == Textile)
        {
            progress.Report(currentProgress = currentProgress with { Step = 0, MaxStep = 1 });
            SKPaint.Color = configure.AreaSelectBorderColor;
            surface.Canvas.DrawRect(configure.GridSize.ToSettings(textile).GetRangeRect(Range), SKPaint);
            progress.Report(currentProgress = currentProgress with { Step = 1, MaxStep = 1 });
        }
        return currentProgress;
    }

    public Progress UpdateDifferences(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TextileIndex, bool> textile, ReadOnlyMemory<ChangedValue<TextileIndex, bool>> changedValues, ITextileEditorViewConfigure configure, IProgress<Progress> progress, Progress currentProgress) => Render(surface, info, structure, textile, configure, progress, currentProgress);
}
