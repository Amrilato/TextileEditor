using DotNext.Buffers;
using MessagePack;
using MessagePack.Formatters;
using SkiaSharp;
using System.Buffers;
using Textile.Data;
using TextileEditor.Shared.Shared.Common;

namespace TextileEditor.Shared.MessagePackFormatters;

public class SKColorMessagePackFormatter : IMessagePackFormatter<SKColor>
{
    public SKColor Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => reader.ReadUInt32();

    public void Serialize(ref MessagePackWriter writer, SKColor value, MessagePackSerializerOptions options) => writer.WriteUInt32((uint)value);
}

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

public class CornerMessagePackFormatter : IMessagePackFormatter<Corner>
{
    public Corner Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => (Corner)reader.ReadInt32();

    public void Serialize(ref MessagePackWriter writer, Corner value, MessagePackSerializerOptions options) => writer.WriteInt32((int)value);
}

public class TextileStructureMessagePackFormatter : IMessagePackFormatter<TextileStructure>
{
    public TextileStructure Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var length = reader.ReadInt32();
        var raw = reader.ReadRaw(length);
        Span<byte> bytes = stackalloc byte[length];
        raw.CopyTo(bytes);
        return TextileStructure.Deserialize(bytes);
    }

    public void Serialize(ref MessagePackWriter writer, TextileStructure value, MessagePackSerializerOptions options)
    {
        PoolingArrayBufferWriter<byte> bytes = new();
        value.Serialize(bytes);
        writer.WriteInt32(bytes.WrittenCount);
        writer.WriteRaw(bytes.WrittenMemory.Span);
    }
}
