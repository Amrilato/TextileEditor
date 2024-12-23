using SkiaSharp;
using Textile.Colors;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;

namespace TextileEditor.Shared.View.TextilePreview.Renderer;

public interface ITextilePreviewFragmentRenderer
{
    Task<RenderProgress> RenderAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, ITextilePreviewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token);
    Task<RenderProgress> UpdateDifferencesAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<TextileIndex, bool>> changedValues, ITextilePreviewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token);
    Task<RenderProgress> UpdateHeddleDifferencesAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<int, Color>> changedValues, ITextilePreviewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token);
    Task<RenderProgress> UpdatePedalDifferencesAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<int, Color>> changedValues, ITextilePreviewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token);
}

public interface ITextilePreviewRenderer
{
    Task<RenderProgress> RenderAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ITextilePreviewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token);
}

public class TextilePreviewRenderer : ITextilePreviewRenderer
{
    public readonly static TextilePreviewRenderer Instance = new();
    private static RenderProgress Render(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ITextilePreviewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token)
    {
        progress.Report(currentProgress = currentProgress with { Step = 0, MaxStep = 1 });
        using var snapshot = fragment.Snapshot();
        using var bitmap = SKBitmap.FromImage(snapshot);
        using var shader = SKShader.CreateBitmap(bitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
        using var paint = new SKPaint { Shader = shader };
        destination.Canvas.DrawRect(destination.Canvas.LocalClipBounds, paint);
        progress.Report(currentProgress = currentProgress with { Step = 1 });
        return currentProgress;
    }
    public Task<RenderProgress> RenderAsync(SKSurface destination, SKImageInfo destinationInfo, SKSurface fragment, SKImageInfo fragInfo, IReadOnlyTextileStructure structure, ITextilePreviewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress, CancellationToken token) => Task.Run(() => Render(destination, destinationInfo, fragment, fragInfo, structure, configure, progress, currentProgress, token));
}
