using MessagePack;
using System.Buffers;

namespace TextileEditor.Shared.Serialization.Configuration;

internal static class AppSettingsSerializer
{
    public static void Serialize(AppSettings appSettings, IBufferWriter<byte> bufferWriter) => MessagePackSerializer.Serialize(bufferWriter, appSettings);
    public static AppSettings Deserialize(ReadOnlyMemory<byte> buffer) => MessagePackSerializer.Deserialize<AppSettings>(buffer);
}
