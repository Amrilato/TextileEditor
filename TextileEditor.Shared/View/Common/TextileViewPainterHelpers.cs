using R3;

namespace TextileEditor.Shared.View.Common;

internal static class TextileViewPainterHelpers
{

    public static void Initializing(this ReactiveProperty<RenderProgress> renderProgress) => renderProgress.OnNext(new() { Status = RenderProgressStates.Initializing });
    public static void InitializingCompleted(this ReactiveProperty<RenderProgress> renderProgress, RenderProgress progress) => renderProgress.OnNext(progress with { Status = RenderProgressStates.Ready });
}