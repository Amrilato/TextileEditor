using Microsoft.JSInterop;
using System.Runtime.InteropServices;

namespace TextileEditor.Web.Services;

public class FileDownloadService(IJSRuntime jSRuntime) : IAsyncDisposable
{
    private readonly IJSRuntime _jSRuntime = jSRuntime;
    private IJSObjectReference? _module;
    private IJSObjectReference? _fileDownloader;
    private readonly Dictionary<string, GCHandle> Handles = [];

    //issue => https://github.com/dotnet/csharplang/issues/6888
    //[MemberNotNull(nameof(_localStorageInstance))]
    private async Task InitializeAsync()
    {
        try
        {
            _module = await _jSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/fileDownloader.js");
            _fileDownloader = await _module.InvokeAsync<IJSObjectReference>("FileDownloader.create");
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException("Failed to initialize the FileDownloaderService. Ensure the JavaScript file is correctly referenced and the method exists.", ex);
        }
    }

    private async Task EnsureInitializedAsync()
    {
        if (_fileDownloader is null)
        {
            await InitializeAsync();
        }
    }

    public async ValueTask<string> CreateBlobUrlAsync(byte[] binaryData)
    {
        await EnsureInitializedAsync();
        var handle = GCHandle.Alloc(binaryData, GCHandleType.Pinned);
        var identifier = await _fileDownloader!.InvokeAsync<string>("createBlobUrl", handle.AddrOfPinnedObject().ToInt64(), binaryData.Length);
        Handles.Add(identifier, handle);
        return identifier;
    }

    public async ValueTask DownloadAsync(byte[] binaryData, string name, string extension)
    {
        await EnsureInitializedAsync();
        var handle = GCHandle.Alloc(binaryData, GCHandleType.Pinned);
        var identifier = await _fileDownloader!.InvokeAsync<string>("createBlobUrl", handle.AddrOfPinnedObject().ToInt64(), binaryData.Length);
        await _fileDownloader!.InvokeVoidAsync("downloadFile", $"{name}.{extension}", identifier);
        await _fileDownloader!.InvokeVoidAsync("revokeUrl", identifier);
        handle.Free();
    }

    public async ValueTask RemoveItemAsync(string url)
    {
        await EnsureInitializedAsync();
        Handles.Remove(url);
        await _fileDownloader!.InvokeVoidAsync("revokeUrl", url);
    }

    public async ValueTask ClearAsync()
    {
        await EnsureInitializedAsync();
        foreach (var (_, handle) in Handles)
            handle.Free();
        Handles.Clear();
        await _fileDownloader!.InvokeVoidAsync("clear");
    }


    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        if (_module is not null)
            await _module.DisposeAsync();
        if (_fileDownloader is not null)
            await _fileDownloader.DisposeAsync();
        await ClearAsync();
    }
}
