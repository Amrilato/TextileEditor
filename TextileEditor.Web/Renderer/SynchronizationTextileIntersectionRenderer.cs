using SkiaSharp;
using Textile.Colors;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;
using TextileEditor.Shared.View.TextileEditor;
using TextileEditor.Shared.View.TextileEditor.Renderer;

namespace TextileEditor.Web.Renderer;

public abstract class SynchronizationTextileIntersectionRenderer<TIndex, TValue> : ITextileEditorViewRenderer<TIndex, TValue>
{
    protected virtual int GetMaxStep(IReadOnlyTextile<TIndex, TValue> textile) => textile.TotalElement();
    protected virtual int GetMaxStep(IReadOnlyTextile<TIndex, TValue> textile, ReadOnlyMemory<ChangedValue<TIndex, TValue>> indices) => indices.Length;

    protected abstract void RenderIntersection(SKSurface surface, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ITextileEditorViewConfigure configure, TIndex index);

    public Task<Progress> RenderAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ITextileEditorViewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        return Task.FromResult(Render(surface, info, structure, textile, configure, token, progress, currentProgress));
    }

    public Progress Render(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ITextileEditorViewConfigure configure, CancellationToken token, IProgress<Progress> progress, Progress currentProgress)
    {
        var setting = configure.GridSize.ToSettings(textile);
        currentProgress = currentProgress with { Step = 0, MaxStep = GetMaxStep(textile) };
        foreach (var index in textile.Indices)
        {
            token.ThrowIfCancellationRequested();
            RenderIntersection(surface, structure, textile, configure, index);
            progress.Report(currentProgress = currentProgress with { Step = currentProgress.Step + 1 });
        }
        return currentProgress;
    }

    public Task<Progress> UpdateDifferencesAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ReadOnlyMemory<ChangedValue<TIndex, TValue>> changedValues, ITextileEditorViewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        return Task.FromResult(UpdateDifferences(surface, info, structure, textile, changedValues, configure, token, progress, currentProgress));
    }

    public Progress UpdateDifferences(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ReadOnlyMemory<ChangedValue<TIndex, TValue>> changedValues, ITextileEditorViewConfigure configure, CancellationToken token, IProgress<Progress> progress, Progress currentProgress)
    {
        var setting = configure.GridSize.ToSettings(textile);
        currentProgress = currentProgress with { Step = 0, MaxStep = GetMaxStep(textile) };
        for (int i = 0; i < changedValues.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            RenderIntersection(surface, structure, textile, configure, changedValues.Span[i].Index);
            progress.Report(currentProgress = currentProgress with { Step = currentProgress.Step + 1 });
        }
        return currentProgress;
    }

    static SynchronizationTextileIntersectionRenderer() => SKPaint ??= new() { BlendMode = SKBlendMode.Src };
    [ThreadStatic]
    protected readonly static SKPaint SKPaint;
}

public class SynchronizationReadTextileColorTextileIntersectionRenderer : SynchronizationTextileIntersectionRenderer<TextileIndex, bool>
{
    public static readonly SynchronizationReadTextileColorTextileIntersectionRenderer Instance = new();

    protected override void RenderIntersection(SKSurface surface, IReadOnlyTextileStructure structure, IReadOnlyTextile<TextileIndex, bool> textile, ITextileEditorViewConfigure configure, TextileIndex index)
    {
        SKPaint.Color = (textile[index] ? structure.HeddleColor[index.X] : structure.PedalColor[index.Y]).AsSKColor();
        surface.Canvas.DrawRect(configure.GridSize.ToSettings(textile).GetCellOffset(index), SKPaint);
    }
}
public class SynchronizationTextileColorIntersectionRenderer : SynchronizationTextileIntersectionRenderer<int, Color>
{
    public static readonly SynchronizationTextileColorIntersectionRenderer Instance = new();

    protected override void RenderIntersection(SKSurface surface, IReadOnlyTextileStructure structure, IReadOnlyTextile<int, Color> textile, ITextileEditorViewConfigure configure, int index)
    {
        SKPaint.Color = textile[index].AsSKColor();
        surface.Canvas.DrawRect(configure.GridSize.ToSettings(textile).GetCellOffset(textile.ToIndex(index, 0)), SKPaint);
    }
}
public class SynchronizationTextileDataIntersectionRenderer : SynchronizationTextileIntersectionRenderer<TextileIndex, bool>
{
    public static readonly SynchronizationTextileDataIntersectionRenderer Instance = new();

    protected override void RenderIntersection(SKSurface surface, IReadOnlyTextileStructure structure, IReadOnlyTextile<TextileIndex, bool> textile, ITextileEditorViewConfigure configure, TextileIndex index)
    {
        SKPaint.Color = textile[index] ? configure.IntersectionColor : SKColors.Transparent;
        surface.Canvas.DrawRect(configure.GridSize.ToSettings(textile).GetCellOffset(index), SKPaint);
    }
}
