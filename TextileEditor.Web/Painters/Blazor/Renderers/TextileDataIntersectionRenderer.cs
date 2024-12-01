using SkiaSharp;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.Painters.Renderers;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Shared.Common;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Painters.Blazor.Renderers;

public class BlazorTextileDataIntersectionRenderer(IBlazorTextileEnvironmentConfigure configure, SKColor intersectionColor) : TextileDataIntersectionRenderer(intersectionColor)
{
    public override async Task RenderAsync(SKSurface surface, IProgress<BackgroundTaskProgressDiff> progress, IReadOnlyTextile<TextileIndex, bool> textile, GridSize gridSize, IEnumerable<TextileIndex> indices, CancellationToken token)
    {
        if (GetMaxStep(textile, gridSize, indices) < configure.Threshold)
            await base.RenderAsync(surface, progress, textile, gridSize, indices, token);
        else
        {
            var chunkSize = configure.ChunkSize;
            var chunkStep = 0;

            var setting = gridSize.ToSettings(textile);
            foreach (var index in indices)
            {
                token.ThrowIfCancellationRequested();
                RenderIntersection(surface, textile, setting, index);

                chunkStep++;
                if (chunkStep % chunkSize == 0)
                    await Task.Delay(1, token).ConfigureAwait(false);
                progress.Report(new(1));
            }
        }
    }

    public override async Task RenderAsync(SKSurface surface, IProgress<BackgroundTaskProgressDiff> progress, IReadOnlyTextile<TextileIndex, bool> textile, GridSize gridSize, ReadOnlyMemory<ChangedValue<TextileIndex, bool>> indices, CancellationToken token)
    {
        if (GetMaxStep(textile, gridSize, indices) < configure.Threshold)
            await base.RenderAsync(surface, progress, textile, gridSize, indices, token);
        else
        {
            var chunkSize = configure.ChunkSize;

            var setting = gridSize.ToSettings(textile);
            for (int i = 0; i < indices.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                RenderIntersection(surface, textile, setting, indices.Span[i].Index);
                if (i % chunkSize == 0)
                    await Task.Delay(1, token).ConfigureAwait(false);
                progress.Report(new(1));
            }
        }
    }
}
