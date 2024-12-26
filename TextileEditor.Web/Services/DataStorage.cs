using DotNext.Buffers;
using System.Buffers;
using TextileEditor.Shared.Services;

namespace TextileEditor.Web.Services;

public class DataStorage(IWebStorage webStorage) : IDataStorage
{
    public async Task DeleteAsync(string key)
    {
        await webStorage.RemoveItemAsync(key);
    }

    public async Task<IMemoryOwner<byte>?> LoadAsync(string key)
    {
        var raw = await webStorage.GetItemAsync(key);
        PoolingArrayBufferWriter<byte> buffer = new(ArrayPool<byte>.Shared);
        if (raw is null)
            return null;
        Parse(raw, buffer);
        return buffer.DetachBuffer();
    }

    public Task SaveAsync(string key, ReadOnlySpan<byte> data) => webStorage.SetItemAsync(key, Stringify(data));
    public Task ClearAsync() => webStorage.ClearAsync();

    #region Stringify
    private static string Stringify(ReadOnlySpan<byte> bytes) => Convert.ToBase64String(bytes);
    private static void Parse(string data, IBufferWriter<byte> bufferWriter)
    {
        Span<byte> byteData = stackalloc byte[CalculateDecodedSize(data)];
        Convert.TryFromBase64String(data, byteData, out int bytesWritten);
        bufferWriter.Write(byteData);

        static int CalculateDecodedSize(string base64)
        {
            if (string.IsNullOrEmpty(base64))
                return 0;

            // Count the number of padding characters ('=')
            int paddingCount = base64.EndsWith("==") ? 2 :
                               base64.EndsWith("=") ? 1 : 0;

            // Actual Base64 character length (excluding padding)
            int base64Length = base64.Length - paddingCount;

            // Calculate the number of bytes after decoding
            int decodedSize = (base64Length * 3) / 4;

            return decodedSize;
        }
    }
    #endregion
}