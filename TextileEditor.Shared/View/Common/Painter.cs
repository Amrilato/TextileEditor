using R3;
using SkiaSharp;

namespace TextileEditor.Shared.View.Common;

public abstract class Painter : IPainter
{
    protected readonly Lock progressLock = new();
    public abstract ReadOnlyReactiveProperty<RenderProgress> RenderProgress { get; }
    public abstract SKSizeI CanvasSize { get; }

    public bool OnPaintSurface(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo, CancellationToken token)
    {
        using (progressLock.EnterScope())
        {
            switch (RenderProgress.CurrentValue.Status)
            {
                case RenderProgressStates.NotStarted:
                case RenderProgressStates.Failed:
                case RenderProgressStates.Canceled:
                    Initialize(info, token);
                    return false;
                case RenderProgressStates.Initializing:
                case RenderProgressStates.Processing:
                    return false;
                case RenderProgressStates.Ready:
                case RenderProgressStates.Completed:
                    Paint(surface, info, rawInfo);
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    protected abstract void Initialize(SKImageInfo info, CancellationToken token);
    protected abstract void Paint(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo);
}
