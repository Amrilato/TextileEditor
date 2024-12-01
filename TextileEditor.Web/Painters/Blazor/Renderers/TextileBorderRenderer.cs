using SkiaSharp;
using Textile.Interfaces;
using TextileEditor.Shared.Painters.Renderers;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Shared.Common;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Painters.Blazor.Renderers;

public class BlazorTextileBorderRenderer(IBlazorTextileEnvironmentConfigure configure, SKColor borderColor) : TextileBorderRenderer(borderColor)
{
    public override async Task RenderAsync(SKSurface surface, IProgress<BackgroundTaskProgressDiff> progress, ITextileSize textile, GridSize gridSize, CancellationToken token)
    {
        if (GetMaxStep(textile, gridSize) < configure.Threshold)
            await base.RenderAsync(surface, progress, textile, gridSize, token);
        else
        {
            var chunkSize = configure.ChunkSize;
            var chunkStep = 0;

            SKPaint.Color = BorderColor;
            var settings = gridSize.ToSettings(textile);
            for (int column = 0; column < textile.Width + 1; column++)
            {
                token.ThrowIfCancellationRequested();
                float lineOffset = settings.ColumnBorderOffset(column);
                surface.Canvas.DrawLine(new(lineOffset, 0), new(lineOffset, settings.RowBorderOffset(settings.RowLength) + settings.BorderWidth), SKPaint);

                chunkStep++;
                if (chunkStep % chunkSize == 0)
                    await Task.Delay(1, token).ConfigureAwait(false);
                progress.Report(new(1));
            }

            for (int row = 0; row < textile.Height + 1; row++)
            {
                token.ThrowIfCancellationRequested();
                float lineOffset = settings.ColumnBorderOffset(row);
                surface.Canvas.DrawLine(new(0, lineOffset), new(settings.ColumnBorderOffset(settings.ColumnLength) + settings.BorderWidth, lineOffset), SKPaint);

                chunkStep++;
                if (chunkStep % chunkSize == 0)
                    await Task.Delay(1, token).ConfigureAwait(false);
                progress.Report(new(1));
            }

        }
    }
}