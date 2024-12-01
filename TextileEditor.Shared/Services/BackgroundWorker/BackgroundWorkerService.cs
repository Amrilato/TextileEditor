using R3;

namespace TextileEditor.Shared.Services;

public class BackgroundWorkerService : IBackgroundWorkerServiceRegister, IDisposable
{
    private readonly object _lock = new();
    private readonly List<IBackgroundWorkContext> contexts = [];
    private readonly List<IRunningTask> runningTasks = [];
    public event Action<IRunningTask, BackgroundTaskProgress>? TaskRegistered;
    public IReadOnlyCollection<IRunningTask> GetRunningTasks()
    {
        lock (_lock)
        {
            List<IRunningTask> copy = new(runningTasks);
            return copy;
        }
    }

    public void Register(BackgroundTask backgroundTask, BackgroundTaskProgress progress)
    {
        RunningTask runningTask;
        lock (_lock)
        {
            foreach (var item in runningTasks)
                if (item.BackgroundTask == backgroundTask)
                    return;

            runningTask = new (backgroundTask, progress, runningTasks, _lock);
            runningTasks.Add(runningTask);
        }
        TaskRegistered?.Invoke(runningTask, progress);
    }

    public T CreateContext<T>()
        where T : IBackgroundWorkContext, IBackgroundWorkContextConstructor<T>
    {
        var context = T.Create(this);
        contexts.Add(context);
        return context;
    }

    public bool RemoveContext(IBackgroundWorkContext context) => contexts.Remove(context);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        for (int i = 0; i < contexts.Count; i++)
            contexts[i].Dispose();
        contexts.Clear();
    }
}

public class RunningTask : Observer<BackgroundTaskProgress>, IRunningTask
{
    private readonly BackgroundTask backgroundTask;
    private readonly ICollection<IRunningTask> backgroundTasks;
    private readonly object @lock;
    private BackgroundTaskProgress latest;

    public RunningTask(BackgroundTask backgroundTask, BackgroundTaskProgress latest, ICollection<IRunningTask> backgroundTasks, object _lock)
    {
        this.backgroundTask = backgroundTask;
        this.latest = latest;
        this.backgroundTasks = backgroundTasks;
        @lock = _lock;
        backgroundTask.Subscribe(this);
    }

    public BackgroundTaskProgress LatestProgress => latest;
    public BackgroundTask BackgroundTask => backgroundTask;
    protected override void OnCompletedCore(Result result)
    {
        lock (@lock)
        {
            backgroundTasks.Remove(this);
        }
    }

    protected override void OnErrorResumeCore(Exception error)
    {

    }

    protected override void OnNextCore(BackgroundTaskProgress value) => latest = value;
}