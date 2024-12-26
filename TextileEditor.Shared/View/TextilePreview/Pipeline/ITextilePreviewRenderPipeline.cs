using SkiaSharp;
using Textile.Colors;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;

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
