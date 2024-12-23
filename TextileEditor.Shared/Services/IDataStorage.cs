namespace TextileEditor.Shared.Services;

/// <summary>
/// Interface for a general-purpose data storage system handling raw byte data.
/// </summary>
public interface IDataStorage
{
    /// <summary>
    /// Saves data to the storage with a specified key.
    /// </summary>
    /// <param name="key">The key to associate with the data.</param>
    /// <param name="data">The data to save as a read-only span of bytes.</param>
    Task SaveAsync(string key, ReadOnlySpan<byte> data);

    Task<byte[]?> LoadAsync(string key);

    /// <summary>
    /// Deletes data associated with the specified key from the storage.
    /// </summary>
    /// <param name="key">The key of the data to delete.</param>
    Task DeleteAsync(string key);
}


    //private static string Stringify(ReadOnlySpan<byte> bytes) => Convert.ToBase64String(bytes);
    //private static void Parse(string data, IBufferWriter<byte> bufferWriter)
    //{
    //    Span<byte> byteData = stackalloc byte[CalculateDecodedSize(data)];
    //    Convert.TryFromBase64String(data, byteData, out int bytesWritten);
    //    Span<byte> buffer = bufferWriter.GetSpan(bytesWritten);
    //    bufferWriter.Write(byteData);

    //    static int CalculateDecodedSize(string base64)
    //    {
    //        if (string.IsNullOrEmpty(base64))
    //            return 0;

    //        // Count the number of padding characters ('=')
    //        int paddingCount = base64.EndsWith("==") ? 2 :
    //                           base64.EndsWith("=") ? 1 : 0;

    //        // Actual Base64 character length (excluding padding)
    //        int base64Length = base64.Length - paddingCount;

    //        // Calculate the number of bytes after decoding
    //        int decodedSize = (base64Length * 3) / 4;

    //        return decodedSize;
    //    }
    //}