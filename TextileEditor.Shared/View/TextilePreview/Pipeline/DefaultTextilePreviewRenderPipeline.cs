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

    public async Task<Progress> RenderAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        progress.Report(currentProgress = currentProgress with { MaxPhase = currentProgress.MaxPhase + 2 });
        currentProgress = await TextilePreviewFragmentRenderer.Instance.RenderAsync(fragment, fragInfo, structure, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        currentProgress = await TextilePreviewRenderer.Instance.RenderAsync(destination, destinationInfo, fragment, fragInfo, structure, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        return currentProgress;
    }

    public async Task<Progress> UpdateDifferencesAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<TextileIndex, bool>> changedValues, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        progress.Report(currentProgress = currentProgress with { MaxPhase = currentProgress.MaxPhase + 2 });
        currentProgress = await TextilePreviewFragmentRenderer.Instance.UpdateDifferencesAsync(fragment, fragInfo, structure, changedValues, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        currentProgress = await TextilePreviewRenderer.Instance.RenderAsync(destination, destinationInfo, fragment, fragInfo, structure, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        return currentProgress;
    }

    public async Task<Progress> UpdateHeddleDifferencesAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<int, Color>> changedValues, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        progress.Report(currentProgress = currentProgress with { MaxPhase = currentProgress.MaxPhase + 2 });
        currentProgress = await TextilePreviewFragmentRenderer.Instance.UpdateHeddleDifferencesAsync(fragment, fragInfo, structure, changedValues, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        currentProgress = await TextilePreviewRenderer.Instance.RenderAsync(destination, destinationInfo, fragment, fragInfo, structure, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        return currentProgress;
    }

    public async Task<Progress> UpdatePedalDifferencesAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<int, Color>> changedValues, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        progress.Report(currentProgress = currentProgress with { MaxPhase = currentProgress.MaxPhase + 2 });
        currentProgress = await TextilePreviewFragmentRenderer.Instance.UpdatePedalDifferencesAsync(fragment, fragInfo, structure, changedValues, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        currentProgress = await TextilePreviewRenderer.Instance.RenderAsync(destination, destinationInfo, fragment, fragInfo, structure, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        return currentProgress;
    }
}