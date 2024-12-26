using R3;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;
using TextileEditor.Shared.View.TextileEditor.DataSelector;
using TextileEditor.Shared.View.TextileEditor.Pipeline;

namespace TextileEditor.Shared.View.TextileEditor;

internal class TextileEditorViewPainter<TIndex, TValue, TSelector> : Painter, ITextileChangedWatcher<TIndex, TValue>, IProgress<RenderProgress>, IDisposable
    where TSelector : ITextileSelector<TIndex, TValue, TSelector>, IDisposable
{
    protected readonly Lock renderTaskLock = new();
    private Task renderTask = Task.FromException(new TaskCanceledException());
    private readonly ConcurrentQueue<ChangedValue<TIndex, TValue>[]> ChangedValueQueue = new();
    protected readonly IManagedMemorySKSurface surfacePainter = ManagedMemorySKSurface.Create();
    protected CancellationTokenSource cancellationTokenSource;
    private bool disposedValue;
    private readonly TSelector selector;
    private readonly IReadOnlyTextileStructure structure;
    private readonly ITextileEditorViewRenderPipeline<TIndex, TValue> pipeline;
    protected readonly ITextileEditorViewConfigure configure;
    private readonly ReactiveProperty<RenderProgress> renderProgress;
    public override ReadOnlyReactiveProperty<RenderProgress> RenderProgress => renderProgress;

    public override SKSizeI CanvasSize => configure.GridSize.ToSettings(selector.Textile).CanvasSize();
    public TextileEditorViewPainter(IReadOnlyTextileStructure structure, ITextileEditorViewRenderPipeline<TIndex, TValue> pipeline, ITextileEditorViewConfigure configure)
    {
        selector = TSelector.Subscribe(this, structure);
        this.structure = structure;
        this.pipeline = pipeline;
        this.configure = configure;
        cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        renderProgress = new();
        renderProgress.OnNext(new(0, 0, 0, 0, RenderProgressStates.NotStarted));
    }

    protected override void Initialize(SKImageInfo info, CancellationToken token)
    {
        if (!info.Equals(surfacePainter.SKImageInfo))
        {
            surfacePainter.ChangeImageInfo(info);
            using (renderTaskLock.EnterScope())
            {
                renderTask = RenderAsync(cancellationTokenSource.Token);
            }
        }
        else
        {
            using (renderTaskLock.EnterScope())
            {
                if (renderTask.IsFaulted || renderTask.IsCanceled)
                    renderTask = RenderAsync(token);
            }
        }
    }

    protected override void Paint(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo)
    {
        using SKPaint sKPaint = new();
        using var source = surfacePainter.CreateSurface(info);
        surface.Draw(source.SKSurface.Canvas, 0, 0, sKPaint);
        Report(new() { Status = RenderProgressStates.Completed });
    }
    private async Task CancelCurrentRenderTask()
    {
        using (renderTaskLock.EnterScope())
        {
            if (renderTask.IsCompletedSuccessfully || renderTask.IsFaulted)
                return;
        }

        cancellationTokenSource.Cancel();
        try
        {
            await renderTask;
        }
        catch (TaskCanceledException) { }
    }

    private async Task RenderAsync(CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return;

        try
        {
            await CancelCurrentRenderTask().ConfigureAwait(false);
            using (progressLock.EnterScope())
            {
                renderProgress.Initializing();
            }
            using var sKSurfaceOwner = surfacePainter.CreateSurface(out var info);
            var progress = await pipeline.RenderAsync(sKSurfaceOwner.SKSurface, info, structure, selector.Textile, configure, this, new(0, pipeline.RenderAsyncPhase, 0, 0, RenderProgressStates.Initializing), token);
            if (ChangedValueQueue.TryDequeue(out var values))
                progress = await UpdateDifferencesAsync(values, progress, token);
            using (progressLock.EnterScope())
            {
                renderProgress.InitializingCompleted(progress);
            }
        }
        catch (TaskCanceledException)
        {
            Report(new() { Status = RenderProgressStates.Canceled });
        }
        catch (Exception e)
        {
            using (progressLock.EnterScope())
            {
                renderProgress.OnNext(new(0, 0, 0, 0, RenderProgressStates.Failed));
                renderProgress.OnErrorResume(e);
            }
        }
    }

    private async Task UpdateDifferencesAsync(ReadOnlyMemory<ChangedValue<TIndex, TValue>> changedValues, CancellationToken token)
    {
        try
        {
            using (progressLock.EnterScope())
            {
                renderProgress.Initializing();
            }
            var progress = await UpdateDifferencesAsync(changedValues, new(0, 0, 0, 0, RenderProgressStates.Initializing), token).ConfigureAwait(false);
            using (progressLock.EnterScope())
            {
                renderProgress.InitializingCompleted(progress);
            }
        }
        catch (TaskCanceledException)
        {
            Report(new() { Status = RenderProgressStates.Canceled });
        }
        catch (Exception e)
        {
            using (progressLock.EnterScope())
            {
                renderProgress.OnNext(new(0, 0, 0, 0, RenderProgressStates.Failed));
                renderProgress.OnErrorResume(e);
            }
        }
    }

    private async Task<RenderProgress> UpdateDifferencesAsync(ReadOnlyMemory<ChangedValue<TIndex, TValue>> changedValues, RenderProgress progress, CancellationToken token)
    {
        using var sKSurfaceOwner = surfacePainter.CreateSurface(out var info);
        progress = await pipeline.UpdateDifferencesAsync(sKSurfaceOwner.SKSurface, info, structure, selector.Textile, changedValues, configure, this, progress with { MaxPhase = progress.MaxPhase + pipeline.UpdateDifferencesAsyncPhase }, token).ConfigureAwait(false);
        if (ChangedValueQueue.TryDequeue(out var values))
            return await UpdateDifferencesAsync(values, progress, token);
        else
            return progress;
    }

    public void Report(RenderProgress value)
    {
        using (progressLock.EnterScope())
        {
            renderProgress.OnNext(value);
        }
    }

    public void OnChanged(ReadOnlySpan<ChangedValue<TIndex, TValue>> changedValues)
    {
        var buffer = new ChangedValue<TIndex, TValue>[changedValues.Length];
        changedValues.CopyTo(buffer);
        using (renderTaskLock.EnterScope())
        {
            if (renderTask.IsCompletedSuccessfully)
                renderTask = UpdateDifferencesAsync(buffer, cancellationTokenSource.Token);
            else if (renderTask.IsCompleted)
                renderTask = RenderAsync(cancellationTokenSource.Token);
            else
                ChangedValueQueue.Enqueue(buffer);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                selector.Dispose();
                surfacePainter.Dispose();
                renderProgress.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~TextileEditorViewPainter()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}