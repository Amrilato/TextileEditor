using SkiaSharp;
using TextileEditor.Shared.Services;

namespace TextileEditor.Shared.Painters;

internal abstract class AsyncSKSurfaceRenderer(ConcurrencyBackgroundWorkContext concurrencyBackgroundWorkContext, SKSizeI sKSizeI) : SKSurfacePainter(sKSizeI)
{
    private readonly object _lock = new();
    private readonly ConcurrencyBackgroundWorkContext ConcurrencyBackgroundWorkContext = concurrencyBackgroundWorkContext;
    private Task renderTask = Task.FromException(new OperationCanceledException("initial value"));
    public bool AlreadyRender => renderTask.IsCompletedSuccessfully;
    public override void OnPaintSurface(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo)
    {
        if (AlreadyRender)
            base.OnPaintSurface(surface, info, rawInfo);
    }
    protected ConcurrencyBackgroundWork CreateWork(int maxStep) => ConcurrencyBackgroundWorkContext.Create(maxStep);
    protected Task Post(Func<Task> renderTaskFactory, ConcurrencyBackgroundWork work) => MonitorTask(renderTaskFactory, work);
    private async Task MonitorTask(Func<Task> renderTaskFactory, ConcurrencyBackgroundWork work)
    {
        lock (_lock)
        {
            if (renderTask.IsCompleted)
                renderTask = renderTaskFactory();
            else
                renderTask = renderTask.ContinueWith(t =>
                {
                    return renderTaskFactory();
                }).Unwrap();
        }
        if (!renderTask.IsCompletedSuccessfully)
            ConcurrencyBackgroundWorkContext.Post(work);
        try
        {
            await renderTask;
            if (AlreadyRender)
                InvokeRequestSurface();
            work.Complete();
        }
        catch (Exception e)
        {
            work.Complete(e);
#if DEBUG
            throw;
#endif
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            ConcurrencyBackgroundWorkContext.Dispose();
    }
}
