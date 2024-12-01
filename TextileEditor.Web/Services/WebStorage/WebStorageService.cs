using Microsoft.JSInterop;

namespace TextileEditor.Web.Services;

public class WebStorageService(IJSRuntime jsRuntime) : IAsyncDisposable, IWebStorageService
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private IJSObjectReference? _module;
    private IJSObjectReference? _localStorageInstance;

    //issue => https://github.com/dotnet/csharplang/issues/6888
    //[MemberNotNull(nameof(_localStorageInstance))]
    private async Task InitializeAsync()
    {
        try
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/webStorage.js");
            _localStorageInstance = await _module.InvokeAsync<IJSObjectReference>("LocalStorage.create");
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException("Failed to initialize the WebStorageService. Ensure the JavaScript file is correctly referenced and the method exists.", ex);
        }
    }

    private async Task EnsureInitializedAsync()
    {
        if (_localStorageInstance is null)
        {
            await InitializeAsync();
        }
    }

    public async ValueTask SetItemAsync(string key, string value)
    {
        await EnsureInitializedAsync();
        await _localStorageInstance!.InvokeVoidAsync("setItem", key, value);
    }

    public async ValueTask<string?> GetItemAsync(string key)
    {
        await EnsureInitializedAsync();
        return await _localStorageInstance!.InvokeAsync<string?>("getItem", key);
    }

    public async ValueTask RemoveItemAsync(string key)
    {
        await EnsureInitializedAsync();
        await _localStorageInstance!.InvokeVoidAsync("removeItem", key);
    }

    public async ValueTask ClearAsync()
    {
        await EnsureInitializedAsync();
        await _localStorageInstance!.InvokeVoidAsync("clear");
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        if (_localStorageInstance is not null)
        {
            await _localStorageInstance.DisposeAsync();
        }

        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}