namespace TextileEditor.Shared.Services.BackgroundWorker;

internal static class ConcurrencyBackgroundWorkContextExtensions
{
    public static void MonitorTask(this ConcurrencyBackgroundWorkContext concurrencyBackgroundWorkContext, Task task)
    {
        if (task.IsCompletedSuccessfully)
            return;
        else
        {
            var work = concurrencyBackgroundWorkContext.Create(0);
            concurrencyBackgroundWorkContext.Post(work);
            _ = MonitorTask(work, task);
        }
    }

    public static async Task MonitorTask(ConcurrencyBackgroundWork concurrencyBackgroundWork, Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
            concurrencyBackgroundWork.Complete();
        }
        catch (Exception e)
        {
            concurrencyBackgroundWork.Complete(e);
        }
    }
}
