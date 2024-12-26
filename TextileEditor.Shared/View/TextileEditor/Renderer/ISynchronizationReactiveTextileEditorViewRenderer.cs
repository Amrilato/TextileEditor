using R3;

namespace TextileEditor.Shared.View.TextileEditor.Renderer;

internal interface ISynchronizationReactiveTextileEditorViewRenderer<TIndex, TValue> : ISynchronizationTextileEditorViewRenderer<TIndex, TValue>
{
    Observable<Unit> RenderStateChanged { get; }
}