using R3;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.Services;
public abstract class BackgroundTask : Observable<BackgroundTaskProgress>, IBackgroundWorkContext, IDisposable
{
    public string Name { get; set; } = "";

    protected readonly List<Observer<BackgroundTaskProgress>> observers = [];

    protected override IDisposable SubscribeCore(Observer<BackgroundTaskProgress> observer)
    {
        if (!observers.Contains(observer))
            observers.Add(observer);
        return Disposable.Create(() => observers.Remove(observer));
    }

    protected void OnNext(BackgroundTaskProgress value)
    {
        RentArray<Observer<BackgroundTaskProgress>> rent = observers.ToRentArray();
        try
        {
            foreach (var observer in rent.Values.Span)
                observer.OnNext(value);
        }
        finally
        {
            rent.Dispose();
        }
    }

    protected void OnErrorResume(Exception exception)
    {
        RentArray<Observer<BackgroundTaskProgress>> rent = observers.ToRentArray();
        try
        {
            foreach (var observer in rent.Values.Span)
                observer.OnErrorResume(exception);
        }
        finally
        {
            rent.Dispose();
        }
    }

    protected void OnCompleted()
    {
        RentArray<Observer<BackgroundTaskProgress>> rent = observers.ToRentArray();
        try
        {
            foreach (var observer in rent.Values.Span)
                observer.OnCompleted();
        }
        finally
        {
            rent.Dispose();
        }
    }
    protected void OnCompleted(Exception exception)
    {
        RentArray<Observer<BackgroundTaskProgress>> rent = observers.ToRentArray();
        try
        {
            foreach (var observer in rent.Values.Span)
                observer.OnCompleted(exception);
        }
        finally
        {
            rent.Dispose();
        }
    }
    protected void OnCompleted(Result result)
    {
        RentArray<Observer<BackgroundTaskProgress>> rent = observers.ToRentArray();
        try
        {
            foreach (var observer in rent.Values.Span)
                observer.OnCompleted(result);
        }
        finally
        {
            rent.Dispose();
        }
    }

    protected virtual void Dispose(bool disposing) { }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
