using DotNext.Buffers;
using MessagePack;
using MessagePack.Formatters;
using System.Buffers;
using Textile.Data;

namespace TextileEditor.Shared.Serialization.MessagePackFormatters;

public class TextileStructureMessagePackFormatter : IMessagePackFormatter<TextileStructure?>
{
    public TextileStructure? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var length = reader.ReadInt32();
        var raw = reader.ReadRaw(length);
        Span<byte> bytes = stackalloc byte[length];
        raw.CopyTo(bytes);
        return TextileStructure.Deserialize(bytes);
    }

    public void Serialize(ref MessagePackWriter writer, TextileStructure? value, MessagePackSerializerOptions options)
    {
        PoolingArrayBufferWriter<byte> bytes = new();
        if(value is not null)
            value.Serialize(bytes);
        writer.WriteInt32(bytes.WrittenCount);
        writer.WriteRaw(bytes.WrittenMemory.Span);
    }
}
