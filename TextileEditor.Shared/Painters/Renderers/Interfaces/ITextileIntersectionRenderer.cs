using SkiaSharp;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Painters.Renderers;

public interface ITextileIntersectionRenderer<TIndex, TValue, TData> : IReceiver<TData>
{
    int GetMaxStep(IReadOnlyTextile<TIndex, TValue> textile, GridSize gridSize, IEnumerable<TIndex> indices);
    int GetMaxStep(IReadOnlyTextile<TIndex, TValue> textile, GridSize gridSize, ReadOnlyMemory<ChangedValue<TIndex, TValue>> indices);
    Task RenderAsync(SKSurface surface, IProgress<BackgroundTaskProgressDiff> progress, IReadOnlyTextile<TIndex, TValue> textile, GridSize gridSize, IEnumerable<TIndex> indices, CancellationToken token);
    Task RenderAsync(SKSurface surface, IProgress<BackgroundTaskProgressDiff> progress, IReadOnlyTextile<TIndex, TValue> textile, GridSize gridSize, ReadOnlyMemory<ChangedValue<TIndex, TValue>> indices, CancellationToken token);
}
