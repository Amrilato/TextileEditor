namespace TextileEditor.Shared.Services;

public interface IConcurrencyBackgroundWorkContext : IBackgroundWorkContext, IProgress<BackgroundTaskProgressDiff>
{
    CancellationToken CancellationToken { get; }
    Task Cancel();
    void Post(Task task, int maxStep);
}
