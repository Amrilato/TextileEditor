using SkiaSharp;
using Textile.Colors;
using Textile.Common;
using Textile.Data;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;

namespace TextileEditor.Shared.View.TextileEditor.Renderer;

public abstract class TextileIntersectionRenderer<TIndex, TValue> : ITextileEditorViewRenderer<TIndex, TValue>
{
    protected virtual int GetMaxStep(IReadOnlyTextile<TIndex, TValue> textile) => textile.TotalElement();
    protected virtual int GetMaxStep(IReadOnlyTextile<TIndex, TValue> textile, ReadOnlyMemory<ChangedValue<TIndex, TValue>> indices) => indices.Length;

    protected abstract void RenderIntersection(SKSurface surface, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ITextileEditorViewConfigure configure, TIndex index);

    public async Task<RenderProgress> RenderAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ITextileEditorViewConfigure configure, CancellationToken token, IProgress<RenderProgress> progress, RenderProgress currentProgress) => await Task.Run(() => Render(surface, info, structure, textile, configure, token, progress, currentProgress));
    public RenderProgress Render(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ITextileEditorViewConfigure configure, CancellationToken token, IProgress<RenderProgress> progress, RenderProgress currentProgress)
    {
        var setting = configure.GridSize.ToSettings(textile);
        currentProgress = currentProgress with { MaxStep = GetMaxStep(textile), Status = RenderProgressStates.Rendering };
        int step = 0;
        foreach (var index in textile.Indices)
        {
            token.ThrowIfCancellationRequested();
            RenderIntersection(surface, structure, textile, configure, index);
            progress.Report(currentProgress with { Step = step++ });
        }
        return currentProgress with { Step = step };
    }

    public async Task<RenderProgress> UpdateDifferencesAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ReadOnlyMemory<ChangedValue<TIndex, TValue>> changedValues, ITextileEditorViewConfigure configure, CancellationToken token, IProgress<RenderProgress> progress, RenderProgress currentProgress) => await Task.Run(() => UpdateDifferences(surface, info, structure, textile, changedValues, configure, token, progress, currentProgress));
    public RenderProgress UpdateDifferences(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ReadOnlyMemory<ChangedValue<TIndex, TValue>> changedValues, ITextileEditorViewConfigure configure, CancellationToken token, IProgress<RenderProgress> progress, RenderProgress currentProgress)
    {
        var setting = configure.GridSize.ToSettings(textile);
        currentProgress = currentProgress with { MaxStep = GetMaxStep(textile), Status = RenderProgressStates.Rendering };
        int step = 0;
        for (int i = 0; i < changedValues.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            RenderIntersection(surface, structure, textile, configure, changedValues.Span[i].Index);
            progress.Report(currentProgress with { Step = step++ });
        }
        return currentProgress with { Step = step };
    }

    static TextileIntersectionRenderer() => SKPaint = new() { BlendMode = SKBlendMode.Src };
    [ThreadStatic]
    protected readonly static SKPaint SKPaint;
}

public class ReadTextileColorTextileIntersectionRenderer : TextileIntersectionRenderer<TextileIndex, bool>
{
    public static readonly ReadTextileColorTextileIntersectionRenderer Instance = new();

    protected override void RenderIntersection(SKSurface surface, IReadOnlyTextileStructure structure, IReadOnlyTextile<TextileIndex, bool> textile, ITextileEditorViewConfigure configure, TextileIndex index)
    {
        SKPaint.Color = (textile[index] ? structure.HeddleColor[index.X] : structure.PedalColor[index.Y]).AsSKColor();
        surface.Canvas.DrawRect(configure.GridSize.ToSettings(textile).GetCellOffset(index), SKPaint);
    }
}
public class TextileColorIntersectionRenderer : TextileIntersectionRenderer<int, Color>
{
    public static readonly TextileColorIntersectionRenderer Instance = new();

    protected override void RenderIntersection(SKSurface surface, IReadOnlyTextileStructure structure, IReadOnlyTextile<int, Color> textile, ITextileEditorViewConfigure configure, int index)
    {
        SKPaint.Color = textile[index].AsSKColor();
        surface.Canvas.DrawRect(configure.GridSize.ToSettings(textile).GetCellOffset(textile.ToIndex(index, 0)), SKPaint);
    }
}
public class TextileDataIntersectionRenderer : TextileIntersectionRenderer<TextileIndex, bool>
{
    public static readonly TextileDataIntersectionRenderer Instance = new();
 
    protected override void RenderIntersection(SKSurface surface, IReadOnlyTextileStructure structure, IReadOnlyTextile<TextileIndex, bool> textile, ITextileEditorViewConfigure configure, TextileIndex index)
    {
        SKPaint.Color = textile[index] ? configure.IntersectionColor : SKColors.Transparent;
        surface.Canvas.DrawRect(configure.GridSize.ToSettings(textile).GetCellOffset(index), SKPaint);
    }
}
