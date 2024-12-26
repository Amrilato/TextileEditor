using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.FluentUI.AspNetCore.Components;
using TextileEditor.Shared.Services;
using TextileEditor.Shared.View.TextileEditor.Pipeline;
using TextileEditor.Shared.View.TextilePreview.Pipeline;
using TextileEditor.Web;
using TextileEditor.Web.Services;
using TextileEditor.Web.Services.BackwardCompatibility;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }); 
builder.Services.AddFluentUIComponents();
builder.Services.AddLocalization();
builder.Services.AddScoped<ILocalizer>(sp => new Localizer(sp.GetRequiredService<IWebStorage>(), sp.GetRequiredService<IStringLocalizer<SharedResource>>(), [new("en-US"), new("ja-JP")]));
builder.Services.AddTextileServices<DataStorage, DefaultTextileEditorRendererPipelineProvider, DefaultTextilePreviewRenderPipelineProvider>();

var webAssemblyHost = builder.Build();

var localizer = webAssemblyHost.Services.GetService<ILocalizer>();
if(localizer is not null)
    await localizer.SetCulture(await localizer.GetCulture());

await webAssemblyHost.RunAsync();