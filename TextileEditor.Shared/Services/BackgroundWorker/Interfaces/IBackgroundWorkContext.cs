namespace TextileEditor.Shared.Services;

public interface IBackgroundWorkContext : IDisposable
{
    string Name { get; }
}

public interface IBackgroundWorkContextConstructor<TSelf>
    where TSelf : IBackgroundWorkContext
{
    static abstract TSelf Create(IBackgroundWorkerServiceRegister backgroundWorkerServiceRegister);
}