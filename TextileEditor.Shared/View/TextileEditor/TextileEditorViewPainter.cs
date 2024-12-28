using SkiaSharp;
using System.Collections.Concurrent;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;
using TextileEditor.Shared.View.Common.Internal;
using TextileEditor.Shared.View.TextileEditor.DataSelector;
using TextileEditor.Shared.View.TextileEditor.Pipeline;

namespace TextileEditor.Shared.View.TextileEditor;

internal abstract class TextileEditorViewPainter<TIndex, TValue, TSelector> : Painter, ITextileChangedWatcher<TIndex, TValue>, IDisposable
    where TSelector : ITextileSelector<TIndex, TValue, TSelector>, IDisposable
{
    private Task renderTask = Task.FromException(new TaskCanceledException());
    private readonly ConcurrentQueue<ChangedValue<TIndex, TValue>[]> ChangedValueQueue = new();
    protected readonly IManagedMemorySKSurface surfacePainter = ManagedMemorySKSurface.Create();
    private CancellationTokenSource cancellationTokenSource;
    private readonly TSelector selector;
    private readonly IReadOnlyTextileStructure structure;
    private readonly ITextileEditorViewRenderPipeline<TIndex, TValue> pipeline;
    protected readonly ITextileEditorViewConfigure configure;

    public override SKSizeI CanvasSize => configure.GridSize.ToSettings(selector.Textile).CanvasSize();
    public TextileEditorViewPainter(IReadOnlyTextileStructure structure, ITextileEditorViewRenderPipeline<TIndex, TValue> pipeline, ITextileEditorViewConfigure configure)
    {
        selector = TSelector.Subscribe(this, structure);
        this.structure = structure;
        this.pipeline = pipeline;
        this.configure = configure;
        cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
    }

    protected override Task InitializeAsync(SKImageInfo info, IProgress<Progress> progress, CancellationToken token)
    {
        var previCancellationTokenSource = cancellationTokenSource;
        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        if (surfacePainter.SKImageInfo == info)
        {
            switch (renderTask.Status)
            {
                case TaskStatus.Created:
                    break;
                case TaskStatus.WaitingForActivation:
                    break;
                case TaskStatus.WaitingToRun:
                    break;
                case TaskStatus.Running:
                    break;
                case TaskStatus.WaitingForChildrenToComplete:
                    break;
                case TaskStatus.RanToCompletion:
                    renderTask = Update(this, info, progress, new(), token);
                    break;
                case TaskStatus.Canceled:
                case TaskStatus.Faulted:
                    ChangedValueQueue.Clear();
                    renderTask = Render(this, info, progress, new(), token);
                    break;
                default:
                    break;
            }
        }
        else
        {
            previCancellationTokenSource.Cancel();
            renderTask = renderTask.ContinueWith(s => Render(this, info, progress, new(), token), TaskContinuationOptions.None).Unwrap();
        }
        return renderTask;

        static async Task<Progress> Render(TextileEditorViewPainter<TIndex, TValue, TSelector> @this, SKImageInfo info, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
        {
            {
                using var surface = @this.surfacePainter.CreateSurface(info);
                currentProgress = await @this.pipeline.RenderAsync(surface.SKSurface, info, @this.structure, @this.selector.Textile, @this.configure, progress, currentProgress, token);
            }
            return await Update(@this, info, progress, currentProgress, token);
        }
        static async Task<Progress> Update(TextileEditorViewPainter<TIndex, TValue, TSelector> @this, SKImageInfo info, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
        {
            if (@this.ChangedValueQueue.TryDequeue(out var changedValues))
            {
                using var surface = @this.surfacePainter.CreateSurface(info);
                currentProgress = await @this.pipeline.UpdateDifferencesAsync(surface.SKSurface, info, @this.structure, @this.selector.Textile, changedValues, @this.configure, progress, currentProgress, token);
                return await Update(@this, info, progress, currentProgress, token);
            }
            return currentProgress;
        }
    }

    protected override void Paint(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo, IProgress<Progress> progress) 
    { 
        using SKPaint sKPaint = new() { BlendMode = SKBlendMode.Src };
        using var source = surfacePainter.CreateSurface(info);
        surface.Canvas.DrawSurface(source.SKSurface, 0, 0, sKPaint);
    }

    public void OnChanged(ReadOnlySpan<ChangedValue<TIndex, TValue>> changedValues)
    {
        var buffer = new ChangedValue<TIndex, TValue>[changedValues.Length];
        changedValues.CopyTo(buffer);
        ChangedValueQueue.Enqueue(buffer);
        InitializeWithLock(surfacePainter.SKImageInfo, cancellationTokenSource.Token);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            selector.Dispose();
            surfacePainter.Dispose();
        }
    }
}