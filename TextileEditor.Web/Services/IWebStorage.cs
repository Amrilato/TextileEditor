namespace TextileEditor.Web.Services;

public interface IWebStorage
{
    Task ClearAsync();
    Task<string?> GetItemAsync(string key);
    Task RemoveItemAsync(string key);
    Task SetItemAsync(string key, string value);
}