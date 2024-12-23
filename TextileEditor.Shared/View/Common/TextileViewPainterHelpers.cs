using R3;

namespace TextileEditor.Shared.View.Common;

internal static class TextileViewPainterHelpers
{

    public static void RenderCompleted(this ReactiveProperty<RenderProgress> renderProgress, RenderProgress progress) => renderProgress.OnNext(progress with { Status = RenderProgressStates.Completed });
}