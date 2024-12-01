namespace TextileEditor.Shared.Services;

public interface IQueueBackgroundWorkContext : IBackgroundWorkContext
{
    Task Post(Func<Task> task, string description);
}
