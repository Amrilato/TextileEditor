using TextileEditor.Shared.View.TextileEditor.Pipeline;
using TextileEditor.Shared.View.TextileEditor;
using System.Diagnostics.CodeAnalysis;
using Textile.Data;
using TextileEditor.Shared.View.TextilePreview;
using TextileEditor.Shared.View.TextilePreview.Pipeline;
using DotNext.Buffers;
using System.Buffers;
using TextileEditor.Shared.Serialization.Textile;

namespace TextileEditor.Shared.Services;

public interface ITextileSessionStorage
{
    IReadOnlyList<TextileSession> Sessions { get; }

    event SessionListChangedEvent? SessionListChanged;

    Task CreateAsync(ITextileStructureSize size, string name);
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

public class TextileSession
{
    internal TextileSession(TextileData textileData, IAppSettings appSettings, ITextileEditorViewRenderPipelineProvider textileEditorViewRenderPipelineProvider, ITextilePreviewRenderPipelineProvider textilePreviewRenderPipelineProvider)
    {
        TextileData = textileData ?? throw new ArgumentNullException(nameof(textileData));
        TextileEditorViewContext = new(textileData.TextileStructure, textileEditorViewRenderPipelineProvider, appSettings);
        TextilePreviewContext = new(textilePreviewRenderPipelineProvider, textileData.TextileStructure, appSettings);
    }

    public TextileData TextileData { get; }
    public TextileEditorViewContext TextileEditorViewContext { get; init; }
    public TextilePreviewContext TextilePreviewContext { get; init; }
}

internal class TextileSessionStorage(IDataStorage dataStorage, IAppSettings appSettings, ITextileEditorViewRenderPipelineProvider textileEditorViewRenderPipelineProvider, ITextilePreviewRenderPipelineProvider textilePreviewRenderPipelineProvider) : ITextileSessionStorage
{
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

    public Task CreateAsync(ITextileStructureSize size, string name) => CreateAsync(new() { TextileStructure = new(size), Name = name });
    public Task CreateAsync(TextileData textileData) => AddOrSaveAsync(new TextileSession(textileData, appSettings, textileEditorViewRenderPipelineProvider, textilePreviewRenderPipelineProvider));
    public async Task AddOrSaveAsync(TextileSession session)
    {
        using (Lock.EnterScope())
        {
            sessions.Add(session);
            version++;
        }
        SessionListChanged?.Invoke(this, new(session, SessionListChangedState.Add));
        PoolingArrayBufferWriter<byte> buffer = new(ArrayPool<byte>.Shared);
        TextileDataSerializer.Serialize(session.TextileData, buffer);
        await dataStorage.SaveAsync($"TextileData-{session.TextileData.Guid}", buffer.DetachBuffer().Span);
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
            await dataStorage.DeleteAsync($"TextileData-{session.TextileData.Guid}");
        }
    }
    public void Serialize(TextileData textileData, IBufferWriter<byte> bufferWriter) => TextileDataSerializer.Serialize(textileData, bufferWriter);
    public Task DeserializeAsync(ReadOnlyMemory<byte> buffer) => CreateAsync(TextileDataSerializer.Deserialize(buffer));
}
