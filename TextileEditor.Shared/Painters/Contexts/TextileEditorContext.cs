using SkiaSharp;
using Textile.Colors;
using Textile.Common;
using Textile.Data;
using Textile.Interfaces;
using TextileEditor.Shared.EventHandlers;
using TextileEditor.Shared.Painters.DataSelector;
using TextileEditor.Shared.Painters.Renderers;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Painters;

internal class TextileEditorContext : TextilePainterContext, ITextileEditorContext
{
    public TextileEditorContext(ITextileIntersectionRenderer<TextileIndex, bool, TextileStructure> textileRenderer,
                                ITextileIntersectionRenderer<TextileIndex, bool, SKColor> settingRenderer,
                                ITextileIntersectionRenderer<int, Color, IReadOnlyTextileColor> heddleColorRenderer,
                                ITextileIntersectionRenderer<int, Color, IReadOnlyTextileColor> pedalColorRenderer,
                                ITextileBorderRenderer<SKColor> borderRenderProvider,
                                TextileSession session,
                                IEditorConfigure editorConfigure,
                                ConcurrencyBackgroundWorkContext concurrencyBackgroundWorkContext) : base(session)
    {
        this.concurrencyBackgroundWorkContext = concurrencyBackgroundWorkContext;
        textileSession = session;
        this.editorConfigure = editorConfigure;
        var textileStructure = session.TextileStructure;

        editorConfigure.PropertyChanged += EditorConfigurePropertyChanged;

        textile = new(textileRenderer, borderRenderProvider, ITextileGridEventHandler<TextileIndex, bool>.Empty, this, textileStructure, editorConfigure.GridSize, concurrencyBackgroundWorkContext);
        _ = InvokeRenderStateChangedAsync(textile.InitializedAsync());

        heddle = new(settingRenderer, borderRenderProvider, ITextileGridEventHandler<TextileIndex, bool>.Empty, this, textileStructure, editorConfigure.GridSize, concurrencyBackgroundWorkContext);
        _ = InvokeRenderStateChangedAsync(heddle.InitializedAsync());

        pedal = new(settingRenderer, borderRenderProvider, ITextileGridEventHandler<TextileIndex, bool>.Empty, this, textileStructure, editorConfigure.GridSize, concurrencyBackgroundWorkContext);
        _ = InvokeRenderStateChangedAsync(pedal.InitializedAsync());

        tieup = new(settingRenderer, borderRenderProvider, ITextileGridEventHandler<TextileIndex, bool>.Empty, this, textileStructure, editorConfigure.GridSize, concurrencyBackgroundWorkContext);
        _ = InvokeRenderStateChangedAsync(tieup.InitializedAsync());

        heddleColor = new(heddleColorRenderer, borderRenderProvider, ITextileGridEventHandler<int, Color>.Empty, this, textileStructure, editorConfigure.GridSize, concurrencyBackgroundWorkContext);
        _ = InvokeRenderStateChangedAsync(heddleColor.InitializedAsync());

        pedalColor = new(pedalColorRenderer, borderRenderProvider, ITextileGridEventHandler<int, Color>.Empty, this, textileStructure, editorConfigure.GridSize, concurrencyBackgroundWorkContext);
        _ = InvokeRenderStateChangedAsync(pedalColor.InitializedAsync());
    }

    private void EditorConfigurePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IEditorConfigure.GridSize):
                _ = SetGridSizeAsync(editorConfigure.GridSize);
                break;
            default:
                break;
        }
    }

    private async Task InvokeRenderStateChangedAsync(Task renderTask)
    {
        lock (_lock)
        {
            if (previousAlreadyRender)
            {
                previousAlreadyRender = AlreadyRender;
                InvokePropertyChanged(nameof(AlreadyRender));
            }
        }
        await renderTask;
        InvokePropertyChanged(nameof(AlreadyRender));
    }

    private readonly object _lock = new();
    private bool previousAlreadyRender = false;

    private readonly CompositePainter<TextileStructure, TextileIndex, bool, TextileSelector> textile;
    private readonly CompositePainter<SKColor, TextileIndex, bool, HeddleSelector> heddle;
    private readonly CompositePainter<SKColor, TextileIndex, bool, PedalSelector> pedal;
    private readonly CompositePainter<SKColor, TextileIndex, bool, TieupSelector> tieup;
    private readonly CompositePainter<IReadOnlyTextileColor, int, Color, HeddleColorSelector> heddleColor;
    private readonly CompositePainter<IReadOnlyTextileColor, int, Color, PedalColorSelector> pedalColor;

    private readonly ConcurrencyBackgroundWorkContext concurrencyBackgroundWorkContext;
    private TextileSession textileSession;
    private IEditorConfigure editorConfigure;

    protected override void OnTextileStructureChanged(TextileStructure textileStructure) => SetTextileStructureAsync(textileStructure);
    protected override void OnBorderColorChanged(SKColor borderColor) => SetBorderColorAsync(borderColor);
    protected override void OnFillColorChanged(SKColor fillColor) => SetFillColorAsync(fillColor);

    internal ITextileGridEventHandler<TextileIndex, bool> TextileGridEventHandler => textile.TextileEventHandler;
    internal ITextileGridEventHandler<int, Color> TextileColorGridEventHandler => heddleColor.TextileEventHandler;


    public override Task SetSessionAsync(TextileSession session)
    {
        SetSession(session);
        var cancel = concurrencyBackgroundWorkContext.Cancel();
        _ = SetAsync(cancel, session);
        return cancel;

        async Task SetAsync(Task cancel, TextileSession session)
        {
            await cancel;
            textileSession = session;
            SetFillColor(session.FillColor);
            SetBorderColor(session.BorderColor);
            await SetTextileStructureAsync(session.TextileStructure);
        }
    }

    private Task SetTextileStructureAsync(TextileStructure textileStructure)
    {
        textile.SetIntersectionData(textileStructure);
        textile.SetTextileStructure(textileStructure);
        heddle.SetTextileStructure(textileStructure);
        pedal.SetTextileStructure(textileStructure);
        tieup.SetTextileStructure(textileStructure);
        heddleColor.SetTextileStructure(textileStructure);
        pedalColor.SetTextileStructure(textileStructure);
        return InvokeRenderStateChangedAsync(InitializeAsync());
    }
    private void SetGridSize(GridSize gridSize)
    {
        textile.SetGridSize(gridSize);
        heddle.SetGridSize(gridSize);
        pedal.SetGridSize(gridSize);
        tieup.SetGridSize(gridSize);
        heddleColor.SetGridSize(gridSize);
        pedalColor.SetGridSize(gridSize);
    }
    private Task SetGridSizeAsync(GridSize gridSize)
    {
        SetGridSize(gridSize);
        return InvokeRenderStateChangedAsync(InitializeAsync());
    }
    private void SetFillColor(SKColor color)
    {
        heddle.SetIntersectionData(color);
        pedal.SetIntersectionData(color);
        tieup.SetIntersectionData(color);
    }
    private Task SetFillColorAsync(SKColor color)
    {
        SetFillColor(color);
        var h = heddle.InitializedAsync();
        var p = pedal.InitializedAsync();
        var t = tieup.InitializedAsync();
        return InvokeRenderStateChangedAsync(Task.WhenAll(h, p, t));
    }
    private void SetBorderColor(SKColor color)
    {
        textile.SetBorderColor(color);
        heddle.SetBorderColor(color);
        pedal.SetBorderColor(color);
        tieup.SetBorderColor(color);
        heddleColor.SetBorderColor(color);
        pedalColor.SetBorderColor(color);
    }
    private Task SetBorderColorAsync(SKColor color)
    {
        SetBorderColor(color);
        return InvokeRenderStateChangedAsync(InitializeAsync());
    }

    internal void SetTextileEventHandler(ITextileGridEventHandler<TextileIndex, bool> eventHandler)
    {
        textile.TextileEventHandler = eventHandler;
        InvokePropertyChanged(nameof(TextileEventHandler));
        heddle.TextileEventHandler = eventHandler;
        InvokePropertyChanged(nameof(HeddleEventHandler));
        pedal.TextileEventHandler = eventHandler;
        InvokePropertyChanged(nameof(PedalEventHandler));
        tieup.TextileEventHandler = eventHandler;
        InvokePropertyChanged(nameof(TieupEventHandler));
    }
    internal void SetTextileEventHandler(ITextileGridEventHandler<int, Color> eventHandler)
    {
        heddleColor.TextileEventHandler = eventHandler;
        InvokePropertyChanged(nameof(HeddleColorEventHandler));
        pedalColor.TextileEventHandler = eventHandler;
        InvokePropertyChanged(nameof(PedalColorEventHandler));
    }

    public async Task InitializeAsync()
    {
        var tt = textile.InitializedAsync();
        var ht = heddle.InitializedAsync();
        var pt = pedal.InitializedAsync();
        var it = tieup.InitializedAsync();
        var hct = heddleColor.InitializedAsync();
        var pct = pedalColor.InitializedAsync();
        InvokePropertyChanged(nameof(AlreadyRender));
        await Task.WhenAll(tt, ht, pt, it, hct, pct);
        InvokePropertyChanged(nameof(AlreadyRender));
    }

    public override bool AlreadyRender => textile.AlreadyRender && heddle.AlreadyRender && pedal.AlreadyRender && tieup.AlreadyRender && heddleColor.AlreadyRender && pedalColor.AlreadyRender;
    public TextileSession TextileSession => textileSession;

    public SKSizeI TextileCanvasSize => editorConfigure.GridSize.ToSettings(TextileSession.TextileStructure.Textile).CanvasSize();
    public ISKSurfacePainter TextilePainter => textile.Painter;
    public ICanvasEventHandler TextileEventHandler => textile.EventHandler;

    public SKSizeI HeddleCanvasSize => editorConfigure.GridSize.ToSettings(TextileSession.TextileStructure.Heddle).CanvasSize();
    public ISKSurfacePainter HeddlePainter => heddle.Painter;
    public ICanvasEventHandler HeddleEventHandler => heddle.EventHandler;

    public SKSizeI PedalCanvasSize => editorConfigure.GridSize.ToSettings(TextileSession.TextileStructure.Pedal).CanvasSize();
    public ISKSurfacePainter PedalPainter => pedal.Painter;
    public ICanvasEventHandler PedalEventHandler => pedal.EventHandler;

    public SKSizeI TieupCanvasSize => editorConfigure.GridSize.ToSettings(TextileSession.TextileStructure.Tieup).CanvasSize();
    public ISKSurfacePainter TieupPainter => tieup.Painter;
    public ICanvasEventHandler TieupEventHandler => tieup.EventHandler;

    public SKSizeI HeddleColorCanvasSize => editorConfigure.GridSize.ToSettings(TextileSession.TextileStructure.HeddleColor).CanvasSize();
    public ISKSurfacePainter HeddleColorPainter => heddleColor.Painter;
    public ICanvasEventHandler HeddleColorEventHandler => heddleColor.EventHandler;

    public SKSizeI PedalColorCanvasSize => editorConfigure.GridSize.ToSettings(TextileSession.TextileStructure.PedalColor).CanvasSize();
    public ISKSurfacePainter PedalColorPainter => pedalColor.Painter;
    public ICanvasEventHandler PedalColorEventHandler => pedalColor.EventHandler;

    protected override async ValueTask DisposeAsyncCore()
    {
        editorConfigure.PropertyChanged -= EditorConfigurePropertyChanged;
        await textile.DisposeAsync();
        await heddle.DisposeAsync();
        await pedal.DisposeAsync();
        await tieup.DisposeAsync();
        await heddleColor.DisposeAsync();
        await pedalColor.DisposeAsync();
    }

    private class CompositePainter<TIntersection, TIndex, TValue, TSelector> : ITextileChangedWatcher<TIndex, TValue>, IAsyncDisposable
        where TSelector : ITextileSelector<TextileStructure, TIndex, TValue, TSelector>, IDisposable
    {
        private readonly TextileGridEventHandlerProxy<TIndex, TValue> proxy;
        private readonly CompositeSKSurfacePainter sKSurfacePainters;
        private readonly TextileSKSurfacePainter<TIntersection, SKColor, TIndex, TValue> painter;
        private readonly TextileEditorContext textileEditorContext;
        private TSelector selector;

        public CompositePainter(ITextileIntersectionRenderer<TIndex, TValue, TIntersection> intersectionRenderer,
                                ITextileBorderRenderer<SKColor> textileBorderRenderer,
                                ITextileGridEventHandler<TIndex, TValue> eventHandler,
                                TextileEditorContext textileEditorContext,
                                TextileStructure structure,
                                GridSize gridSize,
                                ConcurrencyBackgroundWorkContext backgroundWorkContext)
        {
            selector = TSelector.Subscribe(this, structure);
            painter = new(intersectionRenderer, textileBorderRenderer, TSelector.SelectTextileData(structure), gridSize, backgroundWorkContext);
            proxy = new(eventHandler, TSelector.SelectTextileData(structure), gridSize);
            sKSurfacePainters = [painter, proxy];
            this.textileEditorContext = textileEditorContext;
        }

        public ISKSurfacePainter Painter => sKSurfacePainters;
        public ICanvasEventHandler EventHandler => proxy;
        public ITextileGridEventHandler<TIndex, TValue> TextileEventHandler
        {
            get => proxy.EventHandler;
            set
            {
                if (proxy.EventHandler == value)
                    return;
                proxy.EventHandler = value;
            }
        }
        public void SetTextileStructure(TextileStructure structure)
        {
            proxy.TextileData = TSelector.SelectTextileData(structure);
            painter.TextileData = TSelector.SelectTextileData(structure);
            selector.Dispose();
            selector = TSelector.Subscribe(this, structure);
        }
        public void SetGridSize(GridSize value)
        {
            proxy.GridSize = value;
            painter.GridSize = value;
        }
        public void SetIntersectionData(TIntersection value) => painter.IntersectionRenderer.Receive(value);
        public void SetBorderColor(SKColor value) => painter.BorderRenderer.Receive(value);
        public Task InitializedAsync() => painter.InitializedAsync();

        public bool AlreadyRender => painter.AlreadyRender;
        public void OnChanged(ReadOnlySpan<ChangedValue<TIndex, TValue>> changedValues)
        {
            var _ = textileEditorContext.InvokeRenderStateChangedAsync(painter.UpdateAsync(changedValues));
        }

        public async ValueTask DisposeAsync()
        {
            proxy.Dispose();
            sKSurfacePainters.Clear();
            await painter.DisposeAsync();
        }
    }
}
