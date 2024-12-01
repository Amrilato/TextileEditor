using SkiaSharp;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Painters.Renderers;

public abstract class TextileIntersectionRenderer<TIndex, TValue, TData> : ITextileIntersectionRenderer<TIndex, TValue, TData>
{
    public virtual int GetMaxStep(IReadOnlyTextile<TIndex, TValue> textile, GridSize gridSize, IEnumerable<TIndex> indices) => textile.TotalElement();
    public virtual int GetMaxStep(IReadOnlyTextile<TIndex, TValue> textile, GridSize gridSize, ReadOnlyMemory<ChangedValue<TIndex, TValue>> indices) => indices.Length;
    public abstract void Receive(TData data);

    public virtual Task RenderAsync(SKSurface surface, IProgress<BackgroundTaskProgressDiff> progress, IReadOnlyTextile<TIndex, TValue> textile, GridSize gridSize, IEnumerable<TIndex> indices, CancellationToken token)
    {
        var setting = gridSize.ToSettings(textile);
        foreach (var index in indices)
        {
            token.ThrowIfCancellationRequested();
            RenderIntersection(surface, textile, setting, index);
            progress.Report(new(1));
        }
        return Task.CompletedTask;
    }

    public virtual Task RenderAsync(SKSurface surface, IProgress<BackgroundTaskProgressDiff> progress, IReadOnlyTextile<TIndex, TValue> textile, GridSize gridSize, ReadOnlyMemory<ChangedValue<TIndex, TValue>> indices, CancellationToken token)
    {
        var setting = gridSize.ToSettings(textile);
        for (int i = 0; i < indices.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            RenderIntersection(surface, textile, setting, indices.Span[i].Index);
            progress.Report(new(1));
        }
        return Task.CompletedTask;
    }

    protected abstract void RenderIntersection(SKSurface surface, IReadOnlyTextile<TIndex, TValue> textile, GridSettings settings, TIndex index);

    static TextileIntersectionRenderer() => SKPaint = new() { BlendMode = SKBlendMode.Src };
    [ThreadStatic]
    protected readonly static SKPaint SKPaint;
}
