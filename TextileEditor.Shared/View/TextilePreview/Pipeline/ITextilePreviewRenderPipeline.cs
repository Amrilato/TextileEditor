using SkiaSharp;
using Textile.Colors;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;
using TextileEditor.Shared.View.TextilePreview.Renderer;

namespace TextileEditor.Shared.View.TextilePreview.Pipeline;

public interface ITextilePreviewRenderPipeline
{
    int RenderAsyncPhase { get; }
    int UpdateDifferencesAsyncPhase { get; }
    int UpdateHeddleDifferencesAsyncPhase { get; }
    int UpdatePedalDifferencesAsyncPhase { get; }
    Task<RenderProgress> RenderAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ITextilePreviewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token);
    Task<RenderProgress> UpdateDifferencesAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<TextileIndex, bool>> changedValues, ITextilePreviewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token);
    Task<RenderProgress> UpdateHeddleDifferencesAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<int, Color>> changedValues, ITextilePreviewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token);
    Task<RenderProgress> UpdatePedalDifferencesAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<int, Color>> changedValues, ITextilePreviewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token);
}

public interface ITextilePreviewRenderPipelineProvider
{
    ITextilePreviewRenderPipeline Create();
}

public class DefaultTextilePreviewRenderPipeline : ITextilePreviewRenderPipeline
{
    public int RenderAsyncPhase => throw new NotImplementedException();
    public int UpdateDifferencesAsyncPhase => throw new NotImplementedException();
    public int UpdateHeddleDifferencesAsyncPhase => throw new NotImplementedException();
    public int UpdatePedalDifferencesAsyncPhase => throw new NotImplementedException();

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