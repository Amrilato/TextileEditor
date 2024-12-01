using MessagePack;
using System.Buffers;
using TextileEditor.Shared.Services.TextileSessionStorage;

namespace TextileEditor.Shared.Services;

public static class TextileSessionSerializer
{
    private const byte SessionFile = 0;

    public static void Serialize(TextileSession session, IBufferWriter<byte> buffer)
    {
        buffer.GetSpan(1)[0] = SessionFile;
        buffer.Advance(1);
        TextileSessionDataTransferObject transferObject = new(session);
        MessagePackSerializer.Serialize(buffer, transferObject);
    }

    private static TextileSession DeserializeSession(ITextileSessionStorage storage, string fileName, Stream stream, IEditorConfigure editorConfigure)
    {
        var dto = MessagePackSerializer.Deserialize<TextileSessionDataTransferObject>(stream);
        return new(editorConfigure, dto, fileName, storage.GenerateGuid());
    }

    public static async Task DeserializeAsync(this ITextileSessionStorage storage, string fileName, Stream stream, IEditorConfigure editorConfigure)
    {
        var header = stream.ReadByte();
        switch (header)
        {
            case SessionFile:
                {
                    await storage.AddOrSaveAsync(DeserializeSession(storage, fileName, stream, editorConfigure));
                }
                break;
            default:
                break;
        }
    }
}
