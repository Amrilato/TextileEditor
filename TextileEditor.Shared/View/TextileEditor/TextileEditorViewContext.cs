using R3;
using SkiaSharp;
using Textile.Colors;
using Textile.Common;
using Textile.Data;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;
using TextileEditor.Shared.View.TextileEditor.DataSelector;
using TextileEditor.Shared.View.TextileEditor.EventHandler;
using TextileEditor.Shared.View.TextileEditor.Pipeline;
using TextileEditor.Shared.View.TextileEditor.Renderer;

namespace TextileEditor.Shared.View.TextileEditor;

public class TextileEditorViewContext : IBindingTextileEditorViewContext<TextileIndex, bool>, IBindingTextileEditorViewContext<int, Color>, IDisposable
{
    public TextileEditorViewContext(TextileStructure structure,
                                    ITextileEditorViewRenderPipelineProvider builder,
                                    INotifyPropertyTextileEditorViewConfigure configure,
                                    ITextileEditorEventHandler<TextileIndex, bool>? textileEditorEventHandler = null,
                                    ITextileEditorEventHandler<int, Color>? textileEditorColorEventHandler = null)
    {
        disposableBag = new();

        this.structure = structure ?? throw new ArgumentNullException(nameof(structure));
        TextileEditorEventHandler = textileEditorEventHandler ?? ITextileEditorEventHandler<TextileIndex, bool>.Empty;
        TextileEditorColorEventHandler = textileEditorColorEventHandler ?? ITextileEditorEventHandler<int, Color>.Empty;
        Textile = InteractivePainter<TextileIndex, bool, TextileSelector>.Create(this, builder.CreateTextile(), configure, disposableBag);
        Heddle = InteractivePainter<TextileIndex, bool, HeddleSelector>.Create(this, builder.CreateHeddle(), configure, disposableBag);
        Pedal = InteractivePainter<TextileIndex, bool, PedalSelector>.Create(this, builder.CreatePedal(), configure, disposableBag);
        Tieup = InteractivePainter<TextileIndex, bool, TieupSelector>.Create(this, builder.CreateTieup(), configure, disposableBag);
        HeddleColor = InteractivePainter<int, Color, HeddleColorSelector>.Create(this, builder.CreateHeddleColor(), configure, disposableBag);
        PedalColor = InteractivePainter<int, Color, PedalColorSelector>.Create(this, builder.CreatePedalColor(), configure, disposableBag);
    }


    private readonly TextileStructure structure;
    private readonly DisposableBag disposableBag;

    #region Explicitly Interface Member
    ITextileEditorEventHandler<TextileIndex, bool> IBindingTextileEditorViewContext<TextileIndex, bool>.Handler => TextileEditorEventHandler;
    ITextileEditorEventHandler<int, Color> IBindingTextileEditorViewContext<int, Color>.Handler => TextileEditorColorEventHandler;

    ITextileEditorViewRenderer<TextileIndex, bool>? IBindingTextileEditorViewContext<TextileIndex, bool>.Renderer => textileEditorEventHandlerRenderer;
    ITextileEditorViewRenderer<int, Color>? IBindingTextileEditorViewContext<int, Color>.Renderer => textileEditorColorEventHandlerRenderer;

    TextileStructure IBindingTextileEditorViewContext<TextileIndex, bool>.Structure => structure;
    TextileStructure IBindingTextileEditorViewContext<int, Color>.Structure => structure;

    TextileEditorViewContext IBindingTextileEditorViewContext<TextileIndex, bool>.Self => this;
    TextileEditorViewContext IBindingTextileEditorViewContext<int, Color>.Self => this;
    #endregion

    #region EventHandler
    private ITextileEditorViewRenderer<TextileIndex, bool>? textileEditorEventHandlerRenderer;
    public ITextileEditorEventHandler<TextileIndex, bool> TextileEditorEventHandler
    {
        get => field;
        set
        {
            if (value == field)
                return;
            textileEditorEventHandlerRenderer = value is ITextileEditorViewRenderer<TextileIndex, bool> r ? r : null;
            field = value;
        }
    }
    private ITextileEditorViewRenderer<int, Color>? textileEditorColorEventHandlerRenderer;
    public ITextileEditorEventHandler<int, Color> TextileEditorColorEventHandler
    {
        get => field;
        set
        {
            if (value == field)
                return;
            textileEditorColorEventHandlerRenderer = value is ITextileEditorViewRenderer<int, Color> r ? r : null;
            field = value;
        }
    }
    #endregion

    #region TextileEditorPainter
    public IInteractivePainter Textile { get; }
    public IInteractivePainter Heddle { get; }
    public IInteractivePainter Pedal { get; }
    public IInteractivePainter Tieup { get; }
    public IInteractivePainter HeddleColor { get; }
    public IInteractivePainter PedalColor { get; }
    #endregion


    public void Dispose()
    {
        GC.SuppressFinalize(this);
        disposableBag.Dispose();
    }
}
file interface IBindingTextileEditorViewContext<TIndex, TValue>
{
    TextileStructure Structure { get; } 
    TextileEditorViewContext Self { get; }
    ITextileEditorEventHandler<TIndex, TValue> Handler { get; }
    ITextileEditorViewRenderer<TIndex, TValue>? Renderer { get; }

}
file readonly struct InteractivePainter<TIndex, TValue, TSelector> : IInteractivePainter, ITextileEditorViewRenderPipeline<TIndex, TValue>, IDisposable
    where TSelector : ITextileSelector<TIndex, TValue, TSelector>, IDisposable
{
    private InteractivePainter(IBindingTextileEditorViewContext<TIndex, TValue> parent, ITextileEditorViewRenderPipeline<TIndex, TValue> pipeline, INotifyPropertyTextileEditorViewConfigure configure)
    {
        this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
        painter = new TextileEditorViewPainter<TIndex, TValue, TSelector>(parent.Structure, pipeline, configure);
        this.pipeline = pipeline;
        this.configure = configure;
        configure.PropertyChanged += Configure_PropertyChanged;
    }

    private async void Configure_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ITextileEditorViewConfigure.GridSize):
            case nameof(ITextileEditorViewConfigure.BorderColor):
            case nameof(ITextileEditorViewConfigure.AreaSelectBorderColor):
            case nameof(ITextileEditorViewConfigure.IntersectionColor):
            case nameof(ITextileEditorViewConfigure.PastPreviewIntersectionColor):
            case nameof(ITextileEditorViewConfigure.TieupPosition):
                await painter.RenderAsync();
                break;
            default:
                break;
        }
    }

    public static InteractivePainter<TIndex, TValue, TSelector> Create(IBindingTextileEditorViewContext<TIndex, TValue> parent, ITextileEditorViewRenderPipeline<TIndex, TValue> pipeline, INotifyPropertyTextileEditorViewConfigure configure, DisposableBag disposableBag)
    {
        var p = new InteractivePainter<TIndex, TValue, TSelector>(parent, pipeline, configure);
        disposableBag.Add(p);
        return p;
    }

    private readonly IBindingTextileEditorViewContext<TIndex, TValue> parent;
    private readonly TextileEditorViewPainter<TIndex, TValue, TSelector> painter;
    private readonly ITextileEditorViewRenderPipeline<TIndex, TValue> pipeline;
    private readonly INotifyPropertyTextileEditorViewConfigure configure;

    public readonly ReadOnlyReactiveProperty<RenderProgress> RenderProgress => painter.RenderProgress;
    public readonly SKSizeI CanvasSize => painter.CanvasSize;
    public readonly Task OnPaintSurfaceAsync(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo, CancellationToken token) => painter.OnPaintSurfaceAsync(surface, info, rawInfo, token);

    public readonly void OnClick(SKPoint point) => parent.Handler.OnClick(point, TSelector.Select(parent.Structure), parent.Self);
    public readonly void OnPointerDown(SKPoint point) => parent.Handler.OnPointerDown(point, TSelector.Select(parent.Structure), parent.Self);
    public readonly void OnPointerEnter(SKPoint point) => parent.Handler.OnPointerEnter(point, TSelector.Select(parent.Structure), parent.Self);
    public readonly void OnPointerLeave(SKPoint point) => parent.Handler.OnPointerLeave(point, TSelector.Select(parent.Structure), parent.Self);
    public readonly void OnPointerMove(SKPoint point) => parent.Handler.OnPointerMove(point, TSelector.Select(parent.Structure), parent.Self);
    public readonly void OnPointerUp(SKPoint point) => parent.Handler.OnPointerUp(point, TSelector.Select(parent.Structure), parent.Self);

    #region TextileEditorRenderPipeline
    public int RenderAsyncPhase => pipeline.RenderAsyncPhase + 1;
    public int UpdateDifferencesAsyncPhase => pipeline.UpdateDifferencesAsyncPhase + 1;
    public readonly async Task<RenderProgress> RenderAsync(SKSurface surface,
                                           SKImageInfo info,
                                           IReadOnlyTextileStructure structure,
                                           IReadOnlyTextile<TIndex, TValue> textile,
                                           ITextileEditorViewConfigure configure,
                                           CancellationToken token,
                                           IProgress<RenderProgress> progress,
                                           RenderProgress renderProgress)
    {
        renderProgress = await pipeline.RenderAsync(surface, info, structure, textile, configure, token, progress, renderProgress);
        if (parent.Renderer is not null)
            renderProgress = await parent.Renderer.RenderAsync(surface, info, structure, textile, configure, token, progress, renderProgress);
        return renderProgress;
    }
    public readonly async Task<RenderProgress> UpdateDifferencesAsync(SKSurface surface,
                                                      SKImageInfo info,
                                                      IReadOnlyTextileStructure structure,
                                                      IReadOnlyTextile<TIndex, TValue> textile,
                                                      ReadOnlyMemory<ChangedValue<TIndex, TValue>> changedValues,
                                                      ITextileEditorViewConfigure configure,
                                                      CancellationToken token,
                                                      IProgress<RenderProgress> progress,
                                                      RenderProgress renderProgress)
    {
        renderProgress = await pipeline.UpdateDifferencesAsync(surface, info, structure, textile, changedValues, configure, token, progress, renderProgress);
        if (parent.Renderer is not null)
            renderProgress = await parent.Renderer.UpdateDifferencesAsync(surface, info, structure, textile, changedValues, configure, token, progress, renderProgress);
        return renderProgress;
    }
    #endregion
    public void Dispose()
    {
        painter.Dispose();
        configure.PropertyChanged -= Configure_PropertyChanged;
    }
}
