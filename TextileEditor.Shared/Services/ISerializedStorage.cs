using DotNext.Buffers;
using MessagePack.Resolvers;
using MessagePack;
using System.Buffers;
using TextileEditor.Shared.Serialization;
using TextileEditor.Shared.Services.Configuration;

namespace TextileEditor.Shared.Services;

/// <summary>
/// Interface for a serialized storage system that handles saving and loading objects in a serialized format.
/// </summary>
public interface ISerializedStorage
{
    /// <summary>
    /// Saves an object to the storage with a specified key.
    /// </summary>
    /// <typeparam name="T">The type of the object to save.</typeparam>
    /// <param name="key">The key to associate with the object.</param>
    /// <param name="data">The object to save.</param>
    Task SaveAsync<T>(string key, T data);

    /// <summary>
    /// Loads an object from the storage associated with the specified key.
    /// </summary>
    /// <typeparam name="T">The type of the object to load.</typeparam>
    /// <param name="key">The key of the object to load.</param>
    /// <returns>The deserialized object of type <typeparamref name="T"/> or null if not found.</returns>
    Task<T?> LoadAsync<T>(string key);

    /// <summary>
    /// Deletes an object associated with the specified key from the storage.
    /// </summary>
    /// <param name="key">The key of the object to delete.</param>
    Task DeleteAsync(string key);
}

public class SerializedStorage : ISerializedStorage
{
    private readonly IDataStorage dataStorage;

    public SerializedStorage(IDataStorage dataStorage)
    {
        this.dataStorage = dataStorage;
    }

    public Task DeleteAsync(string key) => dataStorage.DeleteAsync(key);

    public async Task<T?> LoadAsync<T>(string key)
    {
        var data = await dataStorage.LoadAsync(key);
        if (data is null)
            return default;
        return MessagePackSerializer.Deserialize<T>(data, IsKnownType<T>());
    }

    public async Task SaveAsync<T>(string key, T data)
    {
        using PoolingBufferWriter<byte> bufferWriter = new(ArrayPool<byte>.Shared.ToAllocator());
        MessagePackSerializer.Serialize(bufferWriter, data, IsKnownType<T>());
        using var buffer = bufferWriter.DetachBuffer();
        await dataStorage.SaveAsync(key, buffer.Span);
    }

    private static MessagePackSerializerOptions IsKnownType<T>()
    {
        switch (default(T))
        {
            case TextileSession:
            case AppSettings:
                return StandardResolver.Options;
            default:
                return ContractlessStandardResolver.Options;
        }
    }
}