using R3;
using SkiaSharp;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;
using TextileEditor.Shared.View.TextileEditor.Renderer;

namespace TextileEditor.Shared.View.TextileEditor.EventHandler;

public interface IStatefulTextileEditorEventHandler<TIndex, TValue> : ITextileEditorEventHandler<TIndex, TValue>;
public interface IStatelessTextileEditorEventHandler<TIndex, TValue> : ITextileEditorEventHandler<TIndex, TValue>;
public interface ITextileEditorEventHandler<TIndex, TValue>
{
    void OnClick(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure);
    void OnPointerMove(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure);
    void OnPointerEnter(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure);
    void OnPointerLeave(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure);
    void OnPointerDown(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure);
    void OnPointerUp(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure);

    static ITextileEditorEventHandler<TIndex, TValue> Empty => EmptyTextileEditorEventHandler<TIndex, TValue>.Instance;
}

file class EmptyTextileEditorEventHandler<TIndex, TValue> : ITextileEditorEventHandler<TIndex, TValue>
{
    public static EmptyTextileEditorEventHandler<TIndex, TValue> Instance = new();

    public void OnClick(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
    public void OnPointerDown(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
    public void OnPointerEnter(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
    public void OnPointerLeave(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
    public void OnPointerMove(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
    public void OnPointerUp(SKPoint point, ITextile<TIndex, TValue> textileData, IReadOnlyTextileStructure structure, ITextileEditorViewConfigure configure) { }
}

public class TextileEditorEventHandlerBundle<TIndex, TValue> : ISynchronizationReactiveTextileEditorViewRenderer<TIndex, TValue>, IDisposable
{
    private Dictionary<Type, IStatefulTextileEditorEventHandler<TIndex, TValue>> Statefuls = new();
    private IDisposable? disposable;
    private ISynchronizationReactiveTextileEditorViewRenderer<TIndex, TValue>? synchronizationReactiveTextileEditorViewRenderer;
    public ITextileEditorEventHandler<TIndex, TValue> TextileEditorEventHandler
    {
        get => field;
        set
        {
            disposable?.Dispose();
            if (value is ISynchronizationReactiveTextileEditorViewRenderer<TIndex, TValue> newRenderer)
            {
                synchronizationReactiveTextileEditorViewRenderer = newRenderer;
                renderStateChanged.OnNext(Unit.Default);
                disposable = newRenderer.RenderStateChanged.Subscribe(renderStateChanged.OnNext);
            }
            else if (disposable is not null)
                renderStateChanged.OnNext(Unit.Default);
            field = value;

        }
    } = ITextileEditorEventHandler<TIndex, TValue>.Empty;
    public THandler SetHandler<THandler>()
        where THandler : IStatelessTextileEditorEventHandler<TIndex, TValue>, new()
    {
        var handler = Cache<THandler>.Value;
        TextileEditorEventHandler = handler;
        return handler;
    }

    public THandler SetHandler<THandler>(int _ = 0)
        where THandler : IStatefulTextileEditorEventHandler<TIndex, TValue>, new()
    {
        if (Statefuls.TryGetValue(typeof(THandler), out var handler))
        {
            TextileEditorEventHandler = handler;
            return (THandler)handler;
        }
        else
        {
            var newHandler = new THandler();
            TextileEditorEventHandler = newHandler;
            Statefuls.Add(typeof(THandler), newHandler);
            return newHandler;
        }
    }

    RenderProgress ISynchronizationTextileEditorViewRenderer<TIndex, TValue>.Render(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ITextileEditorViewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress) => synchronizationReactiveTextileEditorViewRenderer?.Render(surface, info, structure, textile, configure, progress, currentProgress) ?? currentProgress;
    RenderProgress ISynchronizationTextileEditorViewRenderer<TIndex, TValue>.UpdateDifferences(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ReadOnlyMemory<ChangedValue<TIndex, TValue>> changedValues, ITextileEditorViewConfigure configure, IProgress<RenderProgress> progress, RenderProgress currentProgress) => synchronizationReactiveTextileEditorViewRenderer?.UpdateDifferences(surface, info, structure, textile, changedValues, configure, progress, currentProgress) ?? currentProgress;


    private readonly Subject<Unit> renderStateChanged = new();
    Observable<Unit> ISynchronizationReactiveTextileEditorViewRenderer<TIndex, TValue>.RenderStateChanged => renderStateChanged;

    public void Dispose() => disposable?.Dispose();

    private static class Cache<THandler>
        where THandler : IStatelessTextileEditorEventHandler<TIndex, TValue>, new()
    {
        public static THandler Value { get; } = new();
    }
}