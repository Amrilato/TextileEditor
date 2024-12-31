using DotNext.Buffers;
using Textile.Data;
using TextileEditor.Shared.Serialization.Textile;

namespace TextileEditor.Web.Services.BackwardCompatibility;


public class OldTextileSessionStorage(IWebStorage webStorage)
{
    private const string SessionListKey = $"__{nameof(SessionListKey)}__";
    private const string KeyPrefix = $"__TextileSessionStorage__";

    private static string GenerateKey(Guid guid, string identifier) => GenerateKey(guid.ToString(), identifier);
    private static string GenerateKey(string guid, string identifier) => $"{KeyPrefix}{guid}__{identifier}__";
    private const char Separator = ',';

    private Task<string?> GetKeyListAsync() => webStorage.GetItemAsync(SessionListKey);
    private async Task<TextileData?> GetSessionAsync(Guid guid)
    {
        try
        {
            var name = await webStorage.GetItemAsync(GenerateKey(guid, "Name")) ?? "Failed load name…";
            var textile = (await webStorage.GetItemAsync(GenerateKey(guid, "TextileStructure"))).ToTextileStructure();
            await webStorage.RemoveItemAsync(GenerateKey(guid, "Name"));
            await webStorage.RemoveItemAsync(GenerateKey(guid, "TextileStructure"));
            return new() { Name = name, TextileStructure = textile, Guid = guid };
        }
        catch (Exception)
        {
        }
        return null;
    }

    public async Task<IReadOnlyList<TextileData>> ReadStorage()
    {
        var list = await GetKeyListAsync();
        if (list is null)
            return [];
        var sessions = new List<TextileData>();
        foreach (var rawGuid in list.Split(Separator))
        {
            if (Guid.TryParse(rawGuid, out var guid))
            {
                await webStorage.RemoveItemAsync(GenerateKey(guid, "BorderColor"));
                await webStorage.RemoveItemAsync(GenerateKey(guid, "FillColor"));
                await webStorage.RemoveItemAsync(GenerateKey(guid, "TieupPosition"));
                await webStorage.RemoveItemAsync(GenerateKey(guid, "UseDefaultConfigure"));
                var session = await GetSessionAsync(guid);
                if (session is not null)
                    sessions.Add(session);
            }
        }
        await webStorage.RemoveItemAsync(SessionListKey);
        return sessions;
    }
}
file static class Base64Helper
{
    public static string ToBase64(this TextileStructure structure)
    {
        using PoolingArrayBufferWriter<byte> bytes = [];
        structure.Serialize(bytes);
        return Convert.ToBase64String(bytes.WrittenMemory.Span);
    }
    public static TextileStructure ToTextileStructure(this string? base64)
    {
        if (string.IsNullOrEmpty(base64))
            throw new ArgumentException($"'{nameof(base64)}' cannot be null or empty.", nameof(base64));

        var bytes = Convert.FromBase64String(base64);
        return TextileStructure.Deserialize(bytes);
    }
}