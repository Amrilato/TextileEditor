using System.Runtime.CompilerServices;

namespace TextileEditor.Shared.Services.TextileSessionStorage;

public interface ITextileSessionStorage
{
    IReadOnlyList<TextileSession> Sessions { get; }

    event SessionListChangedEvent? SessionListChanged;

    Task AddOrSaveAsync(TextileSession session);
    Task SaveAsync();
    Task RemoveAsync(TextileSession session);
    Task SavePropertyAsync<T>(TextileSession session, T value, [CallerMemberName] string propertyName = "");
}

public delegate void SessionListChangedEvent(ITextileSessionStorage storage, SessionListChangedEventArgs eventArgs);
public readonly ref struct SessionListChangedEventArgs(TextileSession changedSession, SessionListChangedState state)
{
    public readonly TextileSession ChangedSession = changedSession;
    public readonly SessionListChangedState State = state;
}

public enum SessionListChangedState
{
    Add,
    Remove
}