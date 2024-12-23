using R3;
using SkiaSharp;
using System.Diagnostics.CodeAnalysis;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;
using TextileEditor.Shared.View.TextileEditor.DataSelector;
using TextileEditor.Shared.View.TextileEditor.Pipeline;

namespace TextileEditor.Shared.View.TextileEditor;

internal class TextileEditorViewPainter<TIndex, TValue, TSelector> : IPainter, ITextileChangedWatcher<TIndex, TValue>, IProgress<RenderProgress>, IDisposable
    where TSelector : ITextileSelector<TIndex, TValue, TSelector>, IDisposable
{
    private readonly Lock _lockObj = new();
    private Task renderTask = Task.FromException(new TaskCanceledException());
    private readonly Queue<ChangedValue<TIndex, TValue>[]> ChangedValueQueue = new();
    private readonly ManagedMemorySKSurface surfacePainter = new();
    private CancellationTokenSource cancellationTokenSource = new();
    private readonly TSelector selector;
    private readonly IReadOnlyTextileStructure structure;
    private readonly ITextileEditorViewRenderPipeline<TIndex, TValue> pipeline;
    private readonly ITextileEditorViewConfigure configure;
    private readonly ReactiveProperty<RenderProgress> renderProgress;
    public ReadOnlyReactiveProperty<RenderProgress> RenderProgress => renderProgress;
    public SKSizeI CanvasSize => configure.GridSize.ToSettings(selector.Textile).CanvasSize();

    public TextileEditorViewPainter(IReadOnlyTextileStructure structure, ITextileEditorViewRenderPipeline<TIndex, TValue> pipeline, ITextileEditorViewConfigure configure)
    {
        selector = TSelector.Subscribe(this, structure);
        this.structure = structure;
        this.pipeline = pipeline;
        this.configure = configure;
        renderProgress = new();
        renderProgress.OnNext(new(0, 0, 0, 0, RenderProgressStates.NotStarted));
    }

    public async Task OnPaintSurfaceAsync(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo, CancellationToken token)
    {
        if (!info.Equals(surfacePainter.SKImageInfo))
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token); 
            try
            {
                await renderTask;
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception)
            {
                throw;
            }
            surfacePainter.ChangeImageInfo(info);
            var task = RenderAsync(token);
            using (_lockObj.EnterScope())
            {
                renderTask = task;
            }
        }
        await renderTask;
        if (token.IsCancellationRequested)
        {
            renderProgress.OnNext(new(0, 0, 0, 0, RenderProgressStates.Canceled));
            return;
        }
        using SKPaint sKPaint = new();
        using SKSurface source = surfacePainter.CreateSurface(info);
        surface.Draw(source.Canvas, 0, 0, sKPaint);
    }

    public Task RenderAsync() => RenderAsync(cancellationTokenSource.Token);
    private async Task RenderAsync(CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return;
        try
        {
            renderProgress.OnNext(new(0, 0, 0, 0, RenderProgressStates.Initializing));
            using SKSurface sKSurface = surfacePainter.CreateSurface(out var info);
            var progress = await pipeline.RenderAsync(sKSurface, info, structure, selector.Textile, configure, token, this, new(0, pipeline.RenderAsyncPhase, 0, 0, RenderProgressStates.Initializing)).ConfigureAwait(false);
            if (TryDequeue(out var values))
                progress = await UpdateDifferencesAsync(values, progress, token);
            else
                renderProgress.RenderCompleted(progress);
        }
        catch (OperationCanceledException)
        {
            renderProgress.OnNext(new(0, 0, 0, 0, RenderProgressStates.Canceled));
        }
        catch (Exception e)
        {
            renderProgress.OnNext(new(0, 0, 0, 0, RenderProgressStates.Failed));
            renderProgress.OnErrorResume(e);
        }
    }

    private async Task UpdateDifferencesAsync(ReadOnlyMemory<ChangedValue<TIndex, TValue>> changedValues, CancellationToken token)
    {
        try
        {
            renderProgress.OnNext(new(0, 0, 0, 0, RenderProgressStates.Initializing));
            renderProgress.RenderCompleted(await UpdateDifferencesAsync(changedValues, new(0, 0, 0, 0, RenderProgressStates.Initializing), token).ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            renderProgress.OnNext(new(0, 0, 0, 0, RenderProgressStates.Canceled));
        }
        catch (Exception e)
        {
            renderProgress.OnNext(new(0, 0, 0, 0, RenderProgressStates.Failed));
            renderProgress.OnErrorResume(e);
        }
    }

    private async Task<RenderProgress> UpdateDifferencesAsync(ReadOnlyMemory<ChangedValue<TIndex, TValue>> changedValues, RenderProgress progress, CancellationToken token)
    {
        using SKSurface sKSurface = surfacePainter.CreateSurface(out var info);
        progress = await pipeline.UpdateDifferencesAsync(sKSurface, info, structure, selector.Textile, changedValues, configure, token, this, progress with { MaxPhase = progress.MaxPhase + pipeline.UpdateDifferencesAsyncPhase }).ConfigureAwait(false);
        if (TryDequeue(out var values))
            return await UpdateDifferencesAsync(values, progress, token);
        else
            return progress;
    }

    private bool TryDequeue([NotNullWhen(true)] out ChangedValue<TIndex, TValue>[]? values)
    {
        using (_lockObj.EnterScope())
        {
            return ChangedValueQueue.TryDequeue(out values);
        }
    }

    public void Report(RenderProgress value) => renderProgress.OnNext(value);
    public void OnChanged(ReadOnlySpan<ChangedValue<TIndex, TValue>> changedValues)
    {
        var buffer = new ChangedValue<TIndex, TValue>[changedValues.Length];
        changedValues.CopyTo(buffer);
        using (_lockObj.EnterScope())
        {
            if (renderTask.IsCompletedSuccessfully)
                renderTask = UpdateDifferencesAsync(buffer, cancellationTokenSource.Token);
            else if (renderTask.IsCompleted)
                renderTask = RenderAsync(cancellationTokenSource.Token);
            else
                ChangedValueQueue.Enqueue(buffer);
        }
    }

    public void Dispose()
    {
        renderProgress.Dispose();
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
        selector.Dispose();
        surfacePainter.Dispose();
    }
}
