namespace TextileEditor.Shared.Services;

public readonly record struct BackgroundTaskProgressDiff(int Step);

public class ConcurrencyBackgroundWorkContext(IBackgroundWorkerServiceRegister backgroundWorkerService) : BackgroundTask, IBackgroundWorkContextConstructor<ConcurrencyBackgroundWorkContext>
{
    private readonly object _lock = new();
    private int step;
    internal int maxStep;
    private CancellationTokenSource cancellationTokenSource = new();
    private readonly LinkedList<ConcurrencyBackgroundWork> concurrencyBackgroundWorks = [];
    public Func<(int Step, int MaxStep), string> DescriptionFactory { get; init; } = arg => $"Working... {arg.Step} / {arg.MaxStep}";

    public static ConcurrencyBackgroundWorkContext Create(IBackgroundWorkerServiceRegister backgroundWorkerServiceRegister) => new(backgroundWorkerServiceRegister);

    public async Task Cancel(int retryToken = 1000)
    {
        cancellationTokenSource.Cancel();
        OnErrorResume(new OperationCanceledException(Name));
        
        while (retryToken-- > 0)
        {
            if (concurrencyBackgroundWorks.Count > 0)
                if (retryToken > 500)
                    await Task.Yield();
                else
                    await Task.Delay(10).ConfigureAwait(false);
            else
            {
                cancellationTokenSource = new();
                step = 0;
                maxStep = 0;
                return;
            }
        }
        throw new InvalidOperationException("Cancellation is failed, runout retry token");
    }

    public ConcurrencyBackgroundWork Create(int maxStep) => new(cancellationTokenSource.Token) { MaxStep = maxStep };

    public void Post(ConcurrencyBackgroundWork work)
    {
        bool isRunning = false;
        if (work.concurrencyBackgroundWorkContext is not null)
            throw new ArgumentException("", nameof(work));
        try
        {
            lock (_lock)
            {
                isRunning = concurrencyBackgroundWorks.Count > 0;
                concurrencyBackgroundWorks.AddLast(work.node);
                work.concurrencyBackgroundWorkContext = this;
            }
            Interlocked.Add(ref step, work.Step);
            Interlocked.Add(ref maxStep, work.MaxStep);
        }
        catch (InvalidOperationException)
        {
            if (!concurrencyBackgroundWorks.Contains(work))
                throw;
            else
                return;
        }
        if (!isRunning)
            backgroundWorkerService.Register(this, CreateProgress(step, maxStep));
    }

    internal void Report(BackgroundTaskProgressDiff value)
    {
        var step = Interlocked.Add(ref this.step, value.Step);
        var maxStep = this.maxStep;
        OnNext(CreateProgress(step, maxStep));
    }

    private BackgroundTaskProgress CreateProgress(int step, int maxStep) => new(step, maxStep, DescriptionFactory((step, maxStep)));

    internal void Complete(ConcurrencyBackgroundWork concurrencyBackgroundWork)
    {
        if (CompleteCore(concurrencyBackgroundWork))
            OnCompleted();
    }
    internal void Complete(ConcurrencyBackgroundWork concurrencyBackgroundWork, Exception exception)
    {
        if (CompleteCore(concurrencyBackgroundWork))
            OnCompleted(exception);
    }
    private bool CompleteCore(ConcurrencyBackgroundWork concurrencyBackgroundWork)
    {
        lock (_lock)
        {
            concurrencyBackgroundWorks.Remove(concurrencyBackgroundWork.node);
            if (concurrencyBackgroundWorks.Count == 0)
            {
                step = 0;
                maxStep = 0;
                return true;
            }
            else
                return false;
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if(disposing)
        {
            backgroundWorkerService.RemoveContext(this);
            backgroundWorkerService = null!;
        }
    }
}

public class ConcurrencyBackgroundWork : IProgress<BackgroundTaskProgressDiff>
{
    private int step;
    internal ConcurrencyBackgroundWorkContext? concurrencyBackgroundWorkContext;
    internal readonly LinkedListNode<ConcurrencyBackgroundWork> node;

    internal ConcurrencyBackgroundWork(CancellationToken cancellationToken)
    {
        node = new(this);
        CancellationToken = cancellationToken;
    }

    public int Step { get => step; set => step = value; }
    public int MaxStep { get; init; }
    
    public CancellationToken CancellationToken { get; }

    public void Report(BackgroundTaskProgressDiff value)
    {
        Interlocked.Add(ref step, value.Step);
        concurrencyBackgroundWorkContext?.Report(value);
    }

    public void Complete() => concurrencyBackgroundWorkContext?.Complete(this);
    public void Complete(Exception exception) => concurrencyBackgroundWorkContext?.Complete(this, exception);
}