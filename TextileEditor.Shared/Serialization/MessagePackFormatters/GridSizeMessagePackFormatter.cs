using MessagePack;
using MessagePack.Formatters;
using TextileEditor.Shared.View.TextileEditor;

namespace TextileEditor.Shared.Serialization.MessagePackFormatters;

public class GridSizeMessagePackFormatter : IMessagePackFormatter<GridSize>
{
    public GridSize Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => new(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

    public void Serialize(ref MessagePackWriter writer, GridSize value, MessagePackSerializerOptions options)
    {
        writer.WriteInt32(value.BorderWidth);
        writer.WriteInt32(value.ColumnWidth);
        writer.WriteInt32(value.RowHeight);
    }
}
