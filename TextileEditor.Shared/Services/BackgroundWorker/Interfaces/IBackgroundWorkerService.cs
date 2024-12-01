namespace TextileEditor.Shared.Services;

public interface IBackgroundWorkerService
{
    IReadOnlyCollection<IRunningTask> GetRunningTasks();
    event Action<IRunningTask, BackgroundTaskProgress>? TaskRegistered;
    T CreateContext<T>()
        where T : IBackgroundWorkContext, IBackgroundWorkContextConstructor<T>;
}

public interface IBackgroundWorkerServiceRegister : IBackgroundWorkerService
{
    void Register(BackgroundTask task, BackgroundTaskProgress progress);
    bool RemoveContext(IBackgroundWorkContext context);
}