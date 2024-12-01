using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Runtime.InteropServices;

namespace TextileEditor.Web.Components;

public partial class Download : ComponentBase, IAsyncDisposable
{
    [Inject]
    public required IJSRuntime JSRuntime { get; init; }

    private const string JavaScriptPath = "./Components/Download/Download.razor.js";
    private const string Identifier = "downloadFileFromBytes";
    private IJSObjectReference? Module;

    [Parameter, EditorRequired]
    public string FileName { get; set; } = string.Empty;
    [Parameter, EditorRequired]
    public string FileExtension { get; set; } = string.Empty;

    public async Task DownloadFileFromByte(byte[] binaryData)
    {
        Module ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", JavaScriptPath);
        var handle = GCHandle.Alloc(binaryData, GCHandleType.Pinned);
        await Module.InvokeVoidAsync(Identifier, $"{FileName}.{FileExtension}", handle.AddrOfPinnedObject().ToInt64(), binaryData.Length);
        handle.Free();
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        if(Module is not null)
            await Module.DisposeAsync();
    }
}
