﻿using SkiaSharp;
using Textile.Colors;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;
using TextileEditor.Shared.View.TextileEditor.Renderer;

namespace TextileEditor.Shared.View.TextileEditor.Pipeline;

public class DefaultTextileEditorRendererPipelineProvider : ITextileEditorViewRenderPipelineProvider
{
    public ITextileEditorViewRenderPipeline<TextileIndex, bool> CreateHeddle() => TextileSettingRenderPipeline.Instance;
    public ITextileEditorViewRenderPipeline<int, Color> CreateHeddleColor() => TextileColorRenderPipeline.Instance;
    public ITextileEditorViewRenderPipeline<TextileIndex, bool> CreatePedal() => TextileSettingRenderPipeline.Instance;
    public ITextileEditorViewRenderPipeline<int, Color> CreatePedalColor() => TextileColorRenderPipeline.Instance;
    public ITextileEditorViewRenderPipeline<TextileIndex, bool> CreateTextile() => TextileRenderPipeline.Instance;
    public ITextileEditorViewRenderPipeline<TextileIndex, bool> CreateTieup() => TextileSettingRenderPipeline.Instance;
}

file class TextileRenderPipeline : ITextileEditorViewRenderPipeline<TextileIndex, bool>
{
    public static readonly TextileRenderPipeline Instance = new();

    public int RenderAsyncPhase => 2;
    public int UpdateDifferencesAsyncPhase => 1;

    public async Task<Progress> RenderAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TextileIndex, bool> textile, ITextileEditorViewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        progress.Report(currentProgress = currentProgress with { MaxPhase = currentProgress.MaxPhase + 2 });
        currentProgress = await TextileBorderRenderer<TextileIndex, bool>.Instance.RenderAsync(surface, info, structure, textile, configure, progress, currentProgress, token).ConfigureAwait(false);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        currentProgress = await ReadTextileColorTextileIntersectionRenderer.Instance.RenderAsync(surface, info, structure, textile, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        return currentProgress;
    }
    public async Task<Progress> UpdateDifferencesAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TextileIndex, bool> textile, ReadOnlyMemory<ChangedValue<TextileIndex, bool>> changedValues, ITextileEditorViewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        progress.Report(currentProgress = currentProgress with { MaxPhase = currentProgress.MaxPhase + 1 });
        currentProgress = await ReadTextileColorTextileIntersectionRenderer.Instance.UpdateDifferencesAsync(surface, info, structure, textile, changedValues, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        return currentProgress;
    }
}

file class TextileSettingRenderPipeline : ITextileEditorViewRenderPipeline<TextileIndex, bool>
{
    public static readonly TextileSettingRenderPipeline Instance = new();

    public int RenderAsyncPhase => 2;
    public int UpdateDifferencesAsyncPhase => 1;
    
    public async Task<Progress> RenderAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TextileIndex, bool> textile, ITextileEditorViewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        currentProgress = await TextileBorderRenderer<TextileIndex, bool>.Instance.RenderAsync(surface, info, structure, textile, configure, progress, currentProgress, token).ConfigureAwait(false);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        currentProgress = await TextileDataIntersectionRenderer.Instance.RenderAsync(surface, info, structure, textile, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        return currentProgress;
    }

    public async Task<Progress> UpdateDifferencesAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TextileIndex, bool> textile, ReadOnlyMemory<ChangedValue<TextileIndex, bool>> changedValues, ITextileEditorViewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        currentProgress = await TextileDataIntersectionRenderer.Instance.UpdateDifferencesAsync(surface, info, structure, textile, changedValues, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        return currentProgress;
    }
}

file class TextileColorRenderPipeline : ITextileEditorViewRenderPipeline<int, Color>
{
    public static readonly TextileColorRenderPipeline Instance = new();

    public int RenderAsyncPhase => 2;
    public int UpdateDifferencesAsyncPhase => 1;

    public async Task<Progress> RenderAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<int, Color> textile, ITextileEditorViewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        currentProgress = await TextileBorderRenderer<int, Color>.Instance.RenderAsync(surface, info, structure, textile, configure, progress, currentProgress, token).ConfigureAwait(false);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        currentProgress = await TextileColorIntersectionRenderer.Instance.RenderAsync(surface, info, structure, textile, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        return currentProgress;
    }

    public async Task<Progress> UpdateDifferencesAsync(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<int, Color> textile, ReadOnlyMemory<ChangedValue<int, Color>> changedValues, ITextileEditorViewConfigure configure, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
    {
        currentProgress = await TextileColorIntersectionRenderer.Instance.UpdateDifferencesAsync(surface, info, structure, textile, changedValues, configure, progress, currentProgress, token);
        progress.Report(currentProgress = currentProgress with { Phase = currentProgress.Phase + 1 });
        return currentProgress;
    }
}
