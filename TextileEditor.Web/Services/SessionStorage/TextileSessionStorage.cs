using DotNext.Buffers;
using SkiaSharp;
using System.Runtime.CompilerServices;
using Textile.Data;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Services.TextileSessionStorage;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Web.Services;


public class TextileSessionStorage : ITextileSessionStorage
{
    private readonly QueueBackgroundWorkContext backgroundWorkContext;
    private const string SessionListKey = $"__{nameof(SessionListKey)}__";
    private const string KeyPrefix = $"__TextileSessionStorage__";
    private readonly List<TextileSession> sessions = [];
    private readonly IWebStorageService webStorage;

    private static string GenerateKey(TextileSession session, string identifier) => GenerateKey(session.Guid, identifier);
    private static string GenerateKey(Guid guid, string identifier) => GenerateKey(guid.ToString(), identifier);
    private static string GenerateKey(string guid, string identifier) => $"{KeyPrefix}{guid}__{identifier}__";
    private const char Separator = ',';

    public TextileSessionStorage(IWebStorageService webStorage, IBackgroundWorkerService backgroundWorker)
    {
        this.webStorage = webStorage;
        backgroundWorkContext = backgroundWorker.CreateContext<QueueBackgroundWorkContext>();
        backgroundWorkContext.Name = "Session Storage";
    }

    private string GenerateKeyList() => sessions.Select(s => s.Guid.ToString()).Aggregate((c, n) => $"{c}{Separator}{n}");

    public IReadOnlyList<TextileSession> Sessions => sessions;
    public event SessionListChangedEvent? SessionListChanged;

    public Task AddOrSaveAsync(TextileSession session)
    {
        if (sessions.Contains(session))
            return backgroundWorkContext.Post(() => SetSessionAsync(session), $"Save: {session.Name}");
        else
        {
            sessions.Add(session);
            SessionListChanged?.Invoke(this, new(session, SessionListChangedState.Add));
            return SaveAsync();
        }
    }

    public Task SaveAsync()
    {
        return backgroundWorkContext.Post(SetSessionListAsync, "Save Sessions");
    }

    public Task RemoveAsync(TextileSession session)
    {
        if (sessions.Remove(session))
        {
            SessionListChanged?.Invoke(this, new(session, SessionListChangedState.Remove));
            return backgroundWorkContext.Post(SetKeyListAsync, $"Remove: {session.Guid}");
        }
        else
            return Task.CompletedTask;
    }

    public Task SavePropertyAsync<T>(TextileSession session, T value, [CallerMemberName] string propertyName = "")
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        if (sessions.Contains(session))
            return backgroundWorkContext.Post(() => SetPropertyAsync(session, propertyName, value.ToString() ?? throw new NullReferenceException($"ToString Failed, Guid: {session.Guid}, PropertyName: {propertyName}")), $"Save: {propertyName}");
        else
            return SaveAsync();
    }

    private Task SetKeyListAsync()
    {
        var keys = GenerateKeyList();
        return webStorage.SetItemAsync(SessionListKey, keys).AsTask();
    }

    private Task SetPropertyAsync(TextileSession session, string identifier, string value) => webStorage.SetItemAsync(GenerateKey(session, identifier), value).AsTask();
    private async Task SetSessionListAsync()
    {
        foreach (var session in sessions)
            await SetSessionAsync(session);
        await SetKeyListAsync();
    }
    private async Task SetSessionAsync(TextileSession session)
    {
        await SetPropertyAsync(session, nameof(TextileSession.Name), session.Name);
        await SetPropertyAsync(session, nameof(TextileSession.BorderColor), session.BorderColor.ToString());
        await SetPropertyAsync(session, nameof(TextileSession.FillColor), session.FillColor.ToString());
        await SetPropertyAsync(session, nameof(TextileSession.TieupPosition), session.TieupPosition.ToString());
        await SetPropertyAsync(session, nameof(TextileSession.UseDefaultConfigure), session.UseDefaultConfigure.ToString());
        await SetPropertyAsync(session, nameof(TextileSession.TextileStructure), session.TextileStructure.ToBase64());
    }

    private ValueTask<string?> GetKeyListAsync() => webStorage.GetItemAsync(SessionListKey);
    private async Task<TextileSession?> GetSessionAsync(Guid guid, IEditorConfigure editorConfigure)
    {
        try
        {
            var name = await webStorage.GetItemAsync(GenerateKey(guid, nameof(TextileSession.Name)));
            if (SKColor.TryParse(await webStorage.GetItemAsync(GenerateKey(guid, nameof(TextileSession.BorderColor))), out SKColor borderColor)
                && SKColor.TryParse(await webStorage.GetItemAsync(GenerateKey(guid, nameof(TextileSession.FillColor))), out SKColor fillColor)
                && Enum.TryParse<Corner>(await webStorage.GetItemAsync(GenerateKey(guid, nameof(TextileSession.TieupPosition))), out Corner tieupPosition)
                && bool.TryParse(await webStorage.GetItemAsync(GenerateKey(guid, nameof(TextileSession.UseDefaultConfigure))), out bool useDefaultConfigure))
            {

                var textile = (await webStorage.GetItemAsync(GenerateKey(guid, nameof(TextileSession.TextileStructure)))).ToTextileStructure();
                return new(editorConfigure, useDefaultConfigure, textile, borderColor, fillColor, tieupPosition, name ?? throw new NullReferenceException(), guid);
            }
        }
        catch (Exception)
        {
        }
        return null;
    }

    internal async Task InitializeAsync(IEditorConfigure configure)
    {
        var list = await GetKeyListAsync();
        if (list is null)
            return;
        sessions.Clear();
        foreach (var guid in list.Split(Separator))
        {
            if(Guid.TryParse(guid, out var key))
            {
                var session = await GetSessionAsync(key, configure);
                if(session is not null)
                    sessions.Add(session);
            }    
        }

    }
}
file static class Base64Helper
{
    public static string ToBase64(this TextileStructure structure)
    {
        using PoolingArrayBufferWriter<byte> bytes = [];
        structure.Serialize(bytes);
        return Convert.ToBase64String(bytes.WrittenMemory.Span);
    }
    public static TextileStructure ToTextileStructure(this string? base64)
    {
        if (string.IsNullOrEmpty(base64))
            throw new ArgumentException($"'{nameof(base64)}' cannot be null or empty.", nameof(base64));

        var bytes = Convert.FromBase64String(base64);
        return TextileStructure.Deserialize(bytes);
    }
}