using SkiaSharp;
using Textile.Colors;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;

namespace TextileEditor.Shared.View.TextilePreview.Renderer;

public class TextilePreviewFragmentRenderer : ITextilePreviewFragmentRenderer
{
    public readonly static TextilePreviewFragmentRenderer Instance = new();
    static TextilePreviewFragmentRenderer() => SKPaint = new() { BlendMode = SKBlendMode.Src };
    [ThreadStatic]
    private readonly static SKPaint SKPaint;
    private static void RenderIntersection(SKSurface surface, IReadOnlyTextileStructure structure, ITextilePreviewConfigure configure, TextileIndex index)
    {
        SKPaint.Color = (structure.Textile[index] ? structure.HeddleColor[index.X] : structure.PedalColor[index.Y]).AsSKColor();
        surface.Canvas.DrawRect(new GridSettings(0, structure.Textile.Width, structure.Textile.Height, configure.PixelSize.Width, configure.PixelSize.Height).GetCellOffset(index), SKPaint);
    }
    private static Progress RenderHorizontal(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, int lineIndex, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        int step = currentProgress.Step;
        for (int x = 0; x < structure.Textile.Width; x++)
        {
            token.ThrowIfCancellationRequested();
            RenderIntersection(surface, structure, configure, new TextileIndex(x, lineIndex));
            progress.Report(currentProgress with { Step = step++ });
        }
        return currentProgress with { Step = step };
    }
    private static Progress RenderVertical(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, int lineIndex, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        int step = currentProgress.Step;
        for (int y = 0; y < structure.Textile.Height; y++)
        {
            token.ThrowIfCancellationRequested();
            RenderIntersection(surface, structure, configure, new TextileIndex(lineIndex, y));
            progress.Report(currentProgress with { Step = step++ });
        }
        return currentProgress with { Step = step };
    }

    private static Progress Render(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        currentProgress = currentProgress with { Step = 0, MaxStep = structure.Textile.Width * structure.Textile.Height };
        for (int x = 0; x < structure.Textile.Width; x++)
        {
            currentProgress = RenderVertical(surface, info, structure, x, configure, progress, currentProgress, token);
        }
        return currentProgress;
    }
    public Task<Progress> RenderAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token) => Task.Run(() => Render(surface, info, structure, configure, progress, currentProgress, token));

    private static Progress UpdateDifference(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<TextileIndex, bool>> changedValues, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        int step = 0;
        currentProgress = currentProgress with { Step = 0, MaxStep = changedValues.Length };
        ReadOnlySpan<ChangedValue<TextileIndex, bool>> values = changedValues.Span;
        for (int i = 0; i < values.Length; i++)
        {
            RenderIntersection(surface, structure, configure, values[i].Index);
            progress.Report(currentProgress with { Step = step++ });
        }
        return currentProgress with { Step = step };
    }
    public Task<Progress> UpdateDifferencesAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<TextileIndex, bool>> changedValues, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token) => Task.Run(() => UpdateDifference(surface, info, structure, changedValues, configure, progress, currentProgress, token));

    public static Progress UpdateHeddleDifferences(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<int, Color>> changedValues, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        currentProgress = currentProgress with { Step = 0, MaxStep = changedValues.Length * structure.PedalColor.Height };
        ReadOnlySpan<ChangedValue<int, Color>> values = changedValues.Span;
        for (int i = 0; i < values.Length; i++)
        {
            currentProgress = RenderVertical(surface, info, structure, i, configure, progress, currentProgress, token);
        }
        return currentProgress;
    }
    public Task<Progress> UpdateHeddleDifferencesAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<int, Color>> changedValues, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token) => Task.Run(() => UpdateHeddleDifferencesAsync(surface, info, structure, changedValues, configure, progress, currentProgress, token));

    public static Progress UpdatePedalDifferences(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<int, Color>> changedValues, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        currentProgress = currentProgress with { Step = 0, MaxStep = changedValues.Length * structure.HeddleColor.Width };
        ReadOnlySpan<ChangedValue<int, Color>> values = changedValues.Span;
        for (int i = 0; i < values.Length; i++)
        {
            currentProgress = RenderHorizontal(surface, info, structure, i, configure, progress, currentProgress, token);
        }
        return currentProgress;
    }
    public Task<Progress> UpdatePedalDifferencesAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, ReadOnlyMemory<ChangedValue<int, Color>> changedValues, ITextilePreviewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token) => Task.Run(() => UpdatePedalDifferences(surface, info, structure, changedValues, configure, progress, currentProgress, token));
}
