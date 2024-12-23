using MessagePack;
using MessagePack.Formatters;
using SkiaSharp;

namespace TextileEditor.Shared.Serialization.MessagePackFormatters;

public class SKColorMessagePackFormatter : IMessagePackFormatter<SKColor>
{
    public SKColor Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => reader.ReadUInt32();

    public void Serialize(ref MessagePackWriter writer, SKColor value, MessagePackSerializerOptions options) => writer.WriteUInt32((uint)value);
}
