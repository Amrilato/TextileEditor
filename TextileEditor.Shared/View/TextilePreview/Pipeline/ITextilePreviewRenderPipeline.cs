using SkiaSharp;
using Textile.Colors;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;

namespace TextileEditor.Shared.View.TextilePreview.Pipeline;

public interface ITextilePreviewRenderPipeline
{
    Task<Progress> RenderAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token);
    Task<Progress> UpdateDifferencesAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<TextileIndex, bool>> changedValues, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token);
    Task<Progress> UpdateHeddleDifferencesAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<int, Color>> changedValues, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token);
    Task<Progress> UpdatePedalDifferencesAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<int, Color>> changedValues, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token);
}
