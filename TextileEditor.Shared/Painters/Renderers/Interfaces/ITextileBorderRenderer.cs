using SkiaSharp;
using Textile.Interfaces;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Painters.Renderers;

public interface ITextileBorderRenderer<TData> : IReceiver<TData>
{
    int GetMaxStep(ITextileSize textile, GridSize gridSize);
    Task RenderAsync(SKSurface surface, IProgress<BackgroundTaskProgressDiff> progress, ITextileSize textile, GridSize gridSize, CancellationToken token);
}
