using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.Services.Extensions;
using TextileEditor.Shared.Services.TextileSessionStorage;
using TextileEditor.Web;
using TextileEditor.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }); 
builder.Services.AddFluentUIComponents();
builder.Services.AddScoped<IWebStorageService, WebStorageService>();
builder.Services.AddTextileServices<EditorConfigure, TextileSessionStorage>();
builder.Services.AddScoped<IBlazorTextileEnvironmentConfigure, BlazorTextileEnvironmentConfigure>();
builder.Services.AddScoped<FileDownloadService, FileDownloadService>();

var webAssemblyHost = builder.Build();

await InitializeTextileConfigure(webAssemblyHost);

await webAssemblyHost.RunAsync();
static async Task InitializeTextileConfigure(WebAssemblyHost host)
{
    var configure = host.Services.GetRequiredService<IEditorConfigure>();
    await configure.LoadSettingsAsync();
    var textileSessionStorage = host.Services.GetRequiredService<ITextileSessionStorage>() as TextileSessionStorage;
    await textileSessionStorage!.InitializeAsync(configure);
    var blazorEnvironment = host.Services.GetRequiredService<IBlazorTextileEnvironmentConfigure>();
    await blazorEnvironment.LoadSettingsAsync();
}