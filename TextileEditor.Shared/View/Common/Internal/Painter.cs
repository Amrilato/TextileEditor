using R3;
using SkiaSharp;

namespace TextileEditor.Shared.View.Common.Internal;

/// <summary>
/// Abstract class representing a painter with locking mechanisms and reactive progress updates.
/// </summary>
public abstract class Painter : IPainter, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Painter"/> class.
    /// Sets up progress notifications for initialization and processing.
    /// </summary>
    protected Painter()
    {
        notifyInitializeProgress = new() { Painter = this, RenderProgressStates = RenderProgressStates.Initializing };
        notifyProcessingProgress = new() { Painter = this, RenderProgressStates = RenderProgressStates.Processing };
    }

    private readonly Lock @lock = new();
    private readonly NotifyProgress notifyInitializeProgress;
    private readonly NotifyProgress notifyProcessingProgress;
    private readonly ReactiveProperty<RenderProgress> renderProgress = new();
    private bool disposedValue;

    /// <summary>
    /// Gets the current render progress as a read-only reactive property.
    /// </summary>
    public ReadOnlyReactiveProperty<RenderProgress> RenderingProgress => renderProgress;

    /// <summary>
    /// Gets the size of the canvas.
    /// </summary>
    public abstract SKSizeI CanvasSize { get; }

    /// <summary>
    /// Attempts to paint the surface with the provided information.
    /// </summary>
    /// <param name="surface">The surface to paint on.</param>
    /// <param name="info">The image info for the surface.</param>
    /// <param name="rawInfo">The raw image info for the surface.</param>
    /// <returns>True if painting succeeded, otherwise false.</returns>
    public bool TryPaintSurface(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo)
    {
        bool shouldExitLock = true;
        if (@lock.TryEnter())
        {
            try
            {
                if (!ValidateSKImageInfo(info))
                {
                    shouldExitLock = false;
                    _ = BeginInitialization(info, new CancellationTokenSource().Token);
                    return false;
                }

                switch (renderProgress.CurrentValue.Status)
                {
                    case RenderProgressStates.NotStarted:
                    case RenderProgressStates.Failed:
                    case RenderProgressStates.Canceled:
                        shouldExitLock = false;
                        _ = BeginInitialization(info, new CancellationTokenSource().Token);
                        return false;
                    case RenderProgressStates.Ready:
                        shouldExitLock = false;
                        BeginPainting(surface, info, rawInfo);
                        return true;
                    default:
                        break;
                }
            }
            finally
            {
                if (shouldExitLock)
                    @lock.Exit();
            }
        }
        return false;
    }

    /// <summary>
    /// Handles the initialization process asynchronously.
    /// </summary>
    /// <param name="info">The image info for initialization.</param>
    /// <param name="token">A cancellation token to observe.</param>
    private async Task BeginInitialization(SKImageInfo info, CancellationToken token)
    {
        RenderProgressStates states = RenderProgressStates.Ready;
        try
        {
            await InitializeAsync(info, notifyInitializeProgress, token);
        }
        catch (Exception)
        {
            states = RenderProgressStates.Failed;
        }
        finally
        {
            @lock.Exit();
            renderProgress.OnNext(new() { Status = states });
        }
    }

    /// <summary>
    /// Handles the painting process.
    /// </summary>
    /// <param name="surface">The surface to paint on.</param>
    /// <param name="info">The image info for the surface.</param>
    /// <param name="rawInfo">The raw image info for the surface.</param>
    private void BeginPainting(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo)
    {
        RenderProgressStates states = RenderProgressStates.Completed;
        try
        {
            if (!Paint(surface, info, rawInfo, notifyProcessingProgress))
                states = RenderProgressStates.Failed;
        }
        catch (Exception)
        {
            states = RenderProgressStates.Failed;
        }
        finally
        {
            @lock.Exit();
            renderProgress.OnNext(new() { Status = states });
        }
    }

    /// <summary>
    /// Initializes with locking.
    /// </summary>
    /// <param name="info">The image info for initialization.</param>
    /// <param name="token">A cancellation token to observe.</param>
    protected void InitializeWithLock(SKImageInfo info, CancellationToken token)
    {
        @lock.Enter();
        _ = BeginInitialization(info, token);
    }

    /// <summary>
    /// Paints with locking.
    /// </summary>
    /// <param name="surface">The surface to paint on.</param>
    /// <param name="info">The image info for the surface.</param>
    /// <param name="rawInfo">The raw image info for the surface.</param>
    protected void PaintWithLock(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo)
    {
        @lock.Enter();
        BeginPainting(surface, info, rawInfo);
    }

    /// <summary>
    /// Attempts to initialize with locking.
    /// </summary>
    /// <param name="info">The image info for initialization.</param>
    /// <param name="token">A cancellation token to observe.</param>
    /// <returns>True if the lock was successfully entered and initialization started, otherwise false.</returns>
    protected bool TryInitializeWithLock(SKImageInfo info, CancellationToken token)
    {
        if (@lock.TryEnter())
        {
            _ = BeginInitialization(info, token);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Attempts to paint with locking.
    /// </summary>
    /// <param name="surface">The surface to paint on.</param>
    /// <param name="info">The image info for the surface.</param>
    /// <param name="rawInfo">The raw image info for the surface.</param>
    /// <returns>True if the lock was successfully entered and painting started, otherwise false.</returns>
    protected bool TryPaintWithLock(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo)
    {
        if (@lock.TryEnter())
        {
            BeginPainting(surface, info, rawInfo);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Asynchronously initializes the painter.
    /// </summary>
    /// <param name="info">The image info for initialization.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <param name="token">A cancellation token to observe.</param>
    protected abstract Task InitializeAsync(SKImageInfo info, IProgress<Progress> progress, CancellationToken token);

    /// <summary>
    /// Paints the surface.
    /// </summary>
    /// <param name="surface">The surface to paint on.</param>
    /// <param name="info">The image info for the surface.</param>
    /// <param name="rawInfo">The raw image info for the surface.</param>
    /// <param name="progress">The progress reporter.</param>
    /// <returns>True if painting succeeded, otherwise false.</returns>
    protected abstract bool Paint(SKSurface surface, SKImageInfo info, SKImageInfo rawInfo, IProgress<Progress> progress);

    /// <summary>
    /// Validates the given SKImageInfo to determine if it meets specific requirements.
    /// </summary>
    /// <param name="info">The SKImageInfo to validate.</param>
    /// <returns>True if the SKImageInfo is valid, otherwise false.</returns>
    protected abstract bool ValidateSKImageInfo(SKImageInfo info);


    /// <summary>
    /// Notifies that the progress is ready if applicable.
    /// </summary>
    protected void TryNotifyReady()
    {
        if (renderProgress.CurrentValue.Status == RenderProgressStates.Ready || renderProgress.CurrentValue.Status == RenderProgressStates.Completed)
            renderProgress.OnNext(new() { Status = RenderProgressStates.Ready });
    }

    /// <summary>
    /// Resets the render progress status to Ready if it is currently Completed.
    /// </summary>
    public void ResetStatus()
    {
        if (renderProgress.CurrentValue.Status == RenderProgressStates.Completed)
            renderProgress.OnNext(new() { Status = RenderProgressStates.Ready });
    }

    protected void SetStatus(RenderProgressStates states)
    {
        renderProgress.OnNext(new() { Status = states });
    }

    /// <summary>
    /// Helper class for notifying progress.
    /// </summary>
    private class NotifyProgress : IProgress<Progress>
    {
        internal required Painter Painter { get; init; }
        internal required RenderProgressStates RenderProgressStates { get; init; }

        /// <summary>
        /// Reports progress to the painter.
        /// </summary>
        /// <param name="value">The progress value.</param>
        public void Report(Progress value) => Painter.renderProgress.OnNext(new(value, RenderProgressStates));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                renderProgress.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Painter()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
