﻿using SkiaSharp;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;

namespace TextileEditor.Shared.View.TextileEditor.Renderer;

public class TextileBorderRenderer<TIndex, TValue> : ITextileEditorViewRenderer<TIndex, TValue>
{
    public static readonly TextileBorderRenderer<TIndex, TValue> Instance = new();

    private int GetMaxStep(ITextileSize textile) => textile.Width + textile.Height + 2;
    static TextileBorderRenderer() => SKPaint = new() { BlendMode = SKBlendMode.Src };
    [ThreadStatic]
    protected readonly static SKPaint SKPaint;

    public async Task<Progress> RenderAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ITextileEditorViewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token) => await Task.Run(() => Render(surface, info, structure, textile, configure, token, progress, currentProgress));
    public Progress Render(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ITextileEditorViewConfigure configure, CancellationToken token, IProgress<Progress> progress, Progress currentProgress)
    {
        SKPaint.Color = configure.BorderColor;
        currentProgress = currentProgress with { MaxStep = GetMaxStep(textile) };
        int step = 0;
        var settings = configure.GridSize.ToSettings(textile);
        for (int column = 0; column < textile.Width + 1; column++)
        {
            token.ThrowIfCancellationRequested();
            float lineOffset = settings.ColumnBorderOffset(column);
            surface.Canvas.DrawLine(new(lineOffset, 0), new(lineOffset, settings.RowBorderOffset(settings.RowLength) + settings.BorderWidth), SKPaint);
            progress.Report(currentProgress with { Step = step++ });
        }

        for (int row = 0; row < textile.Height + 1; row++)
        {
            token.ThrowIfCancellationRequested();
            float lineOffset = settings.ColumnBorderOffset(row);
            surface.Canvas.DrawLine(new(0, lineOffset), new(settings.ColumnBorderOffset(settings.ColumnLength) + settings.BorderWidth, lineOffset), SKPaint);
            progress.Report(currentProgress with { Step = step++ });
        }
        return currentProgress with { Step = step };
    }

    public Task<Progress> UpdateDifferencesAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ReadOnlyMemory<ChangedValue<TIndex, TValue>> changedValues, ITextileEditorViewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token) => RenderAsync(surface, info, structure, textile, configure, progress, currentProgress, token);
}