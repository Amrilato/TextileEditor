namespace TextileEditor.Shared.Services;

public interface IRunningTask
{
    BackgroundTask BackgroundTask { get; }
    BackgroundTaskProgress LatestProgress { get; }
}