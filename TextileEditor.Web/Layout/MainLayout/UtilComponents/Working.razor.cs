using Microsoft.AspNetCore.Components;
using R3;
using TextileEditor.Shared.Services;

namespace TextileEditor.Web.Layout;

public partial class Working : IDisposable
{
    private readonly object _lock = new();
    [Inject]
    public required IBackgroundWorkerService BackgroundWorker { get; init; }
    private readonly string Id = Guid.NewGuid().ToString();
    private readonly List<ProgressWatcher> runningTasks = [];
    protected override void OnInitialized()
    {
        var tasks = BackgroundWorker.GetRunningTasks();
        BackgroundWorker.TaskRegistered += BackgroundWorker_TaskRegistered;
        foreach (var task in tasks)
            runningTasks.Add(new ProgressWatcher(task, this));
    }

    private void BackgroundWorker_TaskRegistered(IRunningTask task, BackgroundTaskProgress _)
    {
        lock(_lock)
        {
            runningTasks.Add(new ProgressWatcher(task, this));
        }
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        BackgroundWorker.TaskRegistered -= BackgroundWorker_TaskRegistered;
        foreach (var item in runningTasks)
            item.Dispose();
    }

    private class ProgressWatcher : Observer<BackgroundTaskProgress>
    {
        private readonly Working working;

        public IRunningTask RunningTask { get; }

        public ProgressWatcher(IRunningTask runningTask, Working working)
        {
            RunningTask = runningTask;
            this.working = working;
            runningTask.BackgroundTask.Subscribe(this);
        }

        protected override void OnCompletedCore(Result result)
        {
            lock (working._lock)
            {
                working.runningTasks.Remove(this);
            }
            working.InvokeAsync(working.StateHasChanged);
        }

        protected override void OnErrorResumeCore(Exception error) => working.InvokeAsync(working.StateHasChanged);

        protected override void OnNextCore(BackgroundTaskProgress value) => working.InvokeAsync(working.StateHasChanged);
    }
}
