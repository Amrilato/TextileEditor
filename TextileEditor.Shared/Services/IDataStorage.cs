using DotNext.Buffers;
using System.Buffers;

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

    /// <summary>
    /// Loads data associated with the specified key from the storage.
    /// </summary>
    /// <param name="key">The key of the data to load.</param>
    /// <returns>A IMemoryOwner<byte> containing the data if found; otherwise, null.</returns>
    Task<IMemoryOwner<byte>?> LoadAsync(string key);

    /// <summary>
    /// Deletes data associated with the specified key from the storage.
    /// </summary>
    /// <param name="key">The key of the data to delete.</param>
    Task DeleteAsync(string key);

    /// <summary>
    /// Clears all data from the storage.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ClearAsync();
}
