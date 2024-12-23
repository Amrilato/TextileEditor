using MessagePack;
using System.Buffers;

namespace TextileEditor.Shared.Serialization.Textile;

internal static class TextileDataSerializer
{
    private const byte Version = 1;
    public static void Serialize(TextileData textileData, IBufferWriter<byte> bufferWriter)
    {
        bufferWriter.GetSpan(1)[0] = Version;
        bufferWriter.Advance(1);
        MessagePackSerializer.Serialize(bufferWriter, textileData);
    }

    public static TextileData Deserialize(ReadOnlyMemory<byte> buffer)
    {
        switch (buffer.Span[0])
        {
            case 0:
                var transferObj = MessagePackSerializer.Deserialize<TextileSessionDataTransferObject>(buffer.Slice(1));
                return new() { Name = $"Ported from Version 0 TextileData. Required a manually rename", TextileStructure = transferObj.TextileStructure };
            case 1:
                return MessagePackSerializer.Deserialize<TextileData>(buffer.Slice(1));
            default:
                throw new ArgumentException();
        }
    }
}
