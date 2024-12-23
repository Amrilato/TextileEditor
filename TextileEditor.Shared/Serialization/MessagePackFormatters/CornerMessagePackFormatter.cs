using MessagePack;
using MessagePack.Formatters;
using TextileEditor.Shared.View.TextileEditor;

namespace TextileEditor.Shared.Serialization.MessagePackFormatters;

public class CornerMessagePackFormatter : IMessagePackFormatter<Corner>
{
    public Corner Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => (Corner)reader.ReadInt32();

    public void Serialize(ref MessagePackWriter writer, Corner value, MessagePackSerializerOptions options) => writer.WriteInt32((int)value);
}
