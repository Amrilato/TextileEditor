using SkiaSharp;
using Textile.Interfaces;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Painters.Renderers;

public class TextileBorderRenderer(SKColor borderColor) : ITextileBorderRenderer<SKColor>
{
    public SKColor BorderColor { get; set; } = borderColor;
    public void Receive(SKColor data) => BorderColor = data;
    public int GetMaxStep(ITextileSize textile, GridSize gridSize) => textile.Width + textile.Height + 2;
    public virtual Task RenderAsync(SKSurface surface, IProgress<BackgroundTaskProgressDiff> progress, ITextileSize textile, GridSize gridSize, CancellationToken token)
    {
        SKPaint.Color = BorderColor;
        var settings = gridSize.ToSettings(textile);
        for (int column = 0; column < textile.Width + 1; column++)
        {
            token.ThrowIfCancellationRequested();
            float lineOffset = settings.ColumnBorderOffset(column);
            surface.Canvas.DrawLine(new(lineOffset, 0), new(lineOffset, settings.RowBorderOffset(settings.RowLength) + settings.BorderWidth), SKPaint);
            progress.Report(new(1));
        }

        for (int row = 0; row < textile.Height + 1; row++)
        {
            token.ThrowIfCancellationRequested();
            float lineOffset = settings.ColumnBorderOffset(row);
            surface.Canvas.DrawLine(new(0, lineOffset), new(settings.ColumnBorderOffset(settings.ColumnLength) + settings.BorderWidth, lineOffset), SKPaint);
            progress.Report(new(1));
        }

        return Task.CompletedTask;
    }
    static TextileBorderRenderer() => SKPaint = new() { BlendMode = SKBlendMode.Src };
    [ThreadStatic]
    protected readonly static SKPaint SKPaint;

    public ValueTask ColumnBorderRenderAsync(SKSurface surface, GridSettings settings, int index)
    {
        SKPaint.Color = BorderColor;
        float lineOffset = settings.ColumnBorderOffset(index);
        surface.Canvas.DrawLine(new(lineOffset, 0), new(lineOffset, settings.RowBorderOffset(settings.RowLength) + settings.BorderWidth), SKPaint);
        return ValueTask.CompletedTask;
    }

    public ValueTask RowBorderRenderAsync(SKSurface surface, GridSettings settings, int index)
    {
        SKPaint.Color = BorderColor;
        float lineOffset = settings.ColumnBorderOffset(index);
        surface.Canvas.DrawLine(new(0, lineOffset), new(settings.ColumnBorderOffset(settings.ColumnLength) + settings.BorderWidth, lineOffset), SKPaint);
        return ValueTask.CompletedTask;
    }
}