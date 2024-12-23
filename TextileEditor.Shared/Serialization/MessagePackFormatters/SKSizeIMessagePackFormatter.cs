using MessagePack;
using MessagePack.Formatters;
using SkiaSharp;

namespace TextileEditor.Shared.Serialization.MessagePackFormatters;

public class SKSizeIMessagePackFormatter : IMessagePackFormatter<SKSizeI>
{
    public SKSizeI Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var (width, height) = (reader.ReadInt32(), reader.ReadInt32());
        return new(width, height);
    }

    public void Serialize(ref MessagePackWriter writer, SKSizeI value, MessagePackSerializerOptions options)
    {
        writer.WriteInt32(value.Width);
        writer.WriteInt32(value.Height);
    }
}
