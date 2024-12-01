using System.Collections.Concurrent;

namespace TextileEditor.Shared.Services;

public class QueueBackgroundWorkContext(IBackgroundWorkerServiceRegister backgroundWorkerService) : BackgroundTask, IQueueBackgroundWorkContext, IBackgroundWorkContextConstructor<QueueBackgroundWorkContext>
{
    private Task current = Task.CompletedTask;
    private readonly record struct Tuple(Func<Task> Task, string Description);
    private readonly ConcurrentQueue<Tuple> _workItems = [];

    public static QueueBackgroundWorkContext Create(IBackgroundWorkerServiceRegister backgroundWorkerServiceRegister) => new(backgroundWorkerServiceRegister);

    public Task Post(Func<Task> task, string description)
    {
        if (current.IsCompleted)
        {
            Interlocked.Exchange(ref current, Monitor(task));
            backgroundWorkerService.Register(this, new(0, 0, description));
        }
        else
            _workItems.Enqueue(new(task, description));
        return current;
    }

    private async Task Monitor(Func<Task> task)
    {
        var t = task();
        await t.ConfigureAwait(false);
        if (_workItems.TryDequeue(out  var next))
        {
            if (t.IsFaulted || t.IsCanceled)
                OnErrorResume(t?.Exception ?? throw new NullReferenceException());
            else
                OnNext(new(0, 0, next.Description));
            await Monitor(next.Task).ConfigureAwait(false);
        }
        else
        {
            if (t.IsFaulted || t.IsCanceled)
                OnCompleted(t?.Exception ?? throw new NullReferenceException());
            else
                OnCompleted();
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            backgroundWorkerService.RemoveContext(this);
            backgroundWorkerService = null!;
        }
    }
}