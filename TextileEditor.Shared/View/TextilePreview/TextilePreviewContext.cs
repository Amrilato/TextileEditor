using R3;
using SkiaSharp;
using System.Collections.Concurrent;
using Textile.Colors;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;
using TextileEditor.Shared.View.Common.Internal;
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


file class TextilePreviewPainter : Painter
{
    protected readonly Lock renderTaskLock = new();
    private Task renderTask = Task.FromException(new OperationCanceledException());
    private readonly ConcurrentQueue<ChangedValue<TextileIndex, bool>[]> TextileChangedValueQueue = new();
    private readonly ConcurrentQueue<ChangedValue<int, Color>[]> HeddleChangedValueQueue = new();
    private readonly ConcurrentQueue<ChangedValue<int, Color>[]> PedalChangedValueQueue = new();

    private readonly IManagedMemorySKSurface fragmentPainter = ManagedMemorySKSurface.Create();
    private readonly IManagedMemorySKSurface surfacePainter = ManagedMemorySKSurface.Create();
    private readonly ITextilePreviewRenderPipeline pipeline;
    private readonly IReadOnlyTextileStructure structure;
    private readonly INotifyPropertyTextilePreviewConfigure configure;
    private CancellationTokenSource cancellationTokenSource;

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


    public override SKSizeI CanvasSize => new(configure.PixelSize.Width * structure.Textile.Width * configure.RepeatHorizontal, configure.PixelSize.Height * structure.Textile.Height * configure.RepeatVertical);

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
                    renderTask = Render(this, info, progress, new(), token);
                    break;
                default:
                    break;
            }
        }
        else
        {
            previCancellationTokenSource.Cancel();
            renderTask = renderTask.ContinueWith(s => Render(this, info, progress, new(), token)).Unwrap();
        }
        return renderTask;

        static async Task<Progress> Render(TextilePreviewPainter @this, SKImageInfo info, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
        {
            {
                using var dest = @this.surfacePainter.CreateSurface(info);
                using var frag = @this.fragmentPainter.CreateSurface(info with { Width = @this.configure.PixelSize.Width * @this.structure.Textile.Width, Height = @this.configure.PixelSize.Height * @this.structure.Textile.Height });
                currentProgress = await @this.pipeline.RenderAsync(dest.SKSurface, @this.surfacePainter.SKImageInfo, frag.SKSurface, @this.fragmentPainter.SKImageInfo, @this.structure, @this.configure, progress, new(), token);
            }
            return await Update(@this, info, progress, currentProgress, token);
        }
        static async Task<Progress> Update(TextilePreviewPainter @this, SKImageInfo info, IProgress<Progress> progress, Progress currentProgress, CancellationToken token)
        {
            if (@this.TextileChangedValueQueue.TryDequeue(out var changedTextile))
            {
                using var dest = @this.surfacePainter.CreateSurface(info);
                using var frag = @this.fragmentPainter.CreateSurface(info with { Width = @this.configure.PixelSize.Width * @this.structure.Textile.Width, Height = @this.configure.PixelSize.Height * @this.structure.Textile.Height });
                currentProgress = await @this.pipeline.UpdateDifferencesAsync(dest.SKSurface, info, frag.SKSurface, @this.fragmentPainter.SKImageInfo, @this.structure, changedTextile, @this.configure, progress, currentProgress, token);
            }
            else if (@this.HeddleChangedValueQueue.TryDequeue(out var changedHeddle))
            {
                using var dest = @this.surfacePainter.CreateSurface(info);
                using var frag = @this.fragmentPainter.CreateSurface(info with { Width = @this.configure.PixelSize.Width * @this.structure.Textile.Width, Height = @this.configure.PixelSize.Height * @this.structure.Textile.Height });
                currentProgress = await @this.pipeline.UpdateHeddleDifferencesAsync(dest.SKSurface, info, frag.SKSurface, @this.fragmentPainter.SKImageInfo, @this.structure, changedHeddle, @this.configure, progress, currentProgress, token);
            }
            else if (@this.PedalChangedValueQueue.TryDequeue(out var changedPedal))
            {
                using var dest = @this.surfacePainter.CreateSurface(info);
                using var frag = @this.fragmentPainter.CreateSurface(info with { Width = @this.configure.PixelSize.Width * @this.structure.Textile.Width, Height = @this.configure.PixelSize.Height * @this.structure.Textile.Height });
                currentProgress = await @this.pipeline.UpdatePedalDifferencesAsync(dest.SKSurface, info, frag.SKSurface, @this.fragmentPainter.SKImageInfo, @this.structure, changedPedal, @this.configure, progress, currentProgress, token);
            }
            else
                return currentProgress;
            return await Update(@this, info, progress, currentProgress, token);

        }
    }

    protected override bool Paint(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo, IProgress<Progress> progress)
    {
        if (!surfacePainter.SKImageInfo.Equals(info))
            return false;
        using SKPaint sKPaint = new() { BlendMode = SKBlendMode.Src };
        using var source = surfacePainter.CreateSurface(info);
        surface.Canvas.DrawSurface(source.SKSurface, 0, 0, sKPaint);
        return true;
    }


    private void Configure_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ITextilePreviewConfigure.PixelSize):
            case nameof(ITextilePreviewConfigure.RepeatHorizontal):
            case nameof(ITextilePreviewConfigure.RepeatVertical):
                InitializeWithLock(surfacePainter.SKImageInfo, new(false));
                break;
            default:
                break;
        }
    }

    private void PedalColor_TextileStateChanged(IReadOnlyTextile<int, Color> sender, TextileStateChangedEventArgs<int, Color> eventArgs)
    {
        var buffer = new ChangedValue<int, Color>[eventArgs.ChangedIndices.Length];
        eventArgs.ChangedIndices.CopyTo(buffer);
        PedalChangedValueQueue.Enqueue(buffer);
        InitializeWithLock(surfacePainter.SKImageInfo, new(false));
    }

    private void HeddleColor_TextileStateChanged(IReadOnlyTextile<int, Color> sender, TextileStateChangedEventArgs<int, Color> eventArgs)
    {
        var buffer = new ChangedValue<int, Color>[eventArgs.ChangedIndices.Length];
        eventArgs.ChangedIndices.CopyTo(buffer);
        HeddleChangedValueQueue.Enqueue(buffer);
        InitializeWithLock(surfacePainter.SKImageInfo, new(false));
    }

    private void Textile_TextileStateChanged(IReadOnlyTextile<TextileIndex, bool> sender, TextileStateChangedEventArgs<TextileIndex, bool> eventArgs)
    {
        var buffer = new ChangedValue<TextileIndex, bool>[eventArgs.ChangedIndices.Length];
        eventArgs.ChangedIndices.CopyTo(buffer);
                TextileChangedValueQueue.Enqueue(buffer);
        InitializeWithLock(surfacePainter.SKImageInfo, new(false));
    }

    protected override bool ValidateSKImageInfo(SKImageInfo info) => surfacePainter.SKImageInfo.Equals(info);
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            surfacePainter.Dispose();
            fragmentPainter.Dispose();
        }
    }
}
