using R3;
using SkiaSharp;
using Textile.Colors;
using Textile.Common;
using Textile.Data;
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
                                    INotifyPropertyTextileEditorViewConfigure configure)
    {
        disposableBag = new();

        this.structure = structure ?? throw new ArgumentNullException(nameof(structure));
        TextileEditorEventHandler = new();
        TextileEditorColorEventHandler = new();
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

    TextileStructure IBindingTextileEditorViewContext<TextileIndex, bool>.Structure => structure;
    TextileStructure IBindingTextileEditorViewContext<int, Color>.Structure => structure;

    TextileEditorViewContext IBindingTextileEditorViewContext<TextileIndex, bool>.Self => this;
    TextileEditorViewContext IBindingTextileEditorViewContext<int, Color>.Self => this;

    TextileEditorEventHandlerBundle<TextileIndex, bool> IBindingTextileEditorViewContext<TextileIndex, bool>.Bundle => TextileEditorEventHandler;
    TextileEditorEventHandlerBundle<int, Color> IBindingTextileEditorViewContext<int, Color>.Bundle => TextileEditorColorEventHandler;
    #endregion

    #region EventHandler
    public TextileEditorEventHandlerBundle<TextileIndex, bool> TextileEditorEventHandler { get; set; }
    public TextileEditorEventHandlerBundle<int, Color> TextileEditorColorEventHandler { get; set; }
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

    TextileEditorEventHandlerBundle<TIndex, TValue> Bundle { get; }
}
file class InteractivePainter<TIndex, TValue, TSelector> : TextileEditorViewPainter<TIndex, TValue, TSelector>, IInteractivePainter
    where TSelector : ITextileSelector<TIndex, TValue, TSelector>, IDisposable
{
    public static InteractivePainter<TIndex, TValue, TSelector> Create(IBindingTextileEditorViewContext<TIndex, TValue> parent, ITextileEditorViewRenderPipeline<TIndex, TValue> pipeline, INotifyPropertyTextileEditorViewConfigure configure, DisposableBag disposableBag)
    {
        var p = new InteractivePainter<TIndex, TValue, TSelector>(parent, pipeline, configure);
        disposableBag.Add(p);
        return p;
    }
    private InteractivePainter(IBindingTextileEditorViewContext<TIndex, TValue> parent, ITextileEditorViewRenderPipeline<TIndex, TValue> pipeline, INotifyPropertyTextileEditorViewConfigure configure) : base(parent.Structure, pipeline, configure)
    {
        observableConfigure = configure;
        this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
        configure.PropertyChanged += Configure_PropertyChanged;
        disposable = ((ISynchronizationReactiveTextileEditorViewRenderer<TIndex, TValue>)parent.Bundle).RenderStateChanged.Subscribe(u =>
        {
            if (RenderProgress.CurrentValue.Status == RenderProgressStates.Ready || RenderProgress.CurrentValue.Status == RenderProgressStates.Completed)
                Report(new() { Status = RenderProgressStates.Ready });
        });
    }

    private void Configure_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ITextileEditorViewConfigure.GridSize):
            case nameof(ITextileEditorViewConfigure.BorderColor):
            case nameof(ITextileEditorViewConfigure.AreaSelectBorderColor):
            case nameof(ITextileEditorViewConfigure.IntersectionColor):
            case nameof(ITextileEditorViewConfigure.PastPreviewIntersectionColor):
            case nameof(ITextileEditorViewConfigure.TieupPosition):
                Initialize(surfacePainter.SKImageInfo, cancellationTokenSource.Token);
                break;
            default:
                break;
        }
    }

    private readonly INotifyPropertyTextileEditorViewConfigure observableConfigure;
    private readonly IBindingTextileEditorViewContext<TIndex, TValue> parent;
    private readonly IDisposable disposable;

    protected override void Paint(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo)
    {
        base.Paint(surface, info, rawInfo);
        ((ISynchronizationReactiveTextileEditorViewRenderer<TIndex, TValue>)parent.Bundle).Render(surface, info, parent.Structure, TSelector.Select(parent.Structure), configure, this, new() { MaxPhase = 1, Status = RenderProgressStates.Processing });
        Report(new() { Phase = 1, MaxPhase = 1, Status = RenderProgressStates.Completed });
    }

    public void OnClick(SKPoint point) => parent.Bundle.TextileEditorEventHandler.OnClick(point, TSelector.Select(parent.Structure), parent.Structure, configure);
    public void OnPointerDown(SKPoint point) => parent.Bundle.TextileEditorEventHandler.OnPointerDown(point, TSelector.Select(parent.Structure), parent.Structure, configure);
    public void OnPointerEnter(SKPoint point) => parent.Bundle.TextileEditorEventHandler.OnPointerEnter(point, TSelector.Select(parent.Structure), parent.Structure, configure);
    public void OnPointerLeave(SKPoint point) => parent.Bundle.TextileEditorEventHandler.OnPointerLeave(point, TSelector.Select(parent.Structure), parent.Structure, configure);
    public void OnPointerMove(SKPoint point) => parent.Bundle.TextileEditorEventHandler.OnPointerMove(point, TSelector.Select(parent.Structure), parent.Structure, configure);
    public void OnPointerUp(SKPoint point) => parent.Bundle.TextileEditorEventHandler.OnPointerUp(point, TSelector.Select(parent.Structure), parent.Structure, configure);

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        disposable.Dispose();
        observableConfigure.PropertyChanged -= Configure_PropertyChanged;
    }
}
