using Microsoft.Extensions.DependencyInjection;
using TextileEditor.Shared.Services.Internal;
using TextileEditor.Shared.View.TextileEditor.Pipeline;
using TextileEditor.Shared.View.TextilePreview.Pipeline;

namespace TextileEditor.Shared.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTextileServices<TDataStorage, TTextileEditorViewRenderPipelineProvider, TTextilePreviewRenderPipelineProvider>(this IServiceCollection serviceDescriptors)
        where TDataStorage : class, IDataStorage
        where TTextileEditorViewRenderPipelineProvider : class, ITextileEditorViewRenderPipelineProvider
        where TTextilePreviewRenderPipelineProvider : class, ITextilePreviewRenderPipelineProvider
    {
        serviceDescriptors
            .AddScoped<IDataStorage, TDataStorage>()
            .AddScoped<ITextileEditorViewRenderPipelineProvider, TTextileEditorViewRenderPipelineProvider>()
            .AddScoped<ITextilePreviewRenderPipelineProvider, TTextilePreviewRenderPipelineProvider>()
            .AddScoped<ITextileSessionStorage, TextileSessionStorage>()
            .AddScoped<IAppSettings, LazyAppSettings>();

        return serviceDescriptors;
    }
    public static async Task<IServiceProvider> InitializeAsync(this IServiceProvider serviceProvider)
    {
        var settings = serviceProvider.GetService<IAppSettings>();
        if (settings is LazyAppSettings lazy)
            await lazy.InitializeAsync();
        return serviceProvider;
    }
}
