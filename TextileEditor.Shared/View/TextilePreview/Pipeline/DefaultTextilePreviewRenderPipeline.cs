using SkiaSharp;
using Textile.Colors;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;
using TextileEditor.Shared.View.TextilePreview.Renderer;

namespace TextileEditor.Shared.View.TextilePreview.Pipeline;

public class DefaultTextilePreviewRenderPipelineProvider : ITextilePreviewRenderPipelineProvider
{
    public ITextilePreviewRenderPipeline Create() => DefaultTextilePreviewRenderPipeline.Instance;
}

file class DefaultTextilePreviewRenderPipeline : ITextilePreviewRenderPipeline
{
    public static readonly DefaultTextilePreviewRenderPipeline Instance = new();

    public int RenderAsyncPhase => 2;
    public int UpdateDifferencesAsyncPhase => 2;
    public int UpdateHeddleDifferencesAsyncPhase => 2;
    public int UpdatePedalDifferencesAsyncPhase => 2;

    public async Task<RenderProgress> RenderAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ITextilePreviewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token)
    {
        currentProgress = await TextilePreviewFragmentRenderer.Instance.RenderAsync(fragment, fragInfo, structure, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        currentProgress = await TextilePreviewRenderer.Instance.RenderAsync(destination, destinationInfo, fragment, fragInfo, structure, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        return currentProgress;
    }

    public async Task<RenderProgress> UpdateDifferencesAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<TextileIndex, bool>> changedValues, ITextilePreviewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token)
    {
        currentProgress = await TextilePreviewFragmentRenderer.Instance.UpdateDifferencesAsync(fragment, fragInfo, structure, changedValues, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        currentProgress = await TextilePreviewRenderer.Instance.RenderAsync(destination, destinationInfo, fragment, fragInfo, structure, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        return currentProgress;
    }

    public async Task<RenderProgress> UpdateHeddleDifferencesAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<int, Color>> changedValues, ITextilePreviewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token)
    {
        currentProgress = await TextilePreviewFragmentRenderer.Instance.UpdateHeddleDifferencesAsync(fragment, fragInfo, structure, changedValues, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        currentProgress = await TextilePreviewRenderer.Instance.RenderAsync(destination, destinationInfo, fragment, fragInfo, structure, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        return currentProgress;
    }

    public async Task<RenderProgress> UpdatePedalDifferencesAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<int, Color>> changedValues, ITextilePreviewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token)
    {
        currentProgress = await TextilePreviewFragmentRenderer.Instance.UpdatePedalDifferencesAsync(fragment, fragInfo, structure, changedValues, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        currentProgress = await TextilePreviewRenderer.Instance.RenderAsync(destination, destinationInfo, fragment, fragInfo, structure, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        return currentProgress;
    }
}