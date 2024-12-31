using TextileEditor.Shared.View.TextileEditor.Pipeline;
using TextileEditor.Shared.View.TextileEditor;
using System.Diagnostics.CodeAnalysis;
using Textile.Data;
using TextileEditor.Shared.View.TextilePreview;
using TextileEditor.Shared.View.TextilePreview.Pipeline;
using DotNext.Buffers;
using System.Buffers;
using TextileEditor.Shared.Serialization.Textile;
using TextileEditor.Shared.Common.Logger;
using System.Runtime.InteropServices;
using System.Collections.Immutable;

namespace TextileEditor.Shared.Services;

public interface ITextileSessionStorage
{
    IReadOnlyList<TextileSession> Sessions { get; }

    event SessionListChangedEvent? SessionListChanged;

    Task CreateAsync(TextileData textileData);
    Task AddOrSaveAsync(TextileSession session);
    Task RemoveAsync(TextileSession session);
    void Serialize(TextileData textileData, IBufferWriter<byte> bufferWriter);
    Task DeserializeAsync(ReadOnlyMemory<byte> buffer);
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

public static class ITextileSessionStorageExtensions
{
    public static Task CreateAsync(this ITextileSessionStorage sessionStorage, ITextileStructureSize size, string name) => sessionStorage.CreateAsync(new() { TextileStructure = new(size), Name = name });
}

public class TextileSession
{
    private readonly IAppSettings appSettings;
    private readonly ITextileEditorViewRenderPipelineProvider textileEditorViewRenderPipelineProvider;
    private readonly ITextilePreviewRenderPipelineProvider textilePreviewRenderPipelineProvider;

    internal TextileSession(TextileData textileData, IAppSettings appSettings, ITextileEditorViewRenderPipelineProvider textileEditorViewRenderPipelineProvider, ITextilePreviewRenderPipelineProvider textilePreviewRenderPipelineProvider)
    {
        TextileData = textileData ?? throw new ArgumentNullException(nameof(textileData));
        this.appSettings = appSettings;
        this.textileEditorViewRenderPipelineProvider = textileEditorViewRenderPipelineProvider;
        this.textilePreviewRenderPipelineProvider = textilePreviewRenderPipelineProvider;
        Logger = new(textileData);
        TextileEditorViewContext = default!;
        TextilePreviewContext = default!;
    }

    public TextileData TextileData { get; }
    public TextileLogger Logger { get; }
    public TextileEditorViewContext TextileEditorViewContext
    {
        get
        {
            return field ??= new(TextileData.TextileStructure, textileEditorViewRenderPipelineProvider, appSettings);
        }
    }
    public TextilePreviewContext TextilePreviewContext
    {
        get
        {
            return field ??= new(textilePreviewRenderPipelineProvider, TextileData.TextileStructure, appSettings);
        }
    }
}

internal class TextileSessionStorage(IDataStorage dataStorage, IAppSettings appSettings, ITextileEditorViewRenderPipelineProvider textileEditorViewRenderPipelineProvider, ITextilePreviewRenderPipelineProvider textilePreviewRenderPipelineProvider) : ITextileSessionStorage
{
    private const string ListKey = nameof(Sessions);
    private static async Task<IReadOnlyList<TextileData?>> LoadAsync(IDataStorage dataStorage)
    {
        using var owner = await dataStorage.LoadAsync(ListKey);
        if (owner is null)
            return ImmutableArray<TextileData>.Empty;
        var list = new string(MemoryMarshal.Cast<byte, char>(owner.Memory.Span)).Split(',');
        var buffer = new TextileData[list.Length];
        try
        {
            for (int i = 0; i < list.Length; i++)
            {
                try
                {
                    using var memory = await dataStorage.LoadAsync($"{nameof(TextileData)}-{list[i]}");
                    if (memory is not null)
                        buffer[i] = TextileDataSerializer.Deserialize(memory.Memory);
                }
                catch (Exception)
                { }
            }
            return buffer;
        }
        catch (Exception)
        {
            //add logging here
            return ImmutableArray<TextileData>.Empty;
        }
    }
    private Task<IReadOnlyList<TextileData?>>? loadTask = LoadAsync(dataStorage);


    private readonly Lock Lock = new();
    private int version;
    private int readOnlyVersion;
    private readonly List<TextileSession> sessions = [];
    [field: AllowNull]
    public IReadOnlyList<TextileSession> Sessions
    {
        get
        {
            using (Lock.EnterScope())
            {
                if (loadTask is not null)
                {
                    loadTask.ContinueWith(l =>
                    {
                        foreach (var data in l.Result.Where(d => d is not null))
                        {
                            var session = new TextileSession(data!, appSettings, textileEditorViewRenderPipelineProvider, textilePreviewRenderPipelineProvider);
                            sessions.Add(session);
                            version++;
                            SessionListChanged?.Invoke(this, new(session, SessionListChangedState.Add));
                        }
                    }, TaskContinuationOptions.OnlyOnRanToCompletion);
                    loadTask = null;
                }

                if (readOnlyVersion >= version)
                    return field ??= [.. sessions];
                else
                {
                    readOnlyVersion = version;
                    return field = [.. sessions];
                }
            }
        }
    }

    public event SessionListChangedEvent? SessionListChanged;

    public Task CreateAsync(TextileData textileData) => AddOrSaveAsync(new TextileSession(textileData, appSettings, textileEditorViewRenderPipelineProvider, textilePreviewRenderPipelineProvider));
    public async Task AddOrSaveAsync(TextileSession session)
    {
        bool adding = false;
        using (Lock.EnterScope())
        {
            if (!sessions.Select(s => s.TextileData.Guid).Contains(session.TextileData.Guid))
            {
                sessions.Add(session);
                adding = true;
                version++;
            }
        }
        if (adding)
            SessionListChanged?.Invoke(this, new(session, SessionListChangedState.Add));
        PoolingArrayBufferWriter<byte> buffer = new(ArrayPool<byte>.Shared);
        TextileDataSerializer.Serialize(session.TextileData, buffer);
        await dataStorage.SaveAsync($"{nameof(TextileData)}-{session.TextileData.Guid}", buffer.DetachBuffer().Span);
        if (adding)
            await SaveList();
    }

    public async Task RemoveAsync(TextileSession session)
    {
        bool removed;
        using (Lock.EnterScope())
        {
            if (removed = sessions.Remove(session))
                version++;
        }
        if (removed)
        {
            SessionListChanged?.Invoke(this, new(session, SessionListChangedState.Remove));
            await dataStorage.DeleteAsync($"{nameof(TextileData)}-{session.TextileData.Guid}");
            await SaveList();
        }
    }
    public void Serialize(TextileData textileData, IBufferWriter<byte> bufferWriter) => TextileDataSerializer.Serialize(textileData, bufferWriter);
    public Task DeserializeAsync(ReadOnlyMemory<byte> buffer) => CreateAsync(TextileDataSerializer.Deserialize(buffer));

    private async Task SaveList()
    {
        string list;
        using (Lock.EnterScope())
            list = sessions.Select(s => s.TextileData.Guid).Aggregate("", (key, guid) => $"{key}{(key.Length < 1 ? "" : ",")}{guid}");
        await dataStorage.SaveAsync(ListKey, MemoryMarshal.AsBytes<char>(list));
    }
}
