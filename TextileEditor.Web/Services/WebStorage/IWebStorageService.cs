namespace TextileEditor.Web.Services;

public interface IWebStorageService
{
    ValueTask ClearAsync();
    ValueTask DisposeAsync();
    ValueTask<string?> GetItemAsync(string key);
    ValueTask RemoveItemAsync(string key);
    ValueTask SetItemAsync(string key, string value);
}