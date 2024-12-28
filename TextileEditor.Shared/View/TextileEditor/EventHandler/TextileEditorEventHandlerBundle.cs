using R3;
using SkiaSharp;
using Textile.Common;
using Textile.Interfaces;
using TextileEditor.Shared.View.Common;
using TextileEditor.Shared.View.TextileEditor.Renderer;

namespace TextileEditor.Shared.View.TextileEditor.EventHandler;

public class TextileEditorEventHandlerBundle<TIndex, TValue> : ISynchronizationReactiveTextileEditorViewRenderer<TIndex, TValue>, IDisposable
{
    private Dictionary<Type, IStatefulTextileEditorEventHandler<TIndex, TValue>> Statefuls = new();
    private IDisposable? disposable;
    private ISynchronizationReactiveTextileEditorViewRenderer<TIndex, TValue>? synchronizationReactiveTextileEditorViewRenderer;
    public ITextileEditorEventHandler<TIndex, TValue> Handler
    {
        get => field;
        private set
        {
            disposable?.Dispose();
            field = value;
            if (value is ISynchronizationReactiveTextileEditorViewRenderer<TIndex, TValue> newRenderer)
            {
                synchronizationReactiveTextileEditorViewRenderer = newRenderer;
                renderStateChanged.OnNext(Unit.Default);
                disposable = newRenderer.RenderStateChanged.Subscribe(renderStateChanged.OnNext);
            }
            else if (disposable is not null)
            {
                disposable = null;
                synchronizationReactiveTextileEditorViewRenderer = null;
                renderStateChanged.OnNext(Unit.Default);
            }

        }
    } = ITextileEditorEventHandler<TIndex, TValue>.Empty;
    public THandler SetHandler<THandler>()
        where THandler : IStatelessTextileEditorEventHandler<TIndex, TValue>, new()
    {
        var handler = Cache<THandler>.Value;
        Handler = handler;
        return handler;
    }

    public THandler SetHandler<THandler>(int _ = 0)
        where THandler : IStatefulTextileEditorEventHandler<TIndex, TValue>, new()
    {
        if (Statefuls.TryGetValue(typeof(THandler), out var handler))
        {
            Handler = handler;
            return (THandler)handler;
        }
        else
        {
            var newHandler = new THandler();
            Handler = newHandler;
            Statefuls.Add(typeof(THandler), newHandler);
            return newHandler;
        }
    }
    public THandler GetHandler<THandler>()
        where THandler : IStatelessTextileEditorEventHandler<TIndex, TValue>, new() => Cache<THandler>.Value;
    public THandler GetHandler<THandler>(int _ = 0)
        where THandler : IStatefulTextileEditorEventHandler<TIndex, TValue>, new()
    {
        if (Statefuls.TryGetValue(typeof(THandler), out var handler))
        {
            return (THandler)handler;
        }
        else
        {
            var newHandler = new THandler();
            Statefuls.Add(typeof(THandler), newHandler);
            return newHandler;
        }
    }

    Progress ISynchronizationTextileEditorViewRenderer<TIndex, TValue>.Render(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ITextileEditorViewConfigure configure, IProgress<Progress> progress, Progress currentProgress) => synchronizationReactiveTextileEditorViewRenderer?.Render(surface, info, structure, textile, configure, progress, currentProgress) ?? currentProgress;
    Progress ISynchronizationTextileEditorViewRenderer<TIndex, TValue>.UpdateDifferences(SKSurface surface, SKImageInfo info, IReadOnlyTextileStructure structure, IReadOnlyTextile<TIndex, TValue> textile, ReadOnlyMemory<ChangedValue<TIndex, TValue>> changedValues, ITextileEditorViewConfigure configure, IProgress<Progress> progress, Progress currentProgress) => synchronizationReactiveTextileEditorViewRenderer?.UpdateDifferences(surface, info, structure, textile, changedValues, configure, progress, currentProgress) ?? currentProgress;


    private readonly Subject<Unit> renderStateChanged = new();
    Observable<Unit> ISynchronizationReactiveTextileEditorViewRenderer<TIndex, TValue>.RenderStateChanged => renderStateChanged;

    public void Dispose() => disposable?.Dispose();

    private static class Cache<THandler>
        where THandler : IStatelessTextileEditorEventHandler<TIndex, TValue>, new()
    {
        public static THandler Value { get; } = new();
    }
}