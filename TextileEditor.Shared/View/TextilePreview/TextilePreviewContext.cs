using R3;
using SkiaSharp;
using Textile.Colors;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;
using TextileEditor.Shared.View.TextilePreview.Pipeline;

namespace TextileEditor.Shared.View.TextilePreview;

public class TextilePreviewContext : IDisposable
{
    public TextilePreviewContext(ITextilePreviewRenderPipelineProvider provider, IReadOnlyTextileStructure structure, INotifyPropertyTextilePreviewConfigure configure)
    {
        disposableBag = new();
        Painter = new TextilePreviewPainter(provider.Create(), structure, configure, disposableBag);
    }

    private readonly DisposableBag disposableBag;
    public IPainter Painter { get; }

    public void Dispose()
    {
        disposableBag.Dispose();
        GC.SuppressFinalize(this);
    }
}


file class TextilePreviewPainter : IPainter, IProgress<RenderProgress>, IDisposable
{
    private readonly Lock _lockObj = new();
    private Task renderTask = Task.FromException(new TaskCanceledException());
    private readonly Queue<ChangedValue<TextileIndex, bool>[]> TextileChangedValueQueue = new();
    private readonly Queue<ChangedValue<int, Color>[]> HeddleChangedValueQueue = new();
    private readonly Queue<ChangedValue<int, Color>[]> PedalChangedValueQueue = new();

    private readonly ManagedMemorySKSurface fragmentPainter = new();
    private readonly ManagedMemorySKSurface surfacePainter = new();
    private readonly ITextilePreviewRenderPipeline pipeline;
    private readonly IReadOnlyTextileStructure structure;
    private readonly INotifyPropertyTextilePreviewConfigure configure;
    private CancellationTokenSource cancellationTokenSource;

    private readonly ReactiveProperty<RenderProgress> renderProgress = new();

    public ReadOnlyReactiveProperty<RenderProgress> RenderProgress => renderProgress;

    public SKSizeI CanvasSize => new(configure.PixelSize.Width * structure.Textile.Width * configure.RepeatHorizontal, configure.PixelSize.Height * structure.Textile.Height * configure.RepeatVertical);
    private SKSizeI FragmentSize => new(configure.PixelSize.Width * structure.Textile.Width, configure.PixelSize.Height * structure.Textile.Height);


    public TextilePreviewPainter(ITextilePreviewRenderPipeline pipeline, IReadOnlyTextileStructure structure, INotifyPropertyTextilePreviewConfigure configure, DisposableBag disposableBag)
    {
        this.pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        this.structure = structure ?? throw new ArgumentNullException(nameof(structure));
        this.configure = configure;
        cancellationTokenSource = new();
        cancellationTokenSource.Cancel();
        structure.Textile.TextileStateChanged += Textile_TextileStateChanged;
        structure.HeddleColor.TextileStateChanged += HeddleColor_TextileStateChanged;
        structure.PedalColor.TextileStateChanged += PedalColor_TextileStateChanged;
        configure.PropertyChanged += Configure_PropertyChanged;
        disposableBag.Add(this);
    }

    private async void Configure_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ITextilePreviewConfigure.PixelSize):
            case nameof(ITextilePreviewConfigure.RepeatHorizontal):
            case nameof(ITextilePreviewConfigure.RepeatVertical):
                await RenderAsync(cancellationTokenSource.Token);
                break;
            default:
                break;
        }
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
            var fragSize = FragmentSize;
            fragmentPainter.ChangeImageInfo(info with { Width = fragSize.Width, Height = fragSize.Height });
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

    private void PedalColor_TextileStateChanged(IReadOnlyTextile<int, Color> sender, TextileStateChangedEventArgs<int, Color> eventArgs)
    {
        var buffer = new ChangedValue<int, Color>[eventArgs.ChangedIndices.Length];
        eventArgs.ChangedIndices.CopyTo(buffer);
        using (_lockObj.EnterScope())
        {
            if (renderTask.IsCompletedSuccessfully)
            {
                PedalChangedValueQueue.Enqueue(buffer);
                renderTask = UpdateDifferencesAsync(new(0, 0, 0, 0, RenderProgressStates.Initializing), cancellationTokenSource.Token);
            }
            else if (renderTask.IsCompleted)
                renderTask = RenderAsync(cancellationTokenSource.Token);
        }
    }

    private void HeddleColor_TextileStateChanged(IReadOnlyTextile<int, Color> sender, TextileStateChangedEventArgs<int, Color> eventArgs)
    {
        var buffer = new ChangedValue<int, Color>[eventArgs.ChangedIndices.Length];
        eventArgs.ChangedIndices.CopyTo(buffer);
        using (_lockObj.EnterScope())
        {
            if (renderTask.IsCompletedSuccessfully)
            {
                HeddleChangedValueQueue.Enqueue(buffer);
                renderTask = UpdateDifferencesAsync(new(0, 0, 0, 0, RenderProgressStates.Initializing), cancellationTokenSource.Token);
            }
            else if (renderTask.IsCompleted)
                renderTask = RenderAsync(cancellationTokenSource.Token);
        }
    }

    private void Textile_TextileStateChanged(IReadOnlyTextile<TextileIndex, bool> sender, TextileStateChangedEventArgs<TextileIndex, bool> eventArgs)
    {
        var buffer = new ChangedValue<TextileIndex, bool>[eventArgs.ChangedIndices.Length];
        eventArgs.ChangedIndices.CopyTo(buffer);
        using (_lockObj.EnterScope())
        {
            if (renderTask.IsCompletedSuccessfully)
            {
                TextileChangedValueQueue.Enqueue(buffer);
                renderTask = UpdateDifferencesAsync(new(0, 0, 0, 0, RenderProgressStates.Initializing), cancellationTokenSource.Token);
            }
            else if (renderTask.IsCompleted)
                renderTask = RenderAsync(cancellationTokenSource.Token);
        }
    }

    private async Task RenderAsync(CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return;
        try
        {
            renderProgress.OnNext(new(0, 0, 0, 0, RenderProgressStates.Initializing));
            using SKSurface dest = surfacePainter.CreateSurface(out var _);
            using SKSurface frag = surfacePainter.CreateSurface(out var _);
            var progress = await pipeline.RenderAsync(dest, surfacePainter.SKImageInfo, frag, fragmentPainter.SKImageInfo, structure, configure, this, new(0, pipeline.RenderAsyncPhase, 0, 0, RenderProgressStates.Initializing), token).ConfigureAwait(false);
            progress = await UpdateDifferencesAsync(progress, token);
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

    private async Task<RenderProgress> UpdateDifferencesAsync(RenderProgress progress, CancellationToken token)
    {
        Task<RenderProgress>? rTask = default;
        using (_lockObj.EnterScope())
        {
            if (TextileChangedValueQueue.TryDequeue(out var tResult))
                rTask = UpdateTextileDifferencesAsync(progress, tResult, token);
            else if (HeddleChangedValueQueue.TryDequeue(out var hResult))
                rTask = UpdateHeddleDifferencesAsync(progress, hResult, token);
            else if (PedalChangedValueQueue.TryDequeue(out var pResult))
                rTask = UpdatePedalDifferencesAsync(progress, pResult, token);
        }

        if (rTask is not null)
        {
            return (await rTask);
        }
        else
        {
            renderProgress.RenderCompleted(progress);
            return progress;
        }

        async Task<RenderProgress> UpdateTextileDifferencesAsync(RenderProgress progress, ReadOnlyMemory<ChangedValue<TextileIndex, bool>> changed, CancellationToken token)
        {
            using var dest = surfacePainter.CreateSurface(out var _);
            using var frag = surfacePainter.CreateSurface(out var _);
            progress = await pipeline.UpdateDifferencesAsync(dest, surfacePainter.SKImageInfo, frag, fragmentPainter.SKImageInfo, structure, changed, configure, this, progress with { MaxPhase = pipeline.UpdateDifferencesAsyncPhase }, token);
            return await UpdateDifferencesAsync(progress, token);
        }
        async Task<RenderProgress> UpdateHeddleDifferencesAsync(RenderProgress progress, ReadOnlyMemory<ChangedValue<int, Color>> changed, CancellationToken token)
        {
            using var dest = surfacePainter.CreateSurface(out var _);
            using var frag = surfacePainter.CreateSurface(out var _);
            progress = await pipeline.UpdateHeddleDifferencesAsync(dest, surfacePainter.SKImageInfo, frag, fragmentPainter.SKImageInfo, structure, changed, configure, this, progress with { MaxPhase = pipeline.UpdateHeddleDifferencesAsyncPhase }, token);
            return await UpdateDifferencesAsync(progress, token);
        }
        async Task<RenderProgress> UpdatePedalDifferencesAsync(RenderProgress progress, ReadOnlyMemory<ChangedValue<int, Color>> changed, CancellationToken token)
        {
            using var dest = surfacePainter.CreateSurface(out var _);
            using var frag = surfacePainter.CreateSurface(out var _);
            progress = await pipeline.UpdatePedalDifferencesAsync(dest, surfacePainter.SKImageInfo, frag, fragmentPainter.SKImageInfo, structure, changed, configure, this, progress with { MaxPhase = pipeline.UpdatePedalDifferencesAsyncPhase }, token);
            return await UpdateDifferencesAsync(progress, token);
        }

    }
    public void Report(RenderProgress value) => renderProgress.OnNext(value);

    public void Dispose()
    {
        renderProgress.Dispose();
        fragmentPainter.Dispose();
        surfacePainter.Dispose();
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
        structure.Textile.TextileStateChanged -= Textile_TextileStateChanged;
        structure.HeddleColor.TextileStateChanged -= HeddleColor_TextileStateChanged;
        structure.PedalColor.TextileStateChanged -= PedalColor_TextileStateChanged;
        configure.PropertyChanged -= Configure_PropertyChanged;
    }
}
